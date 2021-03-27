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
    public class FetchZoomChannelsForUserHandler : IMessageHandler
    {
        private IUserRepository _repository;
        private HttpClient _httpClient;
        private IConfiguration _config;
        private INotifier _notifier;

        public FetchZoomChannelsForUserHandler(HttpClient httpClient,
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
                var userMessage = (UserMessage)message;
                Console.WriteLine($"Recieved message for fetching zoom channel for user {userMessage.O365UserUPN}" 
                    + " count - " + userMessage.RetryCount);
                var user = await _repository.GetUser(userMessage.O365UserUPN);
                var zoomUserId = user.ZoomUser.Id;
                string nextPageToken = "";
                while (true)
                {
                    var uriString = _config["ZoomApiBaseUrl"] + "/chat/users/" + zoomUserId + "/channels?page_size=50";
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
                        Console.WriteLine($"Error when fetching channel for zoom user {userMessage.O365UserUPN} - #{responseContent}");
                        throw new Exception(responseContent);
                    }

                    var zoomChannels = JsonConvert.DeserializeObject<ChannelList>(responseContent);

                    foreach(ZoomChannel channel in zoomChannels.channelList)
                    {
                        var newlyAddingChannel = await _repository.AddOrUpdateZoomChannel(channel);
                        this._notifier.Notify(new ChannelAddedMessage
                        {
                            MessageType = MessageConstants.FetchParticipantsForZoomChannelUserMessageType,
                            O365UserUPN = userMessage.O365UserUPN,
                            ZoomChannelId = channel.Id,
                        });
                        if(newlyAddingChannel)
                        {
                            this._notifier.Notify(new ChannelAddedMessage
                            {
                                MessageType = MessageConstants.FetchChatMessagesForZoomChannel,
                                O365UserUPN = userMessage.O365UserUPN,
                                ZoomChannelId = channel.Id,
                                ZoomUserId = zoomUserId,
                            });
                        }
                    }
                    nextPageToken = zoomChannels.NextPageToken;
                    if (nextPageToken == null || nextPageToken.Length == 0)
                    {
                        break;
                    }
                    Thread.Sleep(2000);
                }
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
