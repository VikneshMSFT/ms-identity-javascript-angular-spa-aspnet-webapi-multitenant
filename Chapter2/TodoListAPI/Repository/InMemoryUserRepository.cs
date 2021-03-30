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

        public Task<List<ChannelMessage>> GetChannelMessagesForChannelId(string zoomChannelId)
        {
            if (channelIdToMessage.ContainsKey(zoomChannelId))
            {
                return Task.FromResult(channelIdToMessage[zoomChannelId]);
            }
            return Task.FromResult(new List<ChannelMessage>());            
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
            ZoomIdUserDictionary[zoomUser.Id.ToLower()] = userDictionary[O365Upn];
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

        public Task<User> GetUserByZoomMailId(string zoomMailId)
        {
            User user = null;
            if (ZoomEmailUserDictionary.ContainsKey(zoomMailId))
            {
                user = ZoomEmailUserDictionary[zoomMailId];
            }
            return Task.FromResult(user);
        }

        public Task<bool> SaveUser(User user)
        {
            userDictionary.GetOrAdd(user.UserName, user);
            return Task.FromResult(true);
        }

        public Task<ZoomChannel> GetChannel(string zoomChannelId)
        {
            return Task.FromResult(channelDictionary[zoomChannelId]);
        }

        public Task<List<User>> GetOwnerOfZoomChannel(string zoomChannelId)
        {
            List<User> userList = new List<User>();
            if (channelIdToOwnerDictionary.ContainsKey(zoomChannelId))
            {
                var owners = channelIdToOwnerDictionary[zoomChannelId];
                foreach (string zoomUserId in owners)
                {
                    Console.WriteLine("owner Id = " + zoomUserId);
                    if (ZoomIdUserDictionary.ContainsKey(zoomUserId))
                    {
                        userList.Add(ZoomIdUserDictionary[zoomUserId]);
                    }
                }
            }            
            return Task.FromResult(userList);
        }

        public Task<List<User>> GetMembersOfZoomChannel(string zoomChannelId)
        {
            List<User> userList = new List<User>();
            if (channelIdToMemberDictionary.ContainsKey(zoomChannelId))
            {
                var members = channelIdToMemberDictionary[zoomChannelId];
                foreach (string zoomUserId in members)
                {
                    if (ZoomIdUserDictionary.ContainsKey(zoomUserId))
                    {
                        userList.Add(ZoomIdUserDictionary[zoomUserId]);
                    }
                }
            }            
            return Task.FromResult(userList);
        }

        public Task<ICollection<ZoomChannel>> GetAllChannels()
        {
            if (channelDictionary.Keys.Count > 0)
            {
                return Task.FromResult(channelDictionary.Values);
            }
            return null;            
        }
    }
}
