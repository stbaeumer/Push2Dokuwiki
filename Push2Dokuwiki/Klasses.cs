using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace Push2Dokuwiki
{
    public class Klasses : List<Klasse>
    {
        public List<Lehrer> Klassenleitungen { get; private set; }

        public Klasses(int periode, Lehrers lehrers, Raums raums)
        {
            using (SqlConnection odbcConnection = new SqlConnection(Global.ConnectionStringUntis))
            {
                try
                {
                    string queryString = @"SELECT DISTINCT 
Class.Class_ID, 
Class.Name,
Class.TeacherIds,
Class.Longname,
Teacher.Name, 
Class.ClassLevel,
Class.PERIODS_TABLE_ID,
Class.ROOM_ID,
Class.Text
FROM Class LEFT JOIN Teacher ON Class.TEACHER_ID = Teacher.TEACHER_ID WHERE (((Class.SCHOOLYEAR_ID)=" + Global.AktSj[0] + Global.AktSj[1] + ") AND (((Class.TERM_ID)=" + periode + ")) AND ((Teacher.SCHOOLYEAR_ID)=" + Global.AktSj[0] + Global.AktSj[1] + ") AND ((Teacher.TERM_ID)=" + periode + ")) OR (((Class.SCHOOLYEAR_ID)=" + Global.AktSj[0] + Global.AktSj[1] + ") AND ((Class.TERM_ID)=" + periode + ") AND ((Class.SCHOOL_ID)=177659) AND ((Teacher.SCHOOLYEAR_ID) Is Null) AND ((Teacher.TERM_ID) Is Null)) ORDER BY Class.Name ASC;";

                    SqlCommand odbcCommand = new SqlCommand(queryString, odbcConnection);
                    odbcConnection.Open();
                    SqlDataReader sqlDataReader = odbcCommand.ExecuteReader();

                    while (sqlDataReader.Read())
                    {
                        List<Lehrer> klassenleitungen = new List<Lehrer>();

                        foreach (var klassenleitungIdUntis in (Global.SafeGetString(sqlDataReader, 2)).Split(','))
                        {
                            var klassenleitung = (from l in lehrers
                                                  where l.IdUntis.ToString() == klassenleitungIdUntis
                                                  where l.Mail != null
                                                  where l.Mail != "" // Wer keine Mail hat, kann nicht Klassenleitung sein.
                                                  select l).FirstOrDefault();

                            if (klassenleitung != null)
                            {
                                klassenleitungen.Add(klassenleitung);
                            }
                        }

                        bool istVollzeit = IstVollzeitKlasse(Global.SafeGetString(sqlDataReader, 1));

                        Klasse klasse = new Klasse();

                        klasse.IdUntis = sqlDataReader.GetInt32(0);
                        klasse.NameUntis = Global.SafeGetString(sqlDataReader, 1);
                        klasse.BildungsgangLangname = Global.SafeGetString(sqlDataReader, 3);
                        klasse.Klassenleitungen = klassenleitungen;
                        klasse.IstVollzeit = istVollzeit;
                        klasse.Stufe = Global.SafeGetString(sqlDataReader, 5); // BS-35J-01
                        klasse.WikiLink = Global.SafeGetString(sqlDataReader, 8);
                        klasse.Raum = (from r in raums where r.IdUntis == sqlDataReader.GetInt32(7) select r.Raumnummer).FirstOrDefault();

                        if (klasse.BildungsgangLangname.Contains("("))
                        {
                            klasse.BildungsgangGekürzt = klasse.BildungsgangLangname.Substring(0, klasse.BildungsgangLangname.IndexOf('(')).Trim();
                        }
                        else
                        {
                            klasse.BildungsgangGekürzt = klasse.BildungsgangLangname.Trim();
                        }

                        this.Add(klasse);
                    };

                    sqlDataReader.Close();

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    throw new Exception(ex.ToString());
                }
                finally
                {
                    odbcConnection.Close();
                    Global.WriteLine("Klassen", this.Count);
                }
            }
        }

        public Klasses()
        {
        }

        internal Lehrers GetKlassenleitungen(Lehrers lehrers)
        {
            var x = new Lehrers();

            foreach (var item in this)
            {
                foreach (var kl in item.Klassenleitungen)
                {
                    if (!(from xx in x where xx.Mail == kl.Mail select kl).Any())
                    {
                        x.Add(kl);
                    }
                }
            }
            return x;
        }

        private bool IstVollzeitKlasse(string klassenname)
        {
            var vollzeitBeginn = new List<string>() { "BS", "BW", "BT", "FM", "FS", "G", "HB" };

            foreach (var item in vollzeitBeginn)
            {
                if (klassenname.StartsWith(item))
                {
                    return true;
                }
            }
            return false;
        }

        internal List<string> KlassenleitungenBlaueBriefe(int aktJahr)
        {
            var members = new List<string>();

            foreach (var klasse in this)
            {
                if (klasse.IstVollzeit && klasse.NameUntis.Contains(aktJahr.ToString()) && !klasse.NameUntis.StartsWith("F"))
                {
                    foreach (var klassenleitung in klasse.Klassenleitungen)
                    {
                        if (!members.Contains(klassenleitung.Mail))
                        {
                            members.Add(klassenleitung.Mail);
                        }
                    }
                }
            }
            return members;
        }

        internal Klasses MitAbwesenheiten(Abwesenheiten abwesenheiten)
        {
            var klassenMitAbwesenheiten = new Klasses();

            foreach (var klasse in this)
            {
                if ((from a in abwesenheiten where a.Klasse == klasse.NameUntis select a).Any())
                {
                    klasse.Abwesenheiten = new Abwesenheiten();
                    klasse.Abwesenheiten.AddRange((from a in abwesenheiten
                                                   where a.Klasse == klasse.NameUntis
                                                   select a).ToList());
                    klassenMitAbwesenheiten.Add(klasse);
                }
            }
            Console.WriteLine(("Klassen mit Abwesenheiten" + ".".PadRight(klassenMitAbwesenheiten.Count / 150, '.')).PadRight(48, '.') + (" " + klassenMitAbwesenheiten.Count).ToString().PadLeft(4), '.');

            return klassenMitAbwesenheiten;
        }

        internal void Csv(string datei)
        {
            UTF8Encoding utf8NoBom = new UTF8Encoding(false);            

            File.WriteAllText(Global.TempPfad + datei, "\"Name\",\"Klassenleitung\",\"Klassensprecher\",\"Klassensprecher2\"" + Environment.NewLine, utf8NoBom);

            foreach (var l in this.OrderBy(x => x.NameUntis))
            {
                File.AppendAllText(Global.TempPfad + datei, "\"" + l.NameUntis + "\",\"" + string.Join(",", (from le in l.Klassenleitungen select le.Kürzel).ToList()) + "\",\"" + "" + "\",\"\"" + Environment.NewLine, utf8NoBom);
            }
            Global.Dateischreiben(datei);
        }

        internal void Klassenpflegschaft(string dateiname, Schuelers schuelers, Lehrers lehrers, Raums raums, Anrechnungs untisanrechnungs)
        {            
            List<string> vergebeneRäume = new List<string>();
            List<string> mehrfachVergebeneRäume = new List<string>();
            Global.OrdnerAnlegen(dateiname);

            var zeilen = new List<string>();
            
            File.WriteAllText(Global.TempPfad + dateiname + ".txt", "====== Klassenpflegschaft ======" + Environment.NewLine, Encoding.UTF8);
            
            File.WriteAllText(Global.TempPfad + dateiname + "-datenquelle.csv", "\"Klasse\",\"Klassenleitung\",\"Bildungsgang\",\"Raum\",\"Anzahl\"" + Environment.NewLine, Encoding.UTF8);

            File.AppendAllText(Global.TempPfad + dateiname + ".txt", "" + Environment.NewLine, Encoding.UTF8);
            File.AppendAllText(Global.TempPfad + dateiname + ".txt", "" + Environment.NewLine, Encoding.UTF8);
            File.AppendAllText(Global.TempPfad + dateiname + ".txt", "" + Environment.NewLine, Encoding.UTF8);
            File.AppendAllText(Global.TempPfad + dateiname + ".txt", "==== Wir begrüßen alle Eltern sehr herzlich zur Klassenpflegschaft am: ====" + Environment.NewLine, Encoding.UTF8);
            
            File.AppendAllText(Global.TempPfad + dateiname + ".txt", "---- struct global ----" + Environment.NewLine, Encoding.UTF8);
            File.AppendAllText(Global.TempPfad + dateiname + ".txt", "schema: termine_kollegium" + Environment.NewLine, Encoding.UTF8);
            File.AppendAllText(Global.TempPfad + dateiname + ".txt", "cols:Datum" + Environment.NewLine, Encoding.UTF8);
            File.AppendAllText(Global.TempPfad + dateiname + ".txt", "head:Datum/Uhrzeit" + Environment.NewLine, Encoding.UTF8);
            File.AppendAllText(Global.TempPfad + dateiname + ".txt", "filterand: Seite~*lassenpflegschaft*" + Environment.NewLine, Encoding.UTF8);
            File.AppendAllText(Global.TempPfad + dateiname + ".txt", "dynfilters: 0" + Environment.NewLine, Encoding.UTF8);
            File.AppendAllText(Global.TempPfad + dateiname + ".txt", "csv: 0" + Environment.NewLine, Encoding.UTF8);
            File.AppendAllText(Global.TempPfad + dateiname + ".txt", "----" + Environment.NewLine, Encoding.UTF8);

            File.AppendAllText(Global.TempPfad + dateiname + ".txt", "**Haben Sie Fragen? Dann melden Sie sich gerne!**" + Environment.NewLine, Encoding.UTF8);
            File.AppendAllText(Global.TempPfad + dateiname + ".txt", "" + Environment.NewLine, Encoding.UTF8);

            File.AppendAllText(Global.TempPfad + dateiname + ".txt", "==== Raumplan ====" + Environment.NewLine, Encoding.UTF8);

            File.AppendAllText(Global.TempPfad + dateiname + ".txt", "<searchtable>" + Environment.NewLine);
            File.AppendAllText(Global.TempPfad + dateiname + ".txt", "^  Klasse  ^  Klassenleitung  ^  Bildungsgang  ^  Raum  ^" + Environment.NewLine, Encoding.UTF8);

            List<string> berufe = new List<string>();

            foreach (var klasse in (from t in this.OrderBy(x => x.Stufe).ThenBy(x=>x.NameUntis) where t.Stufe != null where t.Stufe != "" select t).ToList())
            {
                // Berufsschule wird nach Berufen geclustert

                if (!berufe.Contains(klasse.BildungsgangLangname) && klasse.Stufe.StartsWith("BS"))
                {
                    berufe.Add(klasse.BildungsgangLangname);

                    var anzahlSuS = (from s in schuelers where SchneideVorErsterZahlAb(s.Klasse.NameUntis) == SchneideVorErsterZahlAb(klasse.NameUntis) select s).Count();

                    try
                    {
                        var lehrer = (from u in untisanrechnungs
                                      where u.Text.Contains("ildungsgangleitung")
                                      where bgAusschneiden(u.Beschr.ToLower()) == SchneideVorErsterZahlAb(klasse.NameUntis.ToLower())
                                      select u.Lehrer).FirstOrDefault();

                        string leitung = "";

                        if (lehrer != null)
                        {
                            leitung = " [[" + lehrer.Mail + "|" + (lehrer.Geschlecht == "m" ? "Herr" : "Frau") + " " + (lehrer.Titel == "" ? "" : lehrer.Titel + " ") + lehrer.Nachname + "]]";
                        }

                        if (klasse.Raum != null && klasse.Raum != "")
                        {
                            if (!vergebeneRäume.Contains(klasse.Raum))
                            {
                                vergebeneRäume.Add(klasse.Raum);
                            }
                            else
                            {
                                mehrfachVergebeneRäume.Add(klasse.Raum);
                            }
                        }

                        File.AppendAllText(Global.TempPfad + dateiname + "-datenquelle.csv", "\"" + SchneideVorErsterZahlAb(klasse.NameUntis) + "*" + "\",\"" + leitung + "\",\"" + klasse.BildungsgangLangname + "\",\"" + klasse.Raum + "\",\"" + anzahlSuS + "\"" + Environment.NewLine, Encoding.UTF8);

                        zeilen.Add("|" + SchneideVorErsterZahlAb(klasse.NameUntis) + "*" + "  |" + leitung + "  |" + klasse.BildungsgangLangname + "  |" + klasse.Raum + "  |");
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
                if(!klasse.Stufe.StartsWith("BS"))
                {
                    try
                    {
                        var leitung = "[[" + klasse.Klassenleitungen[0].Mail + "|" +(klasse.Klassenleitungen[0].Geschlecht == "m" ? "Herr" : "Frau") + " " + (klasse.Klassenleitungen[0].Titel == "" ? "" : klasse.Klassenleitungen[0].Titel + " ") + " " + klasse.Klassenleitungen[0].Nachname + "]]";

                        var anzahlSuS = (from s in schuelers where s.Klasse.NameUntis == klasse.NameUntis select s).Count();

                        if (klasse.Raum != null && klasse.Raum != "")
                        {
                            if (!vergebeneRäume.Contains(klasse.Raum))
                            {
                                vergebeneRäume.Add(klasse.Raum);
                            }
                            else
                            {
                                mehrfachVergebeneRäume.Add(klasse.Raum);
                            }
                        }

                        File.AppendAllText(Global.TempPfad + dateiname + "-datenquelle.csv", "\"" + klasse.NameUntis + "\",\"" + leitung + "\",\"" + klasse.BildungsgangLangname + "\",\"" + klasse.Raum + "\",\"" + anzahlSuS + "\"" + Environment.NewLine, Encoding.UTF8);
                        zeilen.Add("|" + klasse.NameUntis + "  |" + leitung + "  |" + klasse.BildungsgangLangname + "  |" + klasse.Raum + "  |");
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }
            zeilen.Add("</searchtable>" + Environment.NewLine);
            Global.WriteLine("Klassenpflegschaften",zeilen.Count());
            Global.WriteLine(" Mehrfach vergebene Räume:" + String.Join(",", mehrfachVergebeneRäume), Console.WindowWidth);

            string freieR = "";

            foreach (var raum in raums.OrderBy(x => x.Raumnummer))
            {
                if (!vergebeneRäume.Contains(raum.Raumnummer))
                {
                    freieR += raum.Raumnummer + ",";
                }
            }

            freieR = Global.InsertLineBreaks(freieR, 110);

            Console.WriteLine("  Freie Räume: " + freieR.TrimEnd(','));

            foreach (var zeile in zeilen)
            {   
                File.AppendAllText(Global.TempPfad + dateiname + ".txt", zeile + Environment.NewLine, Encoding.UTF8);
            }

            Global.Dateischreiben(dateiname + ".txt");
            Global.Dateischreiben(dateiname + "-datenquelle.csv");
        }

        private string bgAusschneiden(string input)
        {
            // Den Index des letzten und vorletzten Doppelpunkts finden
            int lastColonIndex = input.LastIndexOf(':');
            int secondLastColonIndex = input.LastIndexOf(':', lastColonIndex - 1);

            // Den Teilstring zwischen den letzten beiden Doppelpunkten extrahieren
            if (lastColonIndex != -1 && secondLastColonIndex != -1)
            {
                return input.Substring(secondLastColonIndex + 1, lastColonIndex - secondLastColonIndex - 1);
            }

            // Falls nicht genügend Doppelpunkte vorhanden sind, wird ein leerer String zurückgegeben
            return string.Empty;
        }

        private string SchneideVorErsterZahlAb(string input)
        {
            // Schleife über den String, um die Position der ersten Zahl zu finden
            for (int i = 0; i < input.Length; i++)
            {
                if (char.IsDigit(input[i]))
                {
                    // Den Teil des Strings bis zur ersten Zahl zurückgeben
                    return input.Substring(0, i);
                }
            }

            // Falls keine Zahl gefunden wurde, wird der ursprüngliche String zurückgegeben
            return input;
        }
    }
}