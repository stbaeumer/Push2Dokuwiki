using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace Push2Dokuwiki
{
    public class Fachs : List<Fach>
    {
        public Fachs()
        {
            using (SqlConnection odbcConnection = new SqlConnection(Global.ConnectionStringUntis))
            {
                try
                {
                    string queryString = @"SELECT DISTINCT 
Subjects.Subject_ID,
Subjects.Name,
Subjects.Longname,
Subjects.Text,
Description.Name
FROM Description RIGHT JOIN Subjects ON Description.DESCRIPTION_ID = Subjects.DESCRIPTION_ID
WHERE Subjects.Schoolyear_id = " + Global.AktSj[0] + Global.AktSj[1] + " AND Subjects.Deleted='false'  AND ((Subjects.SCHOOL_ID)=177659) ORDER BY Subjects.Name;";

                    SqlCommand odbcCommand = new SqlCommand(queryString, odbcConnection);
                    odbcConnection.Open();
                    SqlDataReader sqlDataReader = odbcCommand.ExecuteReader();

                    while (sqlDataReader.Read())
                    {
                        Fach fach = new Fach()
                        {
                            IdUntis = sqlDataReader.GetInt32(0),
                            KürzelUntis = Global.SafeGetString(sqlDataReader, 1),
                            Langname = Global.SafeGetString(sqlDataReader, 3),
                            Beschr = Global.SafeGetString(sqlDataReader, 4)
                        };

                        this.Add(fach);
                    };

                    sqlDataReader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    odbcConnection.Close();
                    Global.WriteLine("Fächer", this.Count);
                }
            }
        }

        internal void faecherCsv(string tempdatei)
        {
            var datei = Global.Dateipfad + tempdatei;
            int lastSlashIndex = tempdatei.LastIndexOf('\\');
            string result = (lastSlashIndex != -1) ? tempdatei.Substring(lastSlashIndex + 1) : tempdatei;
            tempdatei = System.IO.Path.GetTempPath() + result;

            File.WriteAllText(tempdatei, "\"name\",\"kuerzel\"" + Environment.NewLine);

            var verschiedeneFaecher = (from t in this select t.Langname).ToList().Distinct();

            foreach (var langname in verschiedeneFaecher)
            {
                if (langname != "")
                {
                    var kürzel = (from t in this where t.Langname == langname select t.KürzelUntis).ToList<string>().Distinct();
                    var kürzelstring = "";
                    foreach (var item in kürzel)
                    {
                        if (item != "" && !kürzelstring.Contains(item+","))
                        {
                            kürzelstring += item + ",";
                        }
                    }
                    File.AppendAllText(tempdatei, "\"" + langname + "\",\"" + kürzelstring.TrimEnd(',') + "\"" + Environment.NewLine);
                }
            }

            Global.Dateischreiben(result, datei, tempdatei);
        }
    }
}