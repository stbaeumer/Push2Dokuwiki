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
                Console.WriteLine("      Push2Dokuwiki.exe | Published under the terms of GPLv3 | Stefan Bäumer " + DateTime.Now.Year + " | Version 20240918");

                TrennerEinfügen();

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
                var anrechnungs = new Anrechnungs(periode, lehrers);
                var abwesenheiten = new Abwesenheiten("AbsencePerStudent");
                var untisUnterrichts = new Unterrichts("ExportLessons");
                var untisGruppen = new Gruppen("StudentgroupStudents");
                var fachs = new Fachs();

                TrennerEinfügen();

                // Kalender

                var kopfzeile = "Betreff,Seite,Hinweise,Datum,Kategorien,Verantwortlich,Ort,Ressourcen,SJ";
                var kriterium = "https://bkb.wiki/";    // Nur Termine mit diesem Nachrichteinhalt kopieren
                var kalenders = new Kalenders(kriterium)
                {
                    new Kalender(kopfzeile,kriterium,@"CSV\termine_kollegium.csv"),
                    new Kalender(kopfzeile,kriterium,@"CSV\termine_fhr.csv"),
                    new Kalender(kopfzeile,kriterium,@"CSV\termine_berufliches_gymnasium.csv"),
                    new Kalender(kopfzeile,kriterium,@"CSV\termine_verwaltung.csv")
                };

                TrennerEinfügen();

                anrechnungs.UntisAnrechnungsToCsv(
                    @"CSV\untisanrechnungen.csv",
                    new List<int>() { 500, 510, 530, 590, 900 },    // nur diese Gründe
                    new List<int>() { 500, 510, 530, 590 },         // nur für diese Gründe Werte
                    (new List<string>() { "PLA", "BM" })            // für diese Lehrer keine Werte
                    );

                TrennerEinfügen();

                klasses.Klassenpflegschaft(@"schulmitwirkung\klassenpflegschaft\räume", schuelers, lehrers, raums, anrechnungs);

                TrennerEinfügen();

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

                TrennerEinfügen();

                klasses.Csv(@"CSV\klassen-utf8OhneBom-einmalig-vor-SJ-Beginn.csv");

                TrennerEinfügen();

                schuelers.Reliabmelder(@"religion_abgemeldete.txt");

                TrennerEinfügen();

                schuelers.PraktikantenCsv(
                    @"CSV\praktikanten-utf8OhneBom-einmalig-vor-SJ-Beginn.csv",
                    new List<string>() { "BW,1", "BT,1", "BS,1", "BS,2", "HBG,1", "HBT,1", "HBW,1", "GG,1", "GT,1", "GW,1", "IFK,1" }
                    );

                TrennerEinfügen();

                lehrers.Csv(@"CSV\lul-utf8OhneBom-einmalig-vor-SJ-Beginn.csv");

                TrennerEinfügen();

                fachs.Csv(@"CSV\faecher.csv");

                TrennerEinfügen();

                var unterrichts = new Unterrichts(periode, klasses, lehrers, fachs, raums);

                TrennerEinfügen();

                var alleLehrerImUnterricht = new Lehrers();
                // Hier weitere TN an Sprechtag einbauen:
                //alleLehrerImUnterricht.Add(new Lehrer("Kessens", "", "w", "Landwirtschaftskammer NRW", "3307"));
                //alleLehrerImUnterricht.Add(new Lehrer("Wenz", "Dr.", "w", "Landwirtschaftskammer NRW", "3301"));
                //alleLehrerImUnterricht.Add(new Lehrer("Plaßmann", "", "m", "Schulleiter, bitte im Schulbüro melden.", "1014"));

                lehrers.Sprechtag(
                    @"oeffentlich\sprechtag.txt",
                    raums,
                    klasses,
                    unterrichts,
                    alleLehrerImUnterricht,
                    "Zum jährlichen Sprechtag laden wir sehr herzlich am Mittwoch nach der Zeugnisausgabe in der Zeit von 13:30 bis 17:30 Uhr ein. Der Unterricht endet nach der 5. Stunde um 12:00 Uhr."
                    );

                TrennerEinfügen();

                Kurswahlen kurswahlen = new Kurswahlen(@"oeffentlich\klausurbelegungsplaene.txt",
                    (from s in schuelers where s.Klasse.NameUntis.StartsWith("G") select s).ToList(),
                    unterrichts,
                    lehrers,
                    klasses,
                    untisUnterrichts,
                    untisGruppen,
                    hzJz
                    );

                TrennerEinfügen();

                var teams = new Teams(klasses, unterrichts, anrechnungs, lehrers);
                teams.GruppenUndMitgliederToCsv(@"CSV\gruppen.csv");

                TrennerEinfügen();

                //Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }

        private static void TrennerEinfügen()
        {
            Console.WriteLine("");
            Console.WriteLine("=".PadRight(Console.WindowWidth, '='));
            Console.WriteLine("");
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