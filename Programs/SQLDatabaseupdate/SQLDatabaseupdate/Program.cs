using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Data.SQLite;

namespace SQLDatabaseupdate
{
    class Program
    {
        static void Main(string[] args)
        {
            SQLiteConnection sqlconn = new SQLiteConnection(@"Data Source=C:\Users\nbiswas\Desktop\Nishan\SASWE\AutoCorrection\Insituwaterlevel.db;Version=3;");
            sqlconn.Open();
            string sql;
            SQLiteCommand command;
            SQLiteTransaction transaction = sqlconn.BeginTransaction();

            DirectoryInfo di = new DirectoryInfo(@"C:\Users\nbiswas\Desktop\Nishan\SASWE\AutoCorrection\RawWL\");
            foreach(FileInfo fi in di.GetFiles("*.txt"))
            {
                Console.WriteLine(fi.Name);
                string[] waterlevelinfo = File.ReadAllLines(fi.FullName);
                foreach (string element in waterlevelinfo)
                {
                    var txt = element.Split(',');
                    DateTime stationDate = DateTime.ParseExact(txt[0], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    string stationName = txt[1];
                    float waterlevel = float.Parse(txt[2]);
                    sql = "insert into Waterlevel (Station, Date, WL) values ('" + stationName + "', '" + stationDate.ToString("yyyy-MM-dd") + "', " + waterlevel + ")";
                    try
                    {
                        command = new SQLiteCommand(sql, sqlconn);
                        command.ExecuteNonQuery();
                    }
                    catch (SQLiteException)
                    {
                        continue;
                    }
                }
            }
            transaction.Commit();
            sqlconn.Close();
        }
    }
}
