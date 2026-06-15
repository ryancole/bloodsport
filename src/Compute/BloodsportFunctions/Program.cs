using Azure.Communication.Email;
using Azure.Storage.Blobs;
using Bloodsport.Data.Sql;
using Bloodsport.Entity.RiotApi;
using BloodsportFunctions.Services;
using Camille.RiotGames;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BloodsportFunctions;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = FunctionsApplication.CreateBuilder(args);

        builder.ConfigureFunctionsWebApplication();

        if (builder.Environment.IsDevelopment())
            builder.Configuration.AddUserSecrets<Program>();

        builder
            .Services
            .AddDbContextFactory<SqlDbContext>(options => options.UseSqlServer(builder.Configuration["SqlConnectionString"]));

        builder
            .Services
            .AddSingleton(new BlobServiceClient(
                builder.Configuration.GetConnectionString("AzureWebJobsStorage")
                    ?? throw new InvalidOperationException("AzureWebJobsStorage is not configured.")));

        builder
            .Services
            .AddSingleton(new EmailClient(
                builder.Configuration.GetConnectionString("EmailCommunicationService")
                    ?? throw new InvalidOperationException("ConnectionStrings:EmailCommunicationService is not configured.")));

        builder
            .Services
            .AddSingleton<EmailTemplateRenderer>();

        builder
            .Services
            .AddTransient<EmailService>();

        RiotApiEndpoints.UseStub = builder.Configuration.GetValue<bool>("RiotApi:UseStub");

        builder
            .Services
            .AddSingleton(RiotGamesApi.NewInstance(
                builder.Configuration["RiotApi:ApiKey"]
                    ?? throw new InvalidOperationException("RiotApi:ApiKey is not configured.")));

        builder.Build().Run();
    }
}
