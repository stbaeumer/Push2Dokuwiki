using System.Globalization;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Push2Dokuwiki
{
    public class Abwesenheiten : List<Abwesenheit>
    {
        public Abwesenheiten()
        {
        }

        public Abwesenheiten(string kriterium)
        {
            string datei = "";

            try
            {
                datei = Global.CheckFile(kriterium);  
                
                int zeile = 1;
                using (var reader = new StreamReader(datei))
                {
                    reader.ReadLine();

                    while (!reader.EndOfStream)
                    {
                        Abwesenheit abwesenheit = new Abwesenheit();
                        string fehler = "";

                        try
                        {
                            var line = reader.ReadLine();
                            var values = line.Split('\t');

                            if (values.Count() != 16)
                            {
                                fehler = values.Count().ToString();
                            }
                            abwesenheit.Name = values[0];
                            abwesenheit.StudentId = Convert.ToInt32(Convert.ToString(values[1]));
                            abwesenheit.Klasse = (values[3] != null) ? Convert.ToString(values[3]) : "";
                            abwesenheit.Datum = DateTime.ParseExact(values[4], "dd.MM.yy", CultureInfo.InvariantCulture);
                            abwesenheit.Fehlstunden = (values[6] != null && values[6] != "") ? Convert.ToInt32(values[6]) : 0;
                            abwesenheit.Fehlminuten = (values[7] != null && values[7] != "") ? Convert.ToInt32(values[7]) : 0;
                            abwesenheit.GanzerFehlTag = (values[15] != null && values[15] != "") ? Convert.ToInt32(values[15]) : 0;
                            abwesenheit.Grund = (values[8] != null && values[8] != "") ? Convert.ToString(values[8]) : ""; // krank, krank
                            //abwesenheit.Text = (values[9] != null && values[9] != "") ? Convert.ToString(values[9]) : "";
                            abwesenheit.Status = (values[14] != null && values[14] != "") ? Convert.ToString(values[14]) : ""; // entsch. 

                            if (
                                    abwesenheit.Status == "offen" ||
                                    abwesenheit.Status == "nicht entsch.")
                            {
                                if (abwesenheit.Fehlstunden <= 9) // maximal 9 Fehlstunden am Tag werden gezählt. Klassenfahrten würden sonst 24 Stunden zählen.
                                {
                                    this.Add(abwesenheit);
                                }
                            }
                            zeile++;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(" Die Zeile " + zeile + " müsste 16 Spalten haben, hat aber " + fehler + ". Die Zeile wird ignoriert.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
                
            }
            finally 
            {   
                Global.WriteLine("Abwesenheiten ......... " + datei.Substring((datei.LastIndexOf("\\")) + 1), this.Count);
                ÄltesteAbwesenheit = (from a in this.OrderBy(x => x.Datum) select a.Datum).FirstOrDefault();
                Global.WriteLine(" Abwesenheiten werden berücksichtigt ab:", ÄltesteAbwesenheit.ToShortDateString());
            }
        }

        public DateTime ÄltesteAbwesenheit { get; private set; }
    }
}