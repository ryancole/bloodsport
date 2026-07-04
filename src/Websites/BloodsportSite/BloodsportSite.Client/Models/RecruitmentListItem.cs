using Bloodsport.Entity.Database;

namespace BloodsportSite.Client.Models;

public record RecruitmentListItem(
    long RiotAccountId,
    string GameName,
    string TagLine,
    long UserId,
    string DisplayName,
    string? FlagUrl,
    List<RiotAccountRecruitmentLanes> Lanes,
    bool AlreadyInvited
);
