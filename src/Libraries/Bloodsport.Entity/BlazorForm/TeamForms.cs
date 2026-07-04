namespace Bloodsport.Entity.BlazorForm
{
    public class TeamCreateForm
    {
        public string Name { get; set; } = string.Empty;
    }

    public class TeamEditForm
    {
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateTeamRecruitmentForm
    {
        public bool IsLookingForUser { get; set; }
    }
}
