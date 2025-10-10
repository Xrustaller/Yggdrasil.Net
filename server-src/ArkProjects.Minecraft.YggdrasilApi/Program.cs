using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using ArkProjects.Minecraft.AspShared.EntityFramework;
using ArkProjects.Minecraft.AspShared.Logging;
using ArkProjects.Minecraft.AspShared.Validation;
using ArkProjects.Minecraft.Database;
using ArkProjects.Minecraft.YggdrasilApi.Misc;
using ArkProjects.Minecraft.YggdrasilApi.Misc.JsonConverters;
using ArkProjects.Minecraft.YggdrasilApi.Models;
using ArkProjects.Minecraft.YggdrasilApi.Options;
using ArkProjects.Minecraft.YggdrasilApi.Services;
using ArkProjects.Minecraft.YggdrasilApi.Services.Server;
using ArkProjects.Minecraft.YggdrasilApi.Services.Service;
using ArkProjects.Minecraft.YggdrasilApi.Services.User;
using ArkProjects.Minecraft.YggdrasilApi.Services.UserPassword;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Npgsql;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"Application: {builder.Environment.ApplicationName}");
Console.WriteLine($"ContentRoot: {builder.Environment.ContentRootPath}");

//logging
builder.Services.ConfigureRbSerilog(builder.Configuration.GetSection("Serilog"));
builder.Host.AddRbSerilog();

builder.Services
    .AddSingleton<IUserPasswordService, UserPasswordService>()
    .AddScoped<IYgServerService, YgServerService>()
    .AddScoped<IYgUserService, YgUserService>()
    .AddScoped<IServiceServerService, ServiceServerService>()
    .AddScoped<IServiceUsersService, ServiceUsersService>();

//security
WebSecurityOptions securityOptions = builder.Configuration.GetSection("WebSecurity").Get<WebSecurityOptions>()!;
builder.Services.Configure<WebSecurityOptions>(builder.Configuration.GetSection("WebSecurity"));

NpgsqlConnectionStringBuilder csb = new()
{
    Host = builder.Configuration["PostgresSQL:Host"],
    Port = builder.Configuration.GetSection("PostgresSQL:Port").Get<int>(),
    Username = builder.Configuration["PostgresSQL:Username"],
    Password = builder.Configuration["PostgresSQL:Password"],
    Database = builder.Configuration["PostgresSQL:Database"],
    SslMode = SslMode.Prefer
};
NpgsqlDataSourceBuilder dbSourceBuilder = new(csb.ConnectionString);
NpgsqlDataSource dbSource = dbSourceBuilder.Build();
builder.Services
    .AddSingleton<IDbSeeder<McDbContext>, McDbContextSeeder>()
    .AddRbDbMigrator<McDbContext>()
    .AddDbContext<McDbContext>(x =>
        x.UseNpgsql(dbSource, y => y
            .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
        )
    );

//controllers
builder.Services
    .Configure<ApiBehaviorOptions>(opts => { opts.SuppressConsumesConstraintForFormFileParameters = true; })
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.Converters.Add(new YggdrasilGuidConverter());
    });

//swagger
SwaggerOptions? swaggerOptions = builder.Configuration.GetSection("Swagger").Get<SwaggerOptions>();
builder.Services
    .Configure<SwaggerOptions>(builder.Configuration.GetSection("Swagger"))
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(o =>
    {
        string xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    });

//#########################################################################


WebApplication app = builder.Build();

//forwarded headers
if (securityOptions.EnableForwardedHeaders)
{
    ForwardedHeadersOptions forwardOptions = new()
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
        ForwardLimit = 10,
        RequireHeaderSymmetry = false
    };
    forwardOptions.KnownIPNetworks.Clear();
    forwardOptions.KnownProxies.Clear();
    app.UseForwardedHeaders(forwardOptions);
}

app.Use(async (context, next) =>
{
    try
    {
        await next(context);
    }
    catch (Exception e)
    {
        ErrorResponse err = e is YgServerException ygE
            ? ygE.Response
            : ErrorResponseFactory.Custom(
                StatusCodes.Status500InternalServerError,
                ErrorResponseFactory.ErrorInternalServerError,
                e.ToString());
        IJsonHelper jsonHelper = context.RequestServices.GetRequiredService<IJsonHelper>();
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = err.StatusCode;
        Stream respStream = context.Response.BodyWriter.AsStream();
        await using StreamWriter textWriter = new(respStream);
        jsonHelper.Serialize(err).WriteTo(textWriter, HtmlEncoder.Default);
    }
});

//logging
app.UseRbSerilogRequestLogging();

// https redirection
if (securityOptions.EnableHttpsRedirections) app.UseHttpsRedirection();

//swagger
if (swaggerOptions?.Enable == true)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

//controllers
app.MapControllers();


//#########################################################################


app.Services.RbCheckTz().RbCheckLocale();

await app.Services.RbEfMigrateAsync<McDbContext>();
await app.RunAsync();