using Bloodsport.Entity.Database;

namespace Bloodsport.Entity.BlazorForm
{
    public class SeasonCreateForm
    {
        public string Name { get; set; } = string.Empty;

        public int Length { get; set; } = 6;

        public string? StartDate { get; set; }

        public int TeamSize { get; set; } = 5;

        public string? PickType { get; set; }

        public string? MapType { get; set; }

        public string? SpectatorType { get; set; }

        public bool? EnoughPlayers { get; set; }
    }

    public class SeasonEditForm
    {
        public SeasonStatus Status { get; set; }

        public string RiotRegion { get; set; } = string.Empty;

        public bool? RegistrationOpen { get; set; }

        public string? StartDate { get; set; }

        public int TeamSize { get; set; } = 5;

        public string? PickType { get; set; }

        public string? MapType { get; set; }

        public string? SpectatorType { get; set; }

        public bool? EnoughPlayers { get; set; }
    }

    public class SeasonRegistrationForm
    {
        public long TeamId { get; set; }
    }
}
