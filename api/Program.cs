using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using JustShareBackend.Database;

var builder = WebApplication.CreateBuilder(args);

var azureSqlConnection = String.Empty;
var azureBlobStorageConnection = String.Empty;
var firebaseProjectId = String.Empty;

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddEnvironmentVariables().AddJsonFile("appsettings.json");
    azureSqlConnection = builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING");
    azureBlobStorageConnection = builder.Configuration.GetConnectionString("AZURE_BLOB_STORAGE_CONNECTIONSTRING");
    firebaseProjectId = builder.Configuration.GetValue<String>("FIREBASE_PROJECT_ID");
}
else
{
    azureSqlConnection = Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTIONSTRING");
    azureBlobStorageConnection = Environment.GetEnvironmentVariable("AZURE_BLOB_STORAGE_CONNECTIONSTRING");
    firebaseProjectId = Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID");
}

var firebaseApp = FirebaseApp.Create(new AppOptions
{
    Credential = GoogleCredential.FromFile("./serviceAccountKey.json"),
    ProjectId = firebaseProjectId
});

var blobServiceClient = new BlobServiceClient(azureBlobStorageConnection);
var firebaseAuth = FirebaseAuth.GetAuth(firebaseApp);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<JustShareDbContext>(options => options.UseSqlServer(azureSqlConnection, (builder) => builder.EnableRetryOnFailure()));
builder.Services.AddSingleton(blobServiceClient);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (context.Request.Headers.TryGetValue("Authorization", out var authorization))
    {
        var token = authorization.ToString().Replace("Bearer ", "");

        try
        {
            var firebaseToken = await firebaseAuth.VerifyIdTokenAsync(token);
            var userId = firebaseToken.Uid;
            
            context.Items.Add("userId", userId);

            await next();
        }

        catch
        {
            context.Response.StatusCode = 401;
            await context.Response.CompleteAsync();
        }
    }

    else
    {
        context.Response.StatusCode = 401;
        await context.Response.CompleteAsync();
    }
});

app.MapControllers();

app.Run();
