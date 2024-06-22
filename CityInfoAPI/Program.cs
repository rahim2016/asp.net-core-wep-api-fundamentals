using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


// Add AddXmlDataContractSerializerFormatters() to return content also as xml other than json only.
builder.Services.AddControllers(options =>
{
    // Control the accept header property either json or xml
    options.ReturnHttpNotAcceptable = true;
}).AddNewtonsoftJson()
.AddXmlDataContractSerializerFormatters();

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

//app.MapControllers();

app.Run();
