using Bloodsport.Entity.Database;

namespace BloodsportSite.Client.Models;

public record RecruitmentListItem(
    long Id,
    string DisplayName,
    DateTime DateCreated,
    string? FlagUrl,
    List<UserRecruitmentLanes> Lanes
);
