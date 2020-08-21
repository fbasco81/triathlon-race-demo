namespace Messages
{
    public class AthleteRegistered
    {
        public string BibId { get; private set; }

        public AthleteRegistered(string bibId)
        {
            BibId = bibId;
        }
    }
}
