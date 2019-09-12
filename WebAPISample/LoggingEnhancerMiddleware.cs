using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WebAPISample;

namespace WebAPISample
{
    public sealed class LoggingEnhancerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggerFactory _loggerFactory;
        private readonly string _environmentName;
        private static readonly string _version = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
        private static readonly string _coreClrVersion = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
        public LoggingEnhancerMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IWebHostEnvironment hostingEnvironment)
        {
            _next = next;
            _loggerFactory = loggerFactory;
            _environmentName = hostingEnvironment.EnvironmentName;
        }

        public async Task Invoke(HttpContext context)
        {
            var userAgent = context.Request.Headers.FirstOrDefault(x => x.Key == "User-Agent").Value;
            var logger = _loggerFactory.CreateLogger("LoggerEnhancer");
            var loggingScope = new List<KeyValuePair<string, object>>();
            loggingScope.Add(new KeyValuePair<string, object>("Host", context.Request.Host));
            loggingScope.Add(new KeyValuePair<string, object>("RemoteIP", context.Features.Get<IHttpConnectionFeature>().RemoteIpAddress.ToString()));
            loggingScope.Add(new KeyValuePair<string, object>("UserAgent", userAgent.ToString()));          
            loggingScope.Add(new KeyValuePair<string, object>("AppVersion", _version));
            loggingScope.Add(new KeyValuePair<string, object>("EnvironmentName", _environmentName));
            loggingScope.Add(new KeyValuePair<string, object>("CoreClrVersion", _coreClrVersion));

          
            using (var scope = logger.BeginScope(loggingScope))
            {
                await _next(context);
            }

        }
    }
}
namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Adds several desirable properties to MVC logging (host, application version, clientrequestid, correlationid, user agent, userID/Name (if configured), remoteendpoint
    /// </summary>
    public static class LoggingEnhancerMiddlewareExtensions
    {
        public static IApplicationBuilder UseLoggingEnhancer(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoggingEnhancerMiddleware>();
        }
    }
}
