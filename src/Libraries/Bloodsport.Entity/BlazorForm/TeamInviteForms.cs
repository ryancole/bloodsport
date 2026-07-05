namespace Bloodsport.Entity.BlazorForm
{
    public class TeamInviteForm
    {
        public string GameName { get; set; } = string.Empty;

        public string TagLine { get; set; } = string.Empty;
    }

    public class TeamApplyForm
    {
        public long RiotAccountId { get; set; }
    }
}
