using System;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Messenger.Core.Services
{
    public class ChannelService: AzureServiceBase
    {
        public async Task<uint?> CreateChannel(string channelName, uint teamId)
        {
            throw new NotSupportedException();
        }

        public async Task<bool> RemoveChannel(uint channelId, uint teamId)
        {
            throw new NotSupportedException();
        }

        public async Task<bool> RenameChannel(string channelName, uint channelId)
        {
            throw new NotSupportedException();
        }
    }
}
