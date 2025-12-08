using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repository.Interface;
using Service.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using UserService.Helper;
using UserService.Service;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews().AddJsonOptions(option => { option.JsonSerializerOptions.PropertyNamingPolicy = null; });
builder.Services.AddCors();
builder.Services.AddService(builder.Configuration);
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddTransient<ITokenHelper, TokenHelper>();
var jwtAppSettingOptions = builder.Configuration.GetSection(nameof(JwtIssuerOptions));
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
builder.Services.AddTokenAuthentication(jwtAppSettingOptions);
builder.Services.Configure<JwtIssuerOptions>(options =>
{
    options.Issuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
    options.Audience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)];
    options.ValidFor = Convert.ToInt32(jwtAppSettingOptions[nameof(JwtIssuerOptions.ValidFor)]);
    options.ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.ValidIssuer)];
    options.ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.ValidAudience)];
    options.IssuerSigningKey = jwtAppSettingOptions[nameof(JwtIssuerOptions.IssuerSigningKey)];
    options.SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtAppSettingOptions[nameof(JwtIssuerOptions.IssuerSigningKey)])), SecurityAlgorithms.HmacSha256);
});
builder.Services.AddSingleton<ICurrentUserService, CurrentUserService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHealthChecks();
builder.Services.AddTransient<ISessionService, SessionService>();
builder.Services.Configure<APIUrl>(builder.Configuration.GetSection(nameof(APIUrl)));
builder.Services.Configure<AppleSarkarCred>(builder.Configuration.GetSection(nameof(AppleSarkarCred)));
builder.Services.AddTransient<IHttpClientService, HttpClientService>();
builder.Services.AddTransient<IBruteForceService, BruteForceService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("UserService", new OpenApiInfo { Title = "UserService API", Version = "v1" });
    c.SwaggerDoc("Administrator", new OpenApiInfo { Title = "Administrator API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference =new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id= "Bearer"
                }
            },
            new string[]{ }
        }
    });
});

var app = builder.Build();
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();

        ExceptionLogging.LogException(Convert.ToString(exceptionHandlerFeature));


        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("An error occurred.");
    });
});
if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/UserService/swagger.json", "API V 1.0");
    c.SwaggerEndpoint("/swagger/Administrator/swagger.json", "Admin API V 1.0");
});
app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/healthz");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try { await services.AddSeed(); }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB. {exceptionMessage}", ex.Message);
    }
}
app.Run();
