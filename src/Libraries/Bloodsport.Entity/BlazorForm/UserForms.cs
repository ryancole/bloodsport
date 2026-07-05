namespace Bloodsport.Entity.BlazorForm
{
    public class UpdateDisplayNameForm
    {
        public string DisplayName { get; set; } = string.Empty;
    }

    public class UpdateRecruitmentForm
    {
        public long RiotAccountId { get; set; }

        public bool IsLookingForTeam { get; set; }
    }
}
