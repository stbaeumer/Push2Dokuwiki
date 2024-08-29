using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Push2Dokuwiki
{
    public static class Global
    {
        public static string User = System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToUpper().Split('\\')[1];

        public static string Dateipfad = @"\\fs01\Wiki\push2dokuwiki-seiten\";
        public static string DateipfadNeu = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "\\";

        public const string ConnectionStringUntis = @"Data Source=SQL01\UNTIS;Initial Catalog=master;Integrated Security=True";
        public const string ConnectionStringAtlantis = @"Dsn=Atlantis17u;uid=DBA";

        public static List<string> AktSj = new List<string>() {
            (DateTime.Now.Month >= 7 ? DateTime.Now.Year : DateTime.Now.Year - 1).ToString(),
            (DateTime.Now.Month >= 7 ? DateTime.Now.Year + 1 : DateTime.Now.Year).ToString()
        };

        internal static string Anrechnungen()
        {
            return @"
# Im Folgenden werden Verteilergruppen gepflegt, die sich aus den Untis-Anrechnungen ergeben. Dabei gilt:
# * Zu jeder Anrechnung in Untis kann ein Text und eine Beschreibung definiert werden.
# * Sobald eine Beschreibung bei 2 oder mehr LuL zum Einsatz kommt, wird eine Verteilergruppe gebildet, in der alle LuL mit dieser Beschreibung Member sind. 
# 
";
        }

        public static string SafeGetString(SqlDataReader reader, int colIndex)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetString(colIndex);
            return string.Empty;
        }

        public static string SafeGetString(OleDbDataReader reader, int colIndex)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetString(colIndex);
            return string.Empty;
        }

        internal static string GruppenAuslesen(int anzahlTeamsIst)
        {
            return @"

  
    
    Write-Host '|'
    Write-Host '| Da die GruppenOwnerMember.csv heute zuletzt aktualisiert wurde, wird ein Abgleich aller Gruppen mit Ownern und Membern gemacht ...'
    Write-Host '|'

";
        }

        internal static string Auth()
        {
            return @"
$testSession = Get-PSSession
if(-not($testSession))
{
    Write-Warning '$targetComputer : Sie sind Nicht angemeldet.'
    $cred = Get-Credential
    $session = New-PSSession -ConfigurationName Microsoft.Exchange -ConnectionUri https://outlook.office365.com/powershell-liveid/ -Credential $cred -Authentication Basic -AllowRedirection
    Import-PSSession $session
    Connect-AzureAD -Credential $cred
    Connect-MicrosoftTeams -Credential $cred
}
else
{
    Write-Host '$targetComputer: Sie sind angemeldet.'    
}

";


        }

        internal static string GruppenLesen()
        {
            return
    @"     
# Da die GruppenOwnerMember.csv nicht von heute ist, wird sie nun erstellt. ...
# Anschließend muss Teams.exe erneut gestartet werden.    

Write-Host -ForegroundColor Green 'Alle Office 365-Gruppen werden geladen ...'
$Groups = Get-UnifiedGroup -ResultSize Unlimited  | Sort-Object DisplayName

$GroupsCSV = @()

$results = foreach ($Group in $Groups)
{
    Write-Host -ForegroundColor Magenta 'Hole alle  Owner der Gruppe ' $Group.DisplayName  '('$Group.Identity')' ...
    $Owners = Get-UnifiedGroupLinks -Identity $Group.Identity -LinkType Owners -ResultSize Unlimited
    
    foreach ($Owner in $Owners)
    {         
            [pscustomobject]@{
            GroupId = $Group.Identity
            GroupDisplayName = $Group.DisplayName
            User = $Owner.PrimarySmtpAddress
            Role = 'Owner'
            Type = 'O365'
        }
    }
 
    Write-Host -ForegroundColor Magenta 'Hole alle Member der Gruppe ' $Group.DisplayName  '('$Group.Identity')' ...
    $Members = Get-UnifiedGroupLinks -Identity $Group.Identity -LinkType Members -ResultSize Unlimited
    $MembersSMTP=@()
    
    foreach ($Member in $Members)
    {
        [pscustomobject]@{
            GroupId = $Group.Identity
            GroupDisplayName = $Group.DisplayName
            User = $Member.PrimarySmtpAddress
            Role = 'Member'
            Type = 'O365'
        }        
    }        
}

Write-Host -ForegroundColor Green 'Alle Verteilergruppen werden geladen'
$Groups = Get-DistributionGroup -ResultSize Unlimited | Sort-Object DisplayName

$resultsV = foreach ($Group in $Groups)
{
    Write-Host -ForegroundColor Magenta 'Hole alle Member der Verteilergruppe ' $Group.DisplayName  '('$Group.Identity')' ...
    $Members = Get-DistributionGroupMember -Identity $Group.Identity -ResultSize Unlimited
    $MembersSMTP=@()
    
    foreach ($Member in $Members)
    {
        [pscustomobject]@{
            GroupId = $Group.Identity
            GroupDisplayName = $Group.DisplayName
            User = $Member.PrimarySmtpAddress
            Role = 'Member'
            Type = 'Distribution'
        }        
    }        
}

$results = $results + $resultsV

# Export to CSV
Write-Host -ForegroundColor Green 'GruppenOwnerMembers.csv wird geschrieben. Nun kann Teams.exe erneut gestartet werden.'
$results | Export-Csv -NoTypeInformation -Path C:\users\bm\Documents\GruppenOwnerMembers.csv -Encoding UTF8 -Delimiter '|'
start notepad++ C:\users\bm\Documents\GruppenOwnerMembers.csv    
# start-process -FilePath 'U:\Source\Repos\teams\teams\bin\Debug\teams.exe'

";
        }

        internal static void WriteLine(string v, int count)
        {
            Console.WriteLine((v + " " + ".".PadRight(count / 170, '.')).PadRight(105, '.') + (" " + count).ToString().PadLeft(6), '.');
        }

        internal static void WriteLine(string v, string go)
        {
            Console.WriteLine(((v + " " + ".").PadRight(111, '.')).Substring(0, 111 - go.Length - 3) + "   " + go);
        }

        internal static void Dateischreiben(string name, string datei, string dateiTemp)
        {
            UTF8Encoding utf8NoBom = new UTF8Encoding(false);

            string contentNeu = File.ReadAllText(dateiTemp, utf8NoBom);

            if (File.Exists(datei) && File.Exists(dateiTemp))
            {
                // Lese den Inhalt der Dateien
                string contentAlt = File.ReadAllText(datei, utf8NoBom);

                // Vergleiche die Inhalte der Dateien
                if (contentAlt != contentNeu)
                {
                    // Überschreibe alt mit dem Inhalt von neu
                    File.WriteAllText(datei, contentNeu, utf8NoBom);
                    Console.WriteLine(" " + datei.Substring((datei.LastIndexOf("\\")) + 1) + ": " + datei + " überschrieben.");
                }
                else
                {
                    Global.WriteLine(" " + name, datei.Substring((datei.LastIndexOf("\\")) + 1) + ": Alte und neue Datei sind identisch. Keine Änderungen vorgenommen.");
                }
            }
            if (!File.Exists(datei))
            {
                File.WriteAllText(datei, contentNeu, utf8NoBom);
                Global.WriteLine(" " + name, datei.Substring((datei.LastIndexOf("\\")) + 1) + ": Datei neu erstellt.");
            }
        }

        public static string InsertLineBreaks(string text, int maxLineLength)
        {
            if (string.IsNullOrEmpty(text) || maxLineLength <= 0)
                return text;

            int currentIndex = 0;
            int length = text.Length;
            System.Text.StringBuilder result = new System.Text.StringBuilder();

            while (currentIndex < length)
            {
                // Calculate the length of the next segment
                int nextSegmentLength = Math.Min(maxLineLength, length - currentIndex);
                // Append the segment and a line break
                result.Append(text.Substring(currentIndex, nextSegmentLength));
                result.Append(Environment.NewLine);
                // Move to the next segment
                currentIndex += nextSegmentLength;
            }

            return result.ToString();
        }

        public static string CheckFile(string kriterium, int sovieleTageDarfDieDateiMaxAltSein)
        {
            var sourceFile = (from f in Directory.GetFiles(@"c:\users\" + Global.User + @"\Downloads", "*.csv", SearchOption.AllDirectories) where f.Contains(kriterium) orderby File.GetLastWriteTime(f) select f).LastOrDefault();

            if (
                sourceFile == null || 
                new FileInfo(sourceFile).LastWriteTime.Date < DateTime.Now.Date.AddDays(-(sovieleTageDarfDieDateiMaxAltSein)) ||
                new FileInfo(sourceFile).Length == 0
                )
            {
                if (sourceFile == null)
                {
                    Console.WriteLine("Die Datei " + kriterium + ".csv existiert nicht.");                    
                }
                else
                {
                    if (new FileInfo(sourceFile).LastWriteTime.Date < DateTime.Now.Date.AddDays(-(sovieleTageDarfDieDateiMaxAltSein)))
                    {
                        Global.WriteLine("Die Datei " + sourceFile + " existiert, ist aber älter als ", sovieleTageDarfDieDateiMaxAltSein + " " + (sovieleTageDarfDieDateiMaxAltSein > 1 ? "Tage" : "Tag"));
                    }
                    else
                    {
                        Global.WriteLine("Die Datei " + sourceFile + " existiert, ist aber leer",0);
                    }
                }
                               

                if (kriterium.Contains("termine"))
                {
                    Console.WriteLine("  Exportieren Sie Export_aus_Outlook_" + kriterium + ".csv frisch aus Outlook:");
                    Console.WriteLine("  1. Ansicht in Outlook auf Liste ändern.");
                    Console.WriteLine("  2. *Beginn* muss in der ersten Spalte stehen.");
                    Console.WriteLine("  3. Alle Listeneinträge markieren");
                    Console.WriteLine("  4. Zwischenablage in Export_aus_Outlook_" + kriterium + ".csv fallenlassen.");
                    
                    if (!File.Exists(@"c:\users\" + Global.User + @"\Downloads\Export_aus_Outlook_" + kriterium + ".csv"))
                    {
                        File.Create(@"c:\users\" + Global.User + @"\Downloads\Export_aus_Outlook_" + kriterium + ".csv").Close();
                        Console.WriteLine("Die Export_aus_Outlook_" + kriterium + ".csv wurde leer angelegt.");
                    }
                    Process.Start(@"c:\users\" + Global.User + @"\Downloads\Export_aus_Outlook_" + kriterium + ".csv");                    
                }
                else
                {
                    if (kriterium.Contains("Student_"))
                    {
                        Console.WriteLine("  Exportieren Sie die Datei frisch aus Webuntis, indem Sie als Administrator:");
                        Console.WriteLine("   1. Stammdaten > Schülerinnen");
                        Console.WriteLine("   2. \"Berichte\" auswählen");
                        Console.WriteLine("   3. Bei \"Schüler\" auf CSV klicken");
                        Console.WriteLine("   4. Die Datei \"Student_<...>.CSV\" im Download-Ordner zu speichern");
                        Console.WriteLine(" ");
                        Console.WriteLine(" ENTER beendet das Programm.");
                        Console.ReadKey();
                        Environment.Exit(0);
                    }

                    if (kriterium.Contains("MarksPerLesson") || kriterium.Contains("AbsencePerStudent"))
                    {
                        Console.WriteLine("  Exportieren Sie die Datei frisch aus Webuntis, indem Sie als Administrator:");
                        Console.WriteLine("   1. Klassenbuch > Berichte klicken");

                        if (kriterium.Contains("MarksPerLesson"))
                        {
                            Console.WriteLine("   2. Alle Klassen auswählen und ggfs. den Zeitraum einschränken");
                            Console.WriteLine("   3. Unter \"Noten\" die Prüfungsart (-Alle-) auswählen");
                            Console.WriteLine("   4. Unter \"Noten\" den Haken bei Notennamen ausgeben _NICHT_ setzen");
                            Console.WriteLine("   5. Hinter \"Noten pro Schüler\" auf CSV klicken");
                            Console.WriteLine("   6. Die Datei \"MarksPerLesson<...>.CSV\" im Download-Ordner zu speichern");
                        }
                        else
                        {
                            Console.WriteLine("   2. Alle Klassen auswählen und als Zeitraum am besten die letzen vier Wochen wählen.");
                            Console.WriteLine("   3. Unter \"Abwesenheiten\" Fehlzeiten pro Schüler*in auswählen");
                            Console.WriteLine("   4. \"pro Tag\" ");
                            Console.WriteLine("   5. Auf CSV klicken");
                        }

                        Console.WriteLine(" ");
                        Console.WriteLine(" ENTER beendet das Programm.");
                        Console.ReadKey();
                        Environment.Exit(0);
                    }
                    else
                    {
                        if (kriterium.Contains("MarksPerLesson") || kriterium.Contains("AbsencePerStudent"))
                        {
                            Console.WriteLine("  Exportieren Sie die Datei frisch aus Webuntis, indem Sie als Administrator:");
                            Console.WriteLine("   1. Klassenbuch > Berichte klicken");

                            if (kriterium.Contains("MarksPerLesson"))
                            {
                                Console.WriteLine("   2. Alle Klassen auswählen und ggfs. den Zeitraum einschränken");
                                Console.WriteLine("   3. Unter \"Noten\" die Prüfungsart (-Alle-) auswählen");
                                Console.WriteLine("   4. Unter \"Noten\" den Haken bei Notennamen ausgeben _NICHT_ setzen");
                                Console.WriteLine("   5. Hinter \"Noten pro Schüler\" auf CSV klicken");
                                Console.WriteLine("   6. Die Datei \"MarksPerLesson<...>.CSV\" im Download-Ordner zu speichern");
                            }
                            else
                            {
                                Console.WriteLine("   2. Alle Klassen auswählen und als Zeitraum am besten die letzen vier Wochen wählen.");
                                Console.WriteLine("   3. Unter \"Abwesenheiten\" Fehlzeiten pro Schüler*in auswählen");
                                Console.WriteLine("   4. \"pro Tag\" ");
                                Console.WriteLine("   5. Auf CSV klicken");
                            }

                            Console.WriteLine(" ");
                            Console.WriteLine(" ENTER beendet das Programm.");
                            Console.ReadKey();
                            Environment.Exit(0);
                        }
                        else
                        {
                            Console.WriteLine("  Exportieren Sie die Datei frisch aus Webuntis, indem Sie als Administrator:");
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
                }

                return null;
            }

            return sourceFile;
        }

        internal static string ListeErzeugen(List<string> kategorien, char delimiter)
        {
            var x = "";

            foreach (var item in kategorien)
            {
                x += item.Trim() + delimiter;
            }
            return x.TrimEnd(delimiter);
        }
    }
}