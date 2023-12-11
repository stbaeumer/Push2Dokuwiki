namespace Push2Dokuwiki
{
    internal class Fachschaft
    {
        public string KürzelUntis { get; private set; }
        public string Beschr { get; }

        public Fachschaft(string kürzelUntis, string beschr)
        {
            KürzelUntis = kürzelUntis;
            Beschr = beschr;
        }
    }
}