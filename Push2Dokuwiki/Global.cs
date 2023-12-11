using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;

namespace Push2Dokuwiki
{
    public static class Global
    {
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
            Console.WriteLine((v + " " + ".".PadRight(count / 150, '.')).PadRight(93, '.') + (" " + count).ToString().PadLeft(6), '.');
        }

        internal static void WriteLine(string v, string go)
        {
            Console.WriteLine((v + " " + ".".PadRight(100 / 150, '.')).PadRight(93, '.') + go.PadLeft(6), '.');
        }

        internal static void DateiTauschen(string dokuwikiPfadUndDatei, string dateiNeu)
        {
            if (File.Exists(dokuwikiPfadUndDatei) && File.Exists(dateiNeu))
            {
                // Lese den Inhalt der Dateien
                string contentAlt = File.ReadAllText(dokuwikiPfadUndDatei);
                string contentNeu = File.ReadAllText(dateiNeu);

                // Vergleiche die Inhalte der Dateien
                if (contentAlt != contentNeu)
                {
                    // Überschreibe alt mit dem Inhalt von neu
                    File.WriteAllText(dokuwikiPfadUndDatei, contentNeu);
                    Console.WriteLine(dokuwikiPfadUndDatei + " wurde mit " + dateiNeu + " überschrieben.");
                }
                else
                {
                    Console.WriteLine(dokuwikiPfadUndDatei + " und " + dateiNeu + " sind identisch. Keine Änderungen vorgenommen.");
                }
            }
        }
    }
}