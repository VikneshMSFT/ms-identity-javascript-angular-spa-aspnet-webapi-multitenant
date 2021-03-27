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
    }
}
