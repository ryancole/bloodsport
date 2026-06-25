using Azure.Communication.Email;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Bloodsport.Data.Sql;
using Bloodsport.Entity.Database;
using Bloodsport.Entity.RiotApi;
using BloodsportSite.Api;
using BloodsportSite.Components;
using Bloodsport.Common.Email;
using BloodsportSite.Services;
using Camille.RiotGames;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Identity.Web;
using System.Reflection;

namespace BloodsportSite
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder
                .Services
                .AddApplicationInsightsTelemetry(options =>
                    options.ConnectionString = builder.Configuration.GetConnectionString("ApplicationInsights"));

            builder
                .Services
                .AddHttpContextAccessor();

            builder
                .Services
                .AddSingleton<ITelemetryInitializer, AuthenticatedUserTelemetryInitializer>();

            builder
                .Services
                .AddMicrosoftIdentityWebAppAuthentication(builder.Configuration);

            builder
                .Services
                .PostConfigure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, ConfigureOpenIdConnect);

            builder
                .Services
                .AddAuthorization();

            builder
                .Services
                .AddRazorComponents()
                .AddInteractiveWebAssemblyComponents()
                .AddAuthenticationStateSerialization();

            builder
                .Services
                .AddHttpClient();

            RiotApiEndpoints.UseStub = builder.Configuration.GetValue<bool>("RiotApi:UseStub");

            builder
                .Services
                .AddSingleton(RiotGamesApi.NewInstance(
                    builder.Configuration["RiotApi:ApiKey"]
                        ?? throw new InvalidOperationException("RiotApi:ApiKey is not configured.")));

            builder
                .Services
                .AddTransient<RiotTournamentClient>();

            builder
                .Services
                .AddSingleton(new ServiceBusClient(
                    builder.Configuration.GetConnectionString("ServiceBus")
                        ?? throw new InvalidOperationException("ConnectionStrings:ServiceBus is not configured.")));

            builder
                .Services
                .AddSingleton(new BlobServiceClient(
                    builder.Configuration.GetConnectionString("BlobStorage")
                        ?? throw new InvalidOperationException("ConnectionStrings:BlobStorage is not configured.")));

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
                .AddTransient<TeamLogoService>()
                .AddTransient<UserLogoService>()
                .AddSingleton<BlobSasService>();

            builder
                .Services
                .AddDbContextFactory<SqlDbContext>(ConfigureSqlDbContext);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveWebAssemblyRenderMode()
                .AddAdditionalAssemblies(typeof(Client._Imports).Assembly);

            app.MapGroup("/auth").MapLoginAndLogout();
            app.MapGroup("/api").MapRiotLink();
            app.MapGroup("/api").MapTeams();
            app.MapGroup("/api").MapTeamInvites();
            app.MapGroup("/api").MapUsers();
            app.MapGroup("/api").MapSeasons();
            app.MapGroup("/api").MapMatchups();
            app.MapGroup("/api").MapPlayoffs();
            app.MapGroup("/api").MapPosts();

            app.Run();
        }

        private static void ConfigureSqlDbContext(IServiceProvider services, DbContextOptionsBuilder options)
        {
            var configuration = services.GetRequiredService<IConfiguration>();

            options.UseSqlServer(configuration["ConnectionStrings:Bloodsport"], ConfigureSqlServer);
        }

        private static void ConfigureSqlServer(SqlServerDbContextOptionsBuilder options)
        {
            var contextTypeInfo = typeof(SqlDbContext).GetTypeInfo();
            var migrationAssemblyName = contextTypeInfo.Assembly.GetName();

            options.MigrationsAssembly(migrationAssemblyName.Name);
        }

        private static readonly string[] _adjectives = [
            "Swift", "Bold", "Fierce", "Silent", "Iron", "Crimson", "Jade", "Storm",
            "Shadow", "Frost", "Blazing", "Steel", "Onyx", "Silver", "Golden", "Phantom"
        ];

        private static readonly string[] _nouns = [
            "Falcon", "Wolf", "Raven", "Dragon", "Viper", "Phoenix", "Titan", "Specter",
            "Hunter", "Striker", "Warden", "Sentinel", "Reaper", "Ember", "Cipher", "Blade"
        ];

        private static string GenerateRandomDisplayName()
        {
            var rng = Random.Shared;
            return _adjectives[rng.Next(_adjectives.Length)] + _nouns[rng.Next(_nouns.Length)];
        }

        private static void ConfigureOpenIdConnect(OpenIdConnectOptions options)
        {
            options.Scope.Add("email");

            var existingHandler = options.Events?.OnTokenValidated;

            options.Events ??= new OpenIdConnectEvents();

            options.Events.OnTokenValidated = async context =>
            {
                if (existingHandler is not null)
                {
                    await existingHandler(context);
                }

                var oid = context.Principal?.FindFirst("oid")?.Value
                       ?? context.Principal?.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;

                if (oid is null)
                {
                    return;
                }

                var dbFactory = context.HttpContext.RequestServices.GetRequiredService<IDbContextFactory<SqlDbContext>>();
                await using var db = dbFactory.CreateDbContext();

                var email = context.Principal?.FindFirst("email")?.Value
                         ?? context.Principal?.FindFirst("preferred_username")?.Value;

                var user = await db.Users.FirstOrDefaultAsync(u => u.EntraObjectId == oid);

                if (user is null)
                {
                    db.Users.Add(new User { EntraObjectId = oid, DisplayName = GenerateRandomDisplayName(), Email = email });
                }
                else if (email is not null && user.Email != email)
                {
                    user.Email = email;
                }

                await db.SaveChangesAsync();
            };
        }
    }
}
