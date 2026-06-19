namespace BloodsportSite.Client.Models;

public record UserListItem(
    long Id,
    string DisplayName,
    DateTime DateCreated,
    List<UserTeamSummary> Teams
);

public record UserTeamSummary(
    long Id,
    string Name,
    string? FlagUrl
);
