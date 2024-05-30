
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Push2Dokuwiki
{
    public class Unterrichts : List<Unterricht>
    {
        private string sourceExportLessons;
        private DateTime dateTime;

        public Unterrichts()
        {
        }

        public Unterrichts(string kriterium, DateTime dateTime)
        {
            string datei = "";

            try
            {
                datei = Global.CheckFile(kriterium, dateTime);

                using (StreamReader reader = new StreamReader(datei))
                {
                    var überschrift = reader.ReadLine();
                    int i = 1;

                    while (true)
                    {
                        i++;
                        Unterricht unterricht = new Unterricht();

                        string line = reader.ReadLine();

                        if (line != null)
                        {
                            var x = line.Split('\t');

                            unterricht = new Unterricht();
                            unterricht.LessonNumbers = new List<int>();
                            unterricht.Zeile = i;
                            unterricht.LessonId = Convert.ToInt32(x[0]);
                            unterricht.LessonNumbers.Add(Convert.ToInt32(x[1]) / 100);
                            unterricht.Fach = x[2];
                            unterricht.Lehrkraft = x[3];
                            unterricht.Klassen = x[4];
                            unterricht.Gruppe = x[5];
                            unterricht.Periode = Convert.ToInt32(x[6]);
                            unterricht.Startdate = DateTime.ParseExact(x[7], "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);
                            unterricht.Enddate = DateTime.ParseExact(x[8], "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);
                            this.Add(unterricht);
                        }

                        if (line == null)
                        {
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Global.WriteLine("Unterrichte ........... " + datei.Substring((datei.LastIndexOf("\\")) + 1), this.Count);                
            }
        }

        public Unterrichts(int periode, Klasses klasses, Lehrers lehrers, Fachs fachs, Raums raums)
        {
            Unterrichtsgruppes unterrichtsgruppes = new Unterrichtsgruppes();

            int kalenderwoche = GetCalendarWeek(DateTime.Now);

            DateTime datumMontagDerKalenderwoche = GetMondayDateOfWeek(kalenderwoche, DateTime.Now.Year);
            DateTime datumErsterTagDesPrüfZyklus = datumMontagDerKalenderwoche;

            using (SqlConnection odbcConnection = new SqlConnection(Global.ConnectionStringUntis))
            {
                int id = 0;

                try
                {
                    string queryString = @"SELECT
Lesson_ID,
LessonElement1,
Periods,
Lesson.LESSON_GROUP_ID,
Lesson_TT,
Flags,
DateFrom,
DateTo
FROM LESSON
WHERE (((SCHOOLYEAR_ID)= " + Global.AktSj[0] + Global.AktSj[1] + ") AND ((TERM_ID)=" + periode + ") AND ((Lesson.SCHOOL_ID)=177659) AND (((Lesson.Deleted)='false'))) ORDER BY LESSON_ID;";

                    SqlCommand odbcCommand = new SqlCommand(queryString, odbcConnection);
                    odbcConnection.Open();
                    SqlDataReader sqlDataReader = odbcCommand.ExecuteReader();

                    
                    while (sqlDataReader.Read())
                    {
                        id = sqlDataReader.GetInt32(0);

                        string wannUndWo = Global.SafeGetString(sqlDataReader, 4);

                        var zur = wannUndWo.Replace("~~", "|").Split('|');

                        ZeitUndOrts zeitUndOrts = new ZeitUndOrts();

                        for (int i = 0; i < zur.Length; i++)
                        {
                            if (zur[i] != "")
                            {
                                var zurr = zur[i].Split('~');

                                int tag = 0;
                                int stunde = 0;
                                List<string> raum = new List<string>();

                                try
                                {
                                    tag = Convert.ToInt32(zurr[1]);
                                }
                                catch (Exception)
                                {
                                    Console.WriteLine("Der Unterricht " + id + " hat keinen Tag.");
                                }

                                try
                                {
                                    stunde = Convert.ToInt32(zurr[2]);
                                }
                                catch (Exception)
                                {
                                    Console.WriteLine("Der Unterricht " + id + " hat keine Stunde.");
                                }

                                try
                                {
                                    var ra = zurr[3].Split(';');

                                    foreach (var item in ra)
                                    {
                                        if (item != "")
                                        {
                                            raum.AddRange((from r in raums
                                                           where item.Replace(";", "") == r.IdUntis.ToString()
                                                           select r.Raumnummer));
                                        }
                                    }

                                    if (raum.Count == 0)
                                    {
                                        raum.Add("");
                                    }
                                }
                                catch (Exception)
                                {
                                    Console.WriteLine("Der Unterricht " + id + " hat keinen Raum.");
                                }


                                ZeitUndOrt zeitUndOrt = new ZeitUndOrt(tag, stunde, raum);
                                zeitUndOrts.Add(zeitUndOrt);
                            }
                        }

                        string lessonElement = Global.SafeGetString(sqlDataReader, 1);

                        int anzahlGekoppelterLehrer = lessonElement.Count(x => x == '~') / 21;

                        List<string> klassenKürzel = new List<string>();

                        for (int i = 0; i < anzahlGekoppelterLehrer; i++)
                        {
                            var lesson = lessonElement.Split(',');

                            try
                            {
                                var les = lesson[i].Split('~');

                                string lehrer = les[0] == "" ? null : (from l in lehrers where l.IdUntis.ToString() == les[0] select l.Kürzel).FirstOrDefault();

                                if (lehrer == "SOE")
                                {
                                    string a = "";
                                }
                                string fach = les[2] == "0" ? "" : (from f in fachs where f.IdUntis.ToString() == les[2] select f.KürzelUntis).FirstOrDefault();

                                string raumDiesesUnterrichts = "";
                                if (les[3] != "")
                                {
                                    raumDiesesUnterrichts = (from r in raums where (les[3].Split(';')).Contains(r.IdUntis.ToString()) select r.Raumnummer).FirstOrDefault();
                                }

                                int anzahlStunden = sqlDataReader.GetInt32(2);

                                var unterrichtsgruppeDiesesUnterrichts = (from u in unterrichtsgruppes where u.IdUntis == sqlDataReader.GetInt32(3) select u).FirstOrDefault();

                                if (les.Count() >= 17)
                                {
                                    foreach (var kla in les[17].Split(';'))
                                    {
                                        Klasse klasse = new Klasse();

                                        if (kla != "")
                                        {
                                            if (!(from kl in klassenKürzel
                                                  where kl == (from k in klasses
                                                               where k.IdUntis == Convert.ToInt32(kla)
                                                               select k.NameUntis).FirstOrDefault()
                                                  select kl).Any())
                                            {
                                                klassenKürzel.Add((from k in klasses
                                                                   where k.IdUntis == Convert.ToInt32(kla)
                                                                   select k.NameUntis).FirstOrDefault());
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                }

                                if (lehrer != null)
                                {
                                    for (int z = 0; z < zeitUndOrts.Count; z++)
                                    {
                                        // Wenn zwei Lehrer gekoppelt sind und zwei Räume zu dieser Stunde gehören, dann werden die Räume entsprechend verteilt.

                                        string r = zeitUndOrts[z].Raum[0];
                                        try
                                        {
                                            if (anzahlGekoppelterLehrer > 1 && zeitUndOrts[z].Raum.Count > 1)
                                            {
                                                r = zeitUndOrts[z].Raum[i];
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            if (anzahlGekoppelterLehrer > 1 && zeitUndOrts[z].Raum.Count > 1)
                                            {
                                                r = zeitUndOrts[z].Raum[0];
                                            }
                                        }

                                        string k = "";

                                        foreach (var item in klassenKürzel)
                                        {
                                            k += item + ",";
                                        }

                                        // Nur wenn der tagDesUnterrichts innerhalb der Befristung stattfindet, wird er angelegt

                                        DateTime von = DateTime.ParseExact((sqlDataReader.GetInt32(6)).ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                                        DateTime bis = DateTime.ParseExact((sqlDataReader.GetInt32(7)).ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);

                                        DateTime tagDesUnterrichts = datumErsterTagDesPrüfZyklus.AddDays(zeitUndOrts[z].Tag - 1);

                                        if ((periode == 0 || von <= tagDesUnterrichts) && tagDesUnterrichts <= bis)
                                        {
                                            Unterricht unterricht = new Unterricht(
                                                id,
                                                lehrer,
                                                fach,
                                                k.TrimEnd(','),
                                                r,
                                                "",
                                                zeitUndOrts[z].Tag,
                                                zeitUndOrts[z].Stunde,
                                                unterrichtsgruppeDiesesUnterrichts,
                                                datumErsterTagDesPrüfZyklus);
                                            this.Add(unterricht);
                                            try
                                            {
                                                string ugg = unterrichtsgruppeDiesesUnterrichts == null ? "" : unterrichtsgruppeDiesesUnterrichts.Name;
                                                // Console.WriteLine(unterricht.Id.ToString().PadLeft(4) + " " + unterricht.LehrerKürzel.PadRight(4) + unterricht.KlasseKürzel.PadRight(20) + unterricht.FachKürzel.PadRight(10) + " Raum: " + r.PadRight(10) + " Tag: " + unterricht.Tag + " Stunde: " + unterricht.Stunde + " " + ugg.PadLeft(3));
                                            }
                                            catch (Exception ex)
                                            {
                                                throw ex;
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("Fehler beim Unterricht: " +  id);
                            }
                        }
                    }
                    sqlDataReader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Fehler beim Unterricht mit der ID " + id + "\n" + ex.ToString());
                    throw new Exception("Fehler beim Unterricht mit der ID " + id + "\n" + ex.ToString());
                }
                finally
                {
                    odbcConnection.Close();
                    Global.WriteLine("Unterrichte", this.Count);
                }
            }
        }
        
        internal List<Lehrer> Fhr(Lehrers lehrers)
        {
            var members = new Lehrers();

            foreach (var unterricht in this)
            {
                if (
                    unterricht.KlasseKürzel.StartsWith("HBW") || 
                    unterricht.KlasseKürzel.StartsWith("HBT") || 
                    unterricht.KlasseKürzel.StartsWith("HBG") || 
                    unterricht.KlasseKürzel.StartsWith("BS") || 
                    unterricht.KlasseKürzel.StartsWith("FM") || 
                    unterricht.KlasseKürzel.StartsWith("FS"))
                {
                    var lehrer = (from l in lehrers where l.Kürzel == unterricht.LehrerKürzel select l).FirstOrDefault();

                    if (!(from m in members where m.Mail == lehrer.Mail select m).Any())
                    {
                        members.Add(lehrer);
                    }
                }
            }

            var mm = new List<Lehrer>();
                        
            foreach (var item in members.OrderBy(x => x.Nachname))
            {
                mm.Add(item);
            }

            return mm;
        }

        internal List<Lehrer> Abitur(Lehrers lehrers)
        {
            var members = new List<Lehrer>();

            foreach (var unterricht in this)
            {
                if (unterricht.KlasseKürzel.StartsWith("G"))
                {
                    var lehrer = (from l in lehrers where l.Kürzel == unterricht.LehrerKürzel select l).FirstOrDefault();

                    if (lehrer != null)
                    {
                        if (!members.Contains(lehrer))
                        {
                            members.Add(lehrer);
                        }
                    }
                }
            }

            return members;
        }

        internal List<Lehrer> AbiturNur13er(Lehrers lehrers)
        {
            var members = new List<Lehrer>();

            var aktJahr = (Convert.ToInt32(DateTime.Now.Month > 7 ? DateTime.Now.Year - 2000 : DateTime.Now.Year - 1 - 2000) - 2).ToString();

            foreach (var unterricht in this)
            {
                if (unterricht.KlasseKürzel.StartsWith("G") && unterricht.KlasseKürzel.Contains(aktJahr))
                {
                    var lehrer = (from l in lehrers where l.Kürzel == unterricht.LehrerKürzel select l).FirstOrDefault();

                    if (lehrer != null)
                    {
                        if (!members.Contains(lehrer))
                        {
                            members.Add(lehrer);
                        }
                    }
                }
            }

            return members;
        }

        private DateTime GetMondayDateOfWeek(int week, int year)
        {
            int i = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(new DateTime(year, 1, 1), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            if (i == 1)
            {
                return CultureInfo.CurrentCulture.Calendar.AddDays(new DateTime(year, 1, 1), ((week - 1) * 7 - GetDayCountFromMonday(CultureInfo.CurrentCulture.Calendar.GetDayOfWeek(new DateTime(year, 1, 1))) + 1));
            }
            else
            {
                int x = Convert.ToInt32(CultureInfo.CurrentCulture.Calendar.GetDayOfWeek(new DateTime(year, 1, 1)));
                return CultureInfo.CurrentCulture.Calendar.AddDays(new DateTime(year, 1, 1), ((week - 1) * 7 + (7 - GetDayCountFromMonday(CultureInfo.CurrentCulture.Calendar.GetDayOfWeek(new DateTime(year, 1, 1)))) + 1));
            }
        }

        private int GetDayCountFromMonday(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    return 1;
                case DayOfWeek.Tuesday:
                    return 2;
                case DayOfWeek.Wednesday:
                    return 3;
                case DayOfWeek.Thursday:
                    return 4;
                case DayOfWeek.Friday:
                    return 5;
                case DayOfWeek.Saturday:
                    return 6;
                default:
                    //Sunday
                    return 7;
            }
        }

        private int GetCalendarWeek(DateTime date)
        {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;

            Calendar calendar = currentCulture.Calendar;

            int calendarWeek = calendar.GetWeekOfYear(date,
               currentCulture.DateTimeFormat.CalendarWeekRule,
               currentCulture.DateTimeFormat.FirstDayOfWeek);

            // Überprüfen, ob eine Kalenderwoche größer als 52
            // ermittelt wurde und ob die Kalenderwoche des Datums
            // in einer Woche 2 ergibt: In diesem Fall hat
            // GetWeekOfYear die Kalenderwoche nicht nach ISO 8601 
            // berechnet (Montag, der 31.12.2007 wird z. B.
            // fälschlicherweise als KW 53 berechnet). 
            // Die Kalenderwoche wird dann auf 1 gesetzt
            if (calendarWeek > 52)
            {
                date = date.AddDays(7);
                int testCalendarWeek = calendar.GetWeekOfYear(date,
                   currentCulture.DateTimeFormat.CalendarWeekRule,
                   currentCulture.DateTimeFormat.FirstDayOfWeek);
                if (testCalendarWeek == 2)
                    calendarWeek = 1;
            }
            return calendarWeek;
        }

        internal List<Unterricht> SortierenUndKumulieren()
        {
            try
            {
                // Die Unterrichte werden chronologisch sortiert

                List<Unterricht> sortierteUnterrichts = (from u in this
                                                         orderby u.KlasseKürzel, u.FachKürzel, u.Raum, u.Tag, u.Stunde
                                                         select u).ToList();

                for (int i = 0; i < sortierteUnterrichts.Count; i++)
                {
                    // Wenn es einen nachfolgenden Unterricht gibt ...

                    if (i < sortierteUnterrichts.Count - 1)
                    {
                        // ... und dieser in allen Eigenschaften identisch ist ...

                        if (sortierteUnterrichts[i].KlasseKürzel == sortierteUnterrichts[i + 1].KlasseKürzel && sortierteUnterrichts[i].FachKürzel == sortierteUnterrichts[i + 1].FachKürzel && sortierteUnterrichts[i].Raum == sortierteUnterrichts[i + 1].Raum)
                        {
                            // ... und der nachfolgende Unterricht unmittelbar (nach der Pause) anschließt ... 

                            if (sortierteUnterrichts[i].Bis == sortierteUnterrichts[i + 1].Von || sortierteUnterrichts[i].Bis.AddMinutes(15) == sortierteUnterrichts[i + 1].Von || sortierteUnterrichts[i].Bis.AddMinutes(20) == sortierteUnterrichts[i + 1].Von)
                            {
                                // ... wird der Beginn des Nachfolgers nach vorne geschoben ...

                                sortierteUnterrichts[i + 1].Von = sortierteUnterrichts[i].Von;

                                // ... und der Vorgänger wird gelöscht.

                                sortierteUnterrichts.RemoveAt(i);

                                // Der Nachfolger bekommt den Index des Vorgängers.
                                i--;
                            }
                        }
                    }
                }
                return (
                    from s in sortierteUnterrichts
                    orderby s.Tag, s.Stunde, s.KlasseKürzel
                    select s).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }
        
        public Unterrichts Kumulieren()
        {
            try
            {
                for (int i = 0; i < this.Count; i++)
                {
                    // Wenn es einen nachfolgenden Unterricht gibt ...

                    if (i < this.Count - 1)
                    {
                        // ... und dieser in allen Eigenschaften identisch ist ...

                        if (this[i].KlasseKürzel == this[i + 1].KlasseKürzel && this[i].FachKürzel == this[i + 1].FachKürzel && this[i].Raum == this[i + 1].Raum)
                        {
                            // ... und der nachfolgende Unterricht unmittelbar (nach der Pause) anschließt ... 

                            if (this[i].Bis == this[i + 1].Von || this[i].Bis.AddMinutes(15) == this[i + 1].Von || this[i].Bis.AddMinutes(20) == this[i + 1].Von)
                            {
                                // ... wird der Beginn des Nachfolgers nach vorne geschoben ...

                                this[i + 1].Von = this[i].Von;

                                // ... und der Vorgänger wird gelöscht.

                                this.RemoveAt(i);

                                // Der Nachfolger bekommt den Index des Vorgängers.
                                i--;
                            }
                        }
                    }
                }
                return this;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw new Exception(ex.ToString());
            }
        }

        internal Lehrers GetMembers(Bildungsgang bildungsgang, Lehrers lehrers, Unterrichts unterrichts)
        {
            List<Lehrer> lehrer = new List<Lehrer>();

            foreach (var unterricht in this)
            {                
                if ((unterricht.KlasseKürzel.StartsWith(bildungsgang.Kurzname) || unterricht.KlasseKürzel.Contains("," + bildungsgang.Kurzname) && unterricht.KlasseKürzel.Any(c => char.IsDigit(c))))
                    {
                    if (bildungsgang.Kurzname.StartsWith("BT"))
                    {
                        string aaa = "";
                    }
                    var leh = (from l in lehrers 
                               where l.Kürzel == unterricht.LehrerKürzel 
                               where unterricht.KlasseKürzel.Any(c => char.IsDigit(c))
                               select l).FirstOrDefault();

                    if (leh != null && !(from l in lehrer where l.Mail == leh.Mail select l).Any())
                    {
                        lehrer.Add(leh);
                    }
                }
            }

            var llll = new Lehrers();

            foreach (var leh in lehrer.OrderBy(x=>x.Nachname)) 
            {
                if (!(from ll in llll where ll.Mail == leh.Mail select ll).Any())
                {
                    if (leh.Vorname != null && leh.Vorname != "" && leh.Nachname!= null && leh.Nachname != "")
                    {
                        llll.Add(leh);
                    }
                    else
                    {
                        Console.WriteLine(leh.Kürzel + " hat keinen Vor- und Nachnamen");
                    }
                }
            }

            return llll;
        }

        internal List<Lehrer> Fachschaften(Lehrers lehrers, List<string> fächer)
        {
            var fachlehrers = new List<Lehrer>();
            
            try
            {
                foreach (var u in (from t in this where fächer.Contains(t.FachKürzel) select t).ToList())
                {
                    var l = (from ll in lehrers where ll.Kürzel == u.LehrerKürzel select ll).FirstOrDefault();
                    if (l != null && !(from ll in fachlehrers where ll.Mail == l.Mail select ll).Any())
                    {
                        fachlehrers.Add(l);
                    }
                }
            }
            catch
            {
            }
            return fachlehrers;
        }
    }
}