using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Push2Dokuwiki
{
    public class Teams : List<Team>
    {
        private Anrechnungs anrechnungen;
        private Lehrers lehrers;
        private Unterrichts unterrichts;
        private Anrechnungs anrechnungs;

        public Teams(Klasses klasses, Lehrers lehrers, Schuelers schuelers, Unterrichts unterrichts, string klasseOderBg)
        {
            try
            {
                foreach (var klasse in (from k in klasses where (k.Klassenleitungen != null && k.Klassenleitungen.Count > 0 && k.Klassenleitungen[0] != null && k.NameUntis.Any(char.IsDigit)) select k))
                {
                    // Wenn das Team mit diesem Namen schon angelegt wurde, wird es nicht nochmal angelegt

                    if (!(from t in this where t.Kurzname == klasse.BildungsgangGekürzt select t).Any())
                    {
                        Team team = new Team();

                        if (klasseOderBg == "Klasse")
                        {
                            team.Langname = klasse.NameUntis;
                            team.Kurzname = klasse.BildungsgangGekürzt;
                        }
                        else
                        {
                            team.Langname = klasse.BildungsgangLangname;
                            team.Kurzname = klasse.BildungsgangGekürzt;
                        }

                        team.Kategorie = klasseOderBg;

                        Lehrers unterrL = new Lehrers();

                        foreach (var unterricht in unterrichts)
                        {
                            if (unterricht.KlasseKürzel == klasse.NameUntis)
                            {
                                if ((from u in unterrL where u.Kürzel == unterricht.LehrerKürzel select u).Any())
                                {
                                    var ll = (from l in lehrers where l.Kürzel == unterricht.LehrerKürzel select l).FirstOrDefault();

                                    unterrL.Add(ll);
                                }
                            }
                        }

                        var unterrichtendeLehrer = (from l in lehrers
                                                    where (from u in unterrichts where u.KlasseKürzel.Split(',').Contains(klasse.NameUntis) select u.LehrerKürzel).ToList().Contains(l.Kürzel)
                                                    where l.Mail != null
                                                    where l.Mail != ""
                                                    select l).ToList();

                        foreach (var unterrichtenderLehrer in unterrichtendeLehrer)
                        {
                            if (!team.Members.Contains(unterrichtenderLehrer))
                            {
                                team.Members.Add(unterrichtenderLehrer); // Owner müssen immer auch member sein.
                            }
                        }

                        team.Schuelers = (from s in schuelers
                                          where s.Klasse.NameUntis == klasse.NameUntis
                                          select s).ToList();

                        if (team.Members.Count() > 0)
                        {
                            this.Add(team);
                        }
                    }
                }

                Global.WriteLine(klasseOderBg + "Soll", this.Count);
                Console.WriteLine("");
            }
            catch (Exception ex)
            {
                throw ex; ;
            }
        }

        internal void DateiPraktiumErzeugen(string tempdatei)
        {
            var datei = Global.Dateipfad + tempdatei;
            int lastSlashIndex = tempdatei.LastIndexOf('\\');
            string result = (lastSlashIndex != -1) ? tempdatei.Substring(lastSlashIndex + 1) : tempdatei;
            tempdatei = System.IO.Path.GetTempPath() + result;

            string schuelerName = "";

            File.WriteAllText(datei, "====== Praktikumsbetreuung ======" + Environment.NewLine);

            foreach (var teamSoll in (from t in this where t.Kategorie == "Klasse" select t).ToList())
            {
                // nur relevante Klassen

                if (
teamSoll.Langname.EndsWith("LuL") &&
(teamSoll.Langname.StartsWith("BT") ||
teamSoll.Langname.StartsWith("BW") ||
teamSoll.Langname.StartsWith("BS") ||
teamSoll.Langname.StartsWith("IFK") ||
teamSoll.Langname.StartsWith("G") ||
teamSoll.Langname.StartsWith("HB")))
                {
                    schuelerName += "" + Environment.NewLine;
                    schuelerName += "===== Praktikumsbetreuung " + teamSoll.Langname.Replace("-LuL", "") + " =====" + Environment.NewLine;
                    schuelerName += "^Nr. ^Schüler*in               ^Betrieb/Kontaktdaten^Lehrkraft Name^" + Environment.NewLine;

                    int i = 1;

                    foreach (var sch in teamSoll.Schuelers)
                    {
                        schuelerName += "|" + i.ToString().PadLeft(4) + "|" + (sch.Nachname + ", " + sch.Vorname).PadRight(25) + "|                    |              |" + Environment.NewLine;
                        i++;
                    }
                }
            }

            File.AppendAllText(datei, "" + Environment.NewLine);
            File.AppendAllText(datei, "Seite erstellt mit [[github>stbaeumer/Push2Dokuwiki|Push2Dokuwiki]]." + Environment.NewLine);

            File.AppendAllText(datei, schuelerName + Environment.NewLine);
            Process.Start("notepad++.exe", datei);
        }
                
        public Teams(Bildungsgangs bildungsgangs)
        {
            foreach (var bildungsgang in bildungsgangs)
            {
                Team team = new Team();                
                team.Kurzname = bildungsgang.Kurzname;
                team.Kategorie = "Bildungsgang";
                team.Members.AddRange(bildungsgang.Members);
                team.WikiLink = bildungsgang.WikiLink;
                this.Add(team);
            }
        }

        public Teams(Klasses klasses, Unterrichts unterrichts, Anrechnungs anrechnungs, Lehrers lehrers)
        {
            // Die Bildungsgaenge werden aus den Anrechnungen ermittelt. Bildungsgang ist, was das Wort "Bildungsgangleitung" im Text in Untis enthält und einen Link 'bildungsgaenge: ...' in der Beschr.   

            var bildungsgangs = new Bildungsgangs(unterrichts, anrechnungs, lehrers);

            this.AddRange(new Teams(bildungsgangs));

            // Jahrgangsspezifische Klassenteams

            this.Add(new Team("versetzung:blaue_briefe", new List<string>() { "BS", "HBG", "HBT", "HBW", "FS" }, new List<int>() { 1 }, unterrichts, lehrers));
            this.Add(new Team("abitur:start", new List<string>() { "GG", "GT", "GW" }, new List<int>() { 3 }, unterrichts, lehrers));
            this.Add(new Team("termine:fhr:start", new List<string>() { "BS", "HBG", "HBT", "HBW", "FS", "FM" }, new List<int>() { 2 }, unterrichts, lehrers));

            // Fachschaften

            this.Add(new Team("Fachschaft Deutsch", "Fachschaft Deutsch", "Vorsitz", ":fachschaften:deutsch_kommunikation", unterrichts.Fachschaften(lehrers, new List<string>() { "E", "E FU", "E1", "E2", "E G1", "E G2", "E L1", "E L2", "E L", "EL", "EL1", "EL2" }), anrechnungs));
            this.Add(new Team("Fachschaft Englisch", "Fachschaft Englisch", "Vorsitz", ":fachschaften:englisch", unterrichts.Fachschaften(lehrers, new List<string>() { "E", "E FU", "E1", "E2", "E G1", "E G2", "E L1", "E L2", "E L", "EL", "EL1", "EL2" }), anrechnungs));
            this.Add(new Team("Fachschaft Religion", "Fachschaft Religion", "Vorsitz", ":fachschaften:religionslehre", unterrichts.Fachschaften(lehrers, new List<string>() { "KR", "KR FU", "KR1", "KR2", "KR G1", "KR G2", "ER", "ER G1" }), anrechnungs));
            this.Add(new Team("Fachschaft Mathematik", "Fachschaft Mathematik", "Vorsitz", ":fachschaften:mathematik_physik", unterrichts.Fachschaften(lehrers, new List<string>() { "M", "M FU", "M1", "M2", "M G1", "M G2", "M L1", "M L2", "M L", "ML", "ML1", "ML2" }), anrechnungs));
            this.Add(new Team("Fachschaft Politik", "Fachschaft Politik", "Vorsitz", ":fachschaften:politik_gesellschaftslehre", unterrichts.Fachschaften(lehrers, new List<string>() { "PK", "PK FU", "PK1", "PK2", "GG G1", "GG G2" }), anrechnungs));
            this.Add(new Team("Fachschaft Wirtschaftslehre", "Fachschaft Wirtschaftslehre", "Vorsitz", ":fachschaften:wirtschaftslehre_in_nicht_kaufm_klassen", unterrichts.Fachschaften(lehrers, new List<string>() { "WL", "WBL" }), anrechnungs));
            this.Add(new Team("Fachschaft Sport", "Fachschaft Sport", "Vorsitz", ":fachschaften:sport", unterrichts.Fachschaften(lehrers, new List<string>() { "SP", "SP G1", "SP G2" }), anrechnungs));
            this.Add(new Team("Fachschaft Biologie", "Fachschaft Biologie", "Vorsitz", ":fachschaften:biologie", unterrichts.Fachschaften(lehrers, new List<string>() { "BI", "Bi", "Bi FU", "Bi1", "Bi G1", "Bi G2", "BI G1", "BI L1" }), anrechnungs));

            this.Add(new Team("Kollegium", "Kollegium", "Schulleiter", ":Kollegium", lehrers, anrechnungs));
            this.Add(new Team("Bildungsgangleitungen", "Bildungsgangleitungen", "", "Bildungsgangleitung", anrechnungs.LuL(lehrers, "Bildungsgangleitung"), anrechnungs));
            this.Add(new Team("Erweiterte Schulleitung", "Erweiterte Schulleitung", "", ":geschaeftsverteilungsplan:erweiterte_schulleitung", anrechnungs.LuL(lehrers, "Erweiterte Schulleitung"), anrechnungs));
            this.Add(new Team("Lehrerinnen", "Lehrerinnen", "Ansprechpartnerin für Gleichstellung", ":lehrerinnen", lehrers.Lehrerinnen(), anrechnungs));

            this.Add(new Team("Verbindungslehrkräfte", "SV", "", ":verbindungslehrkraefte", anrechnungs.LuL(lehrers, "Verbindungslehrkräfte"), anrechnungs));
            this.Add(new Team("Referendare", "Referendare", "", ":referendar_innen", lehrers.Referendare(), anrechnungs));
            this.Add(new Team("Klassenleitungen", "Klassenleitungen", "", ":geschaeftsverteilungsplan:klassenleitungen", klasses.GetKlassenleitungen(lehrers), anrechnungs));

        }

        internal void GruppenUndMitgliederToCsv(string datei)
        {
            UTF8Encoding utf8NoBom = new UTF8Encoding(false);
            Global.OrdnerAnlegen(datei);
            
            File.WriteAllText(Global.TempPfad + datei, "\"Page\",\"Mitglieder\",\"MitgliederMail\",\"MitgliederKuerzel\"" + Environment.NewLine, utf8NoBom);

            foreach (var team in this.ToList())
            {
                if (team.WikiLink != null && team.WikiLink != "")
                {
                    try
                    {
                        string zeile = "\"" + team.WikiLink + "\",\"";

                        try
                        {
                            foreach (var member in team.Members.OrderBy(x => x.Nachname))
                            {
                                try
                                {
                                    zeile += member.Mail + "; ";
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }

                        zeile = zeile.Trim(';');
                        zeile += "\",\"";

                        zeile = zeile.Trim();

                        
                        try
                        {
                            foreach (var member in team.Members.OrderBy(x => x.Nachname))
                            {
                                try
                                {
                                    zeile += member.Mail + ", ";
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }

                        zeile = zeile.Trim(',');
                        zeile += "\",\"";

                        zeile = zeile.Trim();

                        try
                        {
                            foreach (var member in team.Members.OrderBy(x => x.Nachname))
                            {
                                try
                                {
                                    zeile += member.Kürzel + ",";
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }

                        zeile = zeile.Trim(',');
                        zeile += "\"";

                        zeile = zeile.Trim();

                        File.AppendAllText(Global.TempPfad + datei, zeile + Environment.NewLine, utf8NoBom);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
            Global.WriteLine("Gruppen", this.Count);
            Global.Dateischreiben(datei);
        }

        internal void Hinzufügen(Teams teams)
        {
            foreach (var team in teams)
            {
                Add(team);
            }
        }

        internal void AddTeam(List<string> wikiLinkSubStrings, string wikiLink)
        {
            var team = new Team();
            team.WikiLink = wikiLink;
            team.Members = new List<Lehrer>();

            foreach (var t in this)
            {
                foreach (var subString in wikiLinkSubStrings)
                {
                    if (t.WikiLink.Contains(subString))
                    {
                        foreach(var member in team.Members)
                        {
                            if (!(from te in team.Members where te.Mail == member.Mail select te).Any())
                            {
                                team.Members.Add(member);
                            }
                        }
                    }
                }
            }
        }
    }
}

        

        

        

        