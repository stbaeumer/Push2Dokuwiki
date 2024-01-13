using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Push2Dokuwiki
{
    public class Lehrers : List<Lehrer>
    {
        public Lehrers()
        {
        }

        public Lehrers(int periode, Raums raums, int aktSj)
        {
            Anrechnungs anrechnungen = new Anrechnungs(periode);

            using (SqlConnection odbcConnection = new SqlConnection(Global.ConnectionStringUntis))
            {
                try
                {
                    string queryString = @"SELECT DISTINCT 
Teacher.Teacher_ID, 
Teacher.Name,
Teacher.Longname, 
Teacher.FirstName,
Teacher.Email,
Teacher.ROOM_ID,
Teacher.Title,
Teacher.PlannedWeek,
Teacher.Flags,
Teacher.BirthDate,
Teacher.Text2
 FROM Teacher
WHERE (((SCHOOLYEAR_ID)= " + Global.AktSj[0] + Global.AktSj[1] + ") AND  ((TERM_ID)=" + periode + ") AND ((Teacher.SCHOOL_ID)=177659) AND (((Teacher.Deleted)='false'))) ORDER BY Teacher.Name;";

                    SqlCommand odbcCommand = new SqlCommand(queryString, odbcConnection);
                    odbcConnection.Open();
                    SqlDataReader sqlDataReader = odbcCommand.ExecuteReader();

                    while (sqlDataReader.Read())
                    {
                        Lehrer lehrer = new Lehrer();

                        lehrer.IdUntis = sqlDataReader.GetInt32(0);
                        lehrer.Kürzel = Global.SafeGetString(sqlDataReader, 1);
                        lehrer.Nachname = Global.SafeGetString(sqlDataReader, 2);

                        if (lehrer.Nachname != "")
                        {
                            try
                            {
                                lehrer.Flags = Global.SafeGetString(sqlDataReader, 10);
                                lehrer.Anrechnungen = (from a in anrechnungen where a.TeacherIdUntis == sqlDataReader.GetInt32(0) select a).ToList();

                                if (lehrer.Flags.Contains("I") && lehrer.Anrechnungen.Count == 0)
                                {
                                    Console.WriteLine(lehrer.Kürzel + ": Der Lehrer wird ignoriert.");
                                }
                                else
                                {
                                    lehrer.Vorname = Global.SafeGetString(sqlDataReader, 3);
                                    lehrer.Mail = Global.SafeGetString(sqlDataReader, 4);
                                    lehrer.Raum = (from r in raums where r.IdUntis == sqlDataReader.GetInt32(5) select r.Raumnummer).FirstOrDefault();
                                    lehrer.Titel = Global.SafeGetString(sqlDataReader, 6);
                                    lehrer.Text2 = Global.SafeGetString(sqlDataReader, 10);

                                    lehrer.Deputat = Convert.ToDouble(sqlDataReader.GetInt32(7)) / 1000;
                                    lehrer.Geschlecht = Global.SafeGetString(sqlDataReader, 8).Contains("W") ? "w" : "m";

                                    if (lehrer.Geschlecht != "w" && lehrer.Geschlecht != "m")
                                    {
                                        Console.WriteLine(lehrer.Kürzel + " hat kein Geschlecht.");
                                    }
                                    try
                                    {
                                        lehrer.Geburtsdatum = DateTime.ParseExact(sqlDataReader.GetInt32(9).ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);
                                    }
                                    catch (Exception)
                                    {
                                        Console.WriteLine(lehrer.Kürzel + ": Kein Geburtsdatum");
                                    }

                                    if (lehrer.Geburtsdatum.Year > 1)
                                    {
                                        lehrer.AlterAmErstenSchultagDiesesJahres = lehrer.GetAlterAmErstenSchultagDiesesJahres(aktSj);
                                        lehrer.ProzentStelle = lehrer.GetProzentStelle();
                                        lehrer.AusgeschütteteAltersermäßigung = (from a in lehrer.Anrechnungen where a.Grund == 200 select a.Wert).FirstOrDefault();
                                        lehrer.CheckAltersermäßigung();
                                    }

                                    this.Add(lehrer);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Fehler bei Lehrer " + lehrer.Nachname + ": " + ex);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    throw new Exception(ex.ToString());
                }
                finally
                {
                    odbcConnection.Close();
                    Global.WriteLine("Lehrers", this.Count);
                }
            }
        }



        public List<Lehrer> Referendare()
        {
            //var members = new List<string>();
            List<Lehrer> members = new List<Lehrer>();

            foreach (var lehrer in this)
            {
                if (lehrer.Kürzel.StartsWith("Y"))
                {
                    if (!members.Contains(lehrer))
                    {
                        members.Add(lehrer);
                    }
                }
            }
            return members;
        }

        internal void DateiAnrechnungenErzeugen(string dateiAnrechnungen, string anrechnungenNeu, Klasses klasses)
        {
            string anrechnungstring = "";

            File.WriteAllText(anrechnungenNeu, "====== Verteilung des Lehrertopfs ======" + Environment.NewLine);
            File.AppendAllText(anrechnungenNeu, Environment.NewLine);

            File.AppendAllText(anrechnungenNeu, "Zweimal jährlich wird hier die Verteilung der Anrechnungen aktualisiert. Die [[termine:termine|Termine]] werden rund um den 15. Dezember und den 1. März festgelegt." + Environment.NewLine);
            File.AppendAllText(anrechnungenNeu, Environment.NewLine);

            File.AppendAllText(anrechnungenNeu, "Die Verteilung erfolgt durch den [[sl:schulleiter|Schulleiter]] nach den [[sl:grundsaetze_der_verteilung_der_anrechungsstunden|Grundsätzen der Verteilung]]. Der [[:lehrerrat|Lehrerrat]] berät vor der Veröffentlichung." + Environment.NewLine);
            File.AppendAllText(anrechnungenNeu, Environment.NewLine);

            File.AppendAllText(anrechnungenNeu, "In Abhängigkeit von den [[sl:Personalia|Schülerzahlen]] stehen dem BKB Anrechnungsstunden für alle Kolleginnen und Kollegen zur Verfügung. Mit [[sl:personalia|sinkenden Schülerzahlen]] schrumpft dieser Topf. Siehe auch [[sl:personalia|Personalia]]." + Environment.NewLine);
            File.AppendAllText(anrechnungenNeu, Environment.NewLine);

            File.AppendAllText(anrechnungenNeu, "Es wird angestrebt, zu Beginn eines jeden Schuljahres 10% der Anrechnungsstunden für die unterjährige Vergabe zurückzuhalten." + Environment.NewLine);
            File.AppendAllText(anrechnungenNeu, Environment.NewLine);

            File.AppendAllText(anrechnungenNeu, "  Diese Datei bitte nicht manuell editieren." + Environment.NewLine);
            File.AppendAllText(anrechnungenNeu, Environment.NewLine);
            File.AppendAllText(anrechnungenNeu, Environment.NewLine);

            File.AppendAllText(anrechnungenNeu, "<searchtable>" + Environment.NewLine);
            File.AppendAllText(anrechnungenNeu, Environment.NewLine);

            File.AppendAllText(anrechnungenNeu, "^Name^Grund^Wert^Von^Bis^" + Environment.NewLine);
            double summe = 0;

            foreach (var lehrer in this.OrderBy(x => x.Nachname))
            {
                foreach (var anrechnung in lehrer.Anrechnungen)
                {
                    if (anrechnung.Grund == 500 && anrechnung.Wert > 0)
                    {
                        var text = anrechnung.TextGekürzt;

                        if (anrechnung.Beschr != "")
                        {
                            text = "[[:" + anrechnung.Beschr + "|" + anrechnung.TextGekürzt + "]]";
                        }

                        anrechnungstring += "|[[chat>" + lehrer.Mail.Replace("@berufskolleg-borken.de", "") + "|" + lehrer.Nachname + ", " + lehrer.Vorname + "]]|" + text + "|  " + anrechnung.Wert + "|" + (anrechnung.Von.Year == 1 ? "" : anrechnung.Von.ToShortDateString()) + " |" + (anrechnung.Bis.Year == 1 ? "" : anrechnung.Bis.ToShortDateString()) + " |" + Environment.NewLine;
                        summe += anrechnung.Wert;
                    }
                }
            }

            anrechnungstring += "| |  Summe:|  " + summe.ToString() + "| | |";

            File.AppendAllText(anrechnungenNeu, anrechnungstring + Environment.NewLine);

            File.AppendAllText(anrechnungenNeu, "</searchtable>" + Environment.NewLine);
            File.AppendAllText(anrechnungenNeu, Environment.NewLine);


            File.AppendAllText(anrechnungenNeu, "" + Environment.NewLine);
            File.AppendAllText(anrechnungenNeu, "Seite erstellt mit [[github>stbaeumer/Push2Dokuwiki|Push2Dokuwiki]]." + Environment.NewLine);

            Global.DateiTauschen(dateiAnrechnungen, anrechnungenNeu);
        }

        internal void DateiKollegiumErzeugen(string dokuwikiPfadUndDatei, string kollegiumNeu, Unterrichts unterrichts, Klasses klasses, Fachschaften fachschaften, Bildungsgangs bildungsgangs)
        {
            File.WriteAllText(kollegiumNeu, "====== Kollegium ======" + Environment.NewLine);
            File.AppendAllText(kollegiumNeu, Environment.NewLine);
            File.AppendAllText(kollegiumNeu, "  Diese Datei bitte nicht manuell editieren." + Environment.NewLine);
            File.AppendAllText(kollegiumNeu, Environment.NewLine);
            File.AppendAllText(kollegiumNeu, "Siehe auch [[kollegium:gruppen|Gruppen & Mitglieder]]." + Environment.NewLine);

            File.AppendAllText(kollegiumNeu, Environment.NewLine);
            //File.AppendAllText(dateiKollegium, "^  Foto  ^  Name  ^ ^" + Environment.NewLine);

            foreach (var lehrer in this.OrderBy(x => x.Nachname))
            {
                //File.AppendAllText(dateiKollegium,"==== " + lehrer.Kürzel + " ====" + Environment.NewLine);
                //File.AppendAllText(dateiKollegium, "" + Environment.NewLine);

                string aufgaben = "";
                string klassenleitungen = "";
                string faecher = "";
                string bildungsgaenge = "";
                var interessen = "";

                // Auf Fächer prüfen

                var fachs = new List<string>();

                if ((from u in unterrichts where u.LehrerKürzel == lehrer.Kürzel select u).Any())
                {
                    faecher = @"**Fächer:** "; ;

                    foreach (var unterricht in unterrichts)
                    {
                        if (lehrer.Kürzel == unterricht.LehrerKürzel)
                        {
                            if (!fachs.Contains(unterricht.FachKürzel.Split(' ')[0] + "|" + lehrer.Kürzel))
                            {
                                fachs.Add(unterricht.FachKürzel.Split(' ')[0] + "|" + lehrer.Kürzel);

                                var x = (from f in fachschaften where f.KürzelUntis == unterricht.FachKürzel.Split(' ')[0] select f).FirstOrDefault();

                                if (x != null)
                                {
                                    faecher += "[[:" + x.Beschr.Replace("fachschaft:", "fachschaften:") + "|" + unterricht.FachKürzel.Split(' ')[0] + "]], ";
                                }
                                else
                                {
                                    faecher += unterricht.FachKürzel.Split(' ')[0] + ", ";
                                }
                            }
                        }
                    }

                    faecher = faecher.TrimEnd();
                    faecher = faecher.TrimEnd(',');
                    faecher = faecher + @"\\ ";
                }

                // Auf Bildungsgänge prüfen

                if ((from u in unterrichts where u.LehrerKürzel == lehrer.Kürzel select u).Any())
                {
                    bildungsgaenge = @"**Bildungsgänge:** "; ;

                    var bgs = new List<string>();

                    foreach (var unterricht in (from u in unterrichts.OrderBy(x => x.LehrerKürzel) select u))
                    {
                        if (!(new List<string>() { "LRat", "BTeam", "EDV" }.Contains(unterricht.KlasseKürzel)))
                        {
                            if (lehrer.Kürzel == unterricht.LehrerKürzel)
                            {
                                string result = Regex.Match(unterricht.KlasseKürzel, @"^[^0-9]*").Value;

                                var wikilink = (from b in bildungsgangs where b.Kurzname == result select b.WikiLink).FirstOrDefault();

                                if (!bgs.Contains(result + "|" + lehrer.Kürzel))
                                {
                                    bgs.Add(result + "|" + lehrer.Kürzel);

                                    var text = (from k in klasses where Regex.Match(k.NameUntis, @"^[^0-9]*").Value == result select k.WikiLink).FirstOrDefault();

                                    if (result.ToLower() == "agg" || result.ToLower() == "ags")
                                    {
                                        result = "agg-ags";
                                    }
                                    bildungsgaenge += "[[:" + wikilink + "|" + result + "]], ";
                                }
                            }
                        }
                    }

                    bildungsgaenge = bildungsgaenge.TrimEnd();
                    bildungsgaenge = bildungsgaenge.TrimEnd(',');
                    bildungsgaenge = bildungsgaenge + @"\\ ";
                }

                // Auf Klassenleitung prüfen

                if ((from k in klasses from kl in k.Klassenleitungen where kl.Kürzel == lehrer.Kürzel select kl).Any())
                {
                    klassenleitungen = @"**Klassenleitungen:** "; ;

                    foreach (var klasse in klasses)
                    {
                        if (!(new List<string>() { "LRat", "BTeam", "EDV" }.Contains(klasse.NameUntis)))
                        {
                            foreach (var klassenleitung in klasse.Klassenleitungen)
                            {
                                if (lehrer.Kürzel == klassenleitung.Kürzel)
                                {
                                    string result = Regex.Match(klasse.NameUntis, @"^[^0-9]*").Value;

                                    if (result.ToLower() == "agg" || result.ToLower() == "ags")
                                    {
                                        result = "agg-ags";
                                    }

                                    klassenleitungen += "[[:" + klasse.WikiLink + "|" + klasse.NameUntis + "]]" + ", ";
                                }
                            }
                        }
                    }

                    klassenleitungen = klassenleitungen.TrimEnd();
                    klassenleitungen = klassenleitungen.TrimEnd(',');
                    klassenleitungen = klassenleitungen + @"\\ ";
                }

                // Auf Anrechnungen prüfen

                if (lehrer.Anrechnungen.Count > 0)
                {
                    aufgaben += @"**Aufgabenbereich:** ";

                    foreach (var anrechnung in (from l in lehrer.Anrechnungen
                                                where l.Beschr != "Interessen"
                                                where l.Grund != 200 // Altersermäßigung
                                                where l.Grund != 885 // Rundungsgewinne
                                                where l.Grund != 210 // Schwerbehinderung
                                                select l).OrderBy(x => x.Text))
                    {
                        string wert = "";

                        if (anrechnung.Wert > 0 && anrechnung.Grund == 500)
                        {
                            wert = " (" + anrechnung.Wert.ToString() + " Anrechnungsstunde" + (anrechnung.Wert == 1 ? "" : "n");

                            if (anrechnung.Von.Year > 1)
                            {
                                wert += " ab " + anrechnung.Von.ToShortDateString();
                            }
                            if (anrechnung.Bis.Year > 1)
                            {
                                wert += " bis " + anrechnung.Bis.ToShortDateString();
                            }
                            wert += ")";
                        }

                        // Wiki-Link?

                        var text = anrechnung.TextGekürzt;

                        if (anrechnung.Beschr != "")
                        {
                            text = "[[:" + anrechnung.Beschr.Replace("fachschaft:","fachschaften:") + "|" + anrechnung.TextGekürzt + "]]";
                        }

                        aufgaben += " " + text + wert + @",";
                    }

                    aufgaben = aufgaben.TrimEnd();
                    aufgaben = aufgaben.TrimEnd(',');
                    aufgaben = aufgaben + @"\\ ";
                }

                // Auf Interessen prüfen

                if ((from l in lehrer.Anrechnungen where l.Beschr == "Interessen" select l).Any())
                {
                    interessen += @"**Interessen:** ";

                    foreach (var anrechnung in (from l in lehrer.Anrechnungen where l.Beschr == "Interessen" select l).OrderBy(x => x.Text))
                    {

                        var text = anrechnung.TextGekürzt;
                        interessen += " " + text + @",";
                    }

                    interessen = interessen.TrimEnd();
                    interessen = interessen.TrimEnd(',');
                }

                File.AppendAllText(kollegiumNeu, "|{{:lul:lul-fotos:" + lehrer.Kürzel + ".jpg?nolink&100|}}| **" + (lehrer.Titel != "" ? lehrer.Titel + " " : "") + lehrer.Nachname + ", " + lehrer.Vorname + @"** (" + lehrer.Kürzel + @")\\ [[" + lehrer.Mail + @"]]\\ [[chat>" + lehrer.Mail.Replace("@berufskolleg-borken.de", "") + " | " + lehrer.Vorname + " " + lehrer.Nachname + @"]]| " + (lehrer.Deputat == 0 ? "" : "**Deputat:** " + lehrer.Deputat + @" Stunden\\ ") + bildungsgaenge + faecher + klassenleitungen + aufgaben + interessen + "| " + Environment.NewLine);
            }

            File.AppendAllText(kollegiumNeu, "" + Environment.NewLine);
            File.AppendAllText(kollegiumNeu, "Seite erstellt mit [[github>stbaeumer/Push2Dokuwiki|Push2Dokuwiki]]." + Environment.NewLine);

            File.AppendAllText(kollegiumNeu, "{{tag>Zuständigkeiten Personal Klassenleitung}}" + Environment.NewLine);

            Global.DateiTauschen(dokuwikiPfadUndDatei, kollegiumNeu);
        }

        internal List<Lehrer> Lehrerinnen()
        {
            List<Lehrer> members = new List<Lehrer>();

            foreach (var lehrer in this)
            {
                if (lehrer.Geschlecht == "w" && lehrer.Deputat > 0)
                {
                    if (!members.Contains(lehrer))
                    {
                        members.Add(lehrer);
                    }
                }
            }
            return members;
        }

        internal void DateiSprechtagErzeugen(Raums raums, Unterrichts unterrichts, string dateiSprechtag, string sprechtagNeu, Klasses klasses)
        {
            var alleLehrerImUnterrichtKürzel = (from u in unterrichts select u.LehrerKürzel).Distinct().ToList();

            var alleLehrerImUnterricht = new Lehrers();
            var vergebeneRäume = new Raums();

            foreach (var lehrer in this.OrderBy(x => x.Nachname).ThenBy(x => x.Vorname))
            {
                if ((from l in alleLehrerImUnterrichtKürzel where lehrer.Kürzel == l select l).Any())
                {
                    // Wenn Raum und Text2 leer sind, dann wird der Lehrer ignoriert 

                    if (!((lehrer.Raum == null || lehrer.Raum == "") && lehrer.Text2 == ""))
                    {
                        alleLehrerImUnterricht.Add(lehrer);

                        var r = (from v in vergebeneRäume where v.Raumnummer == lehrer.Raum select v).FirstOrDefault();

                        if (r == null)
                        {
                            if (lehrer.Raum != null)
                            {
                                vergebeneRäume.Add(new Raum(lehrer.Raum));
                            }
                        }
                        else
                        {
                            r.Anzahl++;
                        }
                    }
                }
            }

            // Hier weitere TN an Sprechtag einbauen

            alleLehrerImUnterricht.Add(new Lehrer("Kessens", "", "w", "Landwirtschaftskammer NRW", "3307"));
            alleLehrerImUnterricht.Add(new Lehrer("Wenz", "Dr.", "w", "Landwirtschaftskammer NRW", "3301"));
            alleLehrerImUnterricht.Add(new Lehrer("Plaßmann", "", "m", "Schulleiter, bitte im Schulbüro melden.", "1014"));

            File.WriteAllText(sprechtagNeu, "====== Sprechtag ======" + Environment.NewLine);
            File.AppendAllText(sprechtagNeu, Environment.NewLine);

            File.AppendAllText(sprechtagNeu, "Zum jährlichen Sprechtag laden wir sehr herzlich am Mittwoch nach der Zeugnisausgabe in der Zeit von 13:30 bis 17:30 Uhr ein. Der Unterricht endet nach der 5. Stunde um 12:00 Uhr." + Environment.NewLine);

            int i = 1;
            File.AppendAllText(sprechtagNeu, Environment.NewLine);
            File.AppendAllText(sprechtagNeu, "<WRAP column 15em>" + Environment.NewLine);
            File.AppendAllText(sprechtagNeu, Environment.NewLine);
            File.AppendAllText(sprechtagNeu, "^Name^Raum^" + Environment.NewLine);

            var lehrerProSpalteAufSeite2 = ((alleLehrerImUnterricht.Count - 60) / 3) + 1;

            foreach (var l in alleLehrerImUnterricht.OrderBy(x => x.Nachname))
            {
                File.AppendAllText(sprechtagNeu, "|" + (l.Geschlecht == "m" ? "Herr " : "Frau ") + (l.Titel == "" ? "" : l.Titel + " ") + l.Nachname + (l.Text2 == "" ? "" : " ((" + l.Text2 + "))") + "|" + (l.Raum == "" ? "|" : l.Raum + "|") + Environment.NewLine);

                if (i == 20 || i == 40 || i == 60 || i == 60 + lehrerProSpalteAufSeite2 || i == 60 + lehrerProSpalteAufSeite2 * 2)
                {
                    File.AppendAllText(sprechtagNeu, "</WRAP>" + Environment.NewLine);
                    File.AppendAllText(sprechtagNeu, Environment.NewLine);

                    if (i == 60)
                    {
                        File.AppendAllText(sprechtagNeu, "<pagebreak>" + Environment.NewLine);
                    }

                    File.AppendAllText(sprechtagNeu, "<WRAP column 15em>" + Environment.NewLine);
                    File.AppendAllText(sprechtagNeu, Environment.NewLine);
                    File.AppendAllText(sprechtagNeu, "^Name^Raum^" + Environment.NewLine);
                }
                i++;
            }

            File.AppendAllText(sprechtagNeu, "</WRAP>" + Environment.NewLine);
            File.AppendAllText(sprechtagNeu, Environment.NewLine);

            File.AppendAllText(sprechtagNeu, "Klassenleitungen finden die Einladung als Kopiervorlage im [[sharepoint>:f:/s/Kollegium2/EjakJvXmitdCkm_iQcqOTLwB-9EWV5uqXE8j3BrRzKQQAw?e=OwxG0N|Sharepoint]].\r\n" + Environment.NewLine);

            File.AppendAllText(sprechtagNeu, Environment.NewLine);
            File.AppendAllText(sprechtagNeu, "{{tag>Termine}}" + Environment.NewLine);


            if (Global.DateiTauschen(dateiSprechtag, sprechtagNeu))
            {
                // freie Räume suchen

                Console.WriteLine("Freie Räume für Sprechtag");
                Console.WriteLine("=========================");
                string freieR = "";
                foreach (var raum in raums.OrderBy(x => x.Raumnummer))
                {
                    if (!(from v in vergebeneRäume where v.Raumnummer == raum.Raumnummer select v).Any())
                    {
                        freieR += raum.Raumnummer + ",";
                    }
                }
                Console.WriteLine(freieR.TrimEnd(','));
            }
        }

        internal List<Lehrer> GetAnrechungenAusBeschreibung(string name)
        {
            List<Lehrer> a = new List<Lehrer>();


            foreach (var lehrer in this)
            {
                var x = lehrer.GetAnrechnungAusBeschreibung(name);

                if (x != null)
                {
                    var zzzz = (from l in this where l.Mail == x select l).FirstOrDefault();
              
                    if (!a.Contains(zzzz))
                    {
                        a.Add(zzzz);
                    }
                }
            }

            if (a.Count == 0)
            {
                //a.Add("N.N.");
                //a.Add("N.N.");
            }
            return a;
        }


        internal bool istLehrer(Lehrer istMember)
        {
            foreach (var item in this)
            {
                if (item.Mail == istMember.Mail)
                {
                    return true;
                }
            }
            return false;
        }

        internal IEnumerable<string> Teilzeitkraefte()
        {
            var members = new List<string>();

            foreach (var teilzeitkraft in (from l in this where l.Deputat < 25.5 select l))
            {
                if (!members.Contains(teilzeitkraft.Mail))
                {
                    members.Add(teilzeitkraft.Mail);
                }
            }
            return members;
        }

        internal IEnumerable<string> Vollzeitkraefte()
        {
            var members = new List<string>();

            foreach (var vollzeitkraft in (from l in this where l.Deputat >= 25.5 select l))
            {
                if (!members.Contains(vollzeitkraft.Mail))
                {
                    members.Add(vollzeitkraft.Mail);
                }
            }
            return members;
        }
    }
}