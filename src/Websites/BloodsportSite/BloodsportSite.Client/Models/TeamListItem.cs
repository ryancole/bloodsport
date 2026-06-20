namespace BloodsportSite.Client.Models;

public record TeamListItem(
    long Id,
    string Name,
    long ManagerId,
    string ManagerName,
    DateTime DateCreated,
    string? FlagUrl,
    string? ManagerFlagUrl
);
