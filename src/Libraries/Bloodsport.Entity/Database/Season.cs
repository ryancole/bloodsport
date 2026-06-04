namespace Bloodsport.Entity.Database
{
    public class Season
    {
        #region Properties

        public long Id { get; private set; }

        public bool RegistrationOpen { get; set; }

        public required string Name { get; set; }

        public SeasonStatus Status { get; set; } = SeasonStatus.Upcoming;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime DateCreated { get; private set; }

        #endregion

        #region Navigation Properties

        public virtual ICollection<SeasonRegistration> Registrations { get; set; } = [];

        public virtual ICollection<Match> Matches { get; set; } = [];

        #endregion
    }
}
