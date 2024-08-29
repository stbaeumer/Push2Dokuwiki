using System.Collections.Generic;

namespace Push2Dokuwiki
{
    public class Bildungsgang
    {
        public Bildungsgang()
        {
            Members = new Lehrers();
        }

        public string Kurzname { get; internal set; }
        public string Langname { get; internal set; }
        public Lehrer Leitung { get; internal set; }
        public string WikiLink { get; internal set; }
        public Lehrers Members { get; internal set; }
        public List<Fach> Fächer { get; internal set; }
        public string WikiLinkSchulform { get; internal set; }
        public string Schulform { get; internal set; }

        public void ExtractBetweenFirstAndSecondColon(string input)
        {
            int firstColonIndex = input.IndexOf(':');
            if (firstColonIndex < 0)
            {
                Schulform = string.Empty; // Rückgabe eines leeren Strings, wenn kein Doppelpunkt vorhanden ist
            }

            int secondColonIndex = input.IndexOf(':', firstColonIndex + 1);
            if (secondColonIndex < 0)
            {
                Schulform = string.Empty; // Rückgabe eines leeren Strings, wenn nur ein Doppelpunkt vorhanden ist
            }

            Schulform = input.Substring(firstColonIndex + 1, secondColonIndex - firstColonIndex - 1);
        }

        public void ExtractBetweenLastColons(string input)
        {
            int lastColonIndex = input.LastIndexOf(':');
            if (lastColonIndex < 0)
            {
                Kurzname = string.Empty; // Rückgabe eines leeren Strings, wenn kein Doppelpunkt vorhanden ist
            }

            int secondLastColonIndex = input.LastIndexOf(':', lastColonIndex - 1);
            if (secondLastColonIndex < 0)
            {
                Kurzname = string.Empty; // Rückgabe eines leeren Strings, wenn nur ein Doppelpunkt vorhanden ist
            }

            Kurzname = input.Substring(secondLastColonIndex + 1, lastColonIndex - secondLastColonIndex - 1).ToUpper();
        }
    }
}