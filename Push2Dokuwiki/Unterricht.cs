using System;
namespace Push2Dokuwiki
{
    public class Unterricht
    {
        public int Id;
        public string LehrerKürzel;
        public string FachKürzel;
        public string KlasseKürzel;
        public string Raum;
        public string Text { get; private set; }
        public int Tag;
        public int Stunde;
        public Unterrichtsgruppe Unterrichtsgruppe;
        public DateTime DatumTagMontagDerKalenderwoche { get; private set; }
        public DateTime Von { get; set; }
        public DateTime Bis { get; set; }

        public Unterricht()
        {
        }

        public Unterricht(int id, string lehrerKürzel, string fachKürzel, string klasseKürzel, string raum, string text, int tag, int stunde, Unterrichtsgruppe unterrichtsgruppe, DateTime datumTagMontagDerKalenderwoche)
        {
            this.Id = id;
            this.LehrerKürzel = lehrerKürzel;
            this.FachKürzel = fachKürzel;
            this.KlasseKürzel = klasseKürzel;
            this.Raum = raum;
            this.Text = text;
            this.DatumTagMontagDerKalenderwoche = datumTagMontagDerKalenderwoche;
            this.Tag = tag;
            this.Von = GetVon(tag, stunde, datumTagMontagDerKalenderwoche);
            this.Bis = this.Von.AddMinutes(45);
            this.Stunde = stunde;
            this.Unterrichtsgruppe = unterrichtsgruppe;
        }

        private DateTime GetVon(int tag, int stunde, DateTime datumTagMontagDerKalenderwoche)
        {
            DateTime dt = datumTagMontagDerKalenderwoche.AddDays(tag - 1);

            TimeSpan ts = new TimeSpan();

            switch (stunde)
            {
                case 1:
                    ts = new TimeSpan(7, 40, 0);
                    break;
                case 2:
                    ts = new TimeSpan(8, 25, 0);
                    break;
                case 3:
                    ts = new TimeSpan(9, 30, 0);
                    break;
                case 4:
                    ts = new TimeSpan(10, 15, 0);
                    break;
                case 5:
                    ts = new TimeSpan(11, 15, 0);
                    break;
                case 6:
                    ts = new TimeSpan(12, 0, 0);
                    break;
                case 7:
                    ts = new TimeSpan(13, 0, 0);
                    break;
                case 8:
                    ts = new TimeSpan(13, 45, 0);
                    break;
                case 9:
                    ts = new TimeSpan(14, 45, 0);
                    break;
                case 10:
                    ts = new TimeSpan(15, 30, 0);
                    break;
                case 11:
                    ts = new TimeSpan(16, 30, 0);
                    break;
                case 12:
                    ts = new TimeSpan(18, 0, 0);
                    break;
                case 13:
                    ts = new TimeSpan(19, 0, 0);
                    break;
                case 14:
                    ts = new TimeSpan(19, 55, 0);
                    break;
            }

            return dt.Date + ts;
        }
    }
}
