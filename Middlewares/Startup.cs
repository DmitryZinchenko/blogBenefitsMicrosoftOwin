#region [ listing #1 ]
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Owin;

namespace Middlewares
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Func<AppFunc, AppFunc> middleware = (AppFunc next) =>
            {
                IEnumerable<string> apiKeys = new[] { "123", "987" };

                AppFunc inner = (IDictionary<string, object> env) =>
                {
                    Console.WriteLine(Environment.NewLine);

                    var method = (string)env["owin.RequestMethod"];
                    var path = (string)env["owin.RequestPath"];
                    var queryString = (string)env["owin.RequestQueryString"];

                    string apiKey = string.Empty;

                    foreach (var segment in queryString.Split('&'))
                    {
                        var keyValuePair = segment.Split('=');
                        var key = keyValuePair[0];

                        if (key.Equals("apikey", StringComparison.InvariantCultureIgnoreCase))
                        {
                            apiKey = keyValuePair[1];
                            break;
                        }
                    }

                    var requestHeaders = (IDictionary<string, string[]>)env["owin.RequestHeaders"];

                    string contentType = string.Empty;
                    if (requestHeaders.ContainsKey("Content-Type"))
                    {
                        contentType = requestHeaders["Content-Type"][0];
                    }

                    string contentLength = string.Empty;
                    if (requestHeaders.ContainsKey("Content-Length"))
                    {
                        contentLength = requestHeaders["Content-Length"][0];
                    }

                    string accept = string.Empty;
                    if (requestHeaders.ContainsKey("Accept"))
                    {
                        accept = requestHeaders["Accept"][0];
                    }

                    string requestBody;
                    using (var requestContentReader = new StreamReader((Stream)env["owin.RequestBody"]))
                    {
                        requestBody = requestContentReader.ReadToEnd();
                    }

                    Console.WriteLine("Method: {0}", method);
                    Console.WriteLine("Path: {0}", path);
                    Console.WriteLine("Query String: {0}", queryString);
                    Console.WriteLine("API Key: {0}", apiKey);
                    Console.WriteLine("Content-Type: {0}", contentType);
                    Console.WriteLine("Content-Length: {0}", contentLength);
                    Console.WriteLine("Accept: {0}", accept);
                    Console.WriteLine("Request Body: {0}", requestBody);

                    // create HTTP response

                    if (apiKeys.Contains(apiKey))
                    {
                        var responseHeaders = (IDictionary<string, string[]>)env["owin.ResponseHeaders"];

                        if (method.Equals("GET", StringComparison.InvariantCultureIgnoreCase))
                        {
                            // pretend that we retrieved the object based on path
                            var responseBody = "{\"id\":\"10\", \"make\":\"Mercedes\", \"model\":\"C63 AMG\", \"engine\":\"V6\"}";

                            responseHeaders["Content-Type"] = new[] { "application/json" };
                            responseHeaders["Content-Length"] = new[] { responseBody.Length.ToString() };
                            env["owin.ResponseStatusCode"] = 200; // OK

                            using (var responseBodyWriter = new StreamWriter((Stream)env["owin.ResponseBody"]))
                            {
                                responseBodyWriter.Write(responseBody);
                            }
                        }
                        else if (method.Equals("POST", StringComparison.InvariantCultureIgnoreCase))
                        {
                            // pretend that we persisted the object and returning a new one with an identity value property
                            var responseBody = "{\"id\":\"17\", \"make\":\"BMW\", \"model\":\"M3\", \"engine\":\"V6\"}";

                            responseHeaders["Content-Type"] = new[] { "application/json" };
                            responseHeaders["Content-Length"] = new[] { responseBody.Length.ToString() };
                            responseHeaders["Location"] = new[] { "http://localhost:5000/api/cars/17" };
                            env["owin.ResponseStatusCode"] = 201; // Created

                            using (var responseBodyWriter = new StreamWriter((Stream)env["owin.ResponseBody"]))
                            {
                                responseBodyWriter.Write(responseBody);
                            }
                        }
                        else
                        {
                            env["owin.ResponseStatusCode"] = 405; // Method Not Allowed
                            responseHeaders.Add(new KeyValuePair<string, string[]>("Allow", new[] { "GET", "POST" }));
                        }
                    }
                    else
                    {
                        env["owin.ResponseStatusCode"] = 403; // Forbidden
                    }

                    return next.Invoke(env);
                };

                return inner;
            };

            app.Use(middleware);
        }
    }
}
#endregion

#region [ listing #2 ]
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
//using Owin;
//using Microsoft.Owin;

//[assembly: OwinStartup(typeof(Middlewares.Startup))]

//namespace Middlewares
//{
//    using AppFunc = Func<IDictionary<string, object>, Task>;

//    public class Startup
//    {
//        public void Configuration(IAppBuilder app)
//        {
//            Func<AppFunc, AppFunc> middleware = (AppFunc next) =>
//            {
//                IEnumerable<string> apiKeys = new[] { "123", "987" };

//                AppFunc inner = async (IDictionary<string, object> env) =>
//                {
//                    Console.WriteLine(Environment.NewLine);

//                    var context = new OwinContext(env);
//                    var request = context.Request;
//                    var response = context.Response;

//                    var method = request.Method;
//                    var path = request.Path.HasValue ? request.Path.Value : string.Empty;
//                    var queryString = request.QueryString.HasValue ? request.QueryString.Value : string.Empty;
//                    var apiKey = request.Query["apiKey"] ?? string.Empty;

//                    var contentType = request.ContentType;
//                    var contentLength = request.Headers["Content-Length"];
//                    var accept = request.Accept;
//                    string requestBody;

//                    using (var requestContentReader = new StreamReader(request.Body))
//                    {
//                        requestBody = await requestContentReader.ReadToEndAsync();
//                    }

//                    Console.WriteLine("Method: {0}", method);
//                    Console.WriteLine("Path: {0}", path);
//                    Console.WriteLine("Query String: {0}", queryString);
//                    Console.WriteLine("API Key: {0}", apiKey);
//                    Console.WriteLine("Content-Type: {0}", contentType);
//                    Console.WriteLine("Content-Length: {0}", contentLength);
//                    Console.WriteLine("Accept: {0}", accept);
//                    Console.WriteLine("Request Body: {0}", requestBody);

//                    // create HTTP response

//                    if (apiKeys.Contains(apiKey))
//                    {
//                        if (method.Equals("GET", StringComparison.InvariantCultureIgnoreCase))
//                        {
//                            // pretend that we retrieved the object based on path
//                            var responseBody = "{\"id\":\"10\", \"make\":\"Mercedes\", \"model\":\"C63 AMG\", \"engine\":\"V6\"}";

//                            response.ContentType = "application/json";
//                            response.ContentLength = responseBody.Length;
//                            response.StatusCode = 200; // OK

//                            await response.WriteAsync(responseBody);
//                        }
//                        else if (method.Equals("POST", StringComparison.InvariantCultureIgnoreCase))
//                        {
//                            // pretend that we persisted the object and returning a new one with an identity value property
//                            var responseBody = "{\"id\":\"17\", \"make\":\"BMW\", \"model\":\"M3\", \"engine\":\"V6\"}";

//                            response.ContentType = "application/json";
//                            response.ContentLength = responseBody.Length;
//                            response.Headers["Location"] = "http://localhost:5000/api/cars/17";
//                            response.StatusCode = 201; // Created

//                            await response.WriteAsync(responseBody);
//                        }
//                        else
//                        {
//                            response.StatusCode = 405; // Method Not Allowed
//                            response.Headers["Allow"] = "GET,POST";
//                        }
//                    }
//                    else
//                    {
//                        response.StatusCode = 403; // Forbidden
//                    }

//                    await next.Invoke(env);
//                };

//                return inner;
//            };

//            app.Use(middleware);
//        }
//    }
//}
#endregion

#region [ listing #3 ]
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
//using Owin;
//using Microsoft.Owin;

//[assembly: OwinStartup(typeof(Middlewares.Startup))]

//namespace Middlewares
//{
//    public class Startup
//    {
//        public void Configuration(IAppBuilder app)
//        {
//            app.Use(typeof(Middleware));
//        }
//    }

//    public class Middleware : OwinMiddleware
//    {
//        private IEnumerable<string> _apiKeys;

//        public Middleware(OwinMiddleware next)
//            : base(next)
//        {
//            _apiKeys = new[] { "123", "987" };
//        }

//        public override async Task Invoke(IOwinContext context)
//        {
//            Console.WriteLine(Environment.NewLine);

//            var request = context.Request;
//            var response = context.Response;

//            var method = request.Method;
//            var path = request.Path.HasValue ? request.Path.Value : string.Empty;
//            var queryString = request.QueryString.HasValue ? request.QueryString.Value : string.Empty;
//            var apiKey = request.Query["apiKey"] ?? string.Empty;

//            var contentType = request.ContentType;
//            var contentLength = request.Headers["Content-Length"];
//            var accept = request.Accept;
//            string requestBody;

//            using (var requestContentReader = new StreamReader(request.Body))
//            {
//                requestBody = await requestContentReader.ReadToEndAsync();
//            }

//            Console.WriteLine("Method: {0}", method);
//            Console.WriteLine("Path: {0}", path);
//            Console.WriteLine("Query String: {0}", queryString);
//            Console.WriteLine("API Key: {0}", apiKey);
//            Console.WriteLine("Content-Type: {0}", contentType);
//            Console.WriteLine("Content-Length: {0}", contentLength);
//            Console.WriteLine("Accept: {0}", accept);
//            Console.WriteLine("Request Body: {0}", requestBody);

//            // create HTTP response

//            if (_apiKeys.Contains(apiKey))
//            {
//                if (method.Equals("GET", StringComparison.InvariantCultureIgnoreCase))
//                {
//                    // pretend that we retrieved the object based on path
//                    var responseBody = "{\"id\":\"10\", \"make\":\"Mercedes\", \"model\":\"C63 AMG\", \"engine\":\"V6\"}";

//                    response.ContentType = "application/json";
//                    response.ContentLength = responseBody.Length;
//                    response.StatusCode = 200; // OK

//                    await response.WriteAsync(responseBody);
//                }
//                else if (method.Equals("POST", StringComparison.InvariantCultureIgnoreCase))
//                {
//                    // pretend that we persisted the object and returning a new one with an identity value property
//                    var responseBody = "{\"id\":\"17\", \"make\":\"BMW\", \"model\":\"M3\", \"engine\":\"V6\"}";

//                    response.ContentType = "application/json";
//                    response.ContentLength = responseBody.Length;
//                    response.Headers["Location"] = "http://localhost:5000/api/cars/17";
//                    response.StatusCode = 201; // Created

//                    await response.WriteAsync(responseBody);
//                }
//                else
//                {
//                    response.StatusCode = 405; // Method Not Allowed
//                    response.Headers["Allow"] = "GET,POST";
//                }
//            }
//            else
//            {
//                response.StatusCode = 403; // Forbidden
//            }

//            await Next.Invoke(context);
//        }
//    }
//}
#endregion