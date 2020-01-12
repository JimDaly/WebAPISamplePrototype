using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace WebAPISamplePrototype
{
    public class CDSWebApiService : IDisposable
    {
        private readonly HttpClient httpClient;
        private readonly CDSWebApiServiceConfig config;
        /// <summary>
        /// The BaseAddresss property of the HttpClient.
        /// </summary>
        public Uri BaseAddress { get { return httpClient.BaseAddress; } }

        
        
        public CDSWebApiService(CDSWebApiServiceConfig config) {
            this.config = config;
            HttpMessageHandler messageHandler = new OAuthMessageHandler(config,
                new HttpClientHandler());
            httpClient = new HttpClient(messageHandler)
            {
                BaseAddress = new Uri(config.Url + $"/api/data/v{config.Version}/")
            };

            httpClient.Timeout = TimeSpan.FromSeconds(config.TimeoutInSeconds);
            httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            if (config.CallerObjectId != Guid.Empty)
            {
                httpClient.DefaultRequestHeaders.Add("CallerObjectId", config.CallerObjectId.ToString());
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
                using (var message = new HttpRequestMessage(HttpMethod.Post, entitySetName))
                {
                    message.Content = new StringContent(body.ToString());
                    message.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                    HttpResponseMessage response = Send(message);
                    return new Uri(response.Headers.GetValues("OData-EntityId").FirstOrDefault());
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Posts data to a Uri
        /// </summary>
        /// <param name="uri">The Uri to post the data to</param>
        /// <param name="body">The JObject containing the data to post.</param>
        public JObject Post(string path, JObject body, Dictionary<string, List<string>> headers = null)
        {
            try
            {
                using (var message = new HttpRequestMessage(HttpMethod.Post, path))
                {
                    message.Content = new StringContent(body.ToString());
                    message.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                    HttpResponseMessage response = Send(message);
                    string content = response.Content.ReadAsStringAsync().Result;
                    if (string.IsNullOrEmpty(content))
                    {
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
                using (var message = new HttpRequestMessage(HttpMethod.Get, path))
                {
                    if (headers != null)
                    {
                        foreach (KeyValuePair<string, List<string>> header in headers)
                        {
                            message.Headers.Add(header.Key, header.Value);
                        }
                    }

                    HttpResponseMessage response = Send(message, HttpCompletionOption.ResponseContentRead);

                    if (response.StatusCode != HttpStatusCode.NotModified)
                    {
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
            using (var message = new HttpRequestMessage(new HttpMethod("PATCH"), uri))
            {
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
                using (var message = new HttpRequestMessage(HttpMethod.Delete, uri))
                {
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
                using (var message = new HttpRequestMessage(HttpMethod.Put, $"{uri}/{property}"))
                {
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

        /// <summary>
        /// Sends the Http Request
        /// </summary>
        /// <param name="request">The request to send</param>
        /// <param name="httpCompletionOption">The completion option</param>
        /// <param name="retryCount">The current retry count</param>
        /// <returns></returns>
        private HttpResponseMessage Send(HttpRequestMessage request, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseHeadersRead, int retryCount = 0)
        {
            //Sending a copy of the request because if it fails the Content will be disposed and can't be sent again.
            HttpResponseMessage response;
            using (var requestCopy = request.Clone())
            {
                try
                {
                    response = httpClient.SendAsync(requestCopy, httpCompletionOption).Result;
                }
                catch (Exception ex)
                {

                    throw ex;
                }                
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
                    if (++retryCount >= config.MaxRetries)
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
            if (httpClient != null)
            {
                httpClient.Dispose();
            }
        }

        /// <summary>
        ///Custom HTTP message handler that uses OAuth authentication thru ADAL.
        /// </summary>
       private class OAuthMessageHandler : DelegatingHandler
        {
            private readonly CDSWebApiServiceConfig config;
            private readonly UserPasswordCredential _credential = null;
            private readonly AuthenticationContext _authContext;

            public OAuthMessageHandler(CDSWebApiServiceConfig configParam,
                    HttpMessageHandler innerHandler)
                : base(innerHandler)
            {
                config = configParam;

                if (config.UserPrincipalName != null && config.Password != null)
                {
                    _credential = new UserPasswordCredential(config.UserPrincipalName, config.Password);
                }
                _authContext = new AuthenticationContext(config.Authority, false);

            }

            private AuthenticationHeaderValue GetAuthHeader()
            {
                AuthenticationResult authResult;
                if (_credential == null)
                {
                    authResult = _authContext.AcquireTokenAsync(config.Url, config.ClientId, new Uri(config.RedirectUrl), new PlatformParameters(PromptBehavior.Auto)).Result;
                }
                else
                {
                    authResult = _authContext.AcquireTokenAsync(config.Url, config.ClientId, _credential).Result;
                }
                return new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
            }

            protected override Task<HttpResponseMessage> SendAsync(
                      HttpRequestMessage request, CancellationToken cancellationToken)
            {
                try
                {
                    request.Headers.Authorization = GetAuthHeader();
                }
                catch (Exception ex)
                {

                    throw ex;
                }
                return base.SendAsync(request, cancellationToken);
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

    public class CDSWebApiServiceConfig
    {

        private static string connectionString;
        private string authority = "https://login.microsoftonline.com/common";
        private string url = null;
        private string clientId = null;
        private string redirectUrl = null;

        public CDSWebApiServiceConfig(string connectionStringParam)
        {
            connectionString = connectionStringParam;

            string authorityValue = GetParameterValue("Authority");
            if (!string.IsNullOrEmpty(authorityValue))
            {
                Authority = authorityValue;
            }

            Url = GetParameterValue("Url");
            ClientId = GetParameterValue("ClientId");
            RedirectUrl = GetParameterValue("RedirectUrl");

            string userPrincipalNameValue = GetParameterValue("UserPrincipalName");
            if (!string.IsNullOrEmpty(userPrincipalNameValue))
            {
                UserPrincipalName = userPrincipalNameValue;
            }

            if (Guid.TryParse(GetParameterValue("CallerObjectId"), out Guid callerObjectId))
            {
                CallerObjectId = callerObjectId;
            }

            string versionValue = GetParameterValue("Version");
            if (!string.IsNullOrEmpty(versionValue))
            {
                Version = versionValue;
            }

            if (byte.TryParse(GetParameterValue("MaxRetries"), out byte maxRetries))
            {
                MaxRetries = maxRetries;
            }

            if (ushort.TryParse(GetParameterValue("TimeoutInSeconds"), out ushort timeoutInSeconds))
            {
                TimeoutInSeconds = timeoutInSeconds;
            }


            string pwd = GetParameterValue("Password");
            if (!string.IsNullOrEmpty(pwd))
            {
                var ss = new SecureString();

                pwd.ToCharArray().ToList().ForEach(ss.AppendChar);
                ss.MakeReadOnly();

                Password = ss;
            }


        }
        /// <summary>
        /// The authority to use to authorize user. 
        /// Default is 'https://login.microsoftonline.com/common'
        /// </summary>
        public string Authority
        {
            get => authority; set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    authority = value;
                }
                else
                {
                    throw new Exception("CDSWebApiServiceConfig.Authority value cannot be null.");
                }
            }
        }
        /// <summary>
        /// The Url to the CDS environment, i.e "https://yourorg.api.crm.dynamics.com"
        /// </summary>
        public string Url
        {
            get => url; set

            {
                if (!string.IsNullOrEmpty(value))
                {
                    url = value;
                }
                else
                {
                    throw new Exception("CDSWebApiServiceConfig.Url value cannot be null.");
                }
            }
        }
        /// <summary>
        /// The id of the application registered with Azure AD
        /// </summary>
        public string ClientId
        {
            get => clientId; set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    clientId = value;
                }
                else
                {
                    throw new Exception("CDSWebApiServiceConfig.ClientId value cannot be null.");
                }
            }
        }

        /// <summary>
        /// The Redirect Url of the application registered with Azure AD
        /// </summary>
        public string RedirectUrl { get => redirectUrl; set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    redirectUrl = value;
                }
                else
                {
                    throw new Exception("CDSWebApiServiceConfig.RedirectUrl value cannot be null.");
                }
            }
        }

        /// <summary>
        /// The user principal name of the user. i.e. you@yourorg.onmicrosoft.com
        /// </summary>
        public string UserPrincipalName { get; set; } = null;

        /// <summary>
        /// The password for the user principal
        /// </summary>
        public SecureString Password { get; set; } = null;

        /// <summary>
        /// The Azure AD ObjectId for the user to impersonate other users.
        /// </summary>
        public Guid CallerObjectId { get; set; }
        /// <summary>
        /// The version of the Web API to use
        /// Default is '9.1'
        /// </summary>
        public string Version { get; set; } = "9.1";
        /// <summary>
        /// The maximum number of attempts to retry a request blocked by service protection limits.
        /// Default is 3.
        /// </summary>
        public byte MaxRetries { get; set; } = 3;
        /// <summary>
        /// The amount of time to try completing a request before it will be cancelled.
        /// Default is 120 (2 minutes)
        /// </summary>
        public ushort TimeoutInSeconds { get; set; } = 120;
        
        /// <summary>
        /// Extracts a parameter value from a connection string
        /// </summary>
        /// <param name="parameter">The name of the parameter value</param>
        /// <returns></returns>
        private static string GetParameterValue(string parameter)
        {
            try
            {
                string value = connectionString
                    .Split(';')
                    .Where(s => s.Trim()
                    .StartsWith(parameter))
                    .FirstOrDefault()
                    .Split('=')[1];
                if (value.ToLower() == "null")
                {
                    return string.Empty;
                }
                return value;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }

    /// <summary>
    /// Contains extension methods to clone HttpRequestMessage and HttpContent types.
    /// </summary>
    public static class Extensions
    {

        /// <summary>
        /// Clones a HttpRequestMessage instance
        /// </summary>
        /// <param name="request">The HttpRequestMessage to clone.</param>
        /// <returns>A copy of the HttpRequestMessage</returns>
        public static HttpRequestMessage Clone(this HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Content = request.Content.Clone(),
                Version = request.Version
            };
            foreach (KeyValuePair<string, object> prop in request.Properties)
            {
                clone.Properties.Add(prop);
            }
            foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            return clone;
        }
        /// <summary>
        /// Clones a HttpContent instance
        /// </summary>
        /// <param name="content">The HttpContent to clone</param>
        /// <returns>A copy of the HttpContent</returns>
        public static HttpContent Clone(this HttpContent content)
        {

            if (content == null) return null;

            HttpContent clone;

            Type contentType = content.GetType();

            switch (contentType)
            {
                case Type _ when contentType == typeof(StringContent):
                    clone = new StringContent(content.ReadAsStringAsync().Result);
                    break;
                //TODO: Add support for other content types as needed.
                default:
                    throw new Exception($"{content.GetType().ToString()} Content type not implemented for HttpContent.Clone extension method.");
            }

            clone.Headers.Clear();
            foreach (KeyValuePair<string, IEnumerable<string>> header in content.Headers)
            {
                clone.Headers.Add(header.Key, header.Value);
            }
            return clone;

        }
    }
}