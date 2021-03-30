using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TodoListAPI.BackGroundWorker.Message;
using TodoListAPI.BusinessModels;
using TodoListAPI.BusinessModels.TeamsModels;
using TodoListAPI.Repository;
using TodoListAPI.Services;
using User = TodoListAPI.BusinessModels.TeamsModels.User;

namespace TodoListAPI.BackGroundWorker.MessageHandler
{
    public class ImportChatMessagesIntoTeamsHandler : IMessageHandler
    {
        private IUserRepository _repository;
        private HttpClient _httpClient;
        private IConfiguration _config;
        private INotifier _notifier;
        private ProtectedApiCallHelper _apiCaller;
        private Task<AuthenticationResult> _authResult;
        private AuthenticationResult _authenticateResult;
        private IGraphAuthService _graphAuthService;

        public ImportChatMessagesIntoTeamsHandler(HttpClient httpClient,
            IUserRepository repository,
            IConfiguration config,
            INotifier notifier,
            IGraphAuthService graphAuthService)
        {
            this._repository = repository;
            this._httpClient = httpClient;
            this._config = config;
            this._notifier = notifier;
            this._graphAuthService = graphAuthService;
            this._apiCaller = new ProtectedApiCallHelper(httpClient);
            _httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<bool> HandleMessageAsync(AbstractMessage message)
        {
            this._authenticateResult = await this._graphAuthService.GetGraphAuthResult();
            try
            {
                if (this._authenticateResult != null)
                {
                    var channelAddedMessage = (ChannelAddedMessage)message;
                    var messages = await this._repository.GetChannelMessagesForChannelId(channelAddedMessage.ZoomChannelId);
                    
                    var channel = await this._repository.GetChannel(channelAddedMessage.ZoomChannelId);                    
                    var apiCaller = new ProtectedApiCallHelper(_httpClient);
                    //await apiCaller.CallWebApiAndProcessResultASync($"https://teamsgraph.teams.microsoft.com/beta/teams('250dfa22-2334-4d15-a7c0-7d3bb9303e36')/channels", result.AccessToken, Display);
                    //await apiCaller.CallWebApiAndProcessResultASync($"https://graph.microsoft.com/beta/teams", result.AccessToken, Display);

                    string currentTime = DateTime.Now.ToString("yyyy-mm-ddThh:mm:ss") + ".043Z";
                    string channelCreationTime = DateTime.Now.ToString("yyyy-mm-ddThh:mm:ss") + ".053Z";
                    //Create Team with migration mode set - Copy the teamId from Response Header
                    CreateTeam newTeam = new CreateTeam
                    {
                        teamCreationMode = "migration",
                        bind = "https://graph.microsoft.com/beta/teamsTemplates('standard')",
                        displayName = "zoom" + channel.Name ,
                        description = channel.Name,
                        createdDateTime = "2021-03-12T11:22:17.043Z"
                    };

                    var data = new StringContent(JsonConvert.SerializeObject(newTeam), Encoding.UTF8, "application/json");
                    var response = await apiCaller.CallWebApiPostAndProcessResultASync($"https://graph.microsoft.com/beta/teams",
                        this._authenticateResult.AccessToken, Display, data);
                    var location = response.Headers.Location?.ToString();
                    var teamId = ((location.Split('/')[1]).Remove(0, 7)).Remove(36, 2);
                    var channelId = "";

                    CreateChannelRequest newChannel = new CreateChannelRequest
                    {
                        channelCreationMode = "migration",
                        displayName = channel.Name + "Test",
                        description = channel.Name,
                        membershipType = "standard",
                        createdDateTime = "2021-03-12T11:22:17.053Z"
                    };
                    data = new StringContent(JsonConvert.SerializeObject(newChannel), Encoding.UTF8, "application/json");
                    response = await apiCaller.CallWebApiPostAndProcessResultASync($"https://graph.microsoft.com/beta/teams/{teamId}/channels",
                        this._authenticateResult.AccessToken, Display, data);

                    if (response.IsSuccessStatusCode)
                    {
                        string json = await response.Content.ReadAsStringAsync();
                        channelId = JObject.Parse(json)["id"].ToString();
                        Console.WriteLine("ChannelId - " + channelId);
                    }
                    else
                    {
                        throw new Exception("Channel creation failed");
                    }
                    if (channelId == "")
                    {
                        throw new Exception("Channel creation failed");
                    }
                    if (messages != null && messages.Count > 0)
                    {
                        foreach (ChannelMessage textMessage in messages)
                        {
                            DateTimeOffset timeOffset = DateTimeOffset.FromUnixTimeMilliseconds(textMessage.EpochTime);
                            DateTime msgTime = timeOffset.DateTime;
                            //textMessage.Sender
                            var user = await this._repository.GetUserByZoomMailId(textMessage.Sender);
                            if (user == null)
                            {
                                Console.WriteLine($"Zoom user {textMessage.Sender} did not login yet, so skipping message");
                                continue;
                            }
                            ChatMessageRequest newMessage = new ChatMessageRequest
                            {
                                createdDateTime = msgTime.ToString("yyyy-MM-ddThh:mm:ss") + ".053Z",
                                from = new From
                                {
                                    user = new User
                                    {
                                        id = user.AadUser.Id,
                                        displayName = user.AadUser.DisplayName,
                                        userIdentityType = "aadUser"
                                    }
                                },
                                body = new ItemBody
                                {
                                    content = textMessage.Message,
                                    contentType = "html"
                                }
                            };
                            var str = JsonConvert.SerializeObject(newMessage);
                            data = new StringContent(JsonConvert.SerializeObject(newMessage), Encoding.UTF8, "application/json");
                            response = await apiCaller.CallWebApiPostAndProcessResultASync($"https://graph.microsoft.com/beta/teams/{teamId}/channels/{channelId}/messages", _authenticateResult.AccessToken, Display, data);

                            if (response.IsSuccessStatusCode)
                            {
                                Console.WriteLine("Posted msg");
                            }
                            else
                            {
                                throw new Exception("Posting msg failed");
                            }
                            Thread.Sleep(100);
                        }
                    }
                    
                    response = await apiCaller.CallWebApiPostAndProcessResultASync($"https://graph.microsoft.com/beta/teams/{teamId}/channels/{channelId}/completeMigration", _authenticateResult.AccessToken, Display, null);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Completed migration for newly created channel");
                    }
                    else
                    {
                        throw new Exception("Completing migration for channel failed");
                    }

                    string channelListResponse = await apiCaller.CallWebApiAndProcessResultASync($"https://graph.microsoft.com/v1.0/teams/{teamId}/channels", _authenticateResult.AccessToken);
                    var teamsChannels = JsonConvert.DeserializeObject<TeamsChannelList>(channelListResponse);
                    TeamsChannel generalChannel = teamsChannels.channels.Where(channel => channel.DisplayName.Equals("General")).First();
                    
                    //Need to get the 'General' channel Id and complete migration  TODO

                    response = await apiCaller.CallWebApiPostAndProcessResultASync($"https://graph.microsoft.com/beta/teams/{teamId}/channels/{generalChannel.Id}/completeMigration", _authenticateResult.AccessToken, Display, null);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Completed migration for newly created channel");
                    }
                    else
                    {
                        throw new Exception("Completing migration for channel failed");
                    }

                    response = await apiCaller.CallWebApiPostAndProcessResultASync($"https://graph.microsoft.com/beta/teams/{teamId}/completeMigration", _authenticateResult.AccessToken, Display, null);
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Completed migration for team");
                    }
                    else
                    {
                        throw new Exception("Completing migration for team failed");
                    }

                    //Add owner

                    List<TodoListAPI.BusinessModels.User> owners = await _repository.GetOwnerOfZoomChannel(channelAddedMessage.ZoomChannelId);
                    foreach (BusinessModels.User user in owners)
                    {
                        AddMemberToTeam member = new AddMemberToTeam
                        {
                            type = "#microsoft.graph.aadUserConversationMember",
                            roles = new string[] { "owner" },
                            bind = $"https://graph.microsoft.com/beta/users/{user.AadUser.Id}"
                        };
                        data = new StringContent(JsonConvert.SerializeObject(member), Encoding.UTF8, "application/json");
                        await apiCaller.CallWebApiPostAndProcessResultASync($"https://graph.microsoft.com/beta/teams/{teamId}/members", _authenticateResult.AccessToken, Display, data);
                    }

                    // Add Member
                    List<TodoListAPI.BusinessModels.User> members = await _repository.GetMembersOfZoomChannel(channelAddedMessage.ZoomChannelId);
                    foreach (BusinessModels.User user in members)
                    {
                        AddMemberToTeam member = new AddMemberToTeam
                        {
                            type = "#microsoft.graph.aadUserConversationMember",
                            roles = new string[] {  },
                            bind = $"https://graph.microsoft.com/beta/users/{user.AadUser.Id}"
                        };
                        data = new StringContent(JsonConvert.SerializeObject(member), Encoding.UTF8, "application/json");
                        await apiCaller.CallWebApiPostAndProcessResultASync($"https://graph.microsoft.com/beta/teams/{teamId}/members", _authenticateResult.AccessToken, Display, data);
                    }


                }
            }
            catch(Exception)
            {
                message.RetryCount++;
                //this._notifier.Notify(message);
            }            
            return true;
        }

        /// <summary>
        /// Checks if the sample is configured for using ClientSecret or Certificate. This method is just for the sake of this sample.
        /// You won't need this verification in your production application since you will be authenticating in AAD using one mechanism only.
        /// </summary>
        /// <param name="config">Configuration from appsettings.json</param>
        /// <returns></returns>
        private static bool AppUsesClientSecret(AuthenticationConfig config)
        {
            string clientSecretPlaceholderValue = "[Enter here a client secret for your application]";
            string certificatePlaceholderValue = "[Or instead of client secret: Enter here the name of a certificate (from the user cert store) as registered with your application]";

            if (!String.IsNullOrWhiteSpace(config.ClientSecret) && config.ClientSecret != clientSecretPlaceholderValue)
            {
                return true;
            }

            else if (!String.IsNullOrWhiteSpace(config.CertificateName) && config.CertificateName != certificatePlaceholderValue)
            {
                return false;
            }

            else
                throw new Exception("You must choose between using client secret or certificate. Please update appsettings.json file.");
        }

        private static X509Certificate2 ReadCertificate(string certificateName)
        {
            if (string.IsNullOrWhiteSpace(certificateName))
            {
                throw new ArgumentException("certificateName should not be empty. Please set the CertificateName setting in the appsettings.json", "certificateName");
            }
            X509Certificate2 cert = null;

            using (X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certCollection = store.Certificates;

                // Find unexpired certificates.
                X509Certificate2Collection currentCerts = certCollection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);

                // From the collection of unexpired certificates, find the ones with the correct name.
                X509Certificate2Collection signingCert = currentCerts.Find(X509FindType.FindBySubjectDistinguishedName, certificateName, false);

                // Return the first certificate in the collection, has the right name and is current.
                cert = signingCert.OfType<X509Certificate2>().OrderByDescending(c => c.NotBefore).FirstOrDefault();
            }
            return cert;
        }

        /// <summary>
        /// Display the result of the Web API call
        /// </summary>
        /// <param name="result">Object to display</param>
        private void Display(JObject result)
        {
            if (result != null)
            {
                foreach (JProperty child in result.Properties().Where(p => ((p.Name != null) && !p.Name.StartsWith("@"))))
                {
                    Console.WriteLine($"{child.Name} = {child.Value}");
                }
            }
        }

    }
}
