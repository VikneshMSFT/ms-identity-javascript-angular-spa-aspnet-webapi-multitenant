using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoListAPI.BusinessModels;

namespace TodoListAPI.Repository
{
    public class InMemoryUserRepository : IUserRepository
    {
        private ConcurrentDictionary<string, User> userDictionary = new ConcurrentDictionary<string, User>();
        private ConcurrentDictionary<string, User> ZoomIdUserDictionary = new ConcurrentDictionary<string, User>();
        private ConcurrentDictionary<string, User> ZoomEmailUserDictionary = new ConcurrentDictionary<string, User>();

        private ConcurrentDictionary<string, ZoomChannel> channelDictionary = new ConcurrentDictionary<string, ZoomChannel>();
        private ConcurrentDictionary<string, HashSet<string>> channelIdToMemberDictionary = new ConcurrentDictionary<string, HashSet<string>>();
        private ConcurrentDictionary<string, HashSet<string>> channelIdToOwnerDictionary = new ConcurrentDictionary<string, HashSet<string>>();
        private ConcurrentDictionary<string, List<ChannelMessage>> channelIdToMessage = new ConcurrentDictionary<string, List<ChannelMessage>>();

        public Task<bool> AddChatMessageToZoomChannel(string channelId, ChannelMessage message)
        {
            if(!channelIdToMessage.ContainsKey(channelId))
            {
                channelIdToMessage[channelId] = new List<ChannelMessage>();
            }
            channelIdToMessage[channelId].Add(message);
            return Task.FromResult(true);
        }

        public Task<bool> AddMemberToZoomChannel(string channelId, ZoomUser user)
        {
            if("member".Equals(user.Role))
            {
                if(!channelIdToMemberDictionary.ContainsKey(channelId))
                {
                    channelIdToMemberDictionary[channelId] = new HashSet<string>();
                }
                channelIdToMemberDictionary[channelId].Add(user.Id);
            }
            else if("owner".Equals(user.Role))
            {
                if (!channelIdToOwnerDictionary.ContainsKey(channelId))
                {
                    channelIdToOwnerDictionary[channelId] = new HashSet<string>();
                }
                channelIdToOwnerDictionary[channelId].Add(user.Id);
            }
            return Task.FromResult(true);
        }

        public Task<bool> AddOrUpdate(User user)
        {
            userDictionary.AddOrUpdate(user.UserName, user, (key, _) => user);
            return Task.FromResult(true);
        }

        public Task<bool> AddOrUpdateZoomChannel(ZoomChannel channel)
        {
            bool newChannel = false;
            if(!channelDictionary.ContainsKey(channel.Id))
            {
                newChannel = true;
            }
            channelDictionary.AddOrUpdate(channel.Id, channel, (key, _) => channel);
            return Task.FromResult(newChannel);
        }

        public Task<bool> AddZoomUser(string O365Upn, ZoomUser zoomUser)
        {
            userDictionary[O365Upn].ZoomUser = zoomUser;
            ZoomIdUserDictionary[zoomUser.Id] = userDictionary[O365Upn];
            ZoomEmailUserDictionary[zoomUser.Email] = userDictionary[O365Upn];
            return Task.FromResult(true);
        }

        public Task<User> GetUser(string userName)
        {
            User user = null;
            if(userDictionary.ContainsKey(userName))
            {
                user = userDictionary[userName];
            }
            return Task.FromResult(user);
        }

        public Task<bool> SaveUser(User user)
        {
            userDictionary.GetOrAdd(user.UserName, user);
            return Task.FromResult(true);
        }


    }
}
