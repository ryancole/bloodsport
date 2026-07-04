using Bloodsport.Entity.Database;

namespace BloodsportSite.Client.Models;

public record RecruitmentTeamListItem(
    long Id,
    string Name,
    DateTime DateCreated,
    string? FlagUrl,
    List<TeamRecruitmentLanes> Lanes
);
