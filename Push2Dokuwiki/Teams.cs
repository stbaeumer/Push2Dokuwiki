using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Push2Dokuwiki
{
    public class Teams : List<Team>
    {
        private Anrechnungs anrechnungen;

        public Teams(Klasses klasses, Lehrers lehrers, Schuelers schuelers, Unterrichts unterrichts, string klasseOderBg)
        {
            try
            {
                foreach (var klasse in (from k in klasses where (k.Klassenleitungen != null && k.Klassenleitungen.Count > 0 && k.Klassenleitungen[0] != null && k.NameUntis.Any(char.IsDigit)) select k))
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
                                      where s.Klasse == klasse.NameUntis
                                      select s).ToList();

                    if (team.Members.Count() > 0)
                    {
                        this.Add(team);
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

        internal void DateiPraktiumErzeugen()
        {
            string dateiPraktikum = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + @"\\Praktikum.txt";

            if (File.Exists(dateiPraktikum))
            {
                File.Delete(dateiPraktikum);
            }

            string schuelerName = "";

            File.WriteAllText(dateiPraktikum, "====== Praktikumsbetreuung ======" + Environment.NewLine);

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

            File.AppendAllText(dateiPraktikum, "" + Environment.NewLine);
            File.AppendAllText(dateiPraktikum, "Seite erstellt mit [[github>stbaeumer/Push2Dokuwiki|Push2Dokuwiki]]." + Environment.NewLine);

            File.AppendAllText(dateiPraktikum, schuelerName + Environment.NewLine);
            Process.Start("notepad++.exe", dateiPraktikum);
        }
                
        public Teams(Bildungsgangs bildungsgangs)
        {
            foreach (var bildungsgang in bildungsgangs)
            {
                Team team = new Team();
                team.Langname = bildungsgang.Langname;
                team.Kurzname = bildungsgang.Kurzname;
                team.Kategorie = "Bildungsgang";
                team.Members.AddRange(bildungsgang.Members);
                team.WikiLink = bildungsgang.WikiLink;
                team.Leitung = bildungsgang.Leitung;
                team.Leitungsbezeichnung = "Bildungsgangleitung";
                this.Add(team);
            }
        }

        public Teams()
        {
        }

        internal void DateiGruppenUndMitgliederErzeugen(string dokuwikipfadUndDatei, string dateiGruppenUndMitgliederNeu)
        {
            string hyperlink = "";
            
            File.WriteAllText(dateiGruppenUndMitgliederNeu, "====== Gruppen & Mitglieder ======" + Environment.NewLine);
            File.AppendAllText(dateiGruppenUndMitgliederNeu, "  Bitte diese Seite nicht manuell ändern." + Environment.NewLine);
            File.AppendAllText(dateiGruppenUndMitgliederNeu, "Siehe auch [[:kollegium|Kollegium]]." + Environment.NewLine);

            foreach (var team in (from t in this
                                  where t.Langname != null
                                  where t.Langname != ""
                                  where t.Kategorie == null || !(t.Kategorie.StartsWith("Klasse"))
                                  select t).ToList().OrderBy(s => s.Kategorie).ThenBy(s => s.Langname))
            {
                try
                {
                    string mitgliederMail = "";
                    string mitgliederNachname = "";

                    // Die Schulleiterin wird zuerst genannt. Alle anderen Gruppen bleiben alphabetisch sortiert.

                    if (team != null && (team.Langname == "Schulleitung" || team.Members.Count == 1))
                    {
                        foreach (var member in team.Members.OrderBy(x => x.Vorname))
                        {
                            mitgliederNachname += member.Vorname.Substring(0, 1) + "." + member.Nachname + ", ";
                            hyperlink += "<" + member.Mail + ">" + ", ";
                            mitgliederMail += "<" + member.Mail + ">" + "; ";
                        }
                    }
                    else
                    {
                        try
                        {
                            foreach (var member in team.Members.OrderBy(x => x.Nachname))
                            {
                                mitgliederNachname += member.Vorname.Substring(0, 1) + "." + member.Nachname + ", ";
                                mitgliederMail += "<" + member.Mail + ">; ";
                                hyperlink += member.Mail + ", ";
                            }
                        }
                        catch (Exception)
                        {
                            foreach (var member in team.Members)
                            {
                                if (member.Vorname != null && member.Vorname != "")
                                {
                                    mitgliederNachname += member.Vorname.Substring(0, 1) + "." + member.Nachname + ", ";
                                    mitgliederMail += "<" + member.Mail + ">; ";
                                    hyperlink += member.Mail + ", ";
                                }
                            }
                        }
                    }

                    hyperlink = hyperlink.TrimEnd(',');

                    mitgliederMail = mitgliederMail.TrimEnd(' ');
                    mitgliederNachname = mitgliederNachname.TrimEnd(' ');
                    mitgliederNachname = mitgliederNachname.TrimEnd(',');

                    string namensraum = ":" + team.Langname;

                    File.AppendAllText(dateiGruppenUndMitgliederNeu, "===== " + team.Langname + (team.Langname== team.Kurzname ? "": " (" + team.Kurzname+")") + " =====" + Environment.NewLine);

                    File.AppendAllText(dateiGruppenUndMitgliederNeu, "[[:" + team.WikiLink + "|" + team.Langname + "]]" + Environment.NewLine);

                    if (team.Leitung != null)
                    {
                        File.AppendAllText(dateiGruppenUndMitgliederNeu, Environment.NewLine + team.Leitungsbezeichnung + ": [[" + team.Leitung.Mail + "|" + team.Leitung.Vorname + " " + team.Leitung.Nachname + "]]" + Environment.NewLine);
                    }
                    File.AppendAllText(dateiGruppenUndMitgliederNeu, "| " + mitgliederNachname + "|" + mitgliederMail + " |" + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            File.AppendAllText(dateiGruppenUndMitgliederNeu, "" + Environment.NewLine);
            File.AppendAllText(dateiGruppenUndMitgliederNeu, "Seite erstellt mit [[github>stbaeumer/Push2Dokuwiki|Push2Dokuwiki]]." + Environment.NewLine);

            Global.DateiTauschen(dokuwikipfadUndDatei, dateiGruppenUndMitgliederNeu);
        }

        internal void Hinzufügen(Teams teams)
        {
            foreach (var team in teams)
            {
                Add(team);
            }
        }
    }
}

        

        

        

        