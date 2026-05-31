using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Bloodsport.Core.Tdl;

// Tournament Definition Language — YAML schema for declaring tournament formats.
// Write a .tournament.yml file to define any format without touching bracket logic.
public class TournamentDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Format { get; set; } = "single_elimination";
    public int Players { get; set; } = 8;
    public int BestOf { get; set; } = 3;
    public bool ThirdPlaceMatch { get; set; } = false;
    public string Seeding { get; set; } = "trueskill"; // trueskill | random | manual
    public RatingConfig Rating { get; set; } = new();
    public RiotConfig Riot { get; set; } = new();
}

public class RatingConfig
{
    public string Algorithm { get; set; } = "trueskill";
    public double MuInitial { get; set; } = 25.0;
    public double SigmaInitial { get; set; } = 8.333;
    public double Beta { get; set; } = 4.167;
    public double Tau { get; set; } = 0.083;
    public double DrawProbability { get; set; } = 0.0;
}

public class RiotConfig
{
    public string Region { get; set; } = "NA1";
    public string Map { get; set; } = "SUMMONERS_RIFT";
    public string PickType { get; set; } = "TOURNAMENT_DRAFT";
    public string Spectator { get; set; } = "ALL";
}

public static class TournamentDefinitionParser
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public static TournamentDefinition Parse(string yaml) =>
        Deserializer.Deserialize<TournamentDefinition>(yaml);

    public static TournamentDefinition LoadFile(string path) =>
        Parse(File.ReadAllText(path));
}
