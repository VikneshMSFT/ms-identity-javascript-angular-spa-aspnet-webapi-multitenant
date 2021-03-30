using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoListAPI.BackGroundWorker.Message
{
    public static class MessageConstants
    {
        public static string ZoomLoginMessageType = "Zoom_User_Login";

        public static string FetchZoomChannelsForUserMessageType = "Fetch_Zoom_Channel_For_User";

        public static string FetchParticipantsForZoomChannelUserMessageType = "Fetch_Participants_For_Zoom_Channel";

        public static string FetchChatMessagesForZoomChannel = "Fetch_Chat_Messages_For_Zoom_Channel";

        public static string ImportChatMessagesForZoomChannelIntoTeams = "Import_Chat_Messages_Into_Teams";

        public static string FetchAADUser = "Fetch_AAD_User";

    }
}
