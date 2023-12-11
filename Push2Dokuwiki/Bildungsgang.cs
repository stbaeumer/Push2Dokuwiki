using System.Collections.Generic;

namespace Push2Dokuwiki
{
    public class Bildungsgang
    {
        public string Kurzname { get; internal set; }
        public string Langname { get; internal set; }
        public Lehrer Leitung { get; internal set; }
        public string WikiLink { get; internal set; }
        public Lehrers Members { get; internal set; }
        public List<Fach> Fächer { get; internal set; }
    }
}