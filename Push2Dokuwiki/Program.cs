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

                var abwesenheiten = new Abwesenheiten("AbsencePerStudent", DateTime.Now.Date.AddDays(-20));
                var untisUnterrichts = new Unterrichts("ExportLessons", DateTime.Now.Date.AddDays(-20));
                var untisGruppen = new Gruppen("StudentgroupStudents", DateTime.Now.Date.AddDays(-20));

                var raums = new Raums(periode);
                var feriens = new Feriens();

                var lehrers = new Lehrers(periode, raums);
                var klasses = new Klasses(periode, lehrers);

                lehrers.Anrechnungen(@"sl\anrechnungen.txt", klasses);

                var fachs = new Fachs();
                var unterrichts = new Unterrichts(periode, klasses, lehrers, fachs, raums);

                lehrers.Sprechtag(@"oeffentlich\sprechtag.txt", raums, klasses, unterrichts);

                var schuelers = new Schuelers(klasses);

                schuelers.Reliabmelder("religion_abgemeldete.txt");
                var schuelerMitAbwesenheiten = schuelers.MitAbwesenheiten(abwesenheiten, klasses, feriens);

                schuelerMitAbwesenheiten.Schulpflicht(@"sl\schulpflichtueberwachung.txt", klasses);

                var anrechnungs = new Anrechnungs(periode);
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