using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using TodoListAPI.BackGroundWorker.Message;
using TodoListAPI.BackGroundWorker.MessageHandler.ApiResponses;
using TodoListAPI.BusinessModels;
using TodoListAPI.Repository;

namespace TodoListAPI.BackGroundWorker.MessageHandler
{
    public class FetchParticipantsForZoomChannelHandler : IMessageHandler
    {
        private IUserRepository _repository;
        private HttpClient _httpClient;
        private IConfiguration _config;
        private INotifier _notifier;

        public FetchParticipantsForZoomChannelHandler(HttpClient httpClient,
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
                var channelMessage = (ChannelAddedMessage)message;
                Console.WriteLine($"Recieved message for fetching participants for channel {channelMessage.O365UserUPN}"
                    + " count - " + channelMessage.RetryCount);
                var user = await _repository.GetUser(channelMessage.O365UserUPN);
                var zoomUserId = user.ZoomUser.Id;
                string nextPageToken = "";
                while (true)
                {
                    var uriString = _config["ZoomApiBaseUrl"] + "/chat/users/" + zoomUserId + "/channels/"
                        + channelMessage.ZoomChannelId +"/members?page_size=50";
                    if (nextPageToken != null && nextPageToken.Length != 0)
                    {
                        uriString = uriString + "&next_page_token=" + nextPageToken;
                    }
                    var uri = new Uri(uriString);
                    var httpReqMessage = new HttpRequestMessage(HttpMethod.Get, uri);
                    httpReqMessage.Headers.Authorization = new AuthenticationHeaderValue(
                        "Bearer",
                        user.ZoomAccessToken);
                    var response = await _httpClient.SendAsync(httpReqMessage);
                    var responseContent = await response?.Content?.ReadAsStringAsync();
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        Console.WriteLine($"Error when fetching participants for channel -  {channelMessage.O365UserUPN} - #{responseContent}");
                        throw new Exception(responseContent);
                    }
                    var channelMembers = JsonConvert.DeserializeObject<ZoomChannelMembers>(responseContent);

                    foreach (ZoomUser member in channelMembers.users)
                    {                        
                        await _repository.AddMemberToZoomChannel(channelMessage.ZoomChannelId, member);                        
                    }
                    nextPageToken = channelMembers.NextPageToken;
                    if (nextPageToken == null || nextPageToken.Length == 0)
                    {
                        break;
                    }
                    Thread.Sleep(200);
                }
            }
            catch (Exception ex)
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
