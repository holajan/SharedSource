using System;
using System.IO;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace WindowsLive
{
    [System.Diagnostics.DebuggerDisplay("UserID = {UserID}, FullName = {FullName}, FirstName = {FirstName}, LastName = {LastName}, Gender = {Gender}, Locale = {Locale}")]
    public sealed class LiveAuthUser
    {
        #region member varible and default property initialization
        public string UserID { get; private set; }
        public string FullName { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string Gender { get; private set; }
        public string Locale { get; private set; }
        #endregion

        #region constructors and destructors
        internal LiveAuthUser(string userID, string fullName, string firstName, string lastName, string gender, string locale)
        {
            this.UserID = userID;
            this.FullName = fullName;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Gender = gender;
            this.Locale = locale;
        }
        #endregion
    }

    public class LiveAuthException : Exception
    {
        #region member varible and default property initialization
        public string ErrorCode { get; private set; }
        #endregion

        #region constructors and destructors
        public LiveAuthException() { }

        public LiveAuthException(string errorCode, string message)
            : base(message)
        {
            this.ErrorCode = errorCode;
        }

        public LiveAuthException(string errorCode, string message, Exception innerException)
            : base(message, innerException)
        {
            this.ErrorCode = errorCode;
        }
        #endregion

        #region action methods
        public override string ToString()
        {
            return this.ErrorCode + ": " + base.ToString();
        }
        #endregion
    }

    public class LiveAuthClient
    {
        #region member types declaration
        private enum DisplayType
        {
            Popup,
            Touch,
        }

        private class RequestAccessTokenResults
        {
            #region member varible and default property initialization
            public string AccessToken { get; private set; }
            public DateTimeOffset Expires { get; internal set; }
            public IEnumerable<string> Scopes { get; internal set; }
            public Exception Error { get; private set; }
            #endregion

            #region constructors and destructors
            public RequestAccessTokenResults(string accessToken, DateTimeOffset expires, IEnumerable<string> scopes)
            {
                this.AccessToken = accessToken;
                this.Expires = expires;
                this.Scopes = scopes;
            }

            public RequestAccessTokenResults(Exception error)
            {
                this.Error = error;
            }
            #endregion
        }

        private class RequestUserInfoResults
        {
            #region member varible and default property initialization
            public LiveAuthUser User { get; private set; }
            public Exception Error { get; private set; }
            #endregion

            #region constructors and destructors
            public RequestUserInfoResults(LiveAuthUser user)
            {
                this.User = user;
            }

            public RequestUserInfoResults(Exception error)
            {
                this.Error = error;
            }
            #endregion
        }

        [DataContract]
        private class AuthToken
        {
            [DataMember(Name = AuthConstants.AccessToken)]
            public string AccessToken { get; private set; }

            [DataMember(Name = AuthConstants.ExpiresIn)]
            public string ExpiresIn { get; private set; }

            [DataMember(Name = AuthConstants.Scope)]
            public string Scope { get; private set; }
        }

        [DataContract]
        private class AuthUser
        {
            [DataMember(Name = AuthConstants.ID)]
            public string ID { get; set; }

            [DataMember(Name = AuthConstants.Name)]
            public string Name { get; set; }

            [DataMember(Name = AuthConstants.FirstName)]
            public string FirstName { get; set; }

            [DataMember(Name = AuthConstants.LastName)]
            public string LastName { get; set; }

            [DataMember(Name = AuthConstants.Gender)]
            public string Gender { get; set; }

            [DataMember(Name = AuthConstants.Locale)]
            public string Locale { get; set; }
        }

        [DataContract]
        private class AuthError
        {
            [DataMember(Name = AuthConstants.Error)]
            public string ErrorCode { get; private set; }

            [DataMember(Name = AuthConstants.ErrorDescription)]
            public string ErrorDescription { get; private set; }
        }

        private static class AuthConstants
        {
            #region constants
            public const string AccessToken = "access_token";
            public const string ExpiresIn = "expires_in";
            public const string Scope = "scope";
            public const string Logout = "logout";
            public const string ID = "id";
            public const string Name = "name";
            public const string FirstName = "first_name";
            public const string LastName = "last_name";
            public const string Gender = "gender";
            public const string Locale = "locale";
            public const string Error = "error";
            public const string ErrorDescription = "error_description";
            #endregion
        }
        #endregion

        #region constants
        private const string wlCookie = "wl_auth";
        private const string ConsentEndpoint = "https://oauth.live.com";
        private const string AuthorizeUrlTemplate = "{0}/authorize?client_id={1}&redirect_uri={2}&scope={3}&response_type=code&locale={4}&display={5}&state={6}";
        private const string UserInfoUrlTemplate = "https://apis.live.net/v5.0/me?access_token={0}";
        private const string AuthCodePostBodyTemplate = "client_id={0}&code={1}&redirect_uri={2}&client_secret={3}&grant_type=authorization_code";
        private const string CreateSessionState = "create_session";
        private const DisplayType Display = DisplayType.Popup;
        private static readonly string[] DefaultScopes = new[] { "wl.signin" };
        private static readonly char[] ScopeSeparators = new[] { ' ', ',' };
        #endregion

        #region member varible and default property initialization
        private readonly string ClientId;
        private readonly string ClientSecret;
        private readonly string RedirectUri;
        private IEnumerable<string> RequiredScopes;

        private bool IsInitialized;

        private string AccessToken;
        private DateTimeOffset Expires;
        private IEnumerable<string> Scopes;
        #endregion

        #region constructors and destructors
        public LiveAuthClient(string clientId, string clientSecret, string redirectUri, IEnumerable<string> requiredScopes)
        {
            if (clientId == null)
            {
                throw new ArgumentNullException("clientId");
            }
            if (clientId.Length == 0)
            {
                throw new ArgumentException("clientId is empty.", "clientId");
            }
            if (clientSecret == null)
            {
                throw new ArgumentNullException("clientSecret");
            }
            if (clientSecret.Length == 0)
            {
                throw new ArgumentException("clientSecret is empty.", "clientSecret");
            }
            if (redirectUri == null)
            {
                throw new ArgumentNullException("redirectUri");
            }
            if (redirectUri.Length == 0)
            {
                throw new ArgumentException("redirectUri is empty.", "redirectUri");
            }

            this.ClientId = clientId;
            this.ClientSecret = clientSecret;
            this.RedirectUri = redirectUri;
            this.RequiredScopes = requiredScopes ?? DefaultScopes;
        }

        public LiveAuthClient(string clientId, string clientSecret, string redirectUri) : this(clientId, clientSecret, redirectUri, null) { }
        #endregion

        #region action methods
        public LiveAuthUser GetUserInfo(Page page)
        {
            if (page == null)
            {
                throw new ArgumentNullException("page");
            }

            InitializeInternal(page);

            if (this.AccessToken != null)
            {
                var results = RequestUserInfo(this.AccessToken);
                if (results.User != null)
                {
                    return results.User;
                }
            }
            return null;
        }

        public static void SignOut()
        {
            var cookie = new HttpCookie(wlCookie);
            cookie[AuthConstants.Logout] = "1";

            HttpContext.Current.Response.Cookies.Remove(wlCookie);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        public static void ProcessCallback(string clientId, string clientSecret, string redirectUri)
        {
            var client = new LiveAuthClient(clientId, clientSecret, redirectUri);
            client.OnAuthenticateCompleted(HttpContext.Current.Request.Url);
        }
        #endregion

        #region property getters/setters
        public string LoginUrl
        {
            get
            {
                return BuildLoginUrl(this.ClientId, this.RedirectUri, this.RequiredScopes, false, null);
            }
        }
        #endregion

        #region private member functions
        private void InitializeInternal(Page page)
        {
            if (this.IsInitialized)
            {
                return;
            }

            this.IsInitialized = true;

            HttpContext context = HttpContext.Current;
            HttpCookie cookie = context.Request.Cookies[wlCookie];

            bool isLiveSessionCreated = cookie != null;
            if (isLiveSessionCreated)
            {
                string accessToken = cookie[AuthConstants.AccessToken];
                if (!string.IsNullOrEmpty(accessToken))
                {
                    DateTimeOffset expires = DateTimeOffset.FromFileTime(Int64.Parse(cookie[AuthConstants.ExpiresIn]));

                    //Check Expiration
                    if (expires > DateTimeOffset.UtcNow.Add(new TimeSpan(0, 0, 60)))
                    {
                        this.AccessToken = accessToken;
                        this.Scopes = ParseScopeString(cookie[AuthConstants.Scope]);
                        this.Expires = expires;
                        return;
                    }
                    isLiveSessionCreated = false;
                }

                if (cookie[AuthConstants.Logout] == "1")    //Run logout script
                {
                    page.ClientScript.RegisterClientScriptBlock(page.GetType(), "logout", "logoutWindowsLive();", true);
                    SetLiveAuthCookie();    //Create empty cookie
                    return;
                }
            }

            if (!OnAuthenticateCompleted(HttpContext.Current.Request.Url) && !isLiveSessionCreated)
            {
                //Silent authentication
                context.Response.Redirect(BuildLoginUrl(this.ClientId, this.RedirectUri, this.RequiredScopes, true, CreateSessionState));
            }
        }

        private bool OnAuthenticateCompleted(Uri responseUri)
        {
            var dictionary = ParseQueryString(responseUri.Query);

            string code;
            if (dictionary.TryGetValue("code", out code) && !string.IsNullOrEmpty(code))
            {
                var results = RequestAccessToken(code, this.ClientId, this.ClientSecret, this.RedirectUri);
                if (results.Error == null)
                {
                    this.AccessToken = results.AccessToken;
                    this.Expires = results.Expires;
                    this.Scopes = results.Scopes;

                    SetLiveAuthCookie();
                    return true;
                }
            }

            string state;
            if (dictionary.TryGetValue("state", out state) && state == CreateSessionState)
            {
                SetLiveAuthCookie();    //Create empty cookie
                return true;
            }
            return false;
        }

        private void SetLiveAuthCookie()
        {
            var cookie = new HttpCookie(wlCookie);
            if (this.AccessToken != null)
            {
                cookie[AuthConstants.AccessToken] = HttpUtility.UrlEncode(this.AccessToken);
                cookie[AuthConstants.Scope] = HttpUtility.UrlPathEncode(BuildScopeString(this.Scopes));
                cookie[AuthConstants.ExpiresIn] = this.Expires.ToFileTime().ToString();
            }

            HttpContext.Current.Response.Cookies.Remove(wlCookie);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        private static RequestAccessTokenResults RequestAccessToken(string code, string clientId, string clientSecret, string redirectUri)
        {
            Uri url = new Uri(ConsentEndpoint + "/token");
            string body = string.Format(AuthCodePostBodyTemplate, HttpUtility.UrlEncode(clientId), code, redirectUri, HttpUtility.UrlEncode(clientSecret));

            var request = WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";

            try
            {
                using (var writer = new StreamWriter(request.GetRequestStream()))
                {
                    writer.Write(body);
                }

                var response = request.GetResponse();
                if (response != null)
                {
                    Stream responseStream = response.GetResponseStream();
                    if (responseStream != null)
                    {
                        try
                        {
                            var serializer = new DataContractJsonSerializer(typeof(AuthToken));
                            var token = (AuthToken)serializer.ReadObject(responseStream);
                            if (token != null)
                            {
                                return new RequestAccessTokenResults(token.AccessToken, DateTimeOffset.UtcNow.AddSeconds((double)Int64.Parse(token.ExpiresIn)), ParseScopeString(token.Scope));
                            }
                        }
                        catch (FormatException ex)
                        {
                            return new RequestAccessTokenResults(ex);
                        }
                    }
                }
            }
            catch (WebException e)
            {
                var response = e.Response;
                if (response != null)
                {
                    Stream responseStream = response.GetResponseStream();
                    if (responseStream != null)
                    {
                        try
                        {
                            var serializer = new DataContractJsonSerializer(typeof(AuthError));
                            var error = (AuthError)serializer.ReadObject(response.GetResponseStream());
                            if (error != null)
                            {
                                return new RequestAccessTokenResults(new LiveAuthException(error.ErrorCode, error.ErrorDescription));
                            }
                        }
                        catch (FormatException ex)
                        {
                            return new RequestAccessTokenResults(ex);
                        }
                    }
                }
            }
            catch (IOException)
            {
                //Ignore exception
            }

            return new RequestAccessTokenResults(new LiveAuthException("client_error", "A connection to the server could not be established."));
        }

        private static RequestUserInfoResults RequestUserInfo(string accessToken)
        {
            Uri url = new Uri(string.Format(UserInfoUrlTemplate, accessToken));
            var request = WebRequest.Create(url);

            try
            {
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                if (response != null)
                {
                    Stream responseStream = response.GetResponseStream();
                    if (responseStream != null)
                    {
                        try
                        {
                            var serializer = new DataContractJsonSerializer(typeof(AuthUser));
                            var user = (AuthUser)serializer.ReadObject(responseStream);
                            if (user != null)
                            {
                                return new RequestUserInfoResults(new LiveAuthUser(user.ID, user.Name, user.FirstName, user.LastName, user.Gender, user.Locale));
                            }
                        }
                        catch (FormatException ex)
                        {
                            return new RequestUserInfoResults(ex);
                        }
                    }
                }
            }
            catch (WebException e)
            {
                var response = e.Response;
                if (response != null)
                {
                    Stream responseStream = response.GetResponseStream();
                    if (responseStream != null)
                    {
                        try
                        {
                            var serializer = new DataContractJsonSerializer(typeof(AuthError));
                            var error = (AuthError)serializer.ReadObject(response.GetResponseStream());
                            if (error != null)
                            {
                                return new RequestUserInfoResults(new LiveAuthException(error.ErrorCode, error.ErrorDescription));
                            }
                        }
                        catch (FormatException ex)
                        {
                            return new RequestUserInfoResults(ex);
                        }
                    }
                }
            }
            catch (IOException)
            {
                //Ignore exception
            }

            return new RequestUserInfoResults(new LiveAuthException("client_error", "A connection to the server could not be established."));
        }

        private static string BuildLoginUrl(string clientId, string redirectUri, IEnumerable<string> scopes, bool silent, string state)
        {
            return string.Format(AuthorizeUrlTemplate,
                ConsentEndpoint,
                HttpUtility.UrlEncode(clientId),
                HttpUtility.UrlEncode(redirectUri),
                HttpUtility.UrlEncode(BuildScopeString(scopes)),
                HttpUtility.UrlEncode(System.Globalization.CultureInfo.CurrentUICulture.ToString()),
                silent ? "none" : HttpUtility.UrlEncode(Display.ToString().ToLowerInvariant()),
                state);
        }

        private static string BuildScopeString(IEnumerable<string> scopes)
        {
            var sb = new System.Text.StringBuilder();
            if (scopes != null)
            {
                foreach (string str in scopes)
                {
                    sb.Append(str).Append(ScopeSeparators[0]);
                }
            }
            return sb.ToString().TrimEnd(ScopeSeparators);
        }

        private static IEnumerable<string> ParseScopeString(string scopesString)
        {
            return new List<string>(scopesString.Split(ScopeSeparators, StringSplitOptions.RemoveEmptyEntries));
        }

        private static IDictionary<string, string> ParseQueryString(string query)
        {
            var dictionary = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(query))
            {
                query = query.TrimStart(new char[] { '?', '#' });
                if (string.IsNullOrEmpty(query))
                {
                    return dictionary;
                }
                foreach (string str in query.Split(new char[] { '&' }))
                {
                    string[] strArray2 = str.Split(new char[] { '=' });
                    if (strArray2.Length == 2)
                    {
                        dictionary.Add(strArray2[0], strArray2[1]);
                    }
                }
            }
            return dictionary;
        }
        #endregion
    }
}