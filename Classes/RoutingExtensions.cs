using System.Web;
using System.Web.Routing;

namespace System.Web.Routing
{
    /// <summary>
    /// Adds MapHttpHandler method to RouteCollection object
    /// </summary>
    /// <remarks>
    /// The following code is by Phil Haack and was taken from
    /// http://haacked.com/archive/2009/11/04/routehandler-for-http-handlers.aspx
    /// </remarks>
    public static class RoutingExtensions
    {
        #region member types
        private class HttpHandlerRouteHandler<THandler> : IRouteHandler where THandler : IHttpHandler, new()
        {
            public IHttpHandler GetHttpHandler(RequestContext requestContext)
            {
                return new THandler();
            }
        }
        #endregion

        #region action methods
        public static void MapHttpHandler<THandler>(this RouteCollection routes, string url) where THandler : IHttpHandler, new()
        {
            routes.MapHttpHandler<THandler>(null, url, null, null, null);
        }

        public static void MapHttpHandler<THandler>(this RouteCollection routes, string url, RouteValueDictionary defaults) where THandler : IHttpHandler, new()
        {
            routes.MapHttpHandler<THandler>(null, url, defaults, null, null);
        }

        public static void MapHttpHandler<THandler>(this RouteCollection routes, string url, RouteValueDictionary defaults, RouteValueDictionary constraints) where THandler : IHttpHandler, new()
        {
            routes.MapHttpHandler<THandler>(null, url, defaults, constraints, null);
        }

        public static void MapHttpHandler<THandler>(this RouteCollection routes, string url, RouteValueDictionary defaults, RouteValueDictionary constraints, RouteValueDictionary dataTokens) where THandler : IHttpHandler, new()
        {
            routes.MapHttpHandler<THandler>(null, url, defaults, constraints, dataTokens);
        }

        public static void MapHttpHandler<THandler>(this RouteCollection routes, string name, string url) where THandler : IHttpHandler, new()
        {
            routes.MapHttpHandler<THandler>(name, url, null, null, null);
        }

        public static void MapHttpHandler<THandler>(this RouteCollection routes, string name, string url, RouteValueDictionary defaults) where THandler : IHttpHandler, new()
        {
            routes.MapHttpHandler<THandler>(name, url, defaults, null, null);
        }

        public static void MapHttpHandler<THandler>(this RouteCollection routes, string name, string url, RouteValueDictionary defaults, RouteValueDictionary constraints) where THandler : IHttpHandler, new()
        {
            routes.MapHttpHandler<THandler>(name, url, defaults, constraints, null);
        }

        public static void MapHttpHandler<THandler>(this RouteCollection routes, string name, string url, RouteValueDictionary defaults, RouteValueDictionary constraints, RouteValueDictionary dataTokens) where THandler : IHttpHandler, new()
        {
            var route = new Route(url, new HttpHandlerRouteHandler<THandler>());
            route.Defaults = defaults;
            route.Constraints = constraints;
            route.DataTokens = dataTokens;
            routes.Add(name, route);
        }
        #endregion
    }
}
