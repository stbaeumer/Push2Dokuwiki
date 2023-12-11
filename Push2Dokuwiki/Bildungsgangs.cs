using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace Push2Dokuwiki
{
    public class Bildungsgangs : List<Bildungsgang>
    {
        public Bildungsgangs()
        {
        }

        public Bildungsgangs(int periode, Lehrers lehrers, Anrechnungs anrechnungs, Unterrichts unterrichts, Fachs fachs)
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
                        Bildungsgang bildungsgang = new Bildungsgang();
                        bildungsgang.Kurzname = string.Concat(Global.SafeGetString(sqlDataReader, 1).TakeWhile(c => c < '0' || c > '9'));
                        
                        // Beim BTeam und anderen "Klassen" ist keine Jahreszahl im Namen. Daraus wird dann kein Bildungsgang erstellt.

                        if (Global.SafeGetString(sqlDataReader, 1) != bildungsgang.Kurzname)
                        {
                            if (!(from t in this where t.Kurzname == bildungsgang.Kurzname select t).Any())
                            {   
                                bildungsgang.Langname = Global.SafeGetString(sqlDataReader, 3);
                                bildungsgang.WikiLink = Global.SafeGetString(sqlDataReader, 7).StartsWith(":") ? Global.SafeGetString(sqlDataReader, 7) : ":" + Global.SafeGetString(sqlDataReader, 7);
                                bildungsgang.Members = unterrichts.GetMembers(bildungsgang, lehrers, unterrichts);
                                bildungsgang.Leitung = anrechnungs.GetBildungsgangleitung(bildungsgang, lehrers);

                                this.Add(bildungsgang);
                                if (bildungsgang.WikiLink == null || bildungsgang.WikiLink == "")
                                {
                                    Console.WriteLine("Kein Link in das Wiki im Bildungsgang " + bildungsgang.Kurzname + " vorhanden.");
                                    Console.WriteLine("Der Link muss im Text in den Klassenstammdaten stehen.");
                                    Console.WriteLine("Bei der Bildungsgangleitung muss der Langname der Klassen in der Beschreibung im Text in den Anrechnungen enthalten sein.");
                                }
                            }
                        }
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
                    Global.WriteLine("Bildungsgänge", this.Count);
                }
            }
        }
    }
}