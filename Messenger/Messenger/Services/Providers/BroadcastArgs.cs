﻿namespace Messenger.Services.Providers
{
    public class BroadcastArgs
    {
        public object Payload { get; set; }

        public BroadcastReasons Reason { get; set; }
    }
}
