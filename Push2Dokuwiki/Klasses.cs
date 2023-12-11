using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Push2Dokuwiki
{
    public class Klasses : List<Klasse>
    {
        public List<Lehrer> Klassenleitungen { get; private set; }

        public Klasses(int periode, Lehrers lehrers)
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
                        klasse.WikiLink = Global.SafeGetString(sqlDataReader, 7);
                        
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
    }
}