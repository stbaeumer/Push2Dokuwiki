using System;
using System.Collections.Generic;
using System.Linq;

namespace Push2Dokuwiki
{
    public class Team
    {
        public string Langname { get; set; }
        public List<Lehrer> Members { get; set; }
        public string Kategorie { get; set; }
        public List<Schueler> Schuelers { get; internal set; }
        public string WikiLink { get; internal set; }
        public string Kurzname { get; internal set; }
        public string Leitungsbezeichnung { get; set; }
        public Lehrer Leitung { get; internal set; }

        public Team(string langname, string kurzname, string leitungsbezeichnung, string suchkriteriumInAnrechnungen, List<Lehrer> lehrers, Anrechnungs anrechnungs)
        {
            Kurzname = kurzname;
            Kategorie = kurzname;
            Langname = langname;
            WikiLink = anrechnungs.GetWikiLink(lehrers, langname, suchkriteriumInAnrechnungen);
            Leitungsbezeichnung = leitungsbezeichnung;
            Leitung = anrechnungs.GetLeitung(lehrers, langname, leitungsbezeichnung);
            Members = lehrers;
        }

        public Team()
        {
            Members = new List<Lehrer>();
        }
    }
}
