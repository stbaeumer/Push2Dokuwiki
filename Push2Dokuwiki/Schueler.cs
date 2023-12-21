using System;
using System.Collections.Generic;
using System.Linq;

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
        public string Wahlklausur12_1 { get; internal set; }
        public string Wahlklausur12_2 { get; internal set; }
        public string Wahlklausur13_1 { get; internal set; }
        public string Wahlklausur13_2 { get; internal set; }
        public Unterrichts UnterrichteAusWebuntis { get; private set; }

        internal int GetUnterrichte(List<Unterricht> unterrichteDerKlasse, List<Gruppe> alleGruppen)
        {
            int i = 0;
            UnterrichteAusWebuntis = new Unterrichts();

            // Unterrichte der ganzen Klasse

            var unterrichteDerKlasseOhneGruppen = (from a in unterrichteDerKlasse
                                                   where a.Gruppe == ""
                                                   select a).ToList();

            foreach (var u in unterrichteDerKlasseOhneGruppen)
            {
                // Wenn ein Lehrer zweimal mit dem selben Fach in Webuntis eingetragen ist, wird kein weiterer Unterricht angelegt.

                var gibtsSchon = (from x in UnterrichteAusWebuntis where x.Fach == u.Fach where x.Lehrkraft == u.Lehrkraft select x).FirstOrDefault();

                if (gibtsSchon == null)
                {
                    i++;
                    UnterrichteAusWebuntis.Add(new Unterricht(
                        
                        u.LessonNumbers[0],
                        u.Fach,                        
                        u.Lehrkraft,
                        u.Zeile,
                        u.Periode,
                        u.Gruppe,
                        u.Klassen,
                        u.Startdate,
                        u.Enddate));
                }
                else
                {
                    gibtsSchon.LessonNumbers.Add(u.LessonNumbers[0]);
                }
            }

            // Kurse

            foreach (var gruppe in (from g in alleGruppen where g.StudentId == Id select g).ToList())
            {
                var u = (from a in unterrichteDerKlasse where a.Gruppe == gruppe.Gruppenname select a).FirstOrDefault();

                // u ist z.B. null, wenn ein Kurs in der ExportLessons als (lange) abgeschlossen steht.

                if (u != null)
                {
                    // Wenn ein Lehrer zweimal mit dem selben Fach in Webuntis eingetragen ist, wird kein weiterer Unterricht angelegt.

                    var gibtsSchon = (from x in UnterrichteAusWebuntis where x.Fach == u.Fach where x.Lehrkraft == u.Lehrkraft select x).FirstOrDefault();

                    if (gibtsSchon == null)
                    {
                        i++;
                        UnterrichteAusWebuntis.Add(new Unterricht(
                            u.LessonNumbers[0],
                            u.Fach,
                            u.Lehrkraft,
                            u.Zeile,
                            u.Periode,
                            u.Gruppe,
                            u.Klassen,
                            u.Startdate,
                            u.Enddate));
                    }
                    else
                    {
                        gibtsSchon.LessonNumbers.Add(u.LessonNumbers[0]);
                    }
                }
            }
            return i;
        }
    }
}