using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Push2Dokuwiki
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("      Push2Dokuwiki.exe | Published under the terms of GPLv3 | Stefan Bäumer " + DateTime.Now.Year + " | Version 20240528");
                Console.WriteLine("=============================================================================================================");

                var periodes = new Periodes();
                var periode = periodes.GetAktuellePeriode();
                var hzJz = (DateTime.Now.Month > 2 && DateTime.Now.Month <= 9) ? "JZ" : "HZ";

                foreach (var kalender in new List<string>() {
                    "termine_kollegium",                // Diese Kalender werden ausgewertet
                    "termine_fhr", 
                    "termine_berufliches_gymnasium", 
                    "termine_verwaltung" 
                })
                {
                    Termine termine = new Termine();
                    termine.AddRange(new Termine(kalender, DateTime.Today));

                    if (termine.Count > 0)
                    {
                        // Die iteressierenden Spalten aus Dokuwiki werden hier festgelegt
                        termine.ToCsv(new List<string>() { "Betreff", "Seite", "Hinweise", "Datum", "Kategorien", "Verantwortlich", "Ort", "Ressourcen", "SJ" }, kalender + ".csv");
                    }
                }

                var raums = new Raums(periode);
                var feriens = new Feriens();

                var lehrers = new Lehrers(periode, raums);
                lehrers.LulCsv("lul.csv");

                var anrechnungs = new Anrechnungs(periode, lehrers);
                anrechnungs.UntisAnrechnungsCsv(
                    "untisanrechnungen.csv",
                    new List<int>() { 500, 510, 530, 590 },         // nur diese Gründe
                    new List<int>() { 500, 510, 530, 590 },         // nur für diese Gründe Werte
                    (new List<string>() { "PLA", "BM" })            // für diese Lehrer keine Werte
                    );

                var abwesenheiten = new Abwesenheiten("AbsencePerStudent", DateTime.Now.Date.AddDays(-20));
                var untisUnterrichts = new Unterrichts("ExportLessons", DateTime.Now.Date.AddDays(-20));
                var untisGruppen = new Gruppen("StudentgroupStudents", DateTime.Now.Date.AddDays(-20));
                var klasses = new Klasses(periode, lehrers);
                var fachs = new Fachs();
                fachs.faecherCsv("faecher.csv");

                var unterrichts = new Unterrichts(periode, klasses, lehrers, fachs, raums);

                lehrers.Sprechtag(@"oeffentlich\sprechtag.txt", raums, klasses, unterrichts);

                var schuelers = new Schuelers(klasses);
                schuelers.PraktikantenCsv("praktikanten.csv", new List<string>(){
                    "BW,1","BT,1","BS,1", "BS,2", "HBG,1","HBT,1","HBW,1","GG,1","GT,1","GW,1","IFK,1"});

                schuelers.Reliabmelder("religion_abgemeldete.txt");
                var schuelerMitAbwesenheiten = schuelers.VorgängeMaßnahmenUndFehlzeitenSeitLetzterAbwesenheit(abwesenheiten, klasses, feriens);

                schuelerMitAbwesenheiten.SchulpflichtüberwachungCsvTxt(
                    @"sl\schulpflichtueberwachung.txt", 
                    "schulpflichtueberwachung.csv", 
                    14, // Soviele Tage hat die Klassenleitung Zeit offene St. zu bearbeiten, bevor eine Warnung ausgelöst wird.
                    20, // Nach so vielen unent. Stunden ohne Maßnahme wird eine Warnung ausgelöst.
                    8, // Nach so vielen unent. Stunden in den letzten 14 Tagen ohne Mahnung wird eine Warnung ausgelöst
                    klasses);

                var bildungsgangs = new Bildungsgangs(periode, lehrers, anrechnungs, unterrichts, fachs);
                var fachschaften = new Fachschaften(fachs);
                lehrers.DateiKollegiumErzeugen("kollegium.txt", unterrichts, klasses, fachschaften, bildungsgangs);

                var teams = new Teams(bildungsgangs, lehrers, unterrichts, anrechnungs, klasses);
                teams.DateiPraktiumErzeugen("Praktikum.txt");
                teams.DateiGruppenUndMitgliederErzeugen(@"kollegium\gruppen.txt");

                Kurswahlen kurswahlen = new Kurswahlen(@"berufliches_gymnasium\klausurbelegungsplaene.txt",
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
    }
}