using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Push2Dokuwiki
{
    class Program
    {
        public static string User = System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToUpper().Split('\\')[1];
        public static List<string> AktSj = new List<string>
                {
                    (DateTime.Now.Month >= 8 ? DateTime.Now.Year : DateTime.Now.Year - 1).ToString(),
                    (DateTime.Now.Month >= 8 ? DateTime.Now.Year + 1 - 2000 : DateTime.Now.Year - 2000).ToString()
                };

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("      Push2Dokuwiki.exe | Published under the terms of GPLv3 | Stefan Bäumer " + DateTime.Now.Year + " | Version 20231221");
                Console.WriteLine("=============================================================================================================");

                Periodes periodes = new Periodes();


                var periode = (from p in periodes where p.Bis >= DateTime.Now.Date where DateTime.Now.Date >= p.Von select p.IdUntis).FirstOrDefault();
                var aktJahr = DateTime.Now.Month > 7 ? DateTime.Now.Year - 2000 : DateTime.Now.Year - 1 - 2000;
                var hzJz = (DateTime.Now.Month > 2 && DateTime.Now.Month <= 9) ? "JZ" : "HZ";

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

                string sourceExportLessons = CheckFile(User, "ExportLessons", DateTime.Now.Date.AddDays(-20));
                string sourceStudentgroupStudents = CheckFile(User, "StudentgroupStudents", DateTime.Now.Date.AddDays(-20));
                var untisUnterrichts = new Unterrichts(sourceExportLessons);
                var untisGruppen = new Gruppen(sourceStudentgroupStudents);

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

                lehrers.DateiSprechtagErzeugen(
                    raums,
                    unterrichts,
                    @"\\sql01\DokuWiki\DOKUWIKI\data\pages\oeffentlich\sprechtag.txt",
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + @"\\sprechtag.txt",
                    klasses
                    );

                Schuelers vollzeitSuS = new Schuelers();
                vollzeitSuS.AddRange(from s in schuelers
                                     where (s.Klasse.StartsWith("G") ||
                                     s.Klasse.StartsWith("F") ||
                                     s.Klasse.StartsWith("BS") ||
                                     s.Klasse.StartsWith("HBW") ||
                                     s.Klasse.StartsWith("HBT") ||
                                     s.Klasse.StartsWith("HBG"))
                                     select s);

                //vollzeitSuS.Notenlisten(@"\\sql01\Dokuwiki\DOKUWIKI\data\pages\Notenlisten\",
                //    System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + @"\Notenlisten\",
                //    unterrichts,
                //    lehrers,
                //    klasses
                //    );


                Kurswahlen kurswahlen = new Kurswahlen(
                    @"\\sql01\Dokuwiki\DOKUWIKI\data\pages\berufliches_gymnasium\klausurbelegungsplaene.txt",
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + @"\\klausurbelegungsplaene.txt",
                    (from s in schuelers where s.Klasse.StartsWith("G") select s).ToList(),
                    unterrichts,
                    lehrers,
                    klasses,
                    untisUnterrichts, 
                    untisGruppen,
                    aktJahr,
                    hzJz
                    );

                schuelers.Reliabmelder(
                   @"\\sql01\Dokuwiki\DOKUWIKI\data\pages\religion_abgemeldete.txt",
                   System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + @"\\religion_abgemeldete.txt"
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

        private static string CheckFile(string user, string kriterium, DateTime zeitpunkt)
        {
            var sourceFile = (from f in Directory.GetFiles(@"c:\users\" + user + @"\Downloads", "*.csv", SearchOption.AllDirectories) where f.Contains(kriterium) orderby File.GetLastWriteTime(f) select f).LastOrDefault();

            if ((sourceFile == null))
            {
                Console.WriteLine("");
                Console.WriteLine(" Die " + kriterium + "<...>.csv" + (sourceFile == null ? " existiert nicht im Download-Ordner" : " im Download-Ordner ist nicht von heute. \n Es werden keine Daten aus der Datei importiert") + ".");
                Console.WriteLine(" Exportieren Sie die Datei frisch aus Webuntis, indem Sie als Administrator:");

                if (kriterium.Contains("Student_"))
                {
                    Console.WriteLine("   1. Stammdaten > Schülerinnen");
                    Console.WriteLine("   2. \"Berichte\" auswählen");
                    Console.WriteLine("   3. Bei \"Schüler\" auf CSV klicken");
                    Console.WriteLine("   4. Die Datei \"Student_<...>.CSV\" im Download-Ordner zu speichern");
                    Console.WriteLine(" ");
                    Console.WriteLine(" ENTER beendet das Programm.");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
                else
                {
                    if (kriterium.Contains("MarksPerLesson"))
                    {
                        Console.WriteLine("   1. Klassenbuch > Berichte klicken");
                        Console.WriteLine("   2. Alle Klassen auswählen und ggfs. den Zeitraum einschränken");
                        Console.WriteLine("   3. Unter \"Noten\" die Prüfungsart (-Alle-) auswählen");
                        Console.WriteLine("   4. Unter \"Noten\" den Haken bei Notennamen ausgeben _NICHT_ setzen");
                        Console.WriteLine("   5. Hinter \"Noten pro Schüler\" auf CSV klicken");
                        Console.WriteLine("   6. Die Datei \"MarksPerLesson<...>.CSV\" im Download-Ordner zu speichern");
                        Console.WriteLine(" ");
                        Console.WriteLine(" ENTER beendet das Programm.");
                        Console.ReadKey();
                        Environment.Exit(0);
                    }
                    else
                    {
                        Console.WriteLine("   1. Administration > Export klicken");
                        Console.WriteLine("   2. Zeitraum begrenzen, also die Woche der Zeugniskonferenz und vergange Abschnitte herauslassen");
                        Console.WriteLine("   2. Das CSV-Icon hinter Gesamtfehlzeiten klicken");
                    }

                    if (kriterium.Contains("AbsenceTimesTotal"))
                    {
                        Console.WriteLine("   4. Die Gesamtfehlzeiten (\"AbsenceTimesTotal<...>.CSV\") im Download-Ordner zu speichern");
                        Console.WriteLine("WICHTIG: Es kann Sinn machen nur Abwesenheiten bis zur letzten Woche in Webuntis auszuwählen.");
                    }

                    if (kriterium.Contains("StudentgroupStudents"))
                    {
                        Console.WriteLine("   4. Die Schülergruppen  (\"StudentgroupStudents<...>.CSV\") im Download-Ordner zu speichern");
                    }

                    if (kriterium.Contains("ExportLessons"))
                    {
                        Console.WriteLine("   4. Die Unterrichte (\"ExportLessons<...>.CSV\") im Download-Ordner zu speichern");
                    }
                }

                Console.WriteLine(" ");
                sourceFile = null;
            }

            if (sourceFile != null)
            {
                Console.WriteLine((Path.GetFileName(sourceFile) + " ").PadRight(73, '.') + ". Erstell-/Bearbeitungszeitpunkt heute um " + System.IO.File.GetLastWriteTime(sourceFile).ToShortTimeString());
            }

            return sourceFile;
        }
    }
}