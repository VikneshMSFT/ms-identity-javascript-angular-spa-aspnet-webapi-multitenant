using System.Collections.Generic;
using System.Threading.Tasks;
using TodoListAPI.BusinessModels;

namespace TodoListAPI.Repository
{
    public interface IUserRepository
    {
        Task<bool> SaveUser(User user);

        Task<User> GetUser(string userName);

        Task<bool> AddOrUpdate(User user);

        Task<bool> AddZoomUser(string O365Upn, ZoomUser zoomUser);

        Task<bool> AddOrUpdateZoomChannel(ZoomChannel channel);

        Task<bool> AddMemberToZoomChannel(string channelId, ZoomUser user);

        Task<bool> AddChatMessageToZoomChannel(string channelId, ChannelMessage message);

        Task<List<ChannelMessage>> GetChannelMessagesForChannelId(string zoomChannelId);

        Task<ZoomChannel> GetChannel(string zoomChannelId);

        Task<List<User>> GetOwnerOfZoomChannel(string zoomChannelId);

        Task<List<User>> GetMembersOfZoomChannel(string zoomChannelId);

        Task<User> GetUserByZoomMailId(string zoomMailId);

        Task<ICollection<ZoomChannel>> GetAllChannels();

    }
}
