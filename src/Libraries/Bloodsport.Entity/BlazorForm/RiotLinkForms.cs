namespace Bloodsport.Entity.BlazorForm
{
    public class RiotLinkForm
    {
        public string GameName { get; set; } = string.Empty;

        public string TagLine { get; set; } = string.Empty;
    }

    public class RiotUnlinkForm
    {
        public long RiotAccountId { get; set; }
    }
}
