using Bloodsport.Api.Hubs;
using Bloodsport.Api.Services;
using Bloodsport.Core.Rating;
using Bloodsport.Core.Tournament;
using Bloodsport.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<BloodsportDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.UseOpenIddict();
});

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<BloodsportDbContext>()
.AddDefaultTokenProviders();

// OpenIddict — self-hosted OpenID Connect server
builder.Services.AddOpenIddict()
    .AddCore(options => options.UseEntityFrameworkCore().UseDbContext<BloodsportDbContext>())
    .AddServer(options =>
    {
        options.SetTokenEndpointUris("/connect/token");
        options.SetAuthorizationEndpointUris("/connect/authorize");
        options.SetUserinfoEndpointUris("/connect/userinfo");

        options.AllowPasswordFlow();
        options.AllowRefreshTokenFlow();
        options.AllowAuthorizationCodeFlow().RequireProofKeyForCodeExchange();

        options.AcceptAnonymousClients();

        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        options.UseAspNetCore()
               .EnableTokenEndpointPassthrough()
               .EnableAuthorizationEndpointPassthrough()
               .EnableUserinfoEndpointPassthrough();
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

// App services
builder.Services.AddScoped<TrueSkillService>();
builder.Services.AddScoped<BracketService>();
builder.Services.AddScoped<RiotTournamentService>();

// SignalR for real-time bracket updates
builder.Services.AddSignalR();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Allow Blazor WASM to call the API
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(builder.Configuration["ClientUrl"] ?? "https://localhost:5001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<BracketHub>("/hubs/bracket");

// Auto-migrate on startup in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BloodsportDbContext>();
    db.Database.Migrate();
}

app.Run();
