using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace Push2Dokuwiki
{
    public class Anrechnungs : List<Anrechnung>
    {
        public Anrechnungs(int periode)
        {
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
WHERE (SCHOOLYEAR_ID=" + Global.AktSj[0] + Global.AktSj[1] + @");";

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
WHERE (SCHOOLYEAR_ID=" + Global.AktSj[0] + Global.AktSj[1] + @");
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
WHERE (((CountValue.SCHOOLYEAR_ID)=" + Global.AktSj[0] + Global.AktSj[1] + @") AND ((CountValue.Deleted)='false') AND ((CountValue.Deleted)='false'))
ORDER BY CountValue.TEACHER_ID;
";

                    SqlCommand odbcCommand = new SqlCommand(queryString, odbcConnection);
                    odbcConnection.Open();
                    SqlDataReader sqlDataReader = odbcCommand.ExecuteReader();

                    while (sqlDataReader.Read())
                    {
                        Anrechnung anrechnung = new Anrechnung()
                        {
                            TeacherIdUntis = sqlDataReader.GetInt32(0),
                            Beschr = (from b in beschreibungs where b.BeschreibungId == sqlDataReader.GetInt32(1) select b.Name).FirstOrDefault() == null ? "" : (from b in beschreibungs where b.BeschreibungId == sqlDataReader.GetInt32(1) select b.Name).FirstOrDefault(),  // Wiki-URL                            
                            Text = Global.SafeGetString(sqlDataReader, 2) == null ? "" : Global.SafeGetString(sqlDataReader, 2), // Vorsitz etc.                            
                            Wert = Convert.ToDouble(sqlDataReader.GetInt32(3)) / 100000,
                            Von = sqlDataReader.GetInt32(4) > 0 ? DateTime.ParseExact((sqlDataReader.GetInt32(4)).ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture) : new DateTime(),
                            Bis = sqlDataReader.GetInt32(5) > 0 ? DateTime.ParseExact((sqlDataReader.GetInt32(5)).ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture) : new DateTime(),
                            Grund = Convert.ToInt32((from c in cvreasons where c.Id == sqlDataReader.GetInt32(6) select c.Name).FirstOrDefault())
                        };

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
                                this.Add(anrechnung);
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

        internal Lehrer GetBildungsgangleitung(Bildungsgang bildungsgang, Lehrers lehrers)
        {
            foreach (var a in this)
            {
                if (a.Text != null && a.Text != "" && a.Text.Contains("Bildungsgangleitung"))
                {
                    if (bildungsgang.Langname != null && bildungsgang.Langname != "" && ((a.Text.Contains(bildungsgang.Langname)) || (a.Beschr != null && a.Beschr != "" && (a.Beschr.ToLower().Contains(":" + bildungsgang.Kurzname.ToLower()) || (a.Beschr.ToLower().EndsWith(bildungsgang.Kurzname.ToLower()))))));
                    {
                        if (a.TeacherIdUntis != 0)
                        {
                            return (from l in lehrers where a.TeacherIdUntis == l.IdUntis select l).FirstOrDefault();
                        }
                    }
                }
            }            
            Console.WriteLine("Keine Bildungsgangleitung bei " + bildungsgang.Kurzname + ". Stimmt der Text in der Anrechnung mit dem Langnamen in den Klassenstammdaten überein? Im Text der Anrechnung muss das Wort Bildungsgangleitung in Klammern stehen. Zusätzlich muss der Langname der Klassen im Text der Anrechnungen enthalten sein.");
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

            return (from l in lehrers where l.IdUntis == lid select l).FirstOrDefault();
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
            if (url==null)
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