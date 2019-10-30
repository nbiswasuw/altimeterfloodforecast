using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;

namespace FFWCDataImporter
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                SQLiteConnection sqlconn = new SQLiteConnection(@"Data Source=C:\Users\nbiswas\Desktop\Nishan\SASWE\FFWC_Flood\NewDesign\Database\Test_Insituwaterlevel.db;Version=3;");
                sqlconn.Open();
                string sql;
                SQLiteCommand command;

                string[] ffDataFile = File.ReadAllLines(@"C:\Users\nbiswas\Desktop\Nishan\SASWE\FFWC_Flood\NewDesign\Database\ffData.csv");
                var vari = ffDataFile[0].Split(';');
                string[][] jaggedString = new string[vari.Count()][];
                for (int i = 0; i < jaggedString.Length; i++)
                {
                    jaggedString[i] = new string[ffDataFile.Length];
                }

                int x = 0;
                foreach (string element in ffDataFile)
                {
                    var separatedText = element.Split(';');
                    for (int j = 0; j < separatedText.Length; j++)
                    {
                        jaggedString[j][x] = separatedText[j];
                    }
                    x = x + 1;
                }
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < jaggedString.Length; i++)
                {
                    if (jaggedString[i][0] == "WATER LEVEL")
                    {
                        for (int j = 2; j < jaggedString[i].Length; j++)
                        {
                            if (jaggedString[i][j] != "-9999")
                            {
                                string station = jaggedString[i][1].Substring(0, jaggedString[i][1].Length - 6);
                                if (station == "Hardinge-Br")
                                {
                                    station = "Hardinge-RB";
                                }
                                else if (station == "Gomti-Comilla")
                                {
                                    station = "Comilla";
                                }
                                DateTime datadate = DateTime.ParseExact(jaggedString[0][j].Trim(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                                float waterlevel = float.Parse(jaggedString[i][j].Trim());
                                try
                                {
                                    Console.WriteLine("Inserting " + station + "," + jaggedString[0][j] + "," + jaggedString[i][j]);
                                    sql = "insert into Waterlevel (Station, Date, WL) values ('" + station + "', '" + datadate + "', " + waterlevel + ")";
                                    command = new SQLiteCommand(sql, sqlconn);
                                    command.ExecuteNonQuery();
                                }
                                catch (SQLiteException)
                                {
                                    Console.WriteLine("Updating " + station + "," + jaggedString[0][j] + "," + jaggedString[i][j]);
                                    command = new SQLiteCommand("Update Waterlevel SET WL = @Value Where Date = @dataDate AND Station= @station", sqlconn);
                                    command.Parameters.AddWithValue("@dataDate", datadate);
                                    command.Parameters.AddWithValue("@station", station);
                                    command.Parameters.AddWithValue("@Value", waterlevel);
                                    command.ExecuteNonQuery();
                                }
                            }
                            else { continue; }
                        }
                    }
                    else { continue; }
                }
                Console.WriteLine("All WL Data have been imported Successfully.");
            }

            catch (Exception error)
            {
                Console.WriteLine("Data can not be inserted due to an error. Error: " + error.Message);
            }
        }
    }
}
