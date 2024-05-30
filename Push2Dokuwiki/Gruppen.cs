﻿using System.Collections.Generic;
using System.IO;
using System;

namespace Push2Dokuwiki
{
    public class Gruppen : List<Gruppe>
    {
        public Gruppen(string kriterium, DateTime dateTime)
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
                        var gruppe = new Gruppe();

                        string line = reader.ReadLine();

                        try
                        {
                            if (line != null)
                            {
                                var x = line.Split('\t');

                                gruppe = new Gruppe();
                                gruppe.MarksPerLessonZeile = i;
                                gruppe.StudentId = Convert.ToInt32(x[0]);
                                gruppe.Gruppenname = x[3];
                                gruppe.Fach = x[4];
                                try
                                {
                                    gruppe.Startdate = DateTime.ParseExact(x[5], "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);
                                }
                                catch (Exception)
                                {
                                }

                                try
                                {
                                    gruppe.Enddate = DateTime.ParseExact(x[6], "dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);
                                }
                                catch (Exception)
                                {
                                }

                                this.Add(gruppe);
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
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
                Global.WriteLine("Guppen ................ " + datei.Substring((datei.LastIndexOf("\\")) + 1), this.Count);
            }            
        }
    }
}