﻿using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Data;
using System.Linq;

namespace Push2Dokuwiki
{
    public class Maßnahmen : List<Maßnahme>
    {
        public Maßnahmen()
        {
        }

        public Maßnahmen(Abwesenheiten abwesenheiten)
        {
            try
            {
                using (OdbcConnection connection = new OdbcConnection(Global.ConnectionStringAtlantis))
                {
                    DataSet dataSet = new DataSet();
                    OdbcDataAdapter schuelerAdapter = new OdbcDataAdapter(@"
SELECT DBA.schue_sj.pu_id AS ID,
DBA.schueler_info.s_typ_puin AS Kürzel,
DBA.schueler_info.s_typ_puin_2 AS Bezeichnung,
DBA.schueler_info.bezeichnung_2 AS Beschreibung,
DBA.schueler_info.datum AS Datum
FROM DBA.schueler_info CROSS JOIN DBA.schue_sj
WHERE vorgang_schuljahr = check_null (hole_schuljahr_rech ('',  0)) AND info_gruppe = 'STRAF' AND schue_sj.pu_id = schueler_info.pu_id ORDER BY Datum", connection);

                    connection.Open();
                    schuelerAdapter.Fill(dataSet, "DBA.leistungsdaten");

                    foreach (DataRow theRow in dataSet.Tables["DBA.leistungsdaten"].Rows)
                    {
                        int schuelerId = Convert.ToInt32(theRow["ID"]);

                        DateTime datum = theRow["Datum"].ToString().Length < 3 ? new DateTime() : DateTime.ParseExact(theRow["Datum"].ToString(), "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                        string kürzel = theRow["Kürzel"] == null ? "" : theRow["Kürzel"].ToString();

                        string beschreibung = theRow["Beschreibung"] == null ? "" : theRow["Beschreibung"].ToString();
                        string bezeichnung = theRow["Bezeichnung"] == null ? "" : theRow["Bezeichnung"].ToString();

                        Maßnahme maßnahme = new Maßnahme(
                            schuelerId,
                            beschreibung,
                            bezeichnung,
                            datum,
                            kürzel
                            );

                        if ((from a in abwesenheiten where a.StudentId == schuelerId select a).Any())
                        {
                            if (!(from m in this where m.Datum.Date == maßnahme.Datum.Date where m.SchuelerId == maßnahme.SchuelerId select m).Any())
                            {
                                this.Add(maßnahme);
                            }
                        }
                    }

                    connection.Close();
                    Global.WriteLine(" Maßnahmen",this.Count);
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