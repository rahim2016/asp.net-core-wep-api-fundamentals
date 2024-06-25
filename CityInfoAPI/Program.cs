using CityInfoAPI;
using CityInfoAPI.Services;
using Microsoft.AspNetCore.StaticFiles;
using Serilog;

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
builder.Services.AddSwaggerGen();

// Registering FileExtensionContentTypeProvider  to find the correct content type for any file.
builder.Services.AddSingleton<FileExtensionContentTypeProvider>();

// Adding custom services to the container

// Registering the mail service to the container base on the environment using the preprocessor or compiler directive
#if DEBUG
builder.Services.AddTransient<IMailService, LocalMailService>();
#else 
builder.Services.AddTransient<IMailService, CloudMailService>();
#endif

builder.Services.AddSingleton<CitiesDataStore>();

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
