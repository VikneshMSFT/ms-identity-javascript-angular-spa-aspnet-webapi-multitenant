using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using TodoListAPI.BusinessModels;

namespace TodoListAPI.Services
{
    public class AADAuthService : IAADAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public AADAuthService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
            _httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<Token> GetAccessTokenForAuthCode(string authCode)
        {
            Token token = null;
            var kvPairList = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("client_id", _config["AppClientId"]),
                new KeyValuePair<string, string>("code", authCode),                
                new KeyValuePair<string, string>("redirect_uri", _config["RedirectUri"]),
                new KeyValuePair<string, string>("client_secret", _config["AppSecret"]),
                new KeyValuePair<string, string>("scope", GetScope()),                
            };
            using (var content = new FormUrlEncodedContent(kvPairList))
            {
                content.Headers.Clear();
                content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                HttpResponseMessage response = await this._httpClient.PostAsync(_config["AADAuthTokenUrl"], content);
                var responseContent = await response.Content.ReadAsStringAsync();
                token = JsonConvert.DeserializeObject<Token>(responseContent);
            }
            return token;
        }

        private string GetScope()
        {
            return "https://graph.microsoft.com/Calendars.Read https://graph.microsoft.com/Calendars.ReadWrite https://graph.microsoft.com/Channel.Create " +
              "https://graph.microsoft.com/Channel.ReadBasic.All https://graph.microsoft.com/ChannelMember.Read.All https://graph.microsoft.com/ChannelMember.ReadWrite.All " +
              "https://graph.microsoft.com/ChannelMessage.Send https://graph.microsoft.com/ChannelSettings.Read.All https://graph.microsoft.com/ChannelSettings.ReadWrite.All " +
              "https://graph.microsoft.com/Chat.Create https://graph.microsoft.com/Chat.ReadWrite https://graph.microsoft.com/ChatMember.Read https://graph.microsoft.com/ChatMessage.Send " +
              "https://graph.microsoft.com/Contacts.Read https://graph.microsoft.com/email https://graph.microsoft.com/profile https://graph.microsoft.com/Team.Create " +
              "https://graph.microsoft.com/Team.ReadBasic.All https://graph.microsoft.com/TeamMember.Read.All https://graph.microsoft.com/TeamMember.ReadWrite.All " +
              "https://graph.microsoft.com/TeamMember.ReadWriteNonOwnerRole.All https://graph.microsoft.com/User.Read https://graph.microsoft.com/User.Read.All";            
        }
        
    }
}
