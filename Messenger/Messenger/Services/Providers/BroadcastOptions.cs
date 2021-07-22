using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Services.Providers
{
    public enum BroadcastOptions
    {
        TeamsLoaded,
        TeamUpdated,
        ChannelUpdated,
        ChatsLoaded,
        ChatUpdated,
        MessagesSwitched,
        MessageUpdated
    }
}
