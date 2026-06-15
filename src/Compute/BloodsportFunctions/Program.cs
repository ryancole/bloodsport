using Bloodsport.Data.Sql;
using Bloodsport.Entity.RiotApi;
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

        RiotApiEndpoints.UseStub = builder.Configuration.GetValue<bool>("RiotApi:UseStub");

        builder
            .Services
            .AddSingleton(RiotGamesApi.NewInstance(
                builder.Configuration["RiotApi:ApiKey"]
                    ?? throw new InvalidOperationException("RiotApi:ApiKey is not configured.")));

        builder.Build().Run();
    }
}
