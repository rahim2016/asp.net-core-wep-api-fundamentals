using CityInfoAPI;
using CityInfoAPI.DbContexts;
using CityInfoAPI.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
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
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "City API",
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

    // using System.Reflection;
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

});



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
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Used for selecting a given enpoint in the controller 
app.UseRouting();

app.UseAuthorization();

// used for executing and mapping a given endpoint in the controller
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

//app.MapControllers();

app.Run();
