using System;

namespace Push2Dokuwiki
{
    public class Anrechnung
    {
        public double Wert { get; set; }
        public string Text { get; set; }
        public int Grund { get; set; }
        public DateTime Von { get; set; }
        public DateTime Bis { get; set; }
        public string Beschr { get; set; }
        public int TeacherIdUntis { get; internal set; }
        /// <summary>
        /// Die öffende runde Klammer und alles danach wird abgeschnitten.
        /// </summary>
        public string TextGekürzt { get; internal set; }
    }
}