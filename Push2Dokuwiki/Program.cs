using System;
using System.Linq;

namespace Push2Dokuwiki
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("      Push2Dokuwiki.exe | Published under the terms of GPLv3 | Stefan Bäumer " + DateTime.Now.Year + " | Version 20231211");
                Console.WriteLine("=============================================================================================================");

                Periodes periodes = new Periodes();
                var periode = (from p in periodes where p.Bis >= DateTime.Now.Date where DateTime.Now.Date >= p.Von select p.IdUntis).FirstOrDefault();
                var aktJahr = DateTime.Now.Month > 7 ? DateTime.Now.Year - 2000 : DateTime.Now.Year - 1 - 2000;

                Raums raums = new Raums(periode);
                Lehrers lehrers = new Lehrers(periode, raums, aktJahr);
                Klasses klasses = new Klasses(periode, lehrers);
                Anrechnungs anrechnungs = new Anrechnungs(periode);                
                Schuelers schuelers = new Schuelers(klasses);                
                Fachs fachs = new Fachs();                
                Unterrichtsgruppes unterrichtsgruppes = new Unterrichtsgruppes();
                Unterrichts unterrichts = new Unterrichts(periode, klasses, lehrers, fachs, raums, unterrichtsgruppes);
                Bildungsgangs bildungsgangs = new Bildungsgangs(periode, lehrers, anrechnungs, unterrichts, fachs);
                Fachschaften fachschaften = new Fachschaften(fachs);

                Teams teams = new Teams();                
                teams.AddRange(new Teams(klasses, lehrers, schuelers, unterrichts, "Bildungsgang"));
                teams.AddRange(new Teams(klasses, lehrers, schuelers, unterrichts, "Klasse"));

                teams.AddRange(new Teams(bildungsgangs));
                teams.Add(new Team("Fachschaft Englisch", "Fachschaft Englisch", "Vorsitz", ":fachschaften:deutsch_kommunikation", unterrichts.Fachschaften(lehrers, "E"), anrechnungs));
                teams.Add(new Team("Fachschaft Religion", "Fachschaft Religion", "Vorsitz", ":fachschaften:religionslehre", unterrichts.Fachschaften(lehrers, "REL"), anrechnungs));
                teams.Add(new Team("Fachschaft Mathematik", "Fachschaft Mathematik", "Vorsitz", ":fachschaften:mathematik_physik", unterrichts.Fachschaften(lehrers, "M"), anrechnungs));
                teams.Add(new Team("Fachschaft Politik", "Fachschaft Politik", "Vorsitz", ":fachschaften:politik_gesellschaftslehre", unterrichts.Fachschaften(lehrers, "PK"), anrechnungs));
                teams.Add(new Team("Fachschaft Wirtschaftslehre", "Fachschaft Wirtschaftslehre", "Vorsitz", ":fachschaften:wirtschaftslehre_in_nicht_kaufmaennischen_klassen", unterrichts.Fachschaften(lehrers, "WL"), anrechnungs));
                teams.Add(new Team("Fachschaft Sport", "Fachschaft Sport", "Vorsitz", ":fachschaften:sport", unterrichts.Fachschaften(lehrers, "SP"), anrechnungs));
                teams.Add(new Team("Fachschaft Biologie", "Fachschaft Biologie", "Vorsitz", ":fachschaften:biologie", unterrichts.Fachschaften(lehrers, "Bi"), anrechnungs));
                teams.Add(new Team("Kollegium", "Kollegium", "Schulleiter", ":Kollegium", lehrers, anrechnungs));
                teams.Add(new Team("Bildungsgangleitungen", "Bildungsgangleitungen", "", "Bildungsgangleitung", anrechnungs.LuL(lehrers, "Bildungsgangleitung"), anrechnungs));
                teams.Add(new Team("Erweiterte Schulleitung", "Erweiterte Schulleitung", "", "Erweiterte Schulleitung", anrechnungs.LuL(lehrers, "Erweiterte Schulleitung"), anrechnungs));
                teams.Add(new Team("Lehrerinnen", "Lehrerinnen", "Ansprechpartnerin für Gleichstellung", ":ansprechpartnerin_fuer_gleichstellung", lehrers.Lehrerinnen(), anrechnungs));
                teams.Add(new Team("Berufliches Gymnasium", "G", "Bereichsleitung", ":berufliches_gymnasium:start", unterrichts.Abitur(lehrers), anrechnungs));
                teams.Add(new Team("Verbindungslehrkräfte", "SV", "", ":verbindungslehrkraefte", anrechnungs.LuL(lehrers, "Verbindungslehrkräfte"), anrechnungs));
                teams.Add(new Team("Referendare", "Referendare", "", ":referendar_innen", lehrers.Referendare(), anrechnungs));
                teams.Add(new Team("Klassenleitungen", "Klassenleitungen", "", ":geschaeftsverteilungsplan:klassenleitungen", klasses.GetKlassenleitungen(lehrers), anrechnungs));
                teams.Add(new Team("FHR", "FHR", "Bereichsleitung", ":fhr", unterrichts.Fhr(lehrers), anrechnungs));
                                
                // Täglicher Abgleich:

                schuelers.Reliabmelder(
                    @"\\sql01\Dokuwiki\DOKUWIKI\data\pages\religion_abgemeldete.txt",
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + @"\\religion_abgemeldete.txt"
                    );

                Kurswahlen kurswahlen = new Kurswahlen(
                    @"\\sql01\Dokuwiki\DOKUWIKI\data\pages\berufliches_gymnasium\klausurbelegungsplaene.txt",
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + @"\\klausurbelegungsplaene.txt",
                    (from s in schuelers where s.Klasse.StartsWith("G") where !s.Klasse.Contains(aktJahr.ToString()) select s).ToList()
                    );

                teams.DateiGruppenUndMitgliederErzeugen(
                    @"\\sql01\Dokuwiki\DOKUWIKI\data\pages\kollegium\gruppen.txt",
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + @"\\gruppen.txt"
                    );

                lehrers.DateiKollegiumErzeugen(
                    @"\\sql01\Dokuwiki\DOKUWIKI\data\pages\kollegium.txt",
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + @"\\kollegium.txt",
                    unterrichts, 
                    klasses, 
                    fachschaften, 
                    bildungsgangs
                    );

                lehrers.DateiAnrechnungenErzeugen(
                    @"\\sql01\DokuWiki\DOKUWIKI\data\pages\sl\anrechnungen.txt",
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + @"\\anrechnungen.txt",
                    klasses
                    );
                  
                teams.DateiPraktiumErzeugen();
                
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