using System;

namespace Push2Dokuwiki
{
    public class Vorgang
    {
        public Vorgang()
        {
        }

        public Vorgang(int schuelerId, string beschreibung, DateTime datum, string typ, object vId)
        {
            SchuelerId = schuelerId;
            Beschreibung = beschreibung;
            Datum = datum;
            Typ = typ;
            VId = vId;
        }

        public int SchuelerId { get; }
        public string Beschreibung { get; }
        public DateTime Datum { get; }
        public string Typ { get; }
        public object VId { get; }
    }
}