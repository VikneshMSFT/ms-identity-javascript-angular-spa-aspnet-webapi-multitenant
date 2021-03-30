using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoListAPI.Services
{
    public interface IGraphAuthService
    {
        public Task<AuthenticationResult> GetGraphAuthResult();
    }
}
