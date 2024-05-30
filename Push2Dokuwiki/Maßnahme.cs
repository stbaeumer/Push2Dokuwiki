﻿using System;

namespace Push2Dokuwiki
{
    public class Maßnahme
    {
        private string v;
        private int fehltUnunterbrochenUnentschuldigtSeitTagen;

        public int SchuelerId { get; private set; }
        public string Beschreibung { get; private set; }
        public DateTime Datum { get; private set; }
        public string Kürzel { get; private set; }
        public string Rechtsgrundlage { get; private set; }
        public int FehltUnunterbrochenUnentschuldigtSeitTagen { get; private set; }
        public string HtmlTabelle { get; private set; }
        public string MöglicheSanktion { get; private set; }
        public string Bezeichnung { get; private set; }

        public Maßnahme(int schuelerId, string beschreibung, string bezeichnung, DateTime datum, string kürzel)
        {
            SchuelerId = schuelerId;
            Beschreibung = beschreibung;
            Bezeichnung = bezeichnung;
            Datum = datum;
            Kürzel = kürzel;
        }

        public Maßnahme(string kürzel, string rechtsgrundlage, int fehltUnunterbrochenUnentschuldigtSeitTagen, string htmlTabelle, string möglicheSanktion)
        {
            Kürzel = kürzel;
            Rechtsgrundlage = rechtsgrundlage;
            FehltUnunterbrochenUnentschuldigtSeitTagen = fehltUnunterbrochenUnentschuldigtSeitTagen;
            HtmlTabelle = htmlTabelle;
            MöglicheSanktion = möglicheSanktion;
        }

        public Maßnahme()
        {
        }
    }
}