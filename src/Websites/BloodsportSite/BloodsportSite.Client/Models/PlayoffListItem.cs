using Bloodsport.Entity.Database;

namespace BloodsportSite.Client.Models;

public record PlayoffListItem(
    long Id,
    string Name,
    long SeasonId,
    string SeasonName,
    PlayoffStatus Status,
    int TeamCount,
    DateTime DateCreated);
