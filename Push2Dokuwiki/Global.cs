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
        public static string TempPfad = System.IO.Path.GetTempPath();

        public const string ConnectionStringUntis = @"Data Source=SQL01\UNTIS;Initial Catalog=master;Integrated Security=True";
        public const string ConnectionStringAtlantis = @"Dsn=Atlantis17u;uid=DBA";

        public static List<string> AktSj = new List<string>() {
            (DateTime.Now.Month >= 7 ? DateTime.Now.Year : DateTime.Now.Year - 1).ToString(),
            (DateTime.Now.Month >= 7 ? DateTime.Now.Year + 1 : DateTime.Now.Year).ToString()
        };

        public static string ImportPfad { get; internal set; }
        public static int SoAltDürfenImportDateienHöchstesSein { get; internal set; }
        

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
            Console.WriteLine(((v + " " + ".").PadRight(130, '.')).Substring(0, 130 - Math.Min(105,go.Length) - 3) + "   " + go);
        }

        internal static void Dateischreiben(string name)
        {
            UTF8Encoding utf8NoBom = new UTF8Encoding(false);

            if (File.Exists(Global.Dateipfad + name) && File.Exists(Global.TempPfad + name))
            {
                string contentNeu = File.ReadAllText(Global.TempPfad + name, utf8NoBom);

                // Lese den Inhalt der Dateien
                string contentAlt = File.ReadAllText(Global.Dateipfad + name, utf8NoBom);

                // Vergleiche die Inhalte der Dateien
                if (contentAlt != contentNeu)
                {
                    // Überschreibe alt mit dem Inhalt von neu
                    File.WriteAllText(Global.Dateipfad + name, contentNeu, utf8NoBom);
                    Console.WriteLine("     " + name + ": überschrieben.");
                }
                else
                {
                    Console.WriteLine("     " + name + ": Alte und neue Datei sind identisch. Keine Änderungen vorgenommen.");
                }
            }
            if (!File.Exists(Global.Dateipfad + name))
            {
                string contentNeu = File.ReadAllText(Global.TempPfad + name, utf8NoBom);
                File.WriteAllText(Global.Dateipfad + name, contentNeu, utf8NoBom);
                Global.WriteLine(" " + name, (Global.Dateipfad + name).Substring(((Global.Dateipfad + name).LastIndexOf("\\")) + 1) + ": Datei neu erstellt.");
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
                result.Append(Environment.NewLine + "   ");                
                // Move to the next segment
                currentIndex += nextSegmentLength;
            }

            return result.ToString();
        }

        public static string CheckFile(string kriterium)
        {
            var sourceFile = (from f in Directory.GetFiles(Global.ImportPfad, "*.csv", SearchOption.AllDirectories) where f.Contains(Path.GetFileName(kriterium)) orderby File.GetLastWriteTime(f) select f).LastOrDefault();

            if (sourceFile == null)
            {
                File.Create(Global.ImportPfad + @"\" + kriterium + ".csv").Close();
                Console.WriteLine("Die Datei " + Global.ImportPfad + @"\" + kriterium + ".csv wurde jetzt angelegt. Bitte füllen:");
                Hinweis(Global.ImportPfad + @"\" + kriterium + ".csv");
            }
            else
            {
                if (new FileInfo(sourceFile).LastWriteTime.Date < DateTime.Now.Date.AddDays(-(Global.SoAltDürfenImportDateienHöchstesSein)))
                {
                    Console.WriteLine("Die Datei " + sourceFile + "ist älter als " + Global.SoAltDürfenImportDateienHöchstesSein + ". ");
                    Hinweis(Global.ImportPfad + @"\" + kriterium + ".csv");
                }
                if (new FileInfo(sourceFile).Length == 0)
                {
                    Console.WriteLine("Die Datei " + sourceFile + " ist leer. Bitte füllen");
                    Hinweis(Global.ImportPfad + @"\" + kriterium + ".csv");
                }                
            }

            return sourceFile;
        }

        private static void Hinweis(string sourceFile)
        {
            if (sourceFile.Contains("ermine"))
            {
                Console.WriteLine("  1. Ansicht in Outlook auf Liste ändern.");
                Console.WriteLine("  2. *Beginn* muss in der ersten Spalte stehen.");
                Console.WriteLine("  3. Alle Listeneinträge markieren");
                Console.WriteLine("  4. Zwischenablage in " + sourceFile + " fallenlassen.");
            }

            if (sourceFile.Contains("Student_"))
            {
                Console.WriteLine("  Exportieren Sie die Datei frisch aus Webuntis, indem Sie als Administrator:");
                Console.WriteLine("   1. Stammdaten > Schülerinnen");
                Console.WriteLine("   2. \"Berichte\" auswählen");
                Console.WriteLine("   3. Bei \"Schüler\" auf CSV klicken");
                Console.WriteLine("   4. Die Datei \"Student_<...>.CSV\" im Download-Ordner zu speichern");
            }

            if (sourceFile.Contains("MarksPerLesson"))
            {
                Console.WriteLine("   1. Klassenbuch > Berichte klicken");
                Console.WriteLine("   2. Alle Klassen auswählen und ggfs. den Zeitraum einschränken");
                Console.WriteLine("   3. Unter \"Noten\" die Prüfungsart (-Alle-) auswählen");
                Console.WriteLine("   4. Unter \"Noten\" den Haken bei Notennamen ausgeben _NICHT_ setzen");
                Console.WriteLine("   5. Hinter \"Noten pro Schüler\" auf CSV klicken");
                Console.WriteLine("   6. Die Datei \"MarksPerLesson<...>.CSV\" im Download-Ordner zu speichern");
            }

            if (sourceFile.Contains("AbsencePerStudent"))
            {
                Console.WriteLine("  Exportieren Sie die Datei frisch aus Webuntis, indem Sie als Administrator:");
                Console.WriteLine("   1. Klassenbuch > Berichte klicken");
                Console.WriteLine("   2. Alle Klassen auswählen und als Zeitraum am besten die letzen vier Wochen wählen.");
                Console.WriteLine("   3. Unter \"Abwesenheiten\" Fehlzeiten pro Schüler*in auswählen");
                Console.WriteLine("   4. \"pro Tag\" ");
                Console.WriteLine("   5. Auf CSV klicken");
            }

            if (sourceFile.Contains("AbsenceTimesTotal"))
            {
                Console.WriteLine("  Exportieren Sie die Datei frisch aus Webuntis, indem Sie als Administrator:");
                Console.WriteLine("   1. Administration > Export klicken");
                Console.WriteLine("   2. Zeitraum begrenzen, also die Woche der Zeugniskonferenz und vergange Abschnitte herauslassen");
                Console.WriteLine("   3. Das CSV-Icon hinter Gesamtfehlzeiten klicken");
                Console.WriteLine("   4. Die Gesamtfehlzeiten (\"AbsenceTimesTotal<...>.CSV\") im Download-Ordner zu speichern");
                Console.WriteLine("WICHTIG: Es kann Sinn machen nur Abwesenheiten bis zur letzten Woche in Webuntis auszuwählen.");
            }

            if (sourceFile.Contains("StudentgroupStudents"))
            {
                Console.WriteLine("  Exportieren Sie die Datei frisch aus Webuntis, indem Sie als Administrator:");
                Console.WriteLine("   1. Administration > Export klicken");
                Console.WriteLine("   2. Zeitraum begrenzen, also die Woche der Zeugniskonferenz und vergange Abschnitte herauslassen");
                Console.WriteLine("   3. Das CSV-Icon hinter Schülergruppen klicken");
                Console.WriteLine("   4. Die Schülergruppen  (\"StudentgroupStudents<...>.CSV\") im Download-Ordner zu speichern");
            }

            if (sourceFile.Contains("ExportLessons"))
            {
                Console.WriteLine("  Exportieren Sie die Datei frisch aus Webuntis, indem Sie als Administrator:");
                Console.WriteLine("   1. Administration > Export klicken");
                Console.WriteLine("   2. Zeitraum begrenzen, also die Woche der Zeugniskonferenz und vergange Abschnitte herauslassen");
                Console.WriteLine("   3. Das CSV-Icon hinter Unterricht klicken");
                Console.WriteLine("   4. Die Unterrichte (\"ExportLessons<...>.CSV\") im Download-Ordner zu speichern");
            }
            Console.ReadKey();
            Environment.Exit(0);
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

        internal static void OrdnerAnlegen(string datei)
        {
            string temp = Path.GetDirectoryName(Global.TempPfad + datei);
            
            if (!Directory.Exists(temp))
            {
                Directory.CreateDirectory(temp);
                Console.WriteLine($"Verzeichnis erstellt: {temp}");
            }

            string verzeichnis = Path.GetDirectoryName(Global.Dateipfad + datei);

            if (!Directory.Exists(verzeichnis))
            {
                Directory.CreateDirectory(verzeichnis);
                Console.WriteLine($"Verzeichnis erstellt: {verzeichnis}");
            }
        }

        internal static void OrdnerAnlegen(object name)
        {
            throw new NotImplementedException();
        }
    }
}