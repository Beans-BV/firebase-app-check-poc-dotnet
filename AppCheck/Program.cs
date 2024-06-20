using AppCheck.Helper.Header;
using AppCheck.Middleware;
using AppCheck.Settings;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.Bind("FirebaseSettings", new AppSettings());
// Initialize the Firebase SDK
FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile(builder.Configuration["FirebaseSettings:JsonUrl"]),
    ProjectId = builder.Configuration["FirebaseSettings:ProjectId"]
});

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
     .WriteTo.File(
                path: "logs/logs-.txt",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                formatProvider: null)
    .Enrich.FromLogContext()
    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<CustomHeader>();
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<FirebaseAppCheckMiddleware>();
app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
