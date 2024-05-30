using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Data;
using System.Linq;

namespace Push2Dokuwiki
{
    public class Vorgänge : List<Vorgang>
    {
        public Vorgänge()
        {
        }

        public Vorgänge(Abwesenheiten abwesenheiten)
        {
            try
            {
                using (OdbcConnection connection = new OdbcConnection(Global.ConnectionStringAtlantis))
                {
                    DataSet dataSet = new DataSet();
                    OdbcDataAdapter schuelerAdapter = new OdbcDataAdapter(@"
SELECT dba.vorgang.vg_id AS ViD,
dba.vorgang.vorgang_beschreibung AS Beschreibung,
dba.vorgang.s_vorgang_typ AS Typ,
dba.vorgang.erstellungsgrund,
dba.vorgang.erstellungsdatum,
dba.vorgang.datum AS Datum,
dba.vorgang.pu_id AS ID,
dba.vorgang.klasse,
dba.vorgang.schul_jahr,
dba.vorgang.s_jahrgang
FROM dba.vorgang
WHERE s_vorgang_typ = 'Schulversäumnis'", connection);

                    connection.Open();
                    schuelerAdapter.Fill(dataSet, "DBA.leistungsdaten");

                    foreach (DataRow theRow in dataSet.Tables["DBA.leistungsdaten"].Rows)
                    {
                        int schuelerId = Convert.ToInt32(theRow["ID"]);

                        int vId = Convert.ToInt32(theRow["vId"]);

                        DateTime datum = theRow["Datum"].ToString().Length < 3 ? new DateTime() : DateTime.ParseExact(theRow["Datum"].ToString(), "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                        string typ = theRow["typ"] == null ? "" : theRow["typ"].ToString();

                        string beschreibung = theRow["Beschreibung"] == null ? "" : theRow["Beschreibung"].ToString();

                        Vorgang vorgang = new Vorgang(
                            schuelerId,
                            beschreibung,
                            datum,
                            typ,
                            vId
                            );

                        if ((from a in abwesenheiten where a.StudentId == schuelerId select a).Any())
                        {
                            if (!(from m in this where m.Datum.Date == vorgang.Datum.Date where m.SchuelerId == vorgang.SchuelerId select m).Any())
                            {
                                this.Add(vorgang);
                            }
                        }
                    }

                    connection.Close();
                    Console.WriteLine("Schulversäumnisse (Mahnung)",this.Count);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
            }
        }
    }
}