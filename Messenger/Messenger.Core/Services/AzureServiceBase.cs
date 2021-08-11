﻿using System;
using System.Configuration;
using System.Data.SqlClient;
using Serilog;
using Messenger.Core.Helpers;

namespace Messenger.Core.Services
{
    public abstract class AzureServiceBase
    {
        #region Private

        public static ILogger logger => GlobalLogger.Instance;

        #endregion

        public static SqlConnection GetDefaultConnection()
        {
            string connectionString;

            var envVar = Environment.GetEnvironmentVariable("BIB_VPR_DEBUG");

            if (envVar is null || envVar == "false")
            {
                connectionString = Environment.GetEnvironmentVariable("BIB_VPR_CON_STRING_PROD");
            }
            else
            {
                connectionString = Environment.GetEnvironmentVariable("BIB_VPR_CON_STRING_TEST");
            }

            return new SqlConnection(connectionString);
        }

    }
}
