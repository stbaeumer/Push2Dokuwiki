using System;

namespace Push2Dokuwiki
{
    public class Schueler
    {
        public string Status { get; internal set; }
        public int Bezugsjahr { get; internal set; }

        public int Id { get; set; }
        public string ImagePath { get; set; }
        public int MyProperty { get; set; }
        public string Telefon { get; set; }
        public string Mail { get; set; }
        public string Kurzname { get; set; }
        public string Geburtsdatum { get; set; }
        public DateTime Eintrittsdatum { get; set; }
        public DateTime Austrittsdatum { get; set; }
        public string Geschlecht { get; set; }
        public string Mobil { get; set; }
        public string Strasse { get; set; }
        public string Plz { get; set; }
        public string Ort { get; set; }
        public string ErzMobil { get; set; }
        public string ErzTelefon { get; set; }
        public bool Volljährig { get; set; }
        public string ErzName { get; set; }
        public string BetriebName { get; set; }
        public string BetriebStrasse { get; set; }
        public string BetriebPlz { get; set; }
        public string BetriebOrt { get; set; }
        public string BetriebTelefon { get; set; }
        public string Geschlecht34 { get; internal set; }
        public string AktuellJN { get; internal set; }
        public DateTime Relianmeldung { get; internal set; }
        public DateTime Reliabmeldung { get; internal set; }
        public int IdAtlantis { get; internal set; }
        public string MailAtlantis { get; internal set; }
        public DateTime Gebdat { get; internal set; }
        public string Vorname { get; internal set; }
        public string Nachname { get; internal set; }
        public string Anmeldename { get; internal set; }
        public string GeschlechtMw { get; internal set; }
        public int IdUntis { get; internal set; }
        public string Klasse { get; internal set; }
        public string LSSchulnummer { get; internal set; }
    }
}