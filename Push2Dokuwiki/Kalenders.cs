using System;
using System.Collections.Generic;

namespace Push2Dokuwiki
{
    public class Kalenders : List<Kalender>
    {
        private string v;

        public Kalenders(string kriterium)
        {
            Console.WriteLine("");
            Console.WriteLine("Hinweise zu Kalendereinträgen:");
            Console.WriteLine(" * Die Nachricht jedes Termins muss '" + kriterium + "' im Nachrichtentext enthalten.");
            Console.WriteLine("    Anderenfalls wird der Termin nicht nach Wiki übertragen.");
            Console.WriteLine(" * Die Outlook-Termin-Kategorien werden zu Wiki-Seiten verlinkt.");
            Console.WriteLine("");
        }
    }
}