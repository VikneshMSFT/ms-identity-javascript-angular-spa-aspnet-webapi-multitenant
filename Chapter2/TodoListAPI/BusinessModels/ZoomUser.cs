using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoListAPI.BusinessModels
{
    public class ZoomUser
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("role_name")]
        public string RoleName { get; set; }

        [JsonProperty("personal_meeting_url")]
        public string PersonalMeetingUrl { get; set; }

        [JsonProperty("timezone")]
        public string TimeZone { get; set; }

        [JsonProperty("account_id")]
        public string AccountId { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("last_login_time")]
        public string LastLoginTime { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }       

    }
}
