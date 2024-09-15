using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Push2Dokuwiki
{
    public class Kalender
    {
        public string Importdatei { get; private set; }
        public Termine Termine { get; }

        public Kalender(
            string kopfzeile,
            string nurTermineMitDiesemStringInDerNachrichtWerdenAusgewertet,            
            string nameDerImportCsvDatei)
        {
            Importdatei = Global.CheckFile(nameDerImportCsvDatei);
            Termine = new Termine(Importdatei,nurTermineMitDiesemStringInDerNachrichtWerdenAusgewertet);
            ToCsv(kopfzeile, nameDerImportCsvDatei + ".csv");
        }

        private void ToCsv(string kopfzeile, string nameDerImportCsvDatei)
        {
            UTF8Encoding utf8NoBom = new UTF8Encoding(false);

            var filePath = Global.Dateipfad + nameDerImportCsvDatei;

            Type type = this.Termine[0].GetType();

            PropertyInfo[] properties = type.GetProperties();

            // Properties in der gleichen Reihenfolge wie kopfzeilen sortieren
            Array.Sort(properties, (x, y) =>
            {
                int xIndex = kopfzeile.Split(',').ToList().IndexOf(x.Name);
                int yIndex = kopfzeile.Split(',').ToList().IndexOf(y.Name);
                return xIndex.CompareTo(yIndex);
            });

            File.WriteAllText(filePath, kopfzeile + Environment.NewLine, utf8NoBom);

            foreach (var t in Termine.OrderBy(x => x.Datum))
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
                            if (kopfzeile.Contains(property.Name))
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
                        File.AppendAllText(filePath, zeile.TrimEnd(',') + Environment.NewLine, utf8NoBom);
                    }
                }
            }
            Global.WriteLine("                        " + filePath, "erstellt");
        }
    }
}