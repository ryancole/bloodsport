namespace Bloodsport.Entity.Database
{
    public class SeasonRegistration
    {
        #region Properties

        public long Id { get; private set; }

        public long SeasonId { get; set; }

        public long TeamId { get; set; }

        public DateTime DateCreated { get; private set; }

        #endregion

        #region Navigation Properties

        public required virtual Season Season { get; set; }

        public required virtual Team Team { get; set; }

        #endregion
    }
}
