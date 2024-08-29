using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace Push2Dokuwiki
{
    public class Bildungsgangs : List<Bildungsgang>
    {
        public Bildungsgangs()
        {
        }

        public Bildungsgangs(Unterrichts unterrichts, Anrechnungs anrechnungs, Lehrers lehrers)
        {
            // Die verschiedenen Bildungsgänge werden aus den Anrechnungen ermittelt.

            var bildungsgaenge = (from a in anrechnungs where a.Text.Contains("Bildungsgangleitung") where a.Beschr.StartsWith("bildungsgaenge:") select a.Beschr).Distinct().ToList();

            foreach (var b in bildungsgaenge)
            {
                Bildungsgang bildungsgang = new Bildungsgang();
                bildungsgang.ExtractBetweenLastColons(b);
                bildungsgang.WikiLink = b;
                bildungsgang.ExtractBetweenFirstAndSecondColon(b);
                bildungsgang.Members.AddRange(unterrichts.GetMembers(bildungsgang, lehrers, new List<int>() {1,2,3,4}));
                this.Add(bildungsgang);                    
            }

            // Jede Schulform (Berufsschule, Höhere ... ) bildet einen Bildungsgang

            foreach (var schulform in (from t in this select t.Schulform).Distinct().ToList())
            {
                Bildungsgang bildungsgang = new Bildungsgang();
                bildungsgang.Kurzname = schulform;
                bildungsgang.WikiLink = "bildungsgaenge:" + schulform + ":start";
                bildungsgang.Members.AddRange((from t in this from m in t.Members where t.Schulform == schulform select m).Distinct().ToList());
                this.Add(bildungsgang);
            }
        }
    }
}