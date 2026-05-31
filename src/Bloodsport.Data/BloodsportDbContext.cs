using Bloodsport.Core.Models;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;

namespace Bloodsport.Data;

public class BloodsportDbContext : DbContext
{
    public BloodsportDbContext(DbContextOptions<BloodsportDbContext> options) : base(options) { }

    public DbSet<Player> Players => Set<Player>();
    public DbSet<Models.Tournament> Tournaments => Set<Models.Tournament>();
    public DbSet<TournamentPlayer> TournamentPlayers => Set<TournamentPlayer>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Player>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasIndex(p => p.SummonerName).IsUnique();
            e.Property(p => p.TrueSkillMu).HasDefaultValue(25.0);
            e.Property(p => p.TrueSkillSigma).HasDefaultValue(8.333);
        });

        builder.Entity<Models.Tournament>(e =>
        {
            e.HasKey(t => t.Id);
            e.HasMany(t => t.Players).WithOne(tp => tp.Tournament).HasForeignKey(tp => tp.TournamentId);
            e.HasMany(t => t.Matches).WithOne(m => m.Tournament).HasForeignKey(m => m.TournamentId);
        });

        builder.Entity<TournamentPlayer>(e =>
        {
            e.HasKey(tp => tp.Id);
            e.HasOne(tp => tp.Player).WithMany().HasForeignKey(tp => tp.PlayerId);
        });

        builder.Entity<Match>(e =>
        {
            e.HasKey(m => m.Id);
            e.HasOne(m => m.Player1).WithMany().HasForeignKey(m => m.Player1Id).IsRequired(false);
            e.HasOne(m => m.Player2).WithMany().HasForeignKey(m => m.Player2Id).IsRequired(false);
            e.HasOne(m => m.Winner).WithMany().HasForeignKey(m => m.WinnerId).IsRequired(false);
            e.HasIndex(m => m.RiotTournamentCode).IsUnique().HasFilter("\"RiotTournamentCode\" IS NOT NULL");
        });

        // OpenIddict tables
        builder.UseOpenIddict();
    }
}

// Extended Identity user linked to a Player profile
public class ApplicationUser : Microsoft.AspNetCore.Identity.IdentityUser<Guid>
{
    public Guid? PlayerId { get; set; }
    public Player? Player { get; set; }
}
