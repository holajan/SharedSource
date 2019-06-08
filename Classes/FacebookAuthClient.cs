using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web;
using Facebook;

namespace FacebookAuth
{
    [DebuggerDisplay("UserID = {UserID}, UserName = {UserName}, FullName = {FullName}, Gender = {Gender}, Locale = {Locale}, Email = {Email}")]
    public sealed class FacebookAuthUser
    {
        #region member types declaration
        private static class AuthUserConstants
        {
            #region constants
            public const string ID = "id";
            public const string FullName = "name";
            public const string UserName = "username";
            public const string FirstName = "first_name";
            public const string LastName = "last_name";
            public const string Picture = "picture";
            public const string Link = "link";
            public const string Gender = "gender";
            public const string Locale = "locale";
            public const string Email = "email";
            #endregion
        }
        #endregion

        #region member varible and default property initialization
        public string UserID { get; private set; }
        public string UserName { get; private set; }
        public string FullName { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string PictureUrl { get; private set; }
        public string Link { get; private set; }
        public string Gender { get; private set; }
        public string Locale { get; private set; }
        public string Email { get; private set; }
        #endregion

        #region constructors and destructors
        internal FacebookAuthUser(IDictionary<string, object> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            this.UserID = (string)data[AuthUserConstants.ID];
            this.UserName = (string)data[AuthUserConstants.UserName];
            this.FullName = (string)data[AuthUserConstants.FullName];
            this.FirstName = (string)data[AuthUserConstants.FirstName];
            this.LastName = (string)data[AuthUserConstants.LastName];
            this.PictureUrl = (string)data[AuthUserConstants.Picture];
            this.Link = (string)data[AuthUserConstants.Link];
            this.Gender = (string)data[AuthUserConstants.Gender];
            this.Locale = (string)data[AuthUserConstants.Locale];

            if (data.ContainsKey(AuthUserConstants.Email))
            {
                this.Email = (string)data[AuthUserConstants.Email];
            }
        }
        #endregion

        #region property getters/setters
        internal static string[] Fields
        {
            get { return new[] { AuthUserConstants.ID, AuthUserConstants.UserName, AuthUserConstants.FullName, AuthUserConstants.FirstName, AuthUserConstants.LastName, AuthUserConstants.Picture, AuthUserConstants.Link, AuthUserConstants.Gender, AuthUserConstants.Locale, AuthUserConstants.Email }; }
        }
        #endregion
    }

    public class FacebookAuthClient
    {
        #region member types declaration
        private enum DisplayType
        {
            Page,
            Popup,
            Touch,
        }

        private static class AuthCookieConstants
        {
            #region constants
            public const string AccessToken = "access_token";
            public const string ExpiresIn = "expires_in";
            #endregion
        }
        #endregion

        #region constants
        private const string fbCookie = "fb_auth";
        private const string RequestAccessTokenUrl = "https://graph.facebook.com/oauth/access_token";

        /// <summary>
        /// List of additional display modes can be found at http://developers.facebook.com/docs/reference/dialogs/#display
        /// </summary>
        private const DisplayType Display = DisplayType.Page;

        /// <summary>
        /// List of default required application permissions like: email, user_about_me, user_birthday, status_update, publish_stream
        /// You can read more about this at available Facebook permissions at http://developers.facebook.com/docs/authentication/permissions
        /// </summary>
        private static readonly string[] DefaultScopes = new[] { "email" };
        #endregion

        #region member varible and default property initialization
        private readonly string AppId;
        private readonly string AppSecret;
        private readonly string RedirectUri;
        private IEnumerable<string> RequiredScopes;

        private bool IsInitialized;

        public string AccessToken { get; private set; }
        public DateTimeOffset Expires { get; private set; }
        #endregion

        #region constructors and destructors
        public FacebookAuthClient(string appId, string appSecret, string redirectUri, IEnumerable<string> requiredScopes)
        {
            if (appId == null)
            {
                throw new ArgumentNullException("appId");
            }
            if (appId.Length == 0)
            {
                throw new ArgumentException("appId is empty.", "appId");
            }
            if (appSecret == null)
            {
                throw new ArgumentNullException("appSecret");
            }
            if (appSecret.Length == 0)
            {
                throw new ArgumentException("appSecret is empty.", "appSecret");
            }
            if (redirectUri == null)
            {
                throw new ArgumentNullException("redirectUri");
            }
            if (redirectUri.Length == 0)
            {
                throw new ArgumentException("redirectUri is empty.", "redirectUri");
            }

            this.AppId = appId;
            this.AppSecret = appSecret;
            this.RedirectUri = redirectUri;
            this.RequiredScopes = requiredScopes ?? DefaultScopes;

            InitializeInternal();
        }

        public FacebookAuthClient(string appID, string appSecret, string redirectUri) : this(appID, appSecret, redirectUri, null) { }
        #endregion

        #region action methods
        public FacebookAuthUser GetUserInfo()
        {
            if (this.AccessToken != null)
            {
                return RequestUserInfo(this.AccessToken);
            }

            return null;
        }

