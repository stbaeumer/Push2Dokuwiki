using System.Collections.Generic;
using System.Linq;

namespace Push2Dokuwiki
{
    internal class Fachschaften:List<Fachschaft>
    {
        private Fachs fachs;

        public Fachschaften(Fachs fachs)
        {
            foreach (var fach in fachs)
            {
                if (fach.Beschr != "")
                {
                    if (!(from t in this where fach.KürzelUntis == t.KürzelUntis select t).Any())
                    {
                        this.Add(new Fachschaft(fach.KürzelUntis, fach.Beschr));
                    }
                }                
            }
        }
    }
}