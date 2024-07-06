using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using CityInfoAPI;
using CityInfoAPI.DbContexts;
using CityInfoAPI.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;

// using serilog package for logging and saving logs to a file
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/cityinfo.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Addition of logging to the application
/*
 
// Clearing the default logging providers and adding console logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

 */

// Telling asp.net to use serilog for logging instead of the default logging
builder.Host.UseSerilog();


// Add AddXmlDataContractSerializerFormatters() to return content also as xml other than json only.
builder.Services.AddControllers(options =>
{
    // Control the accept header property either json or xml
    options.ReturnHttpNotAcceptable = true;
}).AddNewtonsoftJson()
.AddXmlDataContractSerializerFormatters();

// Enable exception handling for the application to be returned as json
builder.Services.AddProblemDetails();

/*
 
// Adding addition information to error response body
builder.Services.AddProblemDetails(option =>
{
    option.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions.Add("AdditionalInfo", "Adittional Informations");
        context.ProblemDetails.Extensions.Add("Server", Environment.MachineName);
    };
});
 
*/


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Registering FileExtensionContentTypeProvider  to find the correct content type for any file.
builder.Services.AddSingleton<FileExtensionContentTypeProvider>();

// -------------- ADDING CUSTOM SERVICES TO THE CONTAINER ----------------------

// Registering the mail service to the container base on the environment using the preprocessor or compiler directive
#if DEBUG
builder.Services.AddTransient<IMailService, LocalMailService>();
#else 
builder.Services.AddTransient<IMailService, CloudMailService>();
#endif

builder.Services.AddSingleton<CitiesDataStore>();

// Registering the city info context to the container
builder.Services.AddDbContext<CityInfoContext>(dbContextOptions => dbContextOptions.UseSqlite(builder.Configuration["connectionStrings:cityInfoDBConnectionString"]));

// Registering the city info repository to the container
builder.Services.AddScoped<ICityInfoRepository, CityInfoRepository>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Registering the authentication service to the container
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Authentication:Issuer"],
            ValidAudience = builder.Configuration["Authentication:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Convert.FromBase64String(builder.Configuration["Authentication:SecretForKey"]))
        };
    });

// Adding the authorization service or policies to the container
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MustBeFromNewYork", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("city", "New York");
    });
});

// Adding the api versioning to the container
builder.Services.AddApiVersioning(setupAction =>
{
    setupAction.ReportApiVersions = true;
    setupAction.AssumeDefaultVersionWhenUnspecified = true;

    // set default version to 1.0 in case no version is specified
    setupAction.DefaultApiVersion = new ApiVersion(1, 0);
}).AddMvc()
.AddApiExplorer(setupAction =>
{
    setupAction.SubstituteApiVersionInUrl = true;
});

// Getting the api version description provider from the container by creating an instance of IApiVersionDescriptionProvider
// Here we are trying to get the versions descriptions of the API
var apiVersionDescriptionProvider = builder.Services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

builder.Services.AddSwaggerGen(options =>
{
    // Iteratng over the available versions descriptions and
    // Adding the swagger documentation for each version of the API
    foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
    {
        options.SwaggerDoc($"{description.GroupName}", new OpenApiInfo
        {
            Title = "City API",
            Version = description.ApiVersion.ToString(),
            Description = "An ASP.NET Core Web API for managing City and points of interest",
            TermsOfService = new Uri("https://example.com/terms"),
            Contact = new OpenApiContact
            {
                Name = "Rahim NDANE",
                Url = new Uri("https://www.linkedin.com/in/rahim-ndane-075732141/")
            },
            License = new OpenApiLicense
            {
                Name = "License",
                Url = new Uri("https://example.com/license")
            }
        });
    }

    // Adding the security definition to the swagger documentation
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    //  ading the bearer token to the autorization header
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });


    // Tell swagger to include the xml comments in the swagger documentation usinng reflection
    var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);
    options.IncludeXmlComments(xmlCommentsFullPath);


});

var app = builder.Build();

// Configure the HTTP request pipeline.

// Adding the middleware to handle exceptions both in development and production environment
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(setupAction =>
    {
        var descriptions = app.DescribeApiVersions();
        foreach (var description in descriptions)
        {
            setupAction.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
        }
    });
}

app.UseHttpsRedirection();

// Used for selecting a given enpoint in the controller 
app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

// used for executing and mapping a given endpoint in the controller
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

//app.MapControllers();

app.Run();
