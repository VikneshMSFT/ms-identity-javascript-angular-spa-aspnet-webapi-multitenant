using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TodoListAPI.BackGroundWorker.Message;
using TodoListAPI.BusinessModels.TeamsModels;
using TodoListAPI.Repository;
using TodoListAPI.Services;

namespace TodoListAPI.BackGroundWorker.MessageHandler
{
    public class FetchGraphUserHandler : IMessageHandler
    {
        private IUserRepository _repository;
        private HttpClient _httpClient;
        private IConfiguration _config;
        private INotifier _notifier;
        private IGraphAuthService _graphAuthService;

        public FetchGraphUserHandler(HttpClient httpClient,
            IUserRepository repository,
            IConfiguration config,
            INotifier notifier,
            IGraphAuthService graphAuthService
        )
        {
            this._repository = repository;
            this._httpClient = httpClient;
            this._config = config;
            this._notifier = notifier;
            this._graphAuthService = graphAuthService;
            _httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<bool> HandleMessageAsync(AbstractMessage message)
        {
            try
            {
 
                var apiCaller = new ProtectedApiCallHelper(_httpClient);
                var userMessage = (UserMessage)message;
                var user = await _repository.GetUser(userMessage.O365UserUPN);
                Console.WriteLine($"Fetching Graph User {userMessage.O365UserUPN}"
                    + " count - " + message.RetryCount);
                var response = await apiCaller.CallWebApiAndProcessResultASync($"https://graph.microsoft.com/v1.0/me", user.GraphAccessToken);
                var aadUser = JsonConvert.DeserializeObject<AADUser>(response);
                user.AadUser = aadUser;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error when fetching AAD user from graph endpoint" + ex.Message);
                message.RetryCount++;
                _notifier.Notify(message);
            }           
            return true;
        }
    }
}
