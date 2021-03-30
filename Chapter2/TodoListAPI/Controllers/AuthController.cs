using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TodoListAPI.BackGroundWorker;
using TodoListAPI.BackGroundWorker.Message;
using TodoListAPI.BusinessModels;
using TodoListAPI.Models;
using TodoListAPI.Repository;
using TodoListAPI.Services;

namespace TodoListAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        static readonly string[] scopeRequiredByApi = new string[] { "access_as_user" };
        private readonly IZoomAuthService _zoomAuthService;
        private readonly IAADAuthService _aadAuthService;
        private readonly IUserRepository _userRespository;
        private readonly INotifier _notifier;

        public AuthController(IZoomAuthService zoomAuthService, 
            IUserRepository userRepository,
            IAADAuthService aadAuthService,
            INotifier notifier)
        {
            this._zoomAuthService = zoomAuthService;
            this._aadAuthService = aadAuthService;
            this._userRespository = userRepository;
            this._notifier = notifier;
        }

        [HttpGet]
        [Route("TriggerTeamsImport")]
        public async Task<ActionResult<string>> TriggerTeamsSync()
        {
            HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
            var currentUserUPN = HttpContext.User.Identity.Name;
            var allChannels = await this._userRespository.GetAllChannels();
            if (allChannels == null)
            {
                return "No Channels";
            }
            _ = Task.Run(() =>
                {
                    foreach (ZoomChannel channel in allChannels)
                    {

                        _notifier.Notify(
                            new ChannelAddedMessage
                            {
                                O365UserUPN = currentUserUPN,
                                MessageType = MessageConstants.ImportChatMessagesForZoomChannelIntoTeams,
                                ZoomChannelId = channel.Id
                            }
                        );
                    Thread.Sleep(3000);
                    }
                });           
            return "Success";
        }
        
        [HttpPost]
        public async Task<ActionResult<string>> FetchAccessTokenForAuthCode(AuthCode authCode)
        {
            HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
            var currentUserUPN = HttpContext.User.Identity.Name;
            if (authCode.App.Equals("zoom"))
            {
                var user = await this._userRespository.GetUser(currentUserUPN);
                if(user?.ZoomAccessToken != null)
                {
                    return "Success";
                }
                var token = await this._zoomAuthService.GetAccessTokenForAuthCode(authCode.Code);                
                if(user == null)
                {
                    user = new TodoListAPI.BusinessModels.User()
                    {
                        UserName = currentUserUPN,
                    };
                }
                user.ZoomAccessToken = token.AccessToken;
                user.ZoomRefreshToken = token.RefreshToken;
                await this._userRespository.AddOrUpdate(user);
                _notifier.Notify(new UserMessage { 
                    O365UserUPN = currentUserUPN,
                    MessageType = MessageConstants.ZoomLoginMessageType,
                });
            } 
            else if (authCode.App.Equals("graph"))
            {
                var user = await this._userRespository.GetUser(currentUserUPN);
                if(user?.GraphAccessToken != null)
                {
                    return "Success";
                }
                var token = await this._aadAuthService.GetAccessTokenForAuthCode(authCode.Code);                
                if (user == null)
                {
                    user = new TodoListAPI.BusinessModels.User()
                    {
                        UserName = currentUserUPN,
                    };
                }
                user.GraphAccessToken = token.AccessToken;
                user.GraphRefreshToken = token.RefreshToken;
                await this._userRespository.AddOrUpdate(user);
                _notifier.Notify(
                    new UserMessage
                    {
                        O365UserUPN = currentUserUPN,
                        MessageType = MessageConstants.FetchAADUser
                    });
            }
            return "success";
        } 
    }
}
