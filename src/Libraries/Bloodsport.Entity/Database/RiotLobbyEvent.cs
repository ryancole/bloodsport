namespace Bloodsport.Entity.Database
{
    public class RiotLobbyEvent
    {
        #region Properties

        public long Id { get; private set; }

        public required string EventJson { get; set; }

        public required string TournamentCode { get; set; }

        public DateTime DateCreated { get; private set; }

        #endregion
    }
}
