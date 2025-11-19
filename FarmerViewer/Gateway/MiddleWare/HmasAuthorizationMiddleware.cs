
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using System.Configuration;
using System.Data;
using System.Security.Claims;
using YarpGateWay.Models;

namespace YarpGateWay.MiddleWare
{
    public class HmasAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration configuration;
        private readonly HttpClient _httpClient;

        public HmasAuthorizationMiddleware(RequestDelegate next, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _next = next;
            _httpContextAccessor = httpContextAccessor;
            this.configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task InvokeAsync(HttpContext context, IHttpContextAccessor _httpContextAccessor, IConfiguration configuration)
        {

            var requireAuthorizationRoutes = configuration.GetSection("ReverseProxy:Routes")
           .GetChildren()
           .Where(route => route.GetValue<string>("Metadata:RequireAuthorization") == "true")
           .Select(route => route.GetValue<string>("Match:Path"))
           .Select(path => path.Split("/")[1])  // Split the path by '/' and take the first segment
           .ToList();
            var requestPath = context.Request.Path.Value?.Split('/')[1]; 
            if (requireAuthorizationRoutes.Contains(requestPath))
            {
                var userId = context.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).FirstOrDefault();
                var SessionId = context.User.Claims.Where(c => c.Type == ClaimTypes.SerialNumber).Select(c => c.Value).FirstOrDefault();
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(SessionId))
                {

                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Invalid Token.");
                    return;
                }
                else
                {
                    SessionVerifyModel? result = new SessionVerifyModel();
                    try
                    {                        
                        var userServiceCluster = configuration.GetSection("ReverseProxy:Clusters:UserServiceCluster:Destinations:UserService");

                        var userServiceAddress = userServiceCluster.GetValue<string>("Address");

                        var url = $"{userServiceAddress}nm_user-service/api/Account/check-user-session?userId={userId}&sessionId={SessionId}";

                        // Send a GET request to the external API
                        var response = await _httpClient.GetAsync(url);

                        // Check if the response is successful
                        if (response.IsSuccessStatusCode)
                        {                            
                           var data = await response.Content.ReadAsStringAsync();
                        }
                        else
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            await context.Response.WriteAsync("Invalid Token.");
                            return;                           
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Error while checking Session");
                    }                 
                }               
            }
            await _next(context);
        }
    }
}