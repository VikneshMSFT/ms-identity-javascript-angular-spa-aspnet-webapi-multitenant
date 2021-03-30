using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public class FetchChatMessageForChannelHandler : IMessageHandler
    {
        private IUserRepository _repository;
        private HttpClient _httpClient;
        private IConfiguration _config;
        private INotifier _notifier;

        public FetchChatMessageForChannelHandler(HttpClient httpClient,
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
                Console.WriteLine($"Recieved message for fetching chat messages for channel {channelMessage.O365UserUPN}"
                    + " count - " + channelMessage.RetryCount + " date = " + channelMessage.DateToFetch);
                var user = await _repository.GetUser(channelMessage.O365UserUPN);
                var zoomUserId = user.ZoomUser.Id;
                string nextPageToken = "";
                while (true)
                {
                    ///chat/users/{userId}/messages date=2021-03-23
                    /////DateTime.Today.AddDays(-4f).ToString("yyyy-MM-dd");
                    string date = channelMessage.DateToFetch != 0 ? 
                        DateTime.Today.AddDays(-channelMessage.DateToFetch).ToString("yyyy-MM-dd") : 
                        DateTime.Today.ToString("yyyy-MM-dd");
                    var uriString = _config["ZoomApiBaseUrl"] + "/chat/users/" + zoomUserId 
                        + "/messages?page_size=50&to_channel=" + channelMessage.ZoomChannelId + "&date=" + date;
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
                        Console.WriteLine($"Error when fetching chat messages for channel -  {channelMessage.O365UserUPN} - #{responseContent}");
                        throw new Exception(responseContent);
                    }
                    var channelMessages = JsonConvert.DeserializeObject<ChannelMessages>(responseContent);

                    foreach (ChannelMessage channelMsg in channelMessages.messages)
                    {
                        await _repository.AddChatMessageToZoomChannel(channelMessage.ZoomChannelId, channelMsg);                        
                    }
                    nextPageToken = channelMessages.NextPageToken;
                    if (nextPageToken == null || nextPageToken.Length == 0)
                    {
                        break;
                    }
                    Thread.Sleep(2000);
                }
                channelMessage.DateToFetch++;
                if (channelMessage.DateToFetch <= 7)
                {
                    _notifier.Notify(message);
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
