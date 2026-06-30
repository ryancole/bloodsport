namespace BloodsportSite.Components.Shared;

/// <summary>
/// Friendly display labels for the SCREAMING_SNAKE_CASE tournament code
/// parameter tokens returned by the Riot API.
/// </summary>
public static class MatchupParameterLabels
{
    public static string Humanize(string value) => value switch
    {
        "BLIND_PICK" => "Blind Pick",
        "DRAFT_MODE" => "Draft Mode",
        "ALL_RANDOM" => "All Random",
        "TOURNAMENT_DRAFT" => "Tournament Draft",
        "SUMMONERS_RIFT" => "Summoner's Rift",
        "HOWLING_ABYSS" => "Howling Abyss",
        "NONE" => "None",
        "LOBBYONLY" => "Lobby Only",
        "ALL" => "All",
        _ => value,
    };
}
