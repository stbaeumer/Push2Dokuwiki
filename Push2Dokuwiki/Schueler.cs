using System;
using System.Collections.Generic;
using System.Linq;

namespace Push2Dokuwiki
{
    public class Schueler
    {
        private object bildungsgangeintrittsdatum;

        //public Schueler(int id, string nachname, string vorname, DateTime gebdat, Klasse klasse, DateTime bildungsgangeintrittsdatum)
        //{
        //    Id = id;
        //    Nachname = nachname;
        //    Vorname = vorname;
        //    Klasse = klasse;
        //    Gebdat = gebdat;
        //    Bildungsgangeintrittsdatum = bildungsgangeintrittsdatum;
        //    Abwesenheiten = new List<Abwesenheit>();
        //    Maßnahmen = new List<Maßnahme>();
        //    Vorgänge = new List<Vorgang>();
        //}


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
        public Klasse Klasse { get; internal set; }
        public string LSSchulnummer { get; internal set; }
        public string Wahlklausur12_1 { get; internal set; }
        public string Wahlklausur12_2 { get; internal set; }
        public string Wahlklausur13_1 { get; internal set; }
        public string Wahlklausur13_2 { get; internal set; }
        public Unterrichts UnterrichteAusWebuntis { get; private set; }
        public Abwesenheiten Abwesenheiten { get; internal set; }
        public Maßnahmen Maßnahmen { get; internal set; }
        public Vorgänge Vorgänge { get; internal set; }
        public int FehltUnunterbrochenUnentschuldigtSeitTagen { get; private set; }
        public DateTime Bildungsgangeintrittsdatum { get; private set; }
        public Feriens Feriens { get; private set; }
        public int AktSj { get; private set; }
        public Maßnahme AnstehendeMaßnahme { get; set; }
        public bool BußgeldVerfahrenInLetzten12Monaten { get; set; }
        public bool ImLetztenMonatMehrAls1TagUnentschuldigtGefehlt { get; set; }
        public string LetztesBußgeldverfahrenAm { get; set; }
        public int NichtEntschuldigteFehlminutenImLetztenMonat { get; set; }
        public DateTime DieLetzteStattgefundeneMaßnahmeDerVergangenen12Monate { get; set; }
        public string SchuelerKlasseName { get; set; }
        public string Tabelle { get; set; }
        public string VornameNachname { get; set; }
        public bool SchriftlichErinnertInDenLetzten60Tagen { get; set; }
        public int NichtEntschuldigteFehlstundenIn30Tagen { get; set; }
        public int UnentschuldigteFehlMinutenSeitTrotzMaßnahmeOderÜberhaupt { get; set; }
        public bool IrgendeineMaßnahmeInDenLetzten12Monaten { get; set; }
        public int NichtEntschuldigteFehlminuten { get; set; }
        public int NichtEntschuldigteFehlminutenSeitLetzterMaßnahme { get; set; }
        public int OffeneStunden { get; set; }
        public bool MehrAls10FehlzeitenIndenLetzten30Tagen { get; set; }
        public int AnzahlNichtEntschuldigteTage { get; set; }
        public string FehltSeit { get; set; }
        public bool MehrAls10OffeneOderNichtEntschuldigteFehlzeitenDieAuchSchonMehrAlsEineWocheZurückliegen { get; set; }
        public int NichtEntschuldigteFehlstundenImLetztenMonat { get; set; }
        public int NichtEntschuldigteFehlminutenSeitSchuljahresbeginn { get; set; }
        public int NichtEntschuldigteFehlstunden { get; set; }
        public int NichtEntschuldigteFehlstundenSeitLetzterMaßnahme { get; set; }
        public bool IstSchulpflichtig { get; set; }
        public bool IstVolljährig { get; private set; }
        public List<Abwesenheit> AbwesenheitenOffen { get; internal set; }
        public List<Abwesenheit> AbwesenheitenNichtEntsch { get; internal set; }
        public string Zeile { get; internal set; }
        public string Jahrgang { get; internal set; }
        public int UnenschuldigteFehlstunden { get; internal set; }
        public Maßnahme JüngsteMaßnahmeInDiesemSj { get; internal set; }
        public Vorgang JüngsterVorgang { get; internal set; }
        public string JüngsteEskalation { get; internal set; }
        public string AlleMaßnahmenUndVorgänge { get; internal set; }
        public int OffeneStundenSeitJüngsterMaßnahme { get; internal set; }
        public int OffeneStundenSeitJüngsterVorgang { get; internal set; }
        public int NichtEntschStundenSeitJüngsterMaßnahme { get; internal set; }
        public int OffeneOderNichtEntStundenSeitJüngsterMaßnahmeDieMehrAlsAnzahlTageZurückliegen { get; internal set; }
        public string MaßnahmenAlsWikiLinkAufzählung { get; internal set; }
        public object MaßnahmenAlsWikiLinkAufzählungDatum { get; internal set; }
        public int NichtEntschStundenInDenLetzten14Tagen { get; internal set; }
        public int NichtEntschStundenDiesesSchuljahr { get; internal set; }
        public int OffeneOderNichtEntschStundenSeitJüngsterMaßnahme { get; internal set; }
        public int OffeneOderNichtEntStundenSeitMehrAlsAnzahlTageZurückliegen { get; internal set; }
        public int NichtVerjährteNichtEntschStundenUnbescholtene { get; internal set; }
        public int F3 { get; internal set; }
        public int F2 { get; internal set; }
        public int F2MplusF3M { get; internal set; }
        public int F2M { get; internal set; }
        public int F3M { get; internal set; }
        public int F2MplusF3 { get; internal set; }
        public int F2plusF3 { get; internal set; }

        private string GetTabelle()
        {
            string tabelle = "<ul>";

            for (var day = DateTime.Now.Date.AddDays(-360); day.Date <= DateTime.Now.Date; day = day.AddDays(1))
            {
                var maßnahme = (from m in Maßnahmen where m.Datum.Date == day.Date select m).FirstOrDefault();

                if (maßnahme != null)
                {
                    tabelle += "<li>" + day.Date.ToShortDateString() + "  " + maßnahme.Kürzel + "  " + maßnahme.Bezeichnung + " " + maßnahme.Beschreibung + "</li>";
                }

                var fehlzeit = (from t in Abwesenheiten where t.Status == "nicht entsch." where t.Datum.Date == day.Date select t).FirstOrDefault();

                if (fehlzeit != null)
                {
                    if (fehlzeit.Fehlminuten < 45)
                    {
                        tabelle += "<li>" + day.Date.ToShortDateString() + "  Fehlzeitdauer: " + fehlzeit.Fehlminuten + " Minuten, " + fehlzeit.Grund + ", " + fehlzeit.Text + " " + fehlzeit.Status + "</li>";
                    }
                    else
                    {
                        tabelle += "<li>" + day.Date.ToShortDateString() + "  Fehlzeitdauer: " + fehlzeit.Fehlstunden + " Stunden, " + fehlzeit.Grund + ", " + fehlzeit.Text + " " + fehlzeit.Status + "</li>";
                    }
                }
            }

            tabelle += "</ul>";
            return tabelle;
        }

        internal void GetFehltUnunterbrochenUnentschuldigtSeitTagen(Feriens frns)
        {
            this.FehltUnunterbrochenUnentschuldigtSeitTagen = 0;

            for (int t = -1; t > -28; t--)
            {
                DateTime tag = DateTime.Now.Date.AddDays(t);

                if (!(tag.DayOfWeek == DayOfWeek.Sunday))
                {
                    if (!(tag.DayOfWeek == DayOfWeek.Saturday))
                    {
                        if (!frns.IstFerienTag(tag))
                        {
                            if ((from a in this.Abwesenheiten
                                 where a.GanzerFehlTag == 1
                                 where a.Datum.Date == tag.Date
                                 where a.StudentId == Id
                                 select a).Any())
                            {
                                FehltUnunterbrochenUnentschuldigtSeitTagen++;
                            }
                        }
                    }
                }
            }
        }

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

        internal string SetAnstehendeMaßnahme()
        {
            if (MehrAls10OffeneOderNichtEntschuldigteFehlzeitenDieAuchSchonMehrAlsEineWocheZurückliegen)
            {
                return "<li>" + VornameNachname + " hat mehr als 10 offene oder nicht entschuldigte Fehlzeiten, die auch schon mehr als eine Woche zurückliegen ";
            }

            return "";

            if (IstSchulpflichtig && BußgeldVerfahrenInLetzten12Monaten && ImLetztenMonatMehrAls1TagUnentschuldigtGefehlt)
            {
                Console.WriteLine(SchuelerKlasseName + "fehlt trotz Bußgeldverharens");

                string mehrAls = (NichtEntschuldigteFehlminuten % NichtEntschuldigteFehlstunden > 0 ? "mehr als " : "");

                return "<li>" + VornameNachname + " ist schulpflichtig und fehlt trotz Bußgeldverfahrens am " + LetztesBußgeldverfahrenAm + " seit dem " + FehltSeit + " " + mehrAls + NichtEntschuldigteFehlstunden + " Stunden an " + AnzahlNichtEntschuldigteTage + " Tagen unentschuldigt. <u>Bitte mit mir das weitere Vorgehen absprechen.</u>" + Tabelle + "</li>";
            }

            // SchulG §47 (1):  Das Schulverhältnis endet, wenn die nicht mehr schulpflichtige Schülerin oder der nicht mehr schulpflichtige Schüler 
            // trotz schriftlicher Erinnerung 
            // ununterbrochen 20 Unterrichtstage unentschuldigt fehlt

            if (!IstSchulpflichtig && FehltUnunterbrochenUnentschuldigtSeitTagen >= 15 && !SchriftlichErinnertInDenLetzten60Tagen)
            {
                Console.WriteLine(SchuelerKlasseName + "fehlt seit " + FehltUnunterbrochenUnentschuldigtSeitTagen + " Tagen ununterbrochen");

                return "<li><b>" + Nachname + ", " + Vorname + "</b> ist nicht mehr schulpflichtig und fehlt ununterbrochen seit " + FehltUnunterbrochenUnentschuldigtSeitTagen + " Unterrichtstagen nicht entschuldigt. Bitte eine <u>schriftliche Erinnerung</u> <a href='https://recht.nrw.de/lmi/owa/br_bes_detail?sg=0&menu=1&bes_id=7345&anw_nr=2&aufgehoben=N&det_id=461191'>(SchulG § 47 (1), Satz 8)</a> bei <a href=\"mailto:ursula.moritz@berufskolleg-borken.de?subject=Schriftliche%20Erinnerung%20für%20" + Vorname + "%20" + Nachname + "%20(" + Klasse.NameUntis + ")\">Ursula Moritz</a> beauftragen. Wenn " + Vorname + " anschließend nicht unverzüglich den Unterricht aufnimmt, bitte die <u>Ausschulung im Schulbüro</u> beantragen." + Tabelle + "</li>";
            }

            if (!IstSchulpflichtig && FehltUnunterbrochenUnentschuldigtSeitTagen >= 20 && SchriftlichErinnertInDenLetzten60Tagen)
            {
                string maßnahme = SchuelerKlasseName + "; fehlt seit " + FehltUnunterbrochenUnentschuldigtSeitTagen + " Tagen ununterbrochen.";

                Console.WriteLine(maßnahme);

                return "<li>" + VornameNachname + " ist nicht mehr schulpflichtig und fehlt seit " + FehltUnunterbrochenUnentschuldigtSeitTagen + " Tagen ununterbrochen. " + Vorname + " wurde bereits schriftlich erinnert. Bitte die <u>Ausschulung</u> im Schulbüro beauftragen." + Tabelle + "</li>";
            }

            // SchulG §53(4): Die Entlassung einer Schülerin oder eines Schülers, die oder der nicht mehr schulpflichtig ist, kann ohne vorherige Androhung erfolgen, wenn die Schülerin oder der Schüler innerhalb eines Zeitraumes von 30 Tagen insgesamt 20  unentschuldigt versäumt hat

            if (!IstSchulpflichtig && NichtEntschuldigteFehlstundenIn30Tagen > 20)
            {
                string problem = " fehlt in den letzten 30 Tagen <b>" + NichtEntschuldigteFehlstundenIn30Tagen + " Stunden</b> ohne Entschuldigung.";

                Console.WriteLine(SchuelerKlasseName + problem);

                return "<li><b>" + Vorname + " " + Nachname + "</b>" + problem + " Da " + Vorname + " nicht mehr schulpflichtig ist, kommt die Anwendung von <a href='https://recht.nrw.de/lmi/owa/br_bes_detail?sg=0&menu=1&bes_id=7345&anw_nr=2&aufgehoben=N&det_id=461197'>SchulG §53 (4)</a> in Betracht. Bitte mit mir das weitere Vorgehen absprechen." + Tabelle + "</li>";
            }

            if (!IrgendeineMaßnahmeInDenLetzten12Monaten && NichtEntschuldigteFehlstunden > 8)
            {
                string mehrAls = (NichtEntschuldigteFehlminuten % NichtEntschuldigteFehlstunden > 0 ? "mehr als " : "");

                var fehltSeit = " fehlt seit dem " + ((from a in Abwesenheiten select a.Datum).FirstOrDefault()).ToShortDateString() + " ";

                string problem = fehltSeit + " " + mehrAls + "<b>" + NichtEntschuldigteFehlstunden + " Stunden </b> unentschuldigt. ";

                Console.WriteLine(SchuelerKlasseName + problem);

                return "<li>" + VornameNachname + "" + problem + " Bitte eine <u>Mahnung</u> der Fehlzeiten bei <a href=\"mailto:ursula.moritz@berufskolleg-borken.de?subject=Mahnung%20der%20Fehlzeiten%20von%20" + Vorname + "%20" + Nachname + "%20(" + Klasse.NameUntis + ")\">Ursula Moritz</a> beauftragen." + Tabelle + "</li>";
            }

            if (IrgendeineMaßnahmeInDenLetzten12Monaten && NichtEntschuldigteFehlminutenSeitLetzterMaßnahme > 450)
            {
                var letzteMaßnahme = (from m in Maßnahmen select m).LastOrDefault();

                var fehltSeit = " fehlt seit der letzten Maßnahme " + letzteMaßnahme.Kürzel + " " + letzteMaßnahme.Beschreibung + " am " + letzteMaßnahme.Datum.ToShortDateString() + " ";

                string problem = fehltSeit + "<b>" + NichtEntschuldigteFehlstundenSeitLetzterMaßnahme + " Stunden</b> unentschuldigt. ";

                Console.WriteLine(SchuelerKlasseName + problem);

                if (letzteMaßnahme.Kürzel.StartsWith("M"))
                {
                    return "<li>" + VornameNachname + "" + problem + " Bitte kurzfristig mit mir das weitere Vorgehen absprechen." + Tabelle + "</li>";
                }
                return "<li>" + VornameNachname + "" + problem + " Bitte eine <u>Mahnung</u> der Fehlzeiten bei <a href=\"mailto:ursula.moritz@berufskolleg-borken.de?subject=Mahnung%20der%20Fehlzeiten%20von%20" + Vorname + "%20" + Nachname + "%20(" + Klasse.NameUntis + ")\">Ursula Moritz</a> beauftragen." + Tabelle + "</li>";
            }

            return "";
        }

        internal string GetJüngsteEskalation()
        {
            if (JüngsteMaßnahmeInDiesemSj.Datum == JüngsterVorgang.Datum)
            {
                // seit SJ-Beginn
                return "dem Schuljahresbeginn";
            }
            else
            {
                if (JüngsteMaßnahmeInDiesemSj.Datum > JüngsterVorgang.Datum)
                {
                    return "der " + JüngsteMaßnahmeInDiesemSj.Kürzel + " (" + JüngsteMaßnahmeInDiesemSj.Datum.ToShortDateString() + ")";
                }
                else
                {
                    return "der " + JüngsterVorgang.Beschreibung + " (" + JüngsterVorgang.Datum.ToShortDateString() + ")";
                }
            }
        }

        internal string GetMaßnahmenAlsWikiLinkAufzählung()
        {
            var x = "";
            var bezeichnung = "";

            foreach (var item in Maßnahmen.OrderBy(y => y.Datum))
            {
                if (item.Bezeichnung.StartsWith("M") || item.Kürzel.StartsWith("M"))
                {
                    bezeichnung = "Mahnung";
                }
                if (item.Bezeichnung.ToLower().StartsWith("at"))
                {
                    bezeichnung = "Attestpflicht";
                }
                if (item.Bezeichnung.StartsWith("T"))
                {
                    bezeichnung = "Teilkonferenz";
                }
                if (bezeichnung == "")
                {
                     bezeichnung = item.Bezeichnung;
                }
                if (bezeichnung == "")
                {
                    bezeichnung = item.Kürzel;
                }

                //x += "[[" + bezeichnung + "]],";
                x += bezeichnung + " (" + item.Datum.ToShortDateString() + @")\\ ";
            }
            return x.TrimEnd(' '); ;
        }

        internal object GetMaßnahmenAlsWikiLinkAufzählungDatum()
        {
            var x = "";
            foreach (var item in Maßnahmen.OrderBy(y => y.Datum))
            {
                //if (item.Bezeichnung.StartsWith("M") || item.Bezeichnung.StartsWith("At") || item.Bezeichnung.StartsWith("T"))
                {
                    x += item.Datum.ToString("yyyy-MM-dd") + " ";
                }
            }
            return x.TrimEnd(' ');
        }

        public string GetUrl(string v)
        {
            return "https://bkb.wiki/antraege_formulare:" + v + "?@Schüler*in@=" + Vorname + "_" + Nachname + "&@Klasse@=" + Klasse.NameUntis;
        }
        internal string GetWikiLink(string v, int stunden)
        {
            return "\\ [[antraege_formulare:" + v + "?@Schüler*in@=" + Vorname + "_" + Nachname + "&@Klasse@=" + Klasse.NameUntis + "&@... unentschuldigte Fehlstunden:@=" + stunden + "]]";
        }
    }
}