using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using static System.Net.WebRequestMethods;

namespace Push2Dokuwiki
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("      Push2Dokuwiki.exe | Published under the terms of GPLv3 | Stefan Bäumer " + DateTime.Now.Year + " | Version 20240913");
                Console.WriteLine("=".PadRight(Console.WindowWidth, '='));

                Global.ImportPfad = @"c:\users\" + Global.User + @"\Downloads";
                Global.SoAltDürfenImportDateienHöchstesSein = 6;
                var kalenderwoche = GetKalenderwoche();
                var periodes = new Periodes();
                var periode = periodes.GetAktuellePeriode();
                var hzJz = (DateTime.Now.Month > 2 && DateTime.Now.Month <= 9) ? "JZ" : "HZ";
                var feriens = new Feriens();
                var raums = new Raums(periode);
                var lehrers = new Lehrers(periode, raums);
                var klasses = new Klasses(periode, lehrers, raums);                
                var schuelers = new Schuelers(klasses);

                Console.WriteLine("=".PadRight(Console.WindowWidth, '='));
                                
                // Kalender

                var kopfzeile = "Betreff,Seite,Hinweise,Datum,Kategorien,Verantwortlich,Ort,Ressourcen,SJ";
                var kriterium = "https://bkb.wiki/";    // Nur Termine mit diesem Nachrichteinhalt kopieren
                var kalenders = new Kalenders(kriterium)
                {
                    new Kalender(kopfzeile,kriterium,"Export_aus_outlook_termine_kollegium"),
                    new Kalender(kopfzeile,kriterium,"Export_aus_outlook_termine_fhr"),
                    new Kalender(kopfzeile,kriterium,"Export_aus_outlook_termine_berufliches_gymnasium"),
                    new Kalender(kopfzeile,kriterium,"Export_aus_outlook_termine_verwaltung")   
                };

                Console.WriteLine("=".PadRight(Console.WindowWidth, '='));

                var anrechnungs = new Anrechnungs(periode, lehrers);
                anrechnungs.UntisAnrechnungsToCsv(
                    "untisanrechnungen.csv",
                    new List<int>() { 500, 510, 530, 590, 900 },    // nur diese Gründe
                    new List<int>() { 500, 510, 530, 590 },         // nur für diese Gründe Werte
                    (new List<string>() { "PLA", "BM" })            // für diese Lehrer keine Werte
                    );

                Console.WriteLine("=".PadRight(Console.WindowWidth, '='));

                // Klassenpflegschaft

                klasses.KlassenpflegschaftDatenquelle("klassenpflegschaft", schuelers, lehrers, raums, anrechnungs);

                Console.WriteLine("=".PadRight(Console.WindowWidth, '='));

                // Schulpflichtüberwachung

                var abwesenheiten = new Abwesenheiten("AbsencePerStudent");
                var schuelerMitAbwesenheiten = schuelers.GetMaßnahmenUndFehlzeiten(abwesenheiten, klasses, feriens);

                schuelerMitAbwesenheiten.SchulpflichtüberwachungTxt(
                    "schulpflichtueberwachung.txt",
                    10, // Schonfrist: Soviele Tage hat die Klassenleitung Zeit offene Stunden
                        // zu bearbeiten, bevor eine Warnung ausgelöst wird.
                    20, // Nach so vielen unent. Stunden ohne Maßnahme wird eine Warnung ausgelöst.
                    30, // Nach so vielen Tagen verjähren unentschuldigte Fehlstunden für Unbescholtene.
                    90, // Nach so vielen Tagen verjähren unentschuldigte Fehlstunden für SuS mit Maßnahme
                    klasses,
                    kalenderwoche
                    );

                



                // Klassen

                klasses.Csv("klassen-utf8OhneBom-einmalig-vor-SJ-Beginn.csv");

                Console.WriteLine("=".PadRight(Console.WindowWidth, '='));

                schuelers.Reliabmelder("reliabmelder.txt");

                Console.WriteLine("=".PadRight(Console.WindowWidth, '='));

                schuelers.PraktikantenToCsv(
                    "praktikanten-utf8OhneBom-einmalig-vor-SJ-Beginn.csv",
                    new List<string>() { "BW,1", "BT,1", "BS,1", "BS,2", "HBG,1", "HBT,1", "HBW,1", "GG,1", "GT,1", "GW,1", "IFK,1" }
                    );

                Console.WriteLine("");
                Console.WriteLine("=".PadRight(Console.WindowWidth, '='));
                Console.WriteLine("");

                

                Console.WriteLine("");
                Console.WriteLine("=".PadRight(Console.WindowWidth, '='));
                Console.WriteLine("");

                lehrers.LulToCsv("lul-utf8OhneBom-einmalig-vor-SJ-Beginn.csv");

                
                Console.WriteLine("");
                Console.WriteLine("=".PadRight(Console.WindowWidth, '='));
                Console.WriteLine("");

                var untisUnterrichts = new Unterrichts("ExportLessons");
                var untisGruppen = new Gruppen("StudentgroupStudents");

                Console.WriteLine("");
                Console.WriteLine("=".PadRight(Console.WindowWidth, '='));
                Console.WriteLine("");

                var fachs = new Fachs();
                fachs.faecherCsv("faecher.csv");

                Console.WriteLine("");
                Console.WriteLine("=".PadRight(Console.WindowWidth, '='));
                Console.WriteLine("");

                var unterrichts = new Unterrichts(periode, klasses, lehrers, fachs, raums);
                Console.WriteLine("");
                Console.WriteLine("=".PadRight(Console.WindowWidth, '='));
                Console.WriteLine("");

                lehrers.Sprechtag(@"oeffentlich\sprechtag.txt", raums, klasses, unterrichts);

                Console.WriteLine("");
                Console.WriteLine("=".PadRight(Console.WindowWidth, '='));
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
                Console.WriteLine("=".PadRight(Console.WindowWidth, '='));
                Console.WriteLine("");

                //teams.DateiPraktiumErzeugen("Praktikum.txt");               

                Console.WriteLine("");
                Console.WriteLine("=".PadRight(Console.WindowWidth, '='));
                Console.WriteLine("");

                
                Console.WriteLine("");
                Console.WriteLine("=".PadRight(Console.WindowWidth, '='));
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