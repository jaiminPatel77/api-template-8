using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Neuronms.Data.Services.BLServices
{
    /// <summary>
    /// Used singleton httpClient for any HTTP call from a service.
    /// </summary>
    public static class HttpClientService
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        public const string ErrorInfo = "ErrorInfo";


        #region Http calls 

        /// <summary>
        /// wrapper to make actual http get call with error handing.
        /// </summary>
        /// <param name="requestUri">requested URI for HTTP GET request.</param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            var response = await _httpClient.GetAsync(requestUri);
            await CheckAndThrowError(response, requestUri);
            return response;
        }

        /// <summary>
        /// Wrapper to make actual http post with JSON Data as given type with error handing.
        /// </summary>
        /// <typeparam name="T">Object type which is send to be as JSON POST request.</typeparam>
        /// <param name="requestUri">URI information</param>
        /// <param name="value">Object to be set pas JSON body for post request.</param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T value)
        {
            var response = await _httpClient.PostAsJsonAsync(requestUri, value);
            await CheckAndThrowError(response, requestUri);
            return response;
        }

        /// <summary>
        /// Post the give T type as JSON to given URL and read JSON response as U type!
        /// </summary>
        /// <typeparam name="T">Type of Object to be send as JSON body as part of post request.</typeparam>
        /// <typeparam name="U">Type of Object return as response.</typeparam>
        /// <param name="requestUri">URL for a post.</param>
        /// <param name="value">Value to be posted.</param>
        /// <returns>Response of type U..</returns>
        public static async Task<U> PostAsJsonAsync<T, U>(string requestUri, T value)
        {
            using (var response = await _httpClient.PostAsJsonAsync(requestUri, value))
            {
                await CheckAndThrowError(response, requestUri);
                return await response.Content.ReadAsAsync<U>();
            }
        }

        #endregion

        #region Test URL

        /// <summary>
        /// wrapper to make actual http get call with error handing.
        /// </summary>
        /// <param name="requestUri">Requested URL</param>
        /// <returns>return true if HTTP request is successful.</returns>
        public static async Task<bool> TestUrl(string requestUri)
        {
            using (var response = await _httpClient.GetAsync(requestUri))
            {
                await CheckAndThrowError(response, requestUri);
                return true;
            }
        }
        #endregion
        
        #region Throw Error

        /// <summary>
        /// Check and throw error if response of http request is not valid.
        /// </summary>
        /// <param name="response">response of http request.</param>
        /// <param name="url">URL of which response received.</param>
        /// <returns></returns>
        private static async Task CheckAndThrowError(HttpResponseMessage response, String url)
        {
            if (!response.IsSuccessStatusCode)
            {

                Exception exception = null;
                if (!String.IsNullOrEmpty(response.ReasonPhrase))
                {
                    exception = new Exception(String.Format("Service return error:{0}, url: {1}", response.ReasonPhrase, url));
                }
                else
                {
                    exception = new Exception(String.Format("Service unavailable or return unknown error, url: {0}", url));
                }
                try
                {
                    var retVal = await response.Content.ReadAsAsync<JObject>();
                    if (retVal != null)
                    {
                        exception.Data.Add(ErrorInfo, retVal);
                    }
                }
                catch (Exception)
                {
                    //do nothing in case if object is not return..
                }
                //Dispose response if throwing exception.
                response.Dispose();
                throw exception;
            }
        }

        #endregion

    }
}
