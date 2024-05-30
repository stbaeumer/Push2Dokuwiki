using System.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace Push2Dokuwiki
{
    public class Feriens : List<Ferien>
    {
        public Feriens()
        {
            SqlConnection sqlConnection;
            sqlConnection = new SqlConnection(@"Data Source=SQL01\UNTIS;Initial Catalog=master;Integrated Security=True");

            try
            {
                sqlConnection.Open();

                string queryString = @"SELECT DISTINCT Holiday.Holiday_ID,
Holiday.Name, 
Holiday.Longname, 
Holiday.DateFrom, 
Holiday.DateTo, 
Holiday.Flags
FROM Holiday 
WHERE (((Holiday.SCHOOLYEAR_ID)=" + Global.AktSj[0] + Global.AktSj[1] + ") AND ((Holiday.SCHOOL_ID)=177659));";

                using (SqlCommand sqlCommand = new SqlCommand(queryString, sqlConnection))
                {
                    SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();

                    Console.WriteLine("Ferien");                    

                    while (sqlDataReader.Read())
                    {
                        Ferien ferien = new Ferien
                        {
                            Name = Global.SafeGetString(sqlDataReader, 1),
                            LangName = Global.SafeGetString(sqlDataReader, 2),
                            Von = DateTime.ParseExact((sqlDataReader.GetInt32(3)).ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture),
                            Bis = DateTime.ParseExact((sqlDataReader.GetInt32(4)).ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture),
                            Feiertag = Global.SafeGetString(sqlDataReader, 5) == "F" ? true : false
                        };
                        Console.WriteLine("   " + ferien.Name.ToString().PadRight(25) + " " + ferien.Von.ToShortDateString() + " - " + ferien.Bis.ToShortDateString());
                        this.Add(ferien);
                    };

                    // Bewegl. Ferientag
                    Ferien f = new Ferien()
                    {
                        Von = new DateTime(2020, 02, 25),
                        Bis = new DateTime(2020, 02, 25)
                    };
                    this.Add(f);
                    Console.WriteLine("");

                    sqlDataReader.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());                
            }
            finally 
            { 
                sqlConnection.Close(); 
            }
        }

        internal bool IstFerienTag(DateTime tag)
        {
            foreach (var ferien in this)
            {
                if (ferien.Von.Date <= tag.Date && tag.Date <= ferien.Bis.Date)
                {
                    return true;
                }
            }
            return false;
        }
    }
}