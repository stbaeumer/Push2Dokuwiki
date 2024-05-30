using System;
using System.Collections.Generic;
using System.Data.SqlClient;

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
                            Langname = Global.SafeGetString(sqlDataReader, 2),
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
    }
}