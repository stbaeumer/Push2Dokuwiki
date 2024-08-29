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

        public Teams()
        {
        }

        internal void GruppenUndMitgliederToCsv(string datei)
        {
            UTF8Encoding utf8NoBom = new UTF8Encoding(false);
            var filePath = Global.Dateipfad + datei;

            File.WriteAllText(filePath, "\"Page\",\"Mitglieder\",\"MitgliederMail\",\"MitgliederKuerzel\"" + Environment.NewLine, utf8NoBom);

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

                        File.AppendAllText(filePath, zeile + Environment.NewLine, utf8NoBom);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
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

        

        

        

        