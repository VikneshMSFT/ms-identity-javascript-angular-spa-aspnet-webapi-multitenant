using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoListAPI.BusinessModels.TeamsModels;

namespace TodoListAPI.BusinessModels
{
    public class User
    {
        public string UserName { get; set; }
        public string ZoomAccessToken { get; set; }
        public string ZoomRefreshToken { get; set; }
        public string GraphAccessToken { get; set; }
        public string GraphRefreshToken { get; set; }

        public AADUser AadUser { get; set; }

        public ZoomUser ZoomUser { get; set; }
    }
}
