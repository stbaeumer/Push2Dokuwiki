using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Push2Dokuwiki
{
    public class Raum
    {
        private string raum;

        public Raum()
        {
        }

        public Raum(string raum)
        {
            Raumnummer = raum;
            Anzahl = 1;
        }

        public int IdUntis { get; internal set; }
        public string Raumnummer { get; internal set; }
        public int Anzahl { get; internal set; }
    }
}
