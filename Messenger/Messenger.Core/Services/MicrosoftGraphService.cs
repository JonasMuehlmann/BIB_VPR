﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Messenger.Core.Helpers;
using Messenger.Core.Models;

namespace Messenger.Core.Services
{
    /// <summary>
    /// Holds static methods to interact with the microsoft graph service used to
    /// authenticate users and retrieve initial user data
    /// <returns></returns>
    public class MicrosoftGraphService
    {
        //// For more information about Get-User Service, refer to the following documentation
        //// https://docs.microsoft.com/graph/api/user-get?view=graph-rest-1.0
        //// You can test calls to the Microsoft Graph with the Microsoft Graph Explorer
        //// https://developer.microsoft.com/graph/graph-explorer

        private static string _graphAPIEndpoint = "https://graph.microsoft.com/v1.0/";
        private static string _apiServiceMe = "me/";
        private static string _apiServiceMePhoto = "me/photo/$value";

        /// <summary>
        /// Get a user object from a specified accessToken
        /// </summary>
        /// <param name="accessToken">An accessToken used to authenticate a user</param>
        /// <returns>A user object holding the authenticated user's data</returns>
        public static async Task<User> GetUserInfoAsync(string accessToken)
        {
            User user = null;
            var httpContent = await GetDataAsync($"{_graphAPIEndpoint}{_apiServiceMe}", accessToken);
            if (httpContent != null)
            {
                var userData = await httpContent.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(userData))
                {
                    user = await Json.ToObjectAsync<User>(userData);
                }
            }

            return user;
        }

        /// <summary>
        /// Get a user's profile photo from a specified accessToken
        /// </summary>
        /// <param name="accessToken">An accessToken used to authenticate a user</param>
        /// <returns>A user's profile photo as a base64 encoded string</returns>
        public static async Task<string> GetUserPhoto(string accessToken)
        {
            var httpContent = await GetDataAsync($"{_graphAPIEndpoint}{_apiServiceMePhoto}", accessToken);

            if (httpContent == null)
            {
                return string.Empty;
            }

            var stream = await httpContent.ReadAsStreamAsync();
            return stream.ToBase64String();
        }

        private static async Task<HttpContent> GetDataAsync(string url, string accessToken)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    var response = await httpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        return response.Content;
                    }
                    else
                    {
                        // TODO WTS: Please handle other status codes as appropriate to your scenario
                    }
                }
            }
            catch (HttpRequestException)
            {
                // TODO WTS: The request failed due to an underlying issue such as
                // network connectivity, DNS failure, server certificate validation or timeout.
                // Please handle this exception as appropriate to your scenario
            }
            catch (Exception)
            {
                // TODO WTS: This call can fail please handle exceptions as appropriate to your scenario
            }

            return null;
        }
    }
}
