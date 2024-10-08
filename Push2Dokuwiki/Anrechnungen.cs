﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace Push2Dokuwiki
{
    public class Anrechnungs : List<Anrechnung>
    {
        public Anrechnungs(int periode, Lehrers lehrers)
        {
            var sj = Global.AktSj[0] + Global.AktSj[1];

            using (SqlConnection odbcConnection = new SqlConnection(Global.ConnectionStringUntis))
            {
                Beschreibungs beschreibungs = new Beschreibungs();

                CVReasons cvreasons = new CVReasons();

                try
                {
                    string queryString = @"
SELECT 
CV_REASON_ID, 
Name, 
Longname
FROM CV_Reason
WHERE (SCHOOLYEAR_ID=" + sj + @");";

                    SqlCommand odbcCommand = new SqlCommand(queryString, odbcConnection);
                    odbcConnection.Open();
                    SqlDataReader sqlDataReader = odbcCommand.ExecuteReader();

                    while (sqlDataReader.Read())
                    {
                        CVReason cvreason = new CVReason()
                        {
                            Id = sqlDataReader.GetInt32(0),
                            Name = Global.SafeGetString(sqlDataReader, 1),
                            Langname = Global.SafeGetString(sqlDataReader, 2)
                        };

                        cvreasons.Add(cvreason);

                    };
                    sqlDataReader.Close();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    odbcConnection.Close();
                }


                try
                {
                    string queryString = @"SELECT 
DESCRIPTION_ID, 
Name, 
Longname
FROM Description
WHERE (SCHOOLYEAR_ID=" + sj + @");
";
                    SqlCommand odbcCommand = new SqlCommand(queryString, odbcConnection);
                    odbcConnection.Open();
                    SqlDataReader sqlDataReader = odbcCommand.ExecuteReader();

                    while (sqlDataReader.Read())
                    {
                        Beschreibung beschreibung = new Beschreibung()
                        {
                            BeschreibungId = sqlDataReader.GetInt32(0),
                            Name = Global.SafeGetString(sqlDataReader, 1),
                            Langname = Global.SafeGetString(sqlDataReader, 2)
                        };

                        beschreibungs.Add(beschreibung);

                    };
                    sqlDataReader.Close();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    odbcConnection.Close();
                }


                try
                {
                    string queryString = @"SELECT 
CountValue.TEACHER_ID,  
DESCRIPTION_ID, 
CountValue.Text,
CountValue.Value,
CountValue.DateFrom,
CountValue.DateTo,
CountValue.CV_REASON_ID

FROM CountValue
WHERE (((CountValue.SCHOOLYEAR_ID)=" + sj + @") AND ((CountValue.Deleted)='false') AND ((CountValue.Deleted)='false'))
ORDER BY CountValue.TEACHER_ID;
";

                    SqlCommand odbcCommand = new SqlCommand(queryString, odbcConnection);
                    odbcConnection.Open();
                    SqlDataReader sqlDataReader = odbcCommand.ExecuteReader();

                    while (sqlDataReader.Read())
                    {
                        Anrechnung anrechnung = new Anrechnung();

                        anrechnung.TeacherIdUntis = sqlDataReader.GetInt32(0);

                        anrechnung.Grund = Convert.ToInt32((from c in cvreasons where c.Id == sqlDataReader.GetInt32(6) select c.Name).FirstOrDefault());
                        anrechnung.Wert = Convert.ToDouble(sqlDataReader.GetInt32(3)) / 100000;

                        anrechnung.Lehrer = (from l in lehrers where l.IdUntis == sqlDataReader.GetInt32(0) select l).FirstOrDefault();

                        if (anrechnung.Lehrer.Kürzel == "BM")
                        {
                            string a = "";
                        }


                        // Die Beschr muss auf eine Wiki-Seite matchen. Beschr entspricht einem Thema oder einem Gremium
                        anrechnung.Beschr = (from b in beschreibungs where b.BeschreibungId == sqlDataReader.GetInt32(1) select b.Name).FirstOrDefault() == null ? "" : (from b in beschreibungs where b.BeschreibungId == sqlDataReader.GetInt32(1) select b.Name).FirstOrDefault();


                        // Amt und Rolle ergeben sich aus dem Text bei Grund 500 und nur dann, wenn ein KuK zugeordnet wurde. Angaben in Klammern werden ignoriert.
                        anrechnung.Text = Global.SafeGetString(sqlDataReader, 2) == null ? "" : Global.SafeGetString(sqlDataReader, 2); // Vorsitz etc.                            
                        anrechnung.Amt = anrechnung.Text.Contains("A14") ? "A14" : anrechnung.Text.Contains("A15") ? "A15" : anrechnung.Text.Contains("A16") ? "A16" : "";

                        // Regex für alles in runden, eckigen und geschweiften Klammern inklusive der Klammern selbst

                        var allesAußerKlammern = (Regex.Replace(anrechnung.Text, @"[\(\[\{][^)\]\}]*[\)\]\}]", "")).Trim();
                        anrechnung.Rolle = (allesAußerKlammern.Replace("A14", "").Replace("A15", "").Replace("A16", "")).Trim(',').Trim();


                        anrechnung.Hinweis = ZwischenEckigenKlammernStehenHinweise(anrechnung.Text);
                        anrechnung.Kategorien = ZwischenGeschweiftenKlammernStehtDieKategorie(anrechnung.Text);




                        anrechnung.Von = sqlDataReader.GetInt32(4) > 0 ? DateTime.ParseExact((sqlDataReader.GetInt32(4)).ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture) : new DateTime();
                        anrechnung.Bis = sqlDataReader.GetInt32(5) > 0 ? DateTime.ParseExact((sqlDataReader.GetInt32(5)).ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture) : new DateTime();


                        if (anrechnung.Text.Contains("("))
                        {
                            anrechnung.TextGekürzt = anrechnung.Text.Substring(0, anrechnung.Text.IndexOf('(')).Trim();
                        }
                        else
                        {
                            anrechnung.TextGekürzt = anrechnung.Text.Trim();
                        }

                        if (anrechnung.TeacherIdUntis != 0)
                        {
                            if (anrechnung.Grund == 0 || anrechnung.Grund > 210 || anrechnung.Grund == 200 || anrechnung.Beschr == "Interessen") // Schwerbehinderung etc. nicht einlesen
                            {
                                if (anrechnung.Lehrer != null)
                                {
                                    if (anrechnung.Lehrer.Kürzel == "BM")
                                    {
                                        string aa = "";
                                    }
                                    if (anrechnung.Lehrer.Kürzel != null && anrechnung.Lehrer.Kürzel != "")
                                    {
                                        this.Add(anrechnung);
                                    }
                                }
                            }
                        }
                    };
                    sqlDataReader.Close();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    odbcConnection.Close();
                    Global.WriteLine("Anrechnungen", this.Count);
                }
            }
        }

        private List<string> ZwischenGeschweiftenKlammernStehtDieKategorie(string text)
        {
            List<string> list = new List<string>();
            string pattern = @"\{([^}]*)\}";
            MatchCollection matches = Regex.Matches(text, pattern);

            foreach (Match match in matches)
            {
                foreach (var item in match.Value.ToString().Trim().Split(','))
                {
                    // Entferne die geschweiften Klammern selbst
                    string content = match.Value.Trim('{', '}');
                    list.Add(content);
                }
            }
            return list;
        }

        private string ZwischenEckigenKlammernStehenHinweise(string text)
        {
            string pattern = @"\[[^\]]*\]";
            MatchCollection matches = Regex.Matches(text, pattern);

            foreach (Match match in matches)
            {
                // Entferne die eckigen Klammern selbst
                string content = match.Value.Trim('[', ']');
                return content;
            }
            return "";
        }

        internal void UntisAnrechnungsToCsv(string datei, List<int> nurDieseGründe, List<int> fürDieseGründeKeinenWert, List<string> fürDieseLehrerKeineWerte)
        {
            UTF8Encoding utf8NoBom = new UTF8Encoding(false);

            Global.OrdnerAnlegen(datei);

            File.WriteAllText(Global.TempPfad + datei, "\"Name\",\"Kuerzel\",\"Mail\",\"Wert\",\"von\",\"bis\",\"Rolle\",\"Amt\",\"Grund\",\"Beschreibung\",\"Hinweis\",\"Kategorien\"" + Environment.NewLine, utf8NoBom);

            foreach (var a in this.OrderBy(x => x.Lehrer.Kürzel))
            {
                if (nurDieseGründe.Contains(a.Grund))
                {
                    var wert = (a.Wert == 0 ? "" : a.Wert.ToString());

                    if (!fürDieseGründeKeinenWert.Contains(a.Grund))
                    {
                        wert = "";
                    }

                    if (fürDieseLehrerKeineWerte.Contains(a.Lehrer.Kürzel))
                    {
                        wert = "";
                    }

                    var kategorien = "";
                    foreach (var c in a.Kategorien)
                    {
                        kategorien += c + ",";
                    }

                    File.AppendAllText(Global.TempPfad + datei, "\"" + (a.Lehrer.Titel == "" ? "" : a.Lehrer.Titel + " ") + a.Lehrer.Vorname + " " + a.Lehrer.Nachname + "\",\"" + a.Lehrer.Kürzel + "\",\"" + a.Lehrer.Mail + "\",\"" + wert + "\",\"" + (a.Von.Year == 1 ? "" : a.Von.ToShortDateString()) + "\",\"" + (a.Bis.Year == 1 ? "" : a.Bis.ToShortDateString()) + "\",\"" + a.Rolle + "\",\"" + a.Amt + "\",\"" + a.Grund + "\",\"" + (a.Beschr == "" ? "" : "[[" + a.Beschr + "]]") + "\",\"" + a.Hinweis + "\",\"" + kategorien.TrimEnd(',') + "\"" + Environment.NewLine, utf8NoBom);
                }
            }

            Global.WriteLine("Untisanrechungen", this.Count);
            Global.Dateischreiben(datei);
        }

        internal Lehrer GetBildungsgangleitung(Bildungsgang bildungsgang, Lehrers lehrers)
        {
            foreach (var a in this)
            {
                if (a.Text != null && a.Text != "" && a.Text.Contains("Bildungsgangleitung"))
                {
                    if (bildungsgang.Langname != null && bildungsgang.Langname != "" && (a.Beschr.ToLower().EndsWith(bildungsgang.Kurzname.ToLower() + ":start")))
                    {
                        if (a.TeacherIdUntis != 0)
                        {
                            if (a.TeacherIdUntis != 0)
                            {
                                return (from l in lehrers where a.TeacherIdUntis == l.IdUntis select l).FirstOrDefault();
                            }
                        }
                    }
                }
            }
            var x = "Keine Bildungsgangleitung bei " + bildungsgang.Kurzname + ". Stimmt der Text in der Anrechnung mit dem Langnamen in den Klassenstammdaten überein? Im Text der Anrechnung muss das Wort Bildungsgangleitung in Klammern stehen. Zusätzlich muss der Langname der Klassen im Text der Anrechnungen enthalten sein.";
            Console.WriteLine(Global.InsertLineBreaks(x, 77));
            return null;
        }

        internal string GetWikiLink(Bildungsgang bildungsgang, Lehrers lehrers)
        {
            foreach (var a in this)
            {
                if (a.Text != null && a.Text != "" && a.Text.Contains("Bildungsgangleitung"))
                {
                    if (bildungsgang.Langname != null && bildungsgang.Langname != "" && (a.Beschr.ToLower().EndsWith(bildungsgang.Kurzname.ToLower() + ":start")))
                    {
                        if (a.TeacherIdUntis != 0)
                        {
                            return a.Beschr;
                        }
                    }
                }
            }
            var x = "Kein Wikilink.";
            Console.WriteLine(Global.InsertLineBreaks(x, 77));
            return null;
        }

        internal Lehrer GetLeitung(List<Lehrer> lehrers, string name, string leitungsbezeichnung)
        {
            var lid = (from a in this
                       where a.Text == leitungsbezeichnung
                       select a.TeacherIdUntis).FirstOrDefault();
            if (lid == 0)
            {
                lid = (from a in this
                       where a.Text.StartsWith(leitungsbezeichnung)
                       where a.Text.Contains(name)
                       select a.TeacherIdUntis).FirstOrDefault();
            }
            if (lid == 0)
            {
                lid = (from a in this
                       where a.Text.Contains(leitungsbezeichnung)
                       where a.Text.Contains(name)
                       select a.TeacherIdUntis).FirstOrDefault();
            }
            if (lid == 0)
            {
                lid = (from a in this where a.Text.Contains(leitungsbezeichnung) select a.TeacherIdUntis).FirstOrDefault();
            }
            if (leitungsbezeichnung != "")
            {
                return (from l in lehrers where l.IdUntis == lid select l).FirstOrDefault();
            }
            return null;
        }

        internal string GetWikiLink(List<Lehrer> lehrers, string name, string suchkriterium)
        {
            var url = (from a in this
                       where a.Text == suchkriterium
                       select a.Beschr).FirstOrDefault();
            if (url == null)
            {
                url = (from a in this
                       where a.Text.StartsWith(suchkriterium)
                       where a.Text.Contains(name)
                       select a.Beschr).FirstOrDefault();
            }
            if (url == null)
            {
                url = (from a in this
                       where a.Text.Contains(suchkriterium)
                       where a.Text.Contains(name)
                       select a.Beschr).FirstOrDefault();
            }
            if (url == null)
            {
                url = (from a in this where a.Text.Contains(suchkriterium) select a.Beschr).FirstOrDefault();
            }
            // Zuletzt wird das Suchkriterium selbst zum Link
            if (url == null)
            {
                url = suchkriterium;
            }

            return url;
        }

        internal List<Lehrer> LuL(Lehrers lehrers, string v)
        {
            List<Lehrer> lehrer = new List<Lehrer>();

            foreach (var a in this)
            {
                if (a.Text.Contains(v))
                {
                    if (!(from l in lehrer where l.IdUntis == a.TeacherIdUntis select l).Any())
                    {
                        lehrer.Add((from l in lehrers where l.IdUntis == a.TeacherIdUntis select l).FirstOrDefault());
                    }
                }
            }
            return lehrer;
        }
    }
}