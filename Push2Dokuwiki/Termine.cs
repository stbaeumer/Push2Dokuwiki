using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Push2Dokuwiki
{
    public class Termine : List<Termin>
    {
        public Termine()
        {
        }

        public Termine(string kriterium, DateTime dateTime)
        {
            string datei = "";

            try
            {
                datei = Global.CheckFile(kriterium, dateTime);

                if (datei == null) { return; }
                int zeile = 1;
                using (var reader = new StreamReader(datei))
                {
                    var kopfzeile = reader.ReadLine();
                    var values = new List<string>();
                    values.AddRange(kopfzeile.Split('\t'));
                    int anzahlSpalten = 0;

                    int betreff = -1;
                    int beginn = -1;
                    int ende = -1;
                    int kategorien = -1;
                    int nachricht = -1;
                    int fälligUm = -1;
                    int ressourcen = -1;
                    int optionaleTeilnehmer = -1;
                    int beschriftung = -1;
                    int ort = -1;

                    for (int i = 0; i < values.Count; i++)
                    {
                        if (values[i] == "Betreff")
                        {
                            betreff = i;
                            anzahlSpalten++;
                        }
                        if (values[i] == "Beginn")
                        {
                            beginn = i;
                            anzahlSpalten++;
                        }
                        if (values[i] == "Ende")
                        {
                            ende = i;
                            anzahlSpalten++;
                        }
                        if (values[i] == "Kategorien")
                        {
                            kategorien = i;
                            anzahlSpalten++;
                        }
                        if (values[i] == "Ressourcen")
                        {
                            ressourcen = i;
                            anzahlSpalten++;
                        }
                        if (values[i] == "Ort")
                        {
                            ort = i;
                            anzahlSpalten++;
                        }
                        if (values[i] == "Optionale Teilnehmer")
                        {
                            optionaleTeilnehmer = i;
                            anzahlSpalten++;
                        }
                        if (values[i] == "Nachricht")
                        {
                            nachricht = i;
                            anzahlSpalten++;
                        }
                        if (values[i] == "Fällig um")
                        {
                            fälligUm = i;
                            anzahlSpalten++;
                        }
                        if (values[i] == "Beschriftung")
                        {
                            beschriftung = i;
                            anzahlSpalten++;
                        }
                        if (values[i] == "Ganztägig")
                        {
                            beschriftung = i;
                            anzahlSpalten++;
                        }                       
                    }

                    while (!reader.EndOfStream)
                    {
                        Termin termin = new Termin();

                        try
                        {
                            var line = reader.ReadLine();
                            values = new List<string>();
                            values.AddRange(line.Split('\t'));

                            if (beginn >= 0)
                            {
                                try
                                {
                                    string format = "ddd dd.MM.yyyy HH:mm";
                                    CultureInfo provider = new CultureInfo("de-DE");
                                    // Wenn sich die erste Spalte nicht in ein Datum parsen lässt,
                                    // dann ist das vermutlich die umgebrochene Nachricht der Zeile zuvor.
                                    termin.Datum = DateTime.ParseExact(values[beginn], format, provider);                                    

                                    if (betreff >= 0)
                                    {
                                        termin.Betreff = values[betreff];                                        
                                    }
                                    if (ende >= 0)
                                    {
                                        var e = DateTime.ParseExact(values[ende], format, provider);
                                        if (e.Year <= 1)
                                        {
                                            termin.EndeDatum = termin.Datum;                                            
                                        }
                                        else
                                        {
                                            termin.EndeDatum = DateTime.ParseExact(values[ende], format, provider);                                            
                                        }
                                        termin.SJ = new List<string>();

                                        if (new DateTime(Convert.ToInt32(Global.AktSj[0]), 8, 1) < termin.Datum && termin.Datum < new DateTime(Convert.ToInt32(Global.AktSj[1]), 7, 31))
                                        {
                                            termin.SJ.Add("aktuelles");
                                        }
                                        if (new DateTime(Convert.ToInt32(Global.AktSj[1]), 8, 1) < termin.Datum && termin.Datum < new DateTime(Convert.ToInt32(Global.AktSj[1]) +1 , 7, 31))
                                        {
                                            termin.SJ.Add("kommendes");
                                        }
                                        if (new DateTime(Convert.ToInt32(Global.AktSj[0]) - 1, 8, 1) < termin.Datum && termin.Datum < new DateTime(Convert.ToInt32(Global.AktSj[1]) - 1, 7, 31))
                                        {
                                            termin.SJ.Add("vorheriges");
                                        }                                        
                                    }
                                    if (kategorien >= 0)
                                    {
                                        termin.Kategorien = new List<string>();
                                        if (values[kategorien] != null && values[kategorien] != "")
                                        {
                                            termin.Kategorien.AddRange((values[kategorien]).Split(';'));
                                        }
                                    }
                                    if (ressourcen >= 0)
                                    {
                                        termin.Ressourcen = new List<string>();
                                        if (values[ressourcen] != null && values[ressourcen] != "")
                                        {
                                            termin.Ressourcen.AddRange((values[ressourcen]).Split(';'));
                                        }
                                    }
                                    if (ort >= 0)
                                    {
                                        termin.Ort = (values[ort] != null) ? Convert.ToString(values[ort]) : "";
                                    }
                                    if (optionaleTeilnehmer >= 0)
                                    {
                                        termin.OptionaleTeilnehmer = (values[optionaleTeilnehmer] != null) ? Convert.ToString(values[optionaleTeilnehmer]) : "";
                                    }
                                    if (nachricht >= 0)
                                    {
                                        termin.Nachricht = (values[nachricht] != null) ? Convert.ToString(values[nachricht]) : "";
                                    }
                                    if (fälligUm >= 0)
                                    {
                                        termin.FälligUm = (values[fälligUm] != null) ? Convert.ToString(values[fälligUm]) : "";
                                    }
                                    if (beschriftung >= 0)
                                    {
                                        termin.Beschriftung = (values[beschriftung] != null && values[beschriftung] != "") ? Convert.ToString(values[beschriftung]) : "";
                                    }                                    
                                    this.Add(termin);
                                    zeile++;
                                }
                                catch (Exception)
                                {
                                    // Wenn die Anzahl der Einträge in der Zeile kleiner ist als
                                    // die Anzahl der Spalten, wird die Nachricht der vorherigen Zeile verlängert

                                    this[this.Count - 1].Nachricht += " " + values[0];
                                }                                
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }

                foreach (var t in this)
                {
                    t.ToWikiLink();
                }

                Global.WriteLine("Termine ........." + datei.Substring((datei.LastIndexOf("\\")) + 1), this.Count);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string SJ { get; private set; }

        internal void ToCsv(List<string> kopfzeilen, string v)
        {
            var filePath = Global.Dateipfad + v;
                        
            Type type = this[0].GetType();

            string kopfzeile = "";

            foreach (var item in kopfzeilen)
            {
                kopfzeile += "\"" + item + "\",";
            }

            PropertyInfo[] properties = type.GetProperties();

            // Properties in der gleichen Reihenfolge wie kopfzeilen sortieren
            Array.Sort(properties, (x, y) =>
            {
                int xIndex = kopfzeilen.IndexOf(x.Name);
                int yIndex = kopfzeilen.IndexOf(y.Name);
                return xIndex.CompareTo(yIndex);
            });

            File.WriteAllText(filePath, kopfzeile.TrimEnd(',') + Environment.NewLine);

            foreach (var t in this.OrderBy(x => x.Datum))
            {
                // Früheste Termine: die in dieses Schuljahr fallen.

                if (t.Datum > new DateTime(Convert.ToInt32(Global.AktSj[0]), 8, 1) && (t.Datum > new DateTime(2024, 06, 1) || t.Betreff.Contains("eweglich")))
                {
                    // Jüngste Termine: Termine bis Ende des kommenden SJ

                    if (t.Datum < new DateTime(Convert.ToInt32(Global.AktSj[1]) + 1, 7, 31))
                    {
                        CultureInfo deCulture = new CultureInfo("de-DE");
                        string zeile = "";

                        foreach (PropertyInfo property in properties)
                        {
                            if (kopfzeilen.Contains(property.Name))
                            {
                                if (property.PropertyType == typeof(DateTime))
                                {
                                    DateTime dateTimeValue = (DateTime)property.GetValue(t);

                                    if (t.Datum.Year > 1)
                                    {
                                        string datum = "\"" + t.Datum.ToString("ddd dd.MM.yyyy", deCulture);

                                        if (t.Datum.Hour != 0)
                                        {
                                            datum += ", " + t.Datum.ToShortTimeString();
                                        }

                                        // Bei Nicht-ganztägigen Ereignissen

                                        if (t.Datum.AddDays(1) != t.EndeDatum)
                                        {
                                            datum += " - ";

                                            if (t.Datum.Date != t.EndeDatum.Date)
                                            {                                                
                                                datum += t.EndeDatum.AddDays(-1).ToString("ddd dd.MM.yyyy", deCulture);
                                            }
                                            
                                            if (t.EndeDatum.Hour != 0)
                                            {
                                                datum += t.EndeDatum.ToShortTimeString();
                                            }

                                            if (t.Datum.Hour != 0 || t.EndeDatum.Hour != 0)
                                            {
                                                datum += " Uhr";
                                            }
                                        }
                                        zeile += datum + "\",";
                                    }
                                }
                                else
                                {
                                    if (property.PropertyType == typeof(List<string>) && ((List<string>)property.GetValue(t) != null))
                                    {
                                        zeile += "\"" + Global.ListeErzeugen((List<string>)property.GetValue(t), ',') + "\",";
                                    }
                                    else
                                    {
                                        zeile += "\"" + property.GetValue(t) + "\",";
                                    }
                                }
                            }
                        }
                        File.AppendAllText(filePath, zeile.TrimEnd(',') + Environment.NewLine);
                    }
                }
            }
            Global.WriteLine("                        " + filePath, "erstellt");
        }
    }
}