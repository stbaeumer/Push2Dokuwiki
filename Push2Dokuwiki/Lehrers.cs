using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Push2Dokuwiki
{
    public class Lehrers : List<Lehrer>
    {
        public Lehrers()
        {
        }

        public Lehrers(int periode, Raums raums)
        {
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
                                    // Bei Nicht-Lehrern ist das Geb.Dat. egal
                                    if (lehrer.Deputat > 0)
                                    {
                                        if (lehrer.Kürzel != "MOR" && lehrer.Kürzel != "TIS")
                                        {
                                            Console.WriteLine(" " + lehrer.Kürzel + ": Kein Geburtsdatum");
                                        }
                                    }
                                }

                                if (lehrer.Geburtsdatum.Year > 1)
                                {
                                    lehrer.AlterAmErstenSchultagDiesesJahres = lehrer.GetAlterAmErstenSchultagDiesesJahres();
                                    lehrer.ProzentStelle = lehrer.GetProzentStelle();
                                    //lehrer.CheckAltersermäßigung();
                                }

                                this.Add(lehrer);
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
                    Global.WriteLine("Lehrer", this.Count);
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

        internal void Anrechnungen(string tempdatei, Klasses klasses)
        {
            var datei = Global.Dateipfad + tempdatei;
            int lastSlashIndex = tempdatei.LastIndexOf('\\');
            string result = (lastSlashIndex != -1) ? tempdatei.Substring(lastSlashIndex + 1) : tempdatei;
            tempdatei = System.IO.Path.GetTempPath() + result;

            string anrechnungstring = "";

            File.WriteAllText(tempdatei, "====== Verteilung des Lehrertopfs ======" + Environment.NewLine);
            File.AppendAllText(tempdatei, Environment.NewLine);

            File.AppendAllText(tempdatei, "Zweimal jährlich wird hier die Verteilung der Anrechnungen aktualisiert. Die [[termine:termine|Termine]] werden rund um den 15. Dezember und den 1. März festgelegt." + Environment.NewLine);
            File.AppendAllText(tempdatei, Environment.NewLine);

            File.AppendAllText(tempdatei, "Die Verteilung erfolgt durch den [[sl:schulleiter|Schulleiter]] nach den [[sl:grundsaetze_der_verteilung_der_anrechungsstunden|Grundsätzen der Verteilung]]. Der [[:lehrerrat|Lehrerrat]] berät vor der Veröffentlichung." + Environment.NewLine);
            File.AppendAllText(tempdatei, Environment.NewLine);

            File.AppendAllText(tempdatei, "In Abhängigkeit von den [[sl:Personalia|Schülerzahlen]] stehen dem BKB Anrechnungsstunden für alle Kolleginnen und Kollegen zur Verfügung. Mit [[sl:personalia|sinkenden Schülerzahlen]] schrumpft dieser Topf. Siehe auch [[sl:personalia|Personalia]]." + Environment.NewLine);
            File.AppendAllText(tempdatei, Environment.NewLine);

            File.AppendAllText(tempdatei, "Es wird angestrebt, zu Beginn eines jeden Schuljahres 10% der Anrechnungsstunden für die unterjährige Vergabe zurückzuhalten." + Environment.NewLine);
            File.AppendAllText(tempdatei, Environment.NewLine);

            File.AppendAllText(tempdatei, "  Diese Datei bitte nicht manuell editieren." + Environment.NewLine);
            File.AppendAllText(tempdatei, Environment.NewLine);
            File.AppendAllText(tempdatei, Environment.NewLine);

            File.AppendAllText(tempdatei, "<searchtable>" + Environment.NewLine);
            File.AppendAllText(tempdatei, Environment.NewLine);

            File.AppendAllText(tempdatei, "^Name^Grund^Wert^Von^Bis^" + Environment.NewLine);
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

            File.AppendAllText(tempdatei, anrechnungstring + Environment.NewLine);

            File.AppendAllText(tempdatei, "</searchtable>" + Environment.NewLine);
            File.AppendAllText(tempdatei, Environment.NewLine);

            File.AppendAllText(tempdatei, "" + Environment.NewLine);
            File.AppendAllText(tempdatei, "Seite erstellt mit [[github>stbaeumer/Push2Dokuwiki|Push2Dokuwiki]]." + Environment.NewLine);

            Global.Dateischreiben("Anrechnungen");
        }

        internal void DateiKollegiumErzeugen(string tempdatei, Unterrichts unterrichts, Klasses klasses, Fachschaften fachschaften, Bildungsgangs bildungsgangs)
        {
            var datei = Global.Dateipfad + tempdatei;
            int lastSlashIndex = tempdatei.LastIndexOf('\\');
            string result1 = (lastSlashIndex != -1) ? tempdatei.Substring(lastSlashIndex + 1) : tempdatei;
            tempdatei = System.IO.Path.GetTempPath() + result1;

            File.WriteAllText(tempdatei, "====== Kollegium ======" + Environment.NewLine);
            File.AppendAllText(tempdatei, Environment.NewLine);
            File.AppendAllText(tempdatei, "  Diese Datei bitte nicht manuell editieren." + Environment.NewLine);
            File.AppendAllText(tempdatei, Environment.NewLine);
            File.AppendAllText(tempdatei, "Siehe auch [[kollegium:gruppen|Gruppen & Mitglieder]]." + Environment.NewLine);

            File.AppendAllText(tempdatei, Environment.NewLine);
            //File.AppendAllText(dateiKollegium, "^  Foto  ^  Name  ^ ^" + Environment.NewLine);

            foreach (var lehrer in this.OrderBy(x => x.Nachname))
            {
                //File.AppendAllText(dateiKollegium,"==== " + lehrer.Kürzel + " ====" + Environment.NewLine);
                //File.AppendAllText(dateiKollegium, "" + Environment.NewLine);

                string aufgaben = "";
                string klassenleitungen = "";
                string faecher = "";
                string bildungsgaenge = "";
                string bereichsleitung = "";
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
                            text = "[[:" + anrechnung.Beschr.Replace("fachschaft:", "fachschaften:") + "|" + anrechnung.TextGekürzt + "]]";
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

                File.AppendAllText(tempdatei, "|{{:lul:lul-fotos:" + lehrer.Kürzel + ".jpg?nolink&100|}}| **" + (lehrer.Titel != "" ? lehrer.Titel + " " : "") + lehrer.Nachname + ", " + lehrer.Vorname + @"** (" + lehrer.Kürzel + @")\\ [[" + lehrer.Mail + @"]]\\ [[chat>" + lehrer.Mail.Replace("@berufskolleg-borken.de", "") + " | " + lehrer.Vorname + " " + lehrer.Nachname + @"]]| " + (lehrer.Deputat == 0 ? "" : "**Deputat:** " + lehrer.Deputat + @" Stunden\\ ") + bildungsgaenge + faecher + klassenleitungen + aufgaben + interessen + "| " + Environment.NewLine);
            }

            File.AppendAllText(tempdatei, "" + Environment.NewLine);
            File.AppendAllText(tempdatei, "Seite erstellt mit [[github>stbaeumer/Push2Dokuwiki|Push2Dokuwiki]]." + Environment.NewLine);

            Global.Dateischreiben("DateiKollegium");
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

        internal void Sprechtag(string datei, Raums raums, Klasses klasses, Unterrichts unterrichts, Lehrers alleLehrerImUnterricht, string hinweis)
        {
            Global.OrdnerAnlegen(datei);

            var alleLehrerImUnterrichtKürzel = (from u in unterrichts select u.LehrerKürzel).Distinct().ToList();

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

            File.WriteAllText(Global.TempPfad + datei, "====== Sprechtag ======" + Environment.NewLine);
            File.AppendAllText(Global.TempPfad + datei, Environment.NewLine);

            File.AppendAllText(Global.TempPfad + datei, hinweis + Environment.NewLine);

            int i = 1;
            File.AppendAllText(Global.TempPfad + datei, Environment.NewLine);
            File.AppendAllText(Global.TempPfad + datei, "<WRAP column 15em>" + Environment.NewLine);
            File.AppendAllText(Global.TempPfad + datei, Environment.NewLine);
            File.AppendAllText(Global.TempPfad + datei, "^Name^Raum^" + Environment.NewLine);

            var lehrerProSpalteAufSeite2 = ((alleLehrerImUnterricht.Count - 60) / 3) + 1;

            foreach (var l in alleLehrerImUnterricht.OrderBy(x => x.Nachname))
            {
                File.AppendAllText(Global.TempPfad + datei, "|" + (l.Geschlecht == "m" ? "Herr " : "Frau ") + (l.Titel == "" ? "" : l.Titel + " ") + l.Nachname + (l.Text2 == "" ? "" : " ((" + l.Text2 + "))") + "|" + (l.Raum == "" ? "|" : l.Raum + "|") + Environment.NewLine);

                if (i == 20 || i == 40 || i == 60 || i == 60 + lehrerProSpalteAufSeite2 || i == 60 + lehrerProSpalteAufSeite2 * 2)
                {
                    File.AppendAllText(Global.TempPfad + datei, "</WRAP>" + Environment.NewLine);
                    File.AppendAllText(Global.TempPfad + datei, Environment.NewLine);

                    if (i == 60)
                    {
                        File.AppendAllText(Global.TempPfad + datei, "<WRAP pagebreak>" + Environment.NewLine);
                    }

                    File.AppendAllText(Global.TempPfad + datei, "<WRAP column 15em>" + Environment.NewLine);
                    File.AppendAllText(Global.TempPfad + datei, Environment.NewLine);
                    File.AppendAllText(Global.TempPfad + datei, "^Name^Raum^" + Environment.NewLine);
                }
                i++;
            }

            File.AppendAllText(Global.TempPfad + datei, "</WRAP>" + Environment.NewLine);
            File.AppendAllText(Global.TempPfad + datei, Environment.NewLine);

            File.AppendAllText(Global.TempPfad + datei, "Klassenleitungen finden die Einladung als Kopiervorlage im [[sharepoint>:f:/s/Kollegium2/EjakJvXmitdCkm_iQcqOTLwB-9EWV5uqXE8j3BrRzKQQAw?e=OwxG0N|Sharepoint]].\r\n" + Environment.NewLine);

            File.AppendAllText(Global.TempPfad + datei, Environment.NewLine);

            string freieR = "";
            foreach (var raum in raums.OrderBy(x => x.Raumnummer))
            {
                if (!(from v in vergebeneRäume where v.Raumnummer == raum.Raumnummer select v).Any())
                {
                    freieR += raum.Raumnummer + ",";
                }
            }

            freieR = Global.InsertLineBreaks(freieR, 110);

            Console.WriteLine(@"Sprechtag: Freie Räume müssen in Untis in den Lehrer-Stammdaten eingetragen werden:
" + freieR.TrimEnd(','));

            Global.Dateischreiben(datei);
        }

        internal void Csv(string datei)
        {
            UTF8Encoding utf8NoBom = new UTF8Encoding(false);

            File.WriteAllText(Global.TempPfad + datei, "\"Kürzel\",\"Vorname\",\"Nachname\",\"Name\",\"Mail\"" + Environment.NewLine, utf8NoBom);

            foreach (var l in this.OrderBy(x => x.Nachname))
            {
                // Das Deputat unterscheidet LuL von Mitarbeitern
                if (l.Deputat != 0)
                {
                    File.AppendAllText(Global.TempPfad + datei, "\"" + l.Kürzel + "\",\"" + l.Vorname + "\",\"" + l.Nachname + "\",\"" + (l.Titel == "" ? "" : l.Titel + " ") + l.Vorname + " " + l.Nachname + "\",\"" + l.Mail + "\"" + Environment.NewLine, utf8NoBom);
                }
            }
            Global.Dateischreiben(datei);
        }

        internal void AnrechnungenCsv(string tempdatei)
        {
            var datei = Global.Dateipfad + tempdatei;
            int lastSlashIndex = tempdatei.LastIndexOf('\\');
            string result = (lastSlashIndex != -1) ? tempdatei.Substring(lastSlashIndex + 1) : tempdatei;
            tempdatei = System.IO.Path.GetTempPath() + result;

            string anrechnungstring = "";

            File.WriteAllText(tempdatei, "\"Grund\",\"Anzahl Stunden\",\"Hinweise\",\"Lehrkraft\"" + Environment.NewLine);

            foreach (var lehrer in this.OrderBy(x => x.Nachname))
            {
                foreach (var anrechnung in lehrer.Anrechnungen)
                {
                    if (anrechnung.Grund == 500 && anrechnung.Wert > 0)
                    {
                        var hinweise = (anrechnung.Von.Year == 1 ? "" : "von" + anrechnung.Von.ToShortDateString() + " ") + (anrechnung.Bis.Year == 1 ? "" : "bis" + anrechnung.Bis.ToShortDateString());

                        File.AppendAllText(tempdatei, "\"" + anrechnung.Beschr + "\",\"" + anrechnung.Wert + "\",\"" + hinweise + "\",\"" + lehrer.Kürzel + "\"" + Environment.NewLine);
                    }
                }
            }

            Global.Dateischreiben("Anrechnungen");
        }
    }
}