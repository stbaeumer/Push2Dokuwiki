using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Push2Dokuwiki
{
    public class Termine : List<Termin>
    {
        public Termine()
        {
        }

        public Termine(string datei, string nachrichtEnthält)
        {            
            int zeile = 1;
            var termine = new Termine();

            try
            {
                if (datei == null) { return; }

                using (var reader = new StreamReader(datei, true))
                {
                    var kopfzeile = reader.ReadLine();

                    if (kopfzeile == null)
                    { return; }
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
                        var line = reader.ReadLine();
                        zeile++;

                        try
                        {
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
                                        if (new DateTime(Convert.ToInt32(Global.AktSj[1]), 8, 1) < termin.Datum && termin.Datum < new DateTime(Convert.ToInt32(Global.AktSj[1]) + 1, 7, 31))
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

                                    termine.Add(termin);

                                }
                                catch (Exception)
                                {
                                    // Wenn die Anzahl der Einträge in der Zeile kleiner ist als
                                    // die Anzahl der Spalten, wird die Nachricht der vorherigen Zeile verlängert

                                    if (values[0] != "")
                                    {
                                        termine[termine.Count - 1].Nachricht += " " + values[0];
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Fehler in Zeile " + zeile + "\\" + ex.ToString());
                            throw ex;
                        }
                    }
                }

                foreach (var t in termine)
                {
                    t.ToWikiLink(nachrichtEnthält);
                }

                foreach (var termin in termine)
                {
                    if (termin.Nachricht != null && termin.Nachricht.Contains(nachrichtEnthält))
                    {
                        this.Add(termin);
                    }
                }

                Console.WriteLine(Environment.NewLine);
                Console.WriteLine("Es werden nur Termine exportiert, die im Nachrichtentext den Ausdruck: *"+ nachrichtEnthält + "* enthalten.");
                Global.WriteLine("Termine ........." + datei.Substring((datei.LastIndexOf("\\")) + 1), this.Count);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string SJ { get; private set; }

        
    }
}