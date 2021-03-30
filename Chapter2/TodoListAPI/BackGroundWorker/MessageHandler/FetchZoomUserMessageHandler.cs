using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using TodoListAPI.BackGroundWorker.Message;
using TodoListAPI.BusinessModels;
using TodoListAPI.Repository;

namespace TodoListAPI.BackGroundWorker.MessageHandler
{
    public class FetchZoomUserMessageHandler : IMessageHandler
    {
        private IUserRepository _repository;
        private HttpClient _httpClient;
        private IConfiguration _config;
        private INotifier _notifier;

        public FetchZoomUserMessageHandler(HttpClient httpClient, 
            IUserRepository repository,
            IConfiguration config,
            INotifier notifier)
        {
            this._repository = repository;
            this._httpClient = httpClient;
            this._config = config;
            this._notifier = notifier;            
            _httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<bool> HandleMessageAsync(AbstractMessage message)
        {
            try
            {                              
                UserMessage userMessage = (UserMessage)message;
                var user = await _repository.GetUser(userMessage.O365UserUPN);
                Console.WriteLine($"Recieved message for fetching zoom user {userMessage.O365UserUPN}" + " count - " + message.RetryCount);
                var accesssToken = user.ZoomAccessToken;
                var handler = new JwtSecurityTokenHandler();
                var decodedToken = handler.ReadJwtToken(accesssToken);
                var uidClaim = decodedToken.Claims.First((claim) => "uid".Equals(claim.Type));
                var zoomUserId = uidClaim.Value;
                Uri uri = new Uri(_config["ZoomApiBaseUrl"] + "/users/" + zoomUserId);
                var httpReqMessage = new HttpRequestMessage(HttpMethod.Get, uri);
                httpReqMessage.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    user.ZoomAccessToken);
                var response = await _httpClient.SendAsync(httpReqMessage);
                var responseContent = await response?.Content?.ReadAsStringAsync();
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine($"Error when fetching zoom user {userMessage.O365UserUPN} - #{responseContent}");
                    throw new Exception(responseContent);
                }
                var zoomUser = JsonConvert.DeserializeObject<ZoomUser>(responseContent);
                _ = await _repository.AddZoomUser(userMessage.O365UserUPN, zoomUser);                
                _notifier.Notify(new UserMessage
                {
                    MessageType = MessageConstants.FetchZoomChannelsForUserMessageType,
                    O365UserUPN = userMessage.O365UserUPN,
                });

                //todo remove below

               /* _notifier.Notify(new UserMessage
                {
                    MessageType = MessageConstants.ImportChatMessagesForZoomChannelIntoTeams,
                    O365UserUPN = userMessage.O365UserUPN,
                });*/
            } 
            catch(Exception ex)
            {
                message.RetryCount++;
                message.ErrorMessageInPreviousTry = ex.Message;
                _notifier.Notify(message);
            }
            _notifier.NotifyCompletion();          
            return true;   
        }
    }
}
