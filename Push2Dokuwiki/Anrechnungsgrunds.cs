using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace Push2Dokuwiki
{
    public class Anrechnungsgrunds : List<Anrechnungsgrund>
    {
        public Anrechnungsgrunds(string aktSj)
        {
            string topic = "Anrechnungsgründe";

            Console.WriteLine(topic + " ...");

            using (SqlConnection odbcConnection = new SqlConnection(Global.ConnectionStringUntis))
            {
                try
                {
                    string queryString = @"SELECT CV_Reason.CV_REASON_ID, CV_Reason.Name, CV_Reason.Longname, CV_Reason.DESCRIPTION_ID, CV_Reason.SortId
WHERE DESCRIPTION_ID = 99
FROM CV_Reason WHERE (((CV_Reason.SCHOOLYEAR_ID)= " + Global.AktSj[0] + Global.AktSj[1] + ")) ORDER BY CV_Reason.SortId;";


                    SqlCommand odbcCommand = new SqlCommand(queryString, odbcConnection);
                    odbcConnection.Open();
                    SqlDataReader sqlDataReader = odbcCommand.ExecuteReader();

                    while (sqlDataReader.Read())
                    {
                        Anrechnungsgrund anrechnungsgrund = new Anrechnungsgrund()
                        {
                            UntisId = sqlDataReader.GetInt32(0),
                            Nummer = Global.SafeGetString(sqlDataReader, 1),
                            Beschreibung = Global.SafeGetString(sqlDataReader, 2),
                            StatistikName = sqlDataReader.GetInt32(3) == 51 ? 67 : sqlDataReader.GetInt32(3) == 50 ? 66 : 65
                        };
                        this.Add(anrechnungsgrund);
                    };
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    odbcConnection.Close();
                }
            }
        }
    }
}