using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Push2Dokuwiki
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("      Push2Dokuwiki.exe | Published under the terms of GPLv3 | Stefan Bäumer " + DateTime.Now.Year + " | Version 20240826");
                Console.WriteLine("=============================================================================================================");

                Console.WriteLine("");
                Console.WriteLine("Hinweise zu Kalendereinträgen:");                
                Console.WriteLine(" Nachricht:  Hier wird der Link zur konkreten Wiki-Seite gespeichert. So kann man aus dem Kalender ins Wiki abspringen." +
                    "\n             In der Terminübersicht im Wiki zeigen die Links direkt auf die konkrete Seite. Weitere Einträge sind möglich. ");
                Console.WriteLine(" Kategorien: Hier werden alle zugehörigen / übergeordneten Seiten als Kategorie eingetragen. " +
                    "\n             Die Seite selbst wird nicht noch einmal eingetragen.");
                Console.WriteLine("");

                var kalenderwoche = GetKalenderwoche();

                List<string> kalenders = new List<string>();
                kalenders.Add("termine_kollegium");
                kalenders.Add("termine_fhr");
                kalenders.Add("termine_berufliches_gymnasium");
                kalenders.Add("termine_verwaltung");
                
                foreach (var kalender in kalenders)
                {    
                    Termine termine = new Termine();
                    termine.AddRange(new Termine(
                        kalender,                   // Name der CSV-Kalenderdatei
                        1,                          // so alt darf die CSV-Datei maximal sein.
                        "https://bkb.wiki/",        // Nur Termine mit diesem String in der Nachricht werden ausgegeben
                        Encoding.GetEncoding(1252)  // Encoding der Importdatei
                        ));

                    if (termine.Count > 0)
                    {
                        // Die iteressierenden Spalten aus Dokuwiki werden hier festgelegt
                        termine.ToCsv(new List<string>()
                        {
                            "Betreff",
                            "Seite",
                            "Hinweise",
                            "Datum",
                            "Kategorien",
                            "Verantwortlich",
                            "Ort",
                            "Ressourcen",
                            "SJ"
                        }, kalender + ".csv");
                    }
                }

                Console.WriteLine("");
                Console.WriteLine("=============================================================================================================");
                Console.WriteLine("");

                var periodes = new Periodes();
                var periode = periodes.GetAktuellePeriode();
                periode = 1;
                var hzJz = (DateTime.Now.Month > 2 && DateTime.Now.Month <= 9) ? "JZ" : "HZ";

                var raums = new Raums(periode);
                var lehrers = new Lehrers(periode, raums);
                var klasses = new Klasses(periode, lehrers);

                klasses.Csv("klassen-utf8OhneBom-einmalig-vor-SJ-Beginn.csv");

                var schuelers = new Schuelers(klasses);


                Console.WriteLine("");
                Console.WriteLine("=============================================================================================================");
                Console.WriteLine("");

                schuelers.Reliabmelder("reliabmelder.txt");

                Console.WriteLine("");
                Console.WriteLine("=============================================================================================================");
                Console.WriteLine("");

                schuelers.PraktikantenToCsv(
                    "praktikanten-utf8OhneBom-einmalig-vor-SJ-Beginn.csv", 
                    new List<string>(){"BW,1","BT,1","BS,1", "BS,2", "HBG,1","HBT,1","HBW,1","GG,1","GT,1","GW,1","IFK,1"}                 
                    );

                Console.WriteLine("");
                Console.WriteLine("=============================================================================================================");
                Console.WriteLine("");

                var feriens = new Feriens();

                Console.WriteLine("");
                Console.WriteLine("=============================================================================================================");
                Console.WriteLine("");

                lehrers.LulToCsv("lul-utf8OhneBom-einmalig-vor-SJ-Beginn.csv");

                Console.WriteLine("");
                Console.WriteLine("=============================================================================================================");
                Console.WriteLine("");

                var anrechnungs = new Anrechnungs(periode, lehrers);
                anrechnungs.UntisAnrechnungsToCsv(
                    "untisanrechnungen.csv",
                    new List<int>() { 500, 510, 530, 590, 900 },    // nur diese Gründe
                    new List<int>() { 500, 510, 530, 590 },         // nur für diese Gründe Werte
                    (new List<string>() { "PLA", "BM" })            // für diese Lehrer keine Werte
                    );

                Console.WriteLine("");
                Console.WriteLine("=============================================================================================================");
                Console.WriteLine("");
                                
                var untisUnterrichts = new Unterrichts("ExportLessons", 14);
                var untisGruppen = new Gruppen("StudentgroupStudents", 14);

                Console.WriteLine("");
                Console.WriteLine("=============================================================================================================");
                Console.WriteLine("");

                var fachs = new Fachs();
                fachs.faecherCsv("faecher.csv");

                Console.WriteLine("");
                Console.WriteLine("=============================================================================================================");
                Console.WriteLine("");

                var unterrichts = new Unterrichts(periode, klasses, lehrers, fachs, raums);
                Console.WriteLine("");
                Console.WriteLine("=============================================================================================================");
                Console.WriteLine("");

                lehrers.Sprechtag(@"oeffentlich\sprechtag.txt", raums, klasses, unterrichts);

                Console.WriteLine("");
                Console.WriteLine("=============================================================================================================");
                Console.WriteLine("");

                var teams = new Teams();

                // Die Bildungsgaenge werden aus den Anrechnungen ermittelt. Bildungsgang ist, was das Wort "Bildungsgangleitung" im Text in Untis enthält und einen Link 'bildungsgaenge: ...' in der Beschr.   

                var bildungsgangs = new Bildungsgangs(unterrichts, anrechnungs, lehrers);

                teams.AddRange(new Teams(bildungsgangs));

                // Jahrgangsspezifische Klassenteams
                                
                teams.Add(new Team("versetzung:blaue_briefe", new List<string>() { "BS", "HBG", "HBT", "HBW", "FS" }, new List<int>() { 1 }, unterrichts, lehrers));
                teams.Add(new Team("abitur:start", new List<string>() { "GG", "GT", "GW" }, new List<int>() { 3 }, unterrichts, lehrers));
                teams.Add(new Team("termine:fhr:start", new List<string>() { "BS", "HBG", "HBT", "HBW", "FS", "FM" }, new List<int>() { 2 }, unterrichts, lehrers));

                // Fachschaften

                teams.Add(new Team("Fachschaft Deutsch", "Fachschaft Deutsch", "Vorsitz", ":fachschaften:deutsch_kommunikation", unterrichts.Fachschaften(lehrers, new List<string>() { "E", "E FU", "E1", "E2", "E G1", "E G2", "E L1", "E L2", "E L", "EL", "EL1", "EL2" }), anrechnungs));
                teams.Add(new Team("Fachschaft Englisch", "Fachschaft Englisch", "Vorsitz", ":fachschaften:englisch", unterrichts.Fachschaften(lehrers, new List<string>() { "E", "E FU", "E1", "E2", "E G1", "E G2", "E L1", "E L2", "E L", "EL", "EL1", "EL2" }), anrechnungs));
                teams.Add(new Team("Fachschaft Religion", "Fachschaft Religion", "Vorsitz", ":fachschaften:religionslehre", unterrichts.Fachschaften(lehrers, new List<string>() { "KR", "KR FU", "KR1", "KR2", "KR G1", "KR G2", "ER", "ER G1" }), anrechnungs));
                teams.Add(new Team("Fachschaft Mathematik", "Fachschaft Mathematik", "Vorsitz", ":fachschaften:mathematik_physik", unterrichts.Fachschaften(lehrers, new List<string>() { "M", "M FU", "M1", "M2", "M G1", "M G2", "M L1", "M L2", "M L", "ML", "ML1", "ML2" }), anrechnungs));
                teams.Add(new Team("Fachschaft Politik", "Fachschaft Politik", "Vorsitz", ":fachschaften:politik_gesellschaftslehre", unterrichts.Fachschaften(lehrers, new List<string>() { "PK", "PK FU", "PK1", "PK2", "GG G1", "GG G2" }), anrechnungs));
                teams.Add(new Team("Fachschaft Wirtschaftslehre", "Fachschaft Wirtschaftslehre", "Vorsitz", ":fachschaften:wirtschaftslehre_in_nicht_kaufm_klassen", unterrichts.Fachschaften(lehrers, new List<string>() { "WL", "WBL" }), anrechnungs));
                teams.Add(new Team("Fachschaft Sport", "Fachschaft Sport", "Vorsitz", ":fachschaften:sport", unterrichts.Fachschaften(lehrers, new List<string>() { "SP", "SP G1", "SP G2" }), anrechnungs));
                teams.Add(new Team("Fachschaft Biologie", "Fachschaft Biologie", "Vorsitz", ":fachschaften:biologie", unterrichts.Fachschaften(lehrers, new List<string>() { "BI", "Bi", "Bi FU", "Bi1", "Bi G1", "Bi G2", "BI G1", "BI L1" }), anrechnungs));
                


                teams.Add(new Team("Kollegium", "Kollegium", "Schulleiter", ":Kollegium", lehrers, anrechnungs));
                teams.Add(new Team("Bildungsgangleitungen", "Bildungsgangleitungen", "", "Bildungsgangleitung", anrechnungs.LuL(lehrers, "Bildungsgangleitung"), anrechnungs));
                teams.Add(new Team("Erweiterte Schulleitung", "Erweiterte Schulleitung", "", ":geschaeftsverteilungsplan:erweiterte_schulleitung", anrechnungs.LuL(lehrers, "Erweiterte Schulleitung"), anrechnungs));
                teams.Add(new Team("Lehrerinnen", "Lehrerinnen", "Ansprechpartnerin für Gleichstellung", ":lehrerinnen", lehrers.Lehrerinnen(), anrechnungs));
                
                teams.Add(new Team("Verbindungslehrkräfte", "SV", "", ":verbindungslehrkraefte", anrechnungs.LuL(lehrers, "Verbindungslehrkräfte"), anrechnungs));
                teams.Add(new Team("Referendare", "Referendare", "", ":referendar_innen", lehrers.Referendare(), anrechnungs));
                teams.Add(new Team("Klassenleitungen", "Klassenleitungen", "", ":geschaeftsverteilungsplan:klassenleitungen", klasses.GetKlassenleitungen(lehrers), anrechnungs));
                
                teams.GruppenUndMitgliederToCsv(@"gruppen.csv");

                Console.WriteLine("");
                Console.WriteLine("=============================================================================================================");
                Console.WriteLine("");

                //teams.DateiPraktiumErzeugen("Praktikum.txt");               

                Console.WriteLine("");
                Console.WriteLine("=============================================================================================================");
                Console.WriteLine("");

                var abwesenheiten = new Abwesenheiten("AbsencePerStudent", 7);
                var schuelerMitAbwesenheiten = schuelers.VorgängeMaßnahmenUndFehlzeitenSeitLetzterAbwesenheit(abwesenheiten, klasses, feriens);

                schuelerMitAbwesenheiten.SchulpflichtüberwachungCsvTxt(
                    "schulpflichtueberwachung.txt",
                    "schulpflichtueberwachung.csv",
                    10, // Schonfrist: Soviele Tage hat die Klassenleitung Zeit offene St. zu bearbeiten, bevor eine Warnung ausgelöst wird.
                    20, // Nach so vielen unent. Stunden ohne Maßnahme wird eine Warnung ausgelöst.
                    30, // Nach so vielen Tagen verjähren unentschuldigte Fehlstunden für Unbescholtene.
                    90, // Nach so vielen Tagen verjähren unentschuldigte Fehlstunden für SuS mit Maßnahme
                    klasses, 
                    kalenderwoche);

                Console.WriteLine("");
                Console.WriteLine("=============================================================================================================");
                Console.WriteLine("");


                Kurswahlen kurswahlen = new Kurswahlen(@"klausurbelegungsplaene.txt",
                    (from s in schuelers where s.Klasse.NameUntis.StartsWith("G") select s).ToList(),
                    unterrichts,
                    lehrers,
                    klasses,
                    untisUnterrichts,
                    untisGruppen,
                    Global.AktSj[0],
                    hzJz
                    );

                
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }

        private static int GetKalenderwoche()
        {
            // Example date
            DateTime date = DateTime.Now;

            // Define the ISO 8601 calendar
            CultureInfo ci = CultureInfo.CurrentCulture;
            Calendar calendar = ci.Calendar;

            // Define the CalendarWeekRule and the DayOfWeek for the first day of the week
            CalendarWeekRule weekRule = CalendarWeekRule.FirstFourDayWeek;
            DayOfWeek firstDayOfWeek = DayOfWeek.Monday;

            // Get the week number for the specified date
            return calendar.GetWeekOfYear(date, weekRule, firstDayOfWeek);
        }
    }
}