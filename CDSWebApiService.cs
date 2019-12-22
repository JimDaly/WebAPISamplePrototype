using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;

namespace WebAPISamplePrototype
{
    public class CDSWebApiService : IDisposable
    {

        public HttpClient Client;
        private readonly string ApiUrl;
        private readonly string AccessToken;
        private readonly string Version;
        private readonly string CallerObjectId;
        public Uri BaseAddress { get; private set; }
        private const int MaxRetries = 3;
        private const bool DisableCookies = false;
        private const double TimeoutInSeconds = 60;


        /// <summary>
        /// Initializes the CDSWebApiService class with information needed to connect to the service
        /// </summary>
        /// <param name="apiUrl">The URL to access the service.</param>
        /// <param name="accessToken">The AccessToken to use to connect to the service</param>
        /// <param name="version">The version of the service to use</param>
        public CDSWebApiService(string apiUrl, string accessToken, string version = "9.1")
        {
            ApiUrl = apiUrl;
            AccessToken = accessToken;
            Version = version;
            Init();

        }
        /// <summary>
        /// Initializes the CDSWebApiService class with information needed to connect to the service
        /// </summary>
        /// <param name="apiUrl">The URL to access the service.</param>
        /// <param name="accessToken">The AccessToken to use to connect to the service</param>
        /// <param name="callerObjectId">The Azure Active Directory object id of the user to impersonate.</param>
        /// <param name="version">The version of the service to use</param>
        public CDSWebApiService(string apiUrl, string accessToken, string callerObjectId, string version = "9.1")
        {
            ApiUrl = apiUrl;
            AccessToken = accessToken;
            Version = version;
            CallerObjectId = callerObjectId;
            Init();

        }

        private void Init()
        {
            //Handler enables disabling cookies
            // The ARRAffinity cookie specifies a particular server
            // Strategy for optimizing throughput is to ignore this since the Service Protection limits are applied per web server.
            // But I see no immediate improvement by doing this.
            WebRequestHandler handler = new WebRequestHandler
            {
                CookieContainer = new CookieContainer(),
                UseCookies = !DisableCookies
            };
            Client = new HttpClient(handler)
            {
                BaseAddress = new Uri(ApiUrl + $"/api/data/v{Version}/")
            };
            BaseAddress = Client.BaseAddress;

            Client.DefaultRequestHeaders.Authorization =
              new AuthenticationHeaderValue("Bearer", AccessToken);
            Client.Timeout = TimeSpan.FromSeconds(TimeoutInSeconds);
            Client.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            Client.DefaultRequestHeaders.Add("OData-Version", "4.0");
            Client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            if (CallerObjectId != string.Empty)
            {
                Client.DefaultRequestHeaders.Add("CallerObjectId", CallerObjectId);
            }
            

        }
        /// <summary>
        /// Creates an entity record.
        /// </summary>
        /// <param name="entitySetName">The entityset name for the entity.</param>
        /// <param name="body">The JObject that contains the data to set for the entity.</param>
        /// <returns>The Uri of the created record</returns>
        public Uri PostCreate(string entitySetName, JObject body)
        {
            try
            {
                
                using (var message = new HttpRequestMessage(HttpMethod.Post, entitySetName)) {
                    message.Content = new StringContent(body.ToString());
                    message.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                    HttpResponseMessage response = Send(message);
                    return new Uri(response.Headers.GetValues("OData-EntityId").FirstOrDefault());
                }

            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Posts data to a Uri
        /// </summary>
        /// <param name="uri">The Uri to post the data to</param>
        /// <param name="body">The JObject containing the data to post.</param>
        public JObject Post(string path, JObject body)
        {
            try
            {

                using (var message = new HttpRequestMessage(HttpMethod.Post, path)) {
                    message.Content = new StringContent(body.ToString());
                    message.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                    HttpResponseMessage response = Send(message);
                    string content = response.Content.ReadAsStringAsync().Result;
                    if (string.IsNullOrEmpty(content)) {
                        return null;
                    }
                    return JObject.Parse(content);
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Retrieves data
        /// </summary>
        /// <param name="path">The location of the resources to return</param>
        /// <param name="headers">Headers to set for special behaviors</param>
        /// <returns></returns>
        public JToken Get(string path, Dictionary<string, List<string>> headers = null)
        {
            try
            {

                using (var message = new HttpRequestMessage(HttpMethod.Get, path)) {

                    if (headers != null)
                    {
                        foreach (KeyValuePair<string, List<string>> header in headers)
                        {
                            message.Headers.Add(header.Key, header.Value);
                        }

                    }

                    HttpResponseMessage response = Send(message, HttpCompletionOption.ResponseContentRead);

                    if (response.StatusCode != HttpStatusCode.NotModified) {
                        return JToken.Parse(response.Content.ReadAsStringAsync().Result);

                    }
                    return null;
                   
                }
            }
            catch (Exception)
            {
                throw;
            }

        }
        /// <summary>
        /// Sends a PATCH request
        /// </summary>
        /// <param name="uri">The Uri of the entity record to update</param>
        /// <param name="body">The JObject containing the data to post.</param>
        public void Patch(Uri uri, JObject body, Dictionary<string, List<string>> headers = null)
        {

            using (var message = new HttpRequestMessage(new HttpMethod("PATCH"), uri)) {
                message.Content = new StringContent(body.ToString());
                message.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                if (headers != null)
                {
                    foreach (KeyValuePair<string, List<string>> header in headers)
                    {
                        message.Headers.Add(header.Key, header.Value);
                    }

                }
                Send(message);
            }

        }
        /// <summary>
        /// Sends a Delete operation
        /// </summary>
        /// <param name="uri">The URI of the resource to delete</param>
        public void Delete(Uri uri, Dictionary<string, List<string>> headers = null)
        {
            try
            {
                using (var message = new HttpRequestMessage(HttpMethod.Delete, uri)) {

                    if (headers != null)
                    {
                        foreach (KeyValuePair<string, List<string>> header in headers)
                        {
                            message.Headers.Add(header.Key, header.Value);
                        }
                    }

                    Send(message, HttpCompletionOption.ResponseHeadersRead);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Sends a PUT request to set data
        /// </summary>
        /// <param name="uri">The URI of the resource to update</param>
        /// <param name="property">The name of the resource property to update.</param>
        /// <param name="value">The value of the data to update</param>
        public void Put(Uri uri, string property, string value)
        {
            try
            {
                using (var message = new HttpRequestMessage(HttpMethod.Put, $"{uri}/{property}")) {
                    var body = new JObject
                    {
                        ["value"] = value
                    };
                    message.Content = new StringContent(body.ToString());
                    message.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                    Send(message, HttpCompletionOption.ResponseHeadersRead);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        //All public methods use this
        //It includes re-try logic for Service Protection API limits
        //TODO: Add Delegating Handler to manage accessToken refresh
        
        private HttpResponseMessage Send(HttpRequestMessage request, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseHeadersRead, int retryCount = 0)
        {

            //Sending a copy of the request because if it fails the Content will be disposed and can't be sent again.
            HttpResponseMessage response;
            using (var requestCopy = request.Clone())
            {
                
                response = Client.SendAsync(requestCopy, httpCompletionOption).Result;
            }


            if (!response.IsSuccessStatusCode)
            {
                //NotModified is not considered an error for conditional operations
                if (response.StatusCode == HttpStatusCode.NotModified)
                {
                    return response;
                }

                if ((int)response.StatusCode != 429)
                {
                    //Not a service protection limit error
                    throw ParseError(response);
                }
                else
                {

                    // Give up re-trying if exceeding the maxRetries
                    if (++retryCount >= MaxRetries)
                    {
                        throw ParseError(response);
                    }

                    int seconds;
                    if (response.Headers.Contains("Retry-After"))
                    {
                        seconds = int.Parse(response.Headers.GetValues("Retry-After").FirstOrDefault());
                        Console.WriteLine($"Waiting for: {seconds} seconds based on Retry-After value.");
                    }
                    else
                    {
                        seconds = (int)Math.Pow(2, retryCount);
                        Console.WriteLine($"Waiting for: {seconds} seconds based on exponential backoff.");
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(seconds));

                    return Send(request, httpCompletionOption, retryCount);


                }
            }
            else
            {

                //int burstRemaining = int.Parse(response.Headers.GetValues("x-ms-ratelimit-burst-remaining-xrm-requests").FirstOrDefault());
                //Console.WriteLine($"Burst Remaining {burstRemaining}");


                return response;

            }

        }

        private Exception ParseError(HttpResponseMessage response)
        {
            try
            {
                var errorObject = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                string message = errorObject["error"]["message"].Value<string>();
                int code = Convert.ToInt32(errorObject["error"]["code"].Value<string>(), 16);
                int statusCode = (int)response.StatusCode;
                string reasonPhrase = response.ReasonPhrase;

                return new CDSWebApiException(code, statusCode, reasonPhrase, message);

            }
            catch (Exception)
            {
                throw;
            }

        }

        ~CDSWebApiService()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing == true)
            {

                ReleaseClient();
            }
            else
            {

            }

            ReleaseClient();
        }

        private void ReleaseClient()
        {
            if (Client != null)
            {
                Client.Dispose();
            }
        }
    }

    public class CDSWebApiException : Exception
    {

        public int ErrorCode { get; private set; }
        public int StatusCode { get; private set; }
        public string ReasonPhrase { get; private set; }

        public CDSWebApiException(int errorcode, int statuscode, string reasonphrase, string message) : base(message)
        {
            ErrorCode = errorcode;
            StatusCode = statuscode;
            ReasonPhrase = reasonphrase;
        }
    }

}
