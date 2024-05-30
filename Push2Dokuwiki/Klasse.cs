using System.Collections.Generic;

namespace Push2Dokuwiki
{
    public class Klasse
    {
        public int IdUntis { get; internal set; }
        public string NameUntis { get; internal set; }
        public List<Lehrer> Klassenleitungen { get; internal set; }
        public bool IstVollzeit { get; internal set; }
        public string BildungsgangLangname { get; internal set; }
        public string BildungsgangGekürzt { get; internal set; }
        public string WikiLink { get; internal set; }
        public string Stufe { get; internal set; }
        public string Schule { get; private set; }
        public Abwesenheiten Abwesenheiten { get; internal set; }
    }
}
