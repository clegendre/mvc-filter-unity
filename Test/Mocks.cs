using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Routing;
using Moq;
using System.Web.Mvc;
using System.Collections.Generic;

namespace CK.Web.Mvc.Tests
{
    /// <summary>
    /// Provides mock objects for all ASP.NET MVC specific context
    /// classes (e.g. HttpContextBase, HttpRequestBase, etc.)
    /// </summary>
    public static class MvcMockContexts
    {
        /// <summary>
        /// Provides common mock object for
        /// <see cref="System.Web.HttpContextBase"/> class.
        /// </summary>
        public static HttpContextBase MockHttpContext()
        {
            var context  = new Mock<HttpContextBase>();
            var request  = new Mock<HttpRequestBase>();
            var response = new Mock<HttpResponseBase>();
            var session  = new Mock<HttpSessionStateBase>();
            var server   = new Mock<HttpServerUtilityBase>();
            var items    = new Dictionary<object, object>();

            context.Setup( c => c.Request ).Returns( request.Object );
            context.Setup( c => c.Response ).Returns( response.Object );
            context.Setup( c => c.Session ).Returns( session.Object );
            context.Setup( c => c.Server ).Returns( server.Object );
            context.Setup( h => h.Items ).Returns( items );

            return context.Object;
        }

        /// <summary>
        /// Provides mock object for <see cref="System.Web.HttpContextBase"/> class
        /// with mocked URL connected properties (i.e. HttpRequestBase.QueryString,
        /// HttpRequestBase.PathInfo, HttpRequestBase.AppRelativeCurrentExecutionFilePath).
        /// </summary>
        /// <param name="url">URL to parse.</param>
        public static HttpContextBase MockHttpContext( string url )
        {
            var context = MockHttpContext();
            context.Request.SetupRequestUrl( url );
            return context;
        }

        /// <summary>
        /// Sets up common mock object for controller context.
        /// </summary>
        public static void SetMockControllerContext( this Controller controller )
        {
            var httpContext = MockHttpContext();
            controller.ControllerContext = new ControllerContext(
                new RequestContext( httpContext, new RouteData() ), controller );
        }

        /// <summary>
        /// Sets up mock object for controller context with mocked URL connected
        /// properties (i.e. HttpRequestBase.QueryString, HttpRequestBase.PathInfo,
        /// HttpRequestBase.AppRelativeCurrentExecutionFilePath).
        /// </summary>
        /// <param name="url">URL to parse.</param>
        public static void SetMockControllerContext( this Controller controller, string url )
        {
            var httpContext = MockHttpContext( url );
            controller.ControllerContext = new ControllerContext(
                new RequestContext( httpContext, new RouteData() ), controller );
        }

        /// <summary>
        /// Sets up mock behavior for HttpResponseBase.ApplyAppPathModifier method.
        /// </summary>
        /// <param name="modifierFunc">Modification function.</param>
        public static void SetApplyAppPathModifier( this HttpResponseBase response, Func<string, string> modifierFunc )
        {
            Mock.Get( response ).Setup( r => r.ApplyAppPathModifier( It.IsAny<string>() ) ).Returns( modifierFunc );
        }

        /// <summary>
        /// Sets up mock value for HttpRequestBase.HttpMethod property.
        /// </summary>
        public static void SetHttpMethodResult( this HttpRequestBase request, string httpMethod )
        {
            Mock.Get( request ).Setup( r => r.HttpMethod ).Returns( httpMethod );
        }

        /// <summary>
        /// Sets up particular value for HttpRequestBase.ApplicationPath property.
        /// </summary>
        /// <param name="path">Value to set up.</param>
        public static void SetApplicationPath( this HttpRequestBase request, string path )
        {
            Mock.Get( request ).Setup( r => r.ApplicationPath ).Returns( path );
        }

        public static void SetupCookies( this HttpRequestBase request, params string[] cookieNames )
        {
            Mock.Get( request ).Setup( r => r.Cookies ).Returns( () =>
            {
                var cookies = new HttpCookieCollection();
                if( cookieNames != null && cookieNames.Length > 0 )
                {
                    foreach( string name in cookieNames ) cookies.Add( new HttpCookie( name ) );
                }
                return cookies;
            } );
        }

        /// <summary>
        /// Sets up mocked objects for URL connected properties
        /// (i.e. HttpRequestBase.QueryString, HttpRequestBase.PathInfo,
        /// HttpRequestBase.AppRelativeCurrentExecutionFilePath)
        /// </summary>
        /// <param name="url">URL to parse.</param>
        public static void SetupRequestUrl( this HttpRequestBase request, string url )
        {
            // Validate url parameter.
            if( url == null )
            {
                throw new ArgumentNullException( "url", "You must specify url." );
            }
            if( !url.StartsWith( "~/" ) )
            {
                throw new ArgumentException( "Sorry, expect virtual url starting with '~/'.", "url" );
            }

            // Setup url connected properties.
            var mockRequest = Mock.Get( request );
            mockRequest.Setup( r => r.QueryString ).Returns( GetQueryStringParameters( url ) );
            mockRequest.Setup( r => r.AppRelativeCurrentExecutionFilePath ).Returns( GetUrlWithoutQueryString( url ) );
            mockRequest.Setup( r => r.PathInfo ).Returns( string.Empty );
        }

        /// <summary>
        /// Parse URL query string parameters.
        /// </summary>
        /// <param name="url">URL to parse.</param>
        private static NameValueCollection GetQueryStringParameters( string url )
        {
            // If URL doesn't contain query string - return null.
            if( !url.Contains( "?" ) )
            {
                return null;
            }

            // Split URL to query string and main path.
            string queryString = url.Split( "?".ToCharArray(), 2 )[1];

            // Retrieve key-value pairs from query string.
            string[] keyValues = queryString.Split( '&' );

            // Convert key-value pairs from text representation
            // to NameValueCollection object.
            var parameters = new NameValueCollection();
            foreach( var pair in keyValues )
            {
                string[] parts = pair.Split( "=".ToCharArray(), 2 );
                parameters.Add( parts[0], parts[1] );
            }

            return parameters;
        }

        /// <summary>
        /// Separate main URL path from query string.
        /// </summary>
        /// <param name="url">URL to parse.</param>
        /// <returns>Main URL path/</returns>
        private static string GetUrlWithoutQueryString( string url )
        {
            return url.Contains( "?" )
                ? url.Substring( 0, url.IndexOf( "?" ) )
                : url;
        }
    }
}