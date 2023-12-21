using System;

namespace Push2Dokuwiki
{
    public class Gruppe
    {
        public Gruppe()
        {
        }

        public int MarksPerLessonZeile { get; internal set; }
        public int StudentId { get; internal set; }
        public string Gruppenname { get; internal set; }
        public string Fach { get; internal set; }
        public DateTime Startdate { get; internal set; }
        public DateTime Enddate { get; internal set; }
    }
}