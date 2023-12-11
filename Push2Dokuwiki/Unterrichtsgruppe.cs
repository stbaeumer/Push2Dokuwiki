using System;

namespace Push2Dokuwiki
{
    public class Unterrichtsgruppe
    {
        public int IdUntis { get; internal set; }
        public string Name { get; internal set; }
        public DateTime Von { get; internal set; }
        public DateTime Bis { get; internal set; }
        public Interruption Interruption { get; internal set; }
    }
}