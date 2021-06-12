using Messenger.Core.Services;
using System;

namespace Messenger.Tests.MSTest
{
    public abstract class SqlServiceTestBase
    {
        protected const string TEST_CONNECTION_STRING = @"Server=tcp:bib-vpr.database.windows.net,1433;Initial Catalog=vpr_messenger_database_test;Persist Security Info=False;User ID=pbt3h19a;Password=uMb7ZXAA5TjajDw;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        
        /// <summary>
        /// Returns the requested service with the test mode activated
        /// </summary>
        /// <typeparam name="T">Type of service required</typeparam>
        /// <returns>An instance of the requested service</returns>
        protected T InitializeTestMode<T>()where T: AzureServiceBase
        {
            var service = (T)Activator.CreateInstance(typeof(T));
            service.SetTestMode(TEST_CONNECTION_STRING);
            return service;
        }
    }
}
