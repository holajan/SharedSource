using System;
using System.Net;
using System.Web;

namespace IMP.Shared
{
    #region enums
    /// <summary>
    /// Typ verze Internet Information Services (IIS) web serveru
    /// </summary>
    internal enum WebServerType
    {
        /// <summary>
        /// ASP.NET development server
        /// </summary>
        ASPNETDevelopmentServer,
        /// <summary>
        /// IIS 5.1 (Windwows XP) or IIS 6.0 (Windwows Server 2003)
        /// </summary>
        PreIIS7,
        /// <summary>
        /// IIS 7.0, IIS 7.5 or IIS 8.0 or IIS Express
        /// </summary>
        IIS7OrLater
    }
    #endregion

    /// <summary>
    /// Třída s pomocnými funkcemi pro web aplikace
    /// </summary>
    internal static class WebApplicationUtil
    {
        #region constants
        private const string appRelativeCharacterString = "~/";
        #endregion

        #region action methods
        /// <summary>
        /// Vrací app relativní URL
        /// </summary>
        /// <param name="relativeOrAbsoluteUrl">Relativní nebo absolutní URL</param>
        /// <returns>Relativní URL web aplikace</returns>
        public static string GetAppRelativeUrl(string relativeOrAbsoluteUrl)
        {
            if (relativeOrAbsoluteUrl == null)
            {
                throw new ArgumentNullException("relativeOrAbsoluteUrl");
            }

            if (relativeOrAbsoluteUrl.StartsWith(appRelativeCharacterString))
            {
                return relativeOrAbsoluteUrl;
            }

            string path = relativeOrAbsoluteUrl;

            Uri uri = new Uri(relativeOrAbsoluteUrl, UriKind.RelativeOrAbsolute);
            if (uri.IsAbsoluteUri)
            {
                path = uri.PathAndQuery;
            }

            return VirtualPathUtility.ToAppRelative(path);
        }

        /// <summary>
        /// Vrací absolutní URL pro relativní URL web aplikace
        /// </summary>
        /// <param name="relativeUrl">Relativní URL</param>
        /// <returns>Absolutní URL pro relativní URL</returns>
        public static string GetAbsoluteUrl(string relativeUrl)
        {
            if (relativeUrl == null)
            {
                throw new ArgumentNullException("relativeUrl");
            }
            if (!relativeUrl.StartsWith(appRelativeCharacterString))
            {
                throw new ArgumentException("Invalid relative url.", "relativeUrl");
            }

            string path = relativeUrl;
            string query = "";

            if (relativeUrl.Contains("?"))
            {
                int index = relativeUrl.IndexOf('?');
                path = relativeUrl.Substring(0, index);
                query = relativeUrl.Substring(index);
            }

            path = VirtualPathUtility.ToAbsolute(path);
            if (path.EndsWith("/", StringComparison.Ordinal))
            {
                path = path.Substring(0, path.Length - 1);
            }

            Uri baseUri = new Uri(HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority));
            return new Uri(baseUri, path).ToString() + query;
        }

        /// <summary>
        /// Vrací URL bez Query stringu
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns>URL bez Query stringu</returns>
        public static string GetUrlWithoutQuery(string url)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }

            int index = url.IndexOfAny(new[] { '?', '#' });
            if (index == -1)
            {
                return url;
            }

            return url.Substring(0, index);
        }

        /// <summary>
        /// Vrací na jaké verzi Internet Information Services (IIS) je web spuštěný
        /// </summary>
        /// <returns>Typ verze Internet Information Services (IIS) web serveru</returns>
        public static WebServerType GetCurrentServerType()
        {
            switch (HttpContext.Current.Request.ServerVariables["SERVER_SOFTWARE"])
            {
                case "":
                    return WebServerType.ASPNETDevelopmentServer;
                case null:
                case "Microsoft-IIS/5.0":
                case "Microsoft-IIS/5.1":
                case "Microsoft-IIS/6.0":
                    return WebServerType.PreIIS7;
            }

            return WebServerType.IIS7OrLater;
        }
        #endregion

        #region property getters/setters
        /// <summary>
        /// Jměno windows uživatele
        /// </summary>
        public static string WindowsUser
        {
            get { return HttpContext.Current.Request.LogonUserIdentity.Name; }
        }

        /// <summary>
        /// Jméno počítače clienta (pokud lze zjistit, jinak firewallu)
        /// </summary>
        public static string ClientMachineName
        {
            get
            {
                string hostName = null;
                try
                {
                    //UserHostAddress returns client IP address if it's available and if it's not the name of firewall or NAT box.
                    hostName = System.Net.Dns.GetHostEntry(HttpContext.Current.Request.UserHostAddress).HostName;
                }
                catch (System.Net.Sockets.SocketException)
                {
                    //Ignore exception
                }

                string machineName = null;
                if (!string.IsNullOrEmpty(hostName))
                {
                    machineName = hostName.Split(new char[] { '.' })[0];
                }
                return machineName;
            }
        }

        /// <summary>
        /// IP adresa webového serveru nebo jeho NetBIOS jméno
        /// </summary>
        public static string ServerAddressOrMachineName
        {
            get
            {
                //Zjistění IP adresy lokálního počítače
                string address = GetHostAddress();
                if (string.IsNullOrEmpty(address))
                {
                    try
                    {
                        address = Dns.GetHostName();
                    }
                    catch (System.Net.Sockets.SocketException)
                    {
                        //Ignore exception
                    }
                }

                if (string.IsNullOrEmpty(address))
                {
                    address = Environment.MachineName;
                }

                return address;
            }
        }

        /// <summary>     
        /// Vrací absolutní URL kořenu web aplikace
        /// </summary>     
        public static string ApplicationAbsoluteUrl
        {
            get
            {
                var builder = new UriBuilder(HttpContext.Current.Request.Url);
                builder.Fragment = string.Empty;
                builder.Query = string.Empty;
                builder.Path = HttpContext.Current.Request.ApplicationPath;

                string uri = builder.Uri.ToString();
                if (uri.EndsWith("/", StringComparison.Ordinal))
                {
                    uri = uri.Substring(0, uri.Length - 1);
                }

                return uri;
            }
        }

        /// <summary>
        /// Vrací RawUrl aktuálního requestu bez query stringu
        /// </summary>
        public static string RawUrlPath
        {
            get
            {
                return GetUrlWithoutQuery(HttpContext.Current.Request.RawUrl);
            }
        }
        #endregion

        #region private member functions
        private static string GetHostAddress(string host = "")
        {
            try
            {
                var addressList = Dns.GetHostAddresses(host);

                //IPv4
                foreach (var address in Dns.GetHostEntry(host).AddressList)
                {
                    if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&   //IPv4
                        !IPAddress.IsLoopback(address) &&  //Ignore loopback addresses
                        !address.ToString().StartsWith("169.254."))  //Ignore link-local addresses
                    {
                        return address.ToString();
                    }
                }

                //IPv6
                foreach (var address in Dns.GetHostEntry(host).AddressList)
                {
                    if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 &&   //IPv6
                        !IPAddress.IsLoopback(address) &&  //Ignore loopback addresses
                        !address.IsIPv6LinkLocal)  //Ignore link-local IPv6 addresses (FE80:)
                    {
                        return "[" + address.ToString() + "]";
                    }
                }

                //IPv4 link-local
                foreach (var address in Dns.GetHostEntry(host).AddressList)
                {
                    if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&   //IPv4
                        address.ToString().StartsWith("169.254."))  //link-local addresses
                    {
                        return address.ToString();
                    }
                }

                //IPv6 link-local
                foreach (var address in Dns.GetHostEntry(host).AddressList)
                {
                    if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 &&   //IPv6
                        address.IsIPv6LinkLocal)  //link-local IPv6 addresses (FE80:)
                    {
                        return "[" + address.ToString() + "]";
                    }
                }
            }
            catch (System.Net.Sockets.SocketException)
            {
                //Ignore exception                  
            }

            return null;
        }
        #endregion
    }
}