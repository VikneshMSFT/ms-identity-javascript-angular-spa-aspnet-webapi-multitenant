using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web.Resource;
using System.Threading.Tasks;
using TodoListAPI.Models;
using TodoListAPI.Repository;

namespace TodoListAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationsController : ControllerBase
    {
        static readonly string[] scopeRequiredByApi = new string[] { "access_as_user" };
        private readonly IConfiguration _config;
        private readonly IUserRepository _repository;

        public ConfigurationsController(
            IConfiguration configuration,
            IUserRepository repository)
        {
            this._repository = repository;
            this._config = configuration;
        }

        // GET: api/Configurations
        [HttpGet]
        public async Task<AppConfiguration> GetAppConfigurations()
        {
            HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
            var currentUserUPN = HttpContext.User.Identity.Name;
            var user = await _repository.GetUser(currentUserUPN);           
            var config =  new AppConfiguration();
            config.AADAppId = _config["AppClientId"];
            config.ZoomAppId = _config["ZoomClientId"];
            if (user != null)
            {
                config.ZoomLoggedIn = user.ZoomAccessToken != null;
                config.TeamsLoggedIn = user.GraphAccessToken != null;
            }
            return config;
        }
    }
}
