using Bloodsport.Entity.Database;

namespace BloodsportSite.Client.Models;

public record SeasonListItem(
    long Id,
    string Name,
    SeasonStatus Status,
    bool RegistrationOpen,
    DateTime? EstimatedDateEnd,
    int Length,
    DateTime DateCreated);
