using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Push2Dokuwiki
{
    public class Termin
    {
        public string Betreff { get; internal set; }
        public List<string> Seite { get; set; }
        public string Hinweise { get; internal set; }
        public DateTime Datum { get; internal set; }
        public List<string> Kategorien { get; internal set; }
        public List<string> Ressourcen { get; internal set; }
        public string Verantwortlich { get; internal set; }
        public string Ort { get; internal set; }

        public string OptionaleTeilnehmer { get; internal set; }
        public string Nachricht { get; internal set; }
        public string FälligUm { get; internal set; }
        public string Beschriftung { get; internal set; }
        
        
        public DateTime EndeDatum { get; internal set; }
        public List<string> SJ { get; internal set; }

        internal void ToWikiLink(string nachrichtEnthält)
        {            
            if (!string.IsNullOrEmpty(Nachricht))
            {
                Hinweise = Nachricht.Trim();

                // Regex-Muster für Hyperlinks
                string pattern = @"http[s]?://[^\s]+";

                // Regex-Objekt erstellen
                Regex regex = new Regex(pattern);

                // Hyperlinks im Text finden
                MatchCollection matches = regex.Matches(Nachricht);

                Seite = new List<string>();

                // Gefundene Hyperlinks ausgeben
                foreach (Match match in matches)
                {   
                    if (match.Value.Contains(nachrichtEnthält))
                    {
                        Seite.Add(match.Value.Replace(nachrichtEnthält, "").TrimEnd('>'));
                        Hinweise = Hinweise.Replace(match.ToString(), "").Trim();
                    }
                }
            }
        }
    }
}