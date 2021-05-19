using Messenger.Core.Services;
using System;

namespace Messenger.Tests.MSTest
{
    public abstract class SqlServiceTestBase
    {
        private const string TEST_CONNECTION_STRING = @"Server=tcp:vpr.database.windows.net,1433;Initial Catalog=TEST_VPR_DATABASE;Persist Security Info=False;User ID=pbt3h19a;Password=uMb7ZXAA5TjajDw;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        protected T InitializeTestMode<T>()where T: AzureServiceBase
        {
            var service = (T)Activator.CreateInstance(typeof(T));
            service.SetTestMode(TEST_CONNECTION_STRING);
            return service;
        }
    }
}