        public static void SignOut(string redirectUrl, bool signOutFromFacebook)
        {
            HttpContext context = HttpContext.Current;
            HttpCookie cookie = context.Request.Cookies[fbCookie];

            if (cookie == null)
            {
                return;
            }

            string accessToken = cookie[AuthCookieConstants.AccessToken];
            if (string.IsNullOrEmpty(accessToken))
            {
                return;
            }

            HttpContext.Current.Response.Cookies.Remove(fbCookie);
            HttpContext.Current.Response.Cookies.Add(new HttpCookie(fbCookie));

            if (signOutFromFacebook)
            {
                var fb = new FacebookClient();

                var parameters = new Dictionary<string, object>();
                parameters["access_token"] = accessToken;
                parameters["next"] = redirectUrl;

                var logouUrl = fb.GetLogoutUrl(parameters);
                HttpContext.Current.Response.Redirect(logouUrl.AbsoluteUri);
            }
            else
            {
                HttpContext.Current.Response.Redirect(redirectUrl);
            }
        }

        public static void ProcessCallback(string appId, string appSecret, string redirectUri)
        {
            var client = new FacebookAuthClient(appId, appSecret, redirectUri);
            client.OnAuthenticateCompleted(HttpContext.Current.Request.Url);
        }
        #endregion

        #region property getters/setters
        public string LoginUrl
        {
            get
            {
                return BuildLoginUrl(this.AppId, this.RedirectUri, this.RequiredScopes, null);
            }
        }
        #endregion

        #region private member functions
        private void InitializeInternal()
        {
            if (this.IsInitialized)
            {
                return;
            }
            this.IsInitialized = true;

            HttpContext context = HttpContext.Current;
            HttpCookie cookie = context.Request.Cookies[fbCookie];

            if (cookie != null)
            {
                string accessToken = cookie[AuthCookieConstants.AccessToken];
                if (!string.IsNullOrEmpty(accessToken))
                {
                    DateTimeOffset expires = DateTimeOffset.FromFileTime(Int64.Parse(cookie[AuthCookieConstants.ExpiresIn]));

                    //Check Expiration
                    if (expires > DateTimeOffset.UtcNow.Add(new TimeSpan(0, 0, 60)))
                    {
                        this.AccessToken = accessToken;
                        this.Expires = expires;
                    }
                }
            }
        }

        private bool OnAuthenticateCompleted(Uri responseUri)
        {
            var fb = new FacebookClient();
            FacebookOAuthResult oauthResult;
            if (fb.TryParseOAuthCallbackUrl(responseUri, out oauthResult))
            {
                if (oauthResult.IsSuccess)
                {
                    //Exchange the code for a user access token
                    var results = GetAccessTokenForCode(oauthResult.Code, this.AppId, this.AppSecret, this.RedirectUri);
                    this.AccessToken = results.AccessToken;
                    this.Expires = results.Expires;

                    SetFacebookAuthCookie();
                    return true;
                }
            }

            return false;
        }

        private void SetFacebookAuthCookie()
        {
            var cookie = new HttpCookie(fbCookie);
            if (this.AccessToken != null)
            {
                cookie[AuthCookieConstants.AccessToken] = HttpUtility.UrlEncode(this.AccessToken);
                cookie[AuthCookieConstants.ExpiresIn] = this.Expires.ToFileTime().ToString();
            }

            HttpContext.Current.Response.Cookies.Remove(fbCookie);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }

        private static FacebookOAuthResult GetAccessTokenForCode(string code, string appId, string appSecret, string redirectUri)
        {
            var fb = new FacebookClient();

            var parameters = new Dictionary<string, object>();
            parameters["client_id"] = appId;
            parameters["client_secret"] = appSecret;
            parameters["code"] = code;
            parameters["redirect_uri"] = redirectUri;

            var data = (IDictionary<string, object>)fb.Post(RequestAccessTokenUrl, parameters);

            //Parse results to FacebookOAuthResult object
            string responseData = string.Format("access_token={0}&expires_in={1}", data["access_token"], data["expires"]);
            FacebookOAuthResult oauthResult = fb.ParseOAuthCallbackUrl(new Uri(redirectUri + "#" + responseData, UriKind.Absolute));

            return oauthResult;
        }

        private static FacebookAuthUser RequestUserInfo(string accessToken)
        {
            var fb = new FacebookClient(accessToken);
            var result = (IDictionary<string, object>)fb.Get("me", new { fields = string.Join(",", FacebookAuthUser.Fields) });

            return new FacebookAuthUser(result);
        }

        private static string BuildLoginUrl(string appId, string redirectUri, IEnumerable<string> scopes, object state)
        {
            var parameters = new Dictionary<string, object>();
            parameters["client_id"] = appId;
            parameters["response_type"] = "code";   //Must be code, because token is not returned in query string
            parameters["display"] = Display.ToString().ToLowerInvariant();
            parameters["redirect_uri"] = redirectUri;
            if (state != null)
            {
                parameters["state"] = state;
            }

            //Add the scope parameter only if we have some scopes. 
            if (scopes != null)
            {
                string scope = string.Join(",", scopes);
                if (!string.IsNullOrEmpty(scope))
                {
                    parameters["scope"] = scope;
                }
            }

            var fb = new FacebookClient();
            return fb.GetLoginUrl(parameters).AbsoluteUri;
        }
        #endregion
    }
}