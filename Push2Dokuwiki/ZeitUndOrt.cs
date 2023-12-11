using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Push2Dokuwiki
{
    class ZeitUndOrt
    {
        public int Tag;
        public int Stunde;
        public List<string> Raum;

        public ZeitUndOrt(int tag, int stunde, List<string> raum)
        {
            this.Tag = tag;
            this.Stunde = stunde;
            this.Raum = raum;
        }
    }
}
