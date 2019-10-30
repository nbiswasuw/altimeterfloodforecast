using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Research.Science.Data.Imperative;
using HtmlAgilityPack;
using System.Data.SQLite;
using System.Data.SqlClient;

namespace SASWEForecast
{
    class Program
    {
        static void Main(string[] args)
        {
            // ---------------------------------------- Initializing variables  ----------------------------------------------------------------------
            DateTime foreDate = DateTime.Today.Date;
            DateTime bndstarthindDate = foreDate.AddDays(-7);
            StringBuilder tracker = new StringBuilder();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Boundary generator of Altimetry based Flood Forecasting System of SASWE.");
            Console.WriteLine("Forecast Date for generating boundary: " + foreDate.ToString("yyyy-MM-dd"));
            Console.WriteLine("Boundary generation started at: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            tracker.AppendLine("Boundary generator of Altimetry based Flood Forecasting System of SASWE.");
            tracker.AppendLine("Forecast Date for generating boundary: " + foreDate.ToString("yyyy-MM-dd"));
            tracker.AppendLine("Boundary generation started at: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Initializing variables and checking for necessary files ...");
            tracker.AppendLine("Initializing variables and checking for necessary files ...");
            Console.ResetColor();

            string rootdirectory = @"C:\Users\nbiswas\Desktop\Nishan\SASWE\FFWC_Flood\NewDesign\";
            string binDir = rootdirectory + @"Bin\";
            string logDir = rootdirectory + @"\Logs\";
            string preprocessdir = rootdirectory + @"Preprocess\";
            string granuledir = preprocessdir + @"GranuleData\";
            string virtualHdir = preprocessdir + @"VSHeight\";
            string forecastHDir = preprocessdir + @"ForecastHeights\";
            string correctHDir = preprocessdir + @"CorrectForecasts\";
            string latloninfofilepath = binDir + "Minmaxlat.txt";
            string frcCurvepath = binDir + "FRCInfo.txt";
            string logfilepath = logDir + "ForecastInfo_" + foreDate.ToString("yyyy-MM-dd") + ".log";
            string selectedGanges = "";
            string selectedBrahma = "";
            string[] gangesBnds = new string[] { "Faridpur", "Rohanpur" };
            string[] brahmaBnds = new string[] { "Gaibandha", "Kurigram", "Badarganj", "Panchagarh", "Dalia", "Comilla", "Dinajpur" };

            SQLiteConnection sqlconn = new SQLiteConnection(@"Data Source=C:\Users\nbiswas\Desktop\Nishan\SASWE\FFWC_Flood\NewDesign\Database\Insituwaterlevel.db;Version=3;");
            sqlconn.Open();
            string sql;
            SQLiteCommand command;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Necessary files and folders checked and variables initiated successfully.");
            tracker.AppendLine("Necessary files and folders checked and variables initiated successfully.");
            Console.ResetColor();
            
            try
            {
                //-------------------------------------- Creating directory if not exists --------------------------------------------------------------
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Checking for latest Virtual Station file ...");
                tracker.AppendLine("Checking for latest Virtual Station file ...");
                Console.ResetColor();

                if (Directory.Exists(granuledir) != true)
                {
                    Directory.CreateDirectory(granuledir);
                }
                //-------------------------------------- Getting Latest JASON 3 Files from AVISO FTP Server --------------------------------------------- 
                List<string> jason3Files = new List<string>();
                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://avisoftp.cnes.fr/AVISO/pub/jason-3/igdr/latest_data/");
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();
                StreamReader streamReader = new StreamReader(response.GetResponseStream());
                string line = streamReader.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    jason3Files.Add(line);
                    line = streamReader.ReadLine();
                }
                streamReader.Close();

                //-------------------------------------- Getting Latest JASON 2 Files from AVISO FTP Server --------------------------------------------- 
                List<string> jason2Files = new List<string>();
                ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://avisoftp.cnes.fr/AVISO/pub/jason-2/igdr/latest_data/");
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                response = (FtpWebResponse)ftpRequest.GetResponse();
                streamReader = new StreamReader(response.GetResponseStream());
                line = streamReader.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    jason2Files.Add(line);
                    line = streamReader.ReadLine();
                }
                streamReader.Close();

                //// ---------------------------------------------- Comparing JASON 3 and JASON 2 Data to get the latest File ----------------------------------
                //------------------------------------------ Selection of JASON 3 File ------------------------------------------------------
                string j3GangFile = "";
                for (int i = jason3Files.Count - 1; i >= 0; i--)
                {
                    if (jason3Files[i].Substring(16, 3) == "014" || jason3Files[i].Substring(16, 3) == "079" || jason3Files[i].Substring(16, 3) == "155" || jason3Files[i].Substring(16, 3) == "192")
                    {
                        j3GangFile = jason3Files[i];
                        Console.WriteLine("Latest JASON 3 Altimeter data available for Ganges River: " + j3GangFile);
                        tracker.AppendLine("Latest JASON 3 Altimeter data available for Ganges River: " + j3GangFile);
                        break;
                    }
                }

                string j3BrahmaFile = "";
                for (int i = jason3Files.Count - 1; i >= 0; i--)
                {
                    if (jason3Files[i].Substring(16, 3) == "053" || jason3Files[i].Substring(16, 3) == "166" || jason3Files[i].Substring(16, 3) == "242")
                    {
                        j3BrahmaFile = jason3Files[i];
                        Console.WriteLine("Latest JASON 3 Altimeter data available for Brahmaputra River: " + j3BrahmaFile);
                        tracker.AppendLine("Latest JASON 3 Altimeter data available for Brahmaputra River: " + j3BrahmaFile);
                        break;
                    }
                }

                //------------------------------------------ Selection of JASON 2 File ------------------------------------------------------
                string j2GangFile = "";
                for (int i = jason2Files.Count - 1; i >= 0; i--)
                {
                    if (jason2Files[i].Substring(16, 3) == "014" || jason2Files[i].Substring(16, 3) == "079" || jason2Files[i].Substring(16, 3) == "155" || jason2Files[i].Substring(16, 3) == "192")
                    {
                        j2GangFile = jason2Files[i];
                        Console.WriteLine("Latest JASON 2 Altimeter data available for Ganges River: " + j2GangFile);
                        tracker.AppendLine("Latest JASON 2 Altimeter data available for Ganges River: " + j2GangFile);
                        break;
                    }
                }

                string j2BrahmaFile = "";
                for (int i = jason2Files.Count - 1; i >= 0; i--)
                {
                    if (jason2Files[i].Substring(16, 3) == "053" || jason2Files[i].Substring(16, 3) == "166" || jason2Files[i].Substring(16, 3) == "242")
                    {
                        j2BrahmaFile = jason2Files[i];
                        Console.WriteLine("Latest JASON 2 Altimeter data available for Brahmaputra River: " + j2BrahmaFile);
                        tracker.AppendLine("Latest JASON 2 Altimeter data available for Brahmaputra River: " + j2BrahmaFile);
                        break;
                    }
                }
                // -------------------------------------------- Comapring the latest File -------------------------------------------------------------------
                DateTime gangesJA2Date = DateTime.ParseExact(j2GangFile.Substring(20, 8), "yyyyMMdd", CultureInfo.InvariantCulture);
                DateTime gangesJA3Date = DateTime.ParseExact(j3GangFile.Substring(20, 8), "yyyyMMdd", CultureInfo.InvariantCulture);
                DateTime brahmaJA2Date = DateTime.ParseExact(j2BrahmaFile.Substring(20, 8), "yyyyMMdd", CultureInfo.InvariantCulture);
                DateTime brahmaJA3Date = DateTime.ParseExact(j3BrahmaFile.Substring(20, 8), "yyyyMMdd", CultureInfo.InvariantCulture);
                string gangesFile = "";
                string brahmaFile = "";

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Latest Virtual Station file Selected. Downloading the selected file ...");
                tracker.AppendLine("Latest Virtual Station file Selected. Downloading the selected file ...");
                Console.ResetColor();

                // -------------------------------------------- Downloading the latest File from AVISO FTP Server ----------------------------------
                WebClient ftpClient = new WebClient();
                if (gangesJA2Date > gangesJA3Date)
                {
                    gangesFile = j2GangFile;
                    string gangesftp = "ftp://avisoftp.cnes.fr/AVISO/pub/jason-2/igdr/latest_data/" + gangesFile;
                    selectedGanges = gangesFile;
                    string gangesfilepath = granuledir + gangesFile;
                    if (!File.Exists(gangesfilepath))
                    {
                        ftpClient.DownloadFile(gangesftp, gangesfilepath);
                    }

                }
                else
                {
                    gangesFile = j3GangFile;
                    string gangesftp = "ftp://avisoftp.cnes.fr/AVISO/pub/jason-3/igdr/latest_data/" + gangesFile;
                    selectedGanges = gangesFile;
                    string gangesfilepath = granuledir + gangesFile;
                    if (!File.Exists(gangesfilepath))
                    {
                        ftpClient.DownloadFile(gangesftp, gangesfilepath);
                    }

                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Latest downloaded file for Ganges: " + selectedGanges);
                tracker.AppendLine("Latest downloaded file for Ganges: " + selectedGanges);
                Console.ResetColor();

                if (brahmaJA2Date > brahmaJA3Date)
                {
                    brahmaFile = j2BrahmaFile;
                    string brahmaftp = "ftp://avisoftp.cnes.fr/AVISO/pub/jason-2/igdr/latest_data/" + brahmaFile;
                    selectedBrahma = brahmaFile;
                    string brahmafilepath = granuledir + brahmaFile;
                    if (!File.Exists(brahmafilepath))
                    {
                        ftpClient.DownloadFile(brahmaftp, brahmafilepath);
                    }

                }
                else
                {
                    brahmaFile = j3BrahmaFile;
                    string brahmaftp = "ftp://avisoftp.cnes.fr/AVISO/pub/jason-3/igdr/latest_data/" + brahmaFile;
                    selectedBrahma = brahmaFile;
                    string brahmafilepath = granuledir + brahmaFile;
                    if (!File.Exists(brahmafilepath))
                    {
                        ftpClient.DownloadFile(brahmaftp, brahmafilepath);
                    }
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Latest downloaded file for Brahmaputra: " + selectedBrahma);
                tracker.AppendLine("Latest downloaded file for Brahmaputra: " + selectedBrahma);
                Console.ResetColor();

            }
            catch (Exception Error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Latest Virtual Station file cannot be downloaded due to an error. Error: " + Error);
                tracker.AppendLine("Latest Virtual Station file cannot be downloaded due to an error. Error: " + Error);
                Console.ResetColor();
            }

            // ------------------------------Unzipping Files ---------------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Unzipping the latest virtual station files for both basins ...");
                tracker.AppendLine("Unzipping the latest virtual station files for both basins ...");
                Console.ResetColor();

                string gangesfilepath = granuledir + selectedGanges;
                string brahmafilepath = granuledir + selectedBrahma;
                if (!File.Exists(gangesfilepath.Substring(0, gangesfilepath.Length - 4) + ".nc"))
                {
                    ZipFile.ExtractToDirectory(gangesfilepath, granuledir);
                }
                if (!File.Exists(brahmafilepath.Substring(0, brahmafilepath.Length - 4) + ".nc"))
                {
                    ZipFile.ExtractToDirectory(brahmafilepath, granuledir);
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("The selected virtual station file unzipped successfully.");
                tracker.AppendLine("The selected virtual station file unzipped successfully.");
                Console.ResetColor();
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error in unzipping downloaded File. Error: " + error);
                tracker.AppendLine("Error in unzipping downloaded File. Error: " + error);
                Console.ResetColor();
            }

            //------------------------------------ Extracting Heights and Forecasted Rating Curves ----------------------------------------------------------
            string gangesVSfile = "";
            string brahmaVSfile = "";
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Virtual Station files are processing to get NC files ...");
                tracker.AppendLine("Virtual Station files are processing to get NC files ...");
                Console.ResetColor();

                string gangesNCFile = selectedGanges.Substring(0, selectedGanges.Length - 4) + ".nc";
                string brahmaNCFile = selectedBrahma.Substring(0, selectedBrahma.Length - 4) + ".nc";
                string[] files = new string[] { gangesNCFile, brahmaNCFile };
                //-------------------------------- Minimum and Maximum Latitude Searcing --------------------------------------------------------------------
                string[] allTxt = File.ReadAllLines(latloninfofilepath);
                Dictionary<string, float> minlats = new Dictionary<string, float>();
                Dictionary<string, float> maxlats = new Dictionary<string, float>();
                Dictionary<string, float> hcorrs = new Dictionary<string, float>();

                for (int i = 0; i < allTxt.Length - 1; i++)
                {
                    var elements = allTxt[i + 1].Split('\t');
                    minlats.Add(elements[0], float.Parse(elements[1]));
                    maxlats.Add(elements[0], float.Parse(elements[2]));
                    hcorrs.Add(elements[0], float.Parse(elements[3]));
                }
                foreach (string element in files)
                {
                    string passID = element.Substring(0, 3) + element.Substring(16, 3);
                    string filename = virtualHdir + passID;
                    double minlat = minlats[passID];
                    double maxlat = maxlats[passID];
                    double hcorr = hcorrs[passID];
                    Console.WriteLine(minlat.ToString("0.00") + " " + maxlat.ToString("0.00") + hcorr.ToString("0.00"));

                    var dataset = Microsoft.Research.Science.Data.DataSet.Open(granuledir + element + "?openMode=readOnly");
                    int[] lat = dataset.GetData<int[]>("lat");
                    int[] lon = dataset.GetData<int[]>("lon");
                    sbyte[] meas_ind = dataset.GetData<sbyte[]>("meas_ind");
                    double[] time = dataset.GetData<double[]>("time");
                    short[] model_dry_tropo_corr = dataset.GetData<short[]>("model_dry_tropo_corr");
                    short[] model_wet_tropo_corr = dataset.GetData<short[]>("model_wet_tropo_corr");
                    short[] iono_corr_gim_ku = dataset.GetData<short[]>("iono_corr_gim_ku");
                    short[] solid_earth_tide = dataset.GetData<short[]>("solid_earth_tide");
                    short[] pole_tide = dataset.GetData<short[]>("pole_tide");
                    sbyte[] alt_state_flag_ku_band_status = dataset.GetData<sbyte[]>("alt_state_flag_ku_band_status");
                    int[,] lon_20hz = dataset.GetData<int[,]>("lon_20hz");
                    int[,] lat_20hz = dataset.GetData<int[,]>("lat_20hz");
                    sbyte[,] ice_qual_flag_20hz_ku = dataset.GetData<sbyte[,]>("ice_qual_flag_20hz_ku");
                    double[,] time_20hz = dataset.GetData<double[,]>("time_20hz");
                    int[,] alt_20hz = dataset.GetData<int[,]>("alt_20hz");
                    int[,] ice_range_20hz_ku = dataset.GetData<int[,]>("ice_range_20hz_ku");
                    short[,] ice_sig0_20hz_ku = dataset.GetData<short[,]>("ice_sig0_20hz_ku");

                    string datetime = dataset.GetAttr(1, "units").ToString();
                    DateTime refDate = DateTime.ParseExact(datetime.Substring(14, 19), "yyyy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture);

                    double s_latlon = 0.000001;
                    double s_model_wet = 0.0001;
                    double s_model_dry = 0.0001;
                    double s_iono_corr = 0.0001;
                    double s_pole_tide = 0.0001;
                    double s_solid_earth_tide = 0.0001;
                    double s_alt = 0.0001;
                    double s_icerange_ku = 0.0001;
                    double s_ice_sig0_20hz = 0.01;

                    double media_corr;
                    double bsValue;
                    double height;
                    double latitude;
                    double longitude;

                    List<double> heights = new List<double>();
                    List<double> bsValues = new List<double>();

                    DateTime dataDate = new DateTime();
                    StringBuilder sob = new StringBuilder();
                    sob.AppendLine("Lat(D)\tLon(D)\tH(m)\tBS(dB)");
                    for (int i = 0; i < lat.Length; i++)
                    {
                        if (model_dry_tropo_corr[i] != 32767 && model_wet_tropo_corr[i] != 32767 && iono_corr_gim_ku[i] != 32767 && solid_earth_tide[i] != 32767 && pole_tide[i] != 32767 && alt_state_flag_ku_band_status[i] == 0)
                        {
                            media_corr = model_dry_tropo_corr[i] * s_model_dry + model_wet_tropo_corr[i] * s_model_wet + iono_corr_gim_ku[i] * s_iono_corr + solid_earth_tide[i] * s_solid_earth_tide + pole_tide[i] * s_pole_tide;
                            for (int j = 0; j < meas_ind.Length; j++)
                            {
                                if (ice_qual_flag_20hz_ku[i, j] != 1 && lat_20hz[i, j] != 2147483647 && lat_20hz[i, j] * s_latlon >= minlat && lat_20hz[i, j] * s_latlon <= maxlat)
                                {
                                    height = alt_20hz[i, j] * s_alt - (media_corr + ice_range_20hz_ku[i, j] * s_icerange_ku) - 0.7 + hcorr;
                                    bsValue = ice_sig0_20hz_ku[i, j] * s_ice_sig0_20hz;
                                    heights.Add(height);
                                    bsValues.Add(bsValue);

                                    longitude = lon_20hz[i, j] * s_latlon;
                                    latitude = lat_20hz[i, j] * s_latlon;
                                    dataDate = refDate.AddSeconds(time_20hz[i, j]);
                                    sob.AppendLine(latitude.ToString("0.000") + "\t" + longitude.ToString("0.000") + "\t" + Math.Round(height, 2).ToString("0.00") + "\t" + bsValue.ToString("0.00"));
                                }
                            }
                        }
                    }
                    File.WriteAllText(filename +"_" + dataDate.ToString("yyyy-MM-dd") + ".txt", sob.ToString());
                    if (element.Substring(0, 3) + element.Substring(16, 3) == selectedGanges.Substring(0, 3) + selectedGanges.Substring(16, 3))
                    {
                        gangesVSfile = passID + "_" + dataDate.ToString("yyyy-MM-dd") + ".txt";
                    }
                    else
                    {
                        brahmaVSfile = passID + "_" + dataDate.ToString("yyyy-MM-dd") + ".txt";
                    }
                    sob.Clear();
                    bsValues.Clear();
                    heights.Clear();
                }
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("Virtual Station selected for Ganges River: " + gangesVSfile);
                tracker.AppendLine("Virtual Station selected for Ganges River: " + gangesVSfile);
                Console.WriteLine("Virtual Station selected for Brahmaputra River: " + brahmaVSfile);
                tracker.AppendLine("Virtual Station selected for Brahmaputra River: " + brahmaVSfile);
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Virtual Station files are processed successfully.");
                tracker.AppendLine("Virtual Station files are processed successfully.");
                Console.ResetColor();
            }
            
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Virtual Station files cannot be processed due to an error. Error: " + error);
                tracker.AppendLine("Virtual Station files cannot be processed due to an error. Error: " + error);
                Console.ResetColor();
            }

            // ----------------------------------------------------------------------- Forecasting Boundary at Bahadurabad and Hardinge Bridge  ------------------------------------------------------------
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Generating forecasted Water Heights at Bahadurabad and Hardinge Bridge ...");
                tracker.AppendLine("Generating forecasted Water Heights at Bahadurabad and Hardinge Bridge ...");
                Console.ResetColor();

                // ------------------------------------------------------------------- Reading Forecast Rating Curve File ----------------------------------------------------------------------------------
                string[] frcInfo = File.ReadAllLines(frcCurvepath);
                Dictionary<Tuple<string, int>, float> mvalue = new Dictionary<Tuple<string, int>, float>();
                Dictionary<Tuple<string, int>, float> cvalue = new Dictionary<Tuple<string, int>, float>();

                for (int i = 0; i < frcInfo.Length - 1; i++)
                {
                    var elements = frcInfo[i + 1].Split('\t');
                    mvalue.Add(Tuple.Create(elements[0], int.Parse(elements[1])), float.Parse(elements[2]));
                    cvalue.Add(Tuple.Create(elements[0], int.Parse(elements[1])), float.Parse(elements[3]));
                }

                //------------------------------------------------------------------ Virtual Station Output reading ----------------------------------------------------------------------------------------
                int gangesAge = Convert.ToInt32((foreDate - DateTime.ParseExact(gangesVSfile.Substring(7, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture)).TotalDays);
                int brahmaAge = Convert.ToInt32((foreDate - DateTime.ParseExact(brahmaVSfile.Substring(7, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture)).TotalDays);

                string[] vsFiles = new string[] { gangesVSfile, brahmaVSfile };
                int [] ageFile = new int[] { gangesAge, brahmaAge };
                string[] basin = new string[] { "ganges", "brahma" };

                for (int j=0; j<basin.Length; j++)
                {
                    string[] satFileInfo = File.ReadAllLines(virtualHdir + vsFiles[j]);
                    List<float> heights = new List<float>();
                    float height = 0;
                    float[] bScatter = new float[satFileInfo.Length - 1];
                    for (int i = 1; i < satFileInfo.Length; i++)
                    {
                        var values = satFileInfo[i].Split('\t');
                        if (float.Parse(values[3]) >= 30.0)
                        {
                            heights.Add(float.Parse(values[2].Trim()));
                        }
                    }
                    if (heights.Count == 0)
                    {
                        Console.WriteLine("No Heights found with Backscatter greater than 30 dB for " + basin[j] + ", taken height corrsponding to Maximum BS.");
                        tracker.AppendLine("No Heights found with Backscatter greater than 30 dB for " + basin[j] + ", taken height corrsponding to Maximum BS.");
                        for (int i = 1; i < satFileInfo.Length; i++)
                        {
                            var values = satFileInfo[i].Split('\t');
                            heights.Add(float.Parse(values[2].Trim()));
                            bScatter[i - 1] = float.Parse(values[3]);
                        }
                        height = heights[Array.IndexOf(bScatter, bScatter.Max())];
                    }
                    else
                    {
                        height = heights.Average();
                    }
                    heights.Clear();
                    string passID = gangesVSfile.Substring(0, 6);

                    List<DateTime> foredates = new List<DateTime>();
                    List<float> foreheights = new List<float>();
                    if (ageFile[j] < 4)
                    {
                        for (int k = 0; k< (4 + ageFile[j]); k++)
                        {
                            foredates.Add(foreDate.AddDays(4 - ageFile[j] + k + 1));
                            Console.WriteLine(Tuple.Create(passID, (4 - ageFile[j]) + k + 5));
                            foreheights.Add(mvalue[Tuple.Create(passID, (4-ageFile[j]) + k + 5)]*height - cvalue[Tuple.Create(passID, (4 - ageFile[j]) + k + 5)]);
                        }
                    }
                    else
                    {
                        for (int k = 0; k < 8; k++)
                        {
                            foredates.Add(foreDate.AddDays(4 - ageFile[j] + k + 1));
                            foreheights.Add(mvalue[Tuple.Create(passID, k + 5)] * height + cvalue[Tuple.Create(passID, k + 5)]);
                        }
                    }
                    StringBuilder sb = new StringBuilder();
                    for (int k = 0; k < foredates.Count; k++)
                    {
                        sb.AppendLine(foredates[k].ToString("yyyy-MM-dd") + "\t" + foreheights[k].ToString("0.00"));
                    }
                    File.WriteAllText(forecastHDir + "Forecast_" + basin[j] +"_" + foreDate.ToString("yyyy-MM-dd") + ".txt", sb.ToString());
                    sb.Clear();
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Forecasted Water Heights at Bahadurabad and Hardinge Bridge are generated successfully.");
                tracker.AppendLine("Forecasted Water Heights at Bahadurabad and Hardinge Bridge are generated successfully.");
                Console.ResetColor();
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Forecasted Water Heights at Bahadurabad and Hardinge Bridge cannot be generated due to an error. Error: " + error);
                tracker.AppendLine("Forecasted Water Heights at Bahadurabad and Hardinge Bridge cannot be generated due to an error. Error: " + error);
                Console.ResetColor();
            }
            
			// ---------------------------------------------------------------------- FFWC Data Download and Processing -------------------------------------------------------------
			try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Downloading in situ water level data from FFWC website ....");
                tracker.AppendLine("Downloading in situ water level data from FFWC website ....");
                Console.ResetColor();

                StringBuilder sb = new StringBuilder();
                WebClient client = new WebClient();
                string htmlCode = client.DownloadString("http://www.ffwc.gov.bd/ffwc_charts/waterlevel.php");
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(htmlCode);

                HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                HtmlNodeCollection rows = tables[0].SelectNodes(".//tr");
                HtmlNodeCollection col = rows[1].SelectNodes(".//td");
                if (col[4].InnerText.Trim() != DateTime.Today.Day.ToString("00") + "-" + DateTime.Today.Month.ToString("00"))
                {
                    Console.WriteLine("Water Level data of " + DateTime.Today.ToString("yyyy-MM-dd") + " has not been updated on the FFWC Website.");
                    Console.WriteLine("The program will now exit....");
                    Environment.Exit(1);
                }

                else
                {
                    for (int i = 0; i < rows.Count - 3; ++i)
                    {
                        HtmlNodeCollection cols = rows[i + 3].SelectNodes(".//td");
                        if (cols.Count > 4 && cols[4].InnerText != "NP")
                        {
                            Console.WriteLine(cols[1].InnerText + "," + DateTime.Today.ToString("yyyy-MM-dd HH:mm:ss") + "," + cols[4].InnerText);
                            sql = "insert into Waterlevel (Station, Date, WL) values ('" + cols[1].InnerText + "', '" + DateTime.Today.ToString("yyyy-MM-dd") + "', " + cols[4].InnerText + ")";
                            Console.WriteLine(sql);
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
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("In situ Water Level data downloaded from FFWC website and SQL Database updated successfully.");
                tracker.AppendLine("In situ Water Level data downloaded from FFWC website and SQL Database updated successfully.");
                Console.ResetColor();
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("In situ Water Level data from FFWC cannot be downloaded due to an error. Error: " + error);
                tracker.AppendLine("In situ Water Level data from FFWC cannot be downloaded due to an error. Error: " + error);
                Console.ResetColor();
            }
            
            // ----------------------------------------------------------- Bahadurabad Boundary Generation --------------------------------------------------------------------------------

            Dictionary<DateTime, float> trendBrahma = new Dictionary<DateTime, float>();
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Correcting forecasted Water Heights at Bahadurabad Station ...");
                tracker.AppendLine("Correcting forecasted Water Heights at Bahadurabad Station ...");
                Console.ResetColor();

                string station = "Bahadurabad";
                string[] forecastFile = File.ReadAllLines(forecastHDir + "Forecast_brahma_" + foreDate.ToString("yyyy-MM-dd") + ".txt");
                Dictionary<DateTime, float> foreWL = new Dictionary<DateTime, float>();

                foreach (string line in forecastFile)
                {
                    var element = line.Split('\t');
                    foreWL.Add(DateTime.ParseExact(element[0], "yyyy-MM-dd", CultureInfo.InvariantCulture), float.Parse(element[1]));
                }
                int missingDays = Convert.ToInt32((foreWL.FirstOrDefault().Key - foreDate).TotalDays);
                Console.WriteLine(missingDays);
                Dictionary<DateTime, float> hindWL = new Dictionary<DateTime, float>();

                sql = "Select Date, WL from WaterLevel where Date >= '" + bndstarthindDate.ToString("yyyy-MM-dd") + "' and Date<='" + foreDate.ToString("yyyy-MM-dd") + "' and Station='" + station + "' Order by Date ASC";
                command = new SQLiteCommand(sql, sqlconn);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    hindWL.Add(Convert.ToDateTime(reader["Date"]), Convert.ToSingle(reader["WL"]));
                }
                float foredateWL = hindWL[foreDate];
                float day_1WL = hindWL[foreDate.AddDays(-1)];
                float day_2WL = hindWL[foreDate.AddDays(-2)];
                float day_3WL = hindWL[foreDate.AddDays(-3)];
                float meandiff = ((foredateWL - day_1WL) + (foredateWL - day_2WL) / 2.0f + (foredateWL - day_3WL) / 3.0f) / 3.0f;

                for (int i = 1; i < missingDays; i++)
                {
                    hindWL.Add(foreDate.AddDays(i), foredateWL + meandiff * i);
                }
                float correction = foreWL.FirstOrDefault().Value - hindWL.LastOrDefault().Value;
                foreach (var record in foreWL)
                {
                    hindWL.Add(record.Key, record.Value - correction);
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Bnd Date\tWL");
                foreach (DateTime dates in hindWL.Keys)
                {
                    sb.AppendLine(dates.ToString("yyyy-MM-dd") + "\t" + hindWL[dates].ToString("0.00"));
                    Console.WriteLine(dates.ToString("yyyy-MM-dd") + "," + hindWL[dates].ToString("0.00"));
                }
                File.WriteAllText(correctHDir + station + "_" + foreDate.ToString("yyyy-MM-dd") + ".txt", sb.ToString());
                sb.Clear();
                // ----------------- Calculating forecast Trend in WL of Bahadurabad Station -------------------------------------------  
                for (int i = 1; i < 9; i++)
                {
                    trendBrahma.Add(foreDate.AddDays(i), (hindWL[foreDate.AddDays(i)] - hindWL[foreDate]));
                    Console.WriteLine((hindWL[foreDate.AddDays(i)] - hindWL[foreDate]).ToString("0.00"));
                }
                //---------------------- USing forecast Trend of Bahadurabad Station for depended Boundaries ------------------------------

                foreach(string element in brahmaBnds)
                {
                    Dictionary<DateTime, float> hindcastWL = new Dictionary<DateTime, float>();
                    sql = "Select Date, WL from WaterLevel where Date >= '" + bndstarthindDate.ToString("yyyy-MM-dd") + "' and Date<='" + foreDate.ToString("yyyy-MM-dd") + "' and Station= '" + element +"' Order by Date ASC";
                    command = new SQLiteCommand(sql, sqlconn);
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        hindcastWL.Add(Convert.ToDateTime(reader["Date"]), Convert.ToSingle(reader["WL"]));
                    }
                    float foredayWL = hindcastWL.Last().Value;
                    foreach (var record in trendBrahma)
                    {
                        hindcastWL.Add(record.Key, record.Value + foredayWL);
                    }

                    sb.AppendLine("Bnd Date\tWL");
                    foreach (DateTime dates in hindcastWL.Keys)
                    {
                        sb.AppendLine(dates.ToString("yyyy-MM-dd") + "\t" + hindcastWL[dates].ToString("0.00"));
                        Console.WriteLine(dates.ToString("yyyy-MM-dd") + "," + hindcastWL[dates].ToString("0.00"));
                    }
                    File.WriteAllText(correctHDir + element + "_" + foreDate.ToString("yyyy-MM-dd") + ".txt", sb.ToString());
                    sb.Clear();
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Bahadurabad and depended station's forecasts generated successfully.");
                tracker.AppendLine("Bahadurabad and depended station's forecasts generated successfully.");
                Console.ResetColor();
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error occurred in boundary correction at Bahadurabad Station. Error: " + error);
                tracker.AppendLine("Error occurred in boundary correction at Bahadurabad Station. Error: " + error);
                Console.ResetColor();
            }


            //------------------------------------ Hardinge Bridge Boundary Writing ---------------------------------------------------------------
            Dictionary<DateTime, float> trendGanges = new Dictionary<DateTime, float>();
            try
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Correcting forecasted Water Heights at Hardinge Bridge Station ...");
                tracker.AppendLine("Correcting forecasted Water Heights at Hardinge Bridge Station ...");
                Console.ResetColor();

                string station = "Hardinge-RB";
                string[] forecastFile = File.ReadAllLines(forecastHDir + "Forecast_ganges_" + foreDate.ToString("yyyy-MM-dd") + ".txt");
                Dictionary<DateTime, float> foreWL = new Dictionary<DateTime, float>();

                foreach (string line in forecastFile)
                {
                    var element = line.Split('\t');
                    foreWL.Add(DateTime.ParseExact(element[0], "yyyy-MM-dd", CultureInfo.InvariantCulture), float.Parse(element[1]));
                }
                int missingDays = Convert.ToInt32((foreWL.FirstOrDefault().Key - foreDate).TotalDays);
                Console.WriteLine(missingDays);
                Dictionary<DateTime, float> hindWL = new Dictionary<DateTime, float>();

                sql = "Select Date, WL from WaterLevel where Date >= '" + bndstarthindDate.ToString("yyyy-MM-dd") + "' and Date<='" + foreDate.ToString("yyyy-MM-dd") + "' and Station='" + station + "' Order by Date ASC";
                command = new SQLiteCommand(sql, sqlconn);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    hindWL.Add(Convert.ToDateTime(reader["Date"]), Convert.ToSingle(reader["WL"]));
                }
                float foredateWL = hindWL[foreDate];
                float day_1WL = hindWL[foreDate.AddDays(-1)];
                float day_2WL = hindWL[foreDate.AddDays(-2)];
                float day_3WL = hindWL[foreDate.AddDays(-3)];
                float meandiff = ((foredateWL - day_1WL) + (foredateWL - day_2WL) / 2.0f + (foredateWL - day_3WL) / 3.0f) / 3.0f;

                for (int i = 1; i < missingDays; i++)
                {
                    hindWL.Add(foreDate.AddDays(i), foredateWL + meandiff * i);
                }
                float correction = foreWL.FirstOrDefault().Value - hindWL.LastOrDefault().Value;
                foreach (var record in foreWL)
                {
                    hindWL.Add(record.Key, record.Value - correction);
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Bnd Date\tWL");
                foreach (DateTime dates in hindWL.Keys)
                {
                    sb.AppendLine(dates.ToString("yyyy-MM-dd") + "\t" + hindWL[dates].ToString("0.00"));
                    Console.WriteLine(dates.ToString("yyyy-MM-dd") + "," + hindWL[dates].ToString("0.00"));
                }
                File.WriteAllText(correctHDir + station + "_" + foreDate.ToString("yyyy-MM-dd") + ".txt", sb.ToString());
                sb.Clear();
                // ----------------- Calculating forecast Trend in WL of Hardinge Bridge Station -------------------------------------------  
                for (int i = 1; i < 9; i++)
                {
                    trendGanges.Add(foreDate.AddDays(i), (hindWL[foreDate.AddDays(i)] - hindWL[foreDate]));
                    Console.WriteLine((hindWL[foreDate.AddDays(i)] - hindWL[foreDate]).ToString("0.00"));
                }

                //---------------------- USing forecast Trend of Bahadurabad Station for depended Boundaries ------------------------------
                foreach (string element in gangesBnds)
                {
                    Dictionary<DateTime, float> hindcastWL = new Dictionary<DateTime, float>();
                    sql = "Select Date, WL from WaterLevel where Date >= '" + bndstarthindDate.ToString("yyyy-MM-dd") + "' and Date<='" + foreDate.ToString("yyyy-MM-dd") + "' and Station= '" + element + "' Order by Date ASC";
                    command = new SQLiteCommand(sql, sqlconn);
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        hindcastWL.Add(Convert.ToDateTime(reader["Date"]), Convert.ToSingle(reader["WL"]));
                    }
                    float foredayWL = hindcastWL.Last().Value;
                    foreach (var record in trendBrahma)
                    {
                        hindcastWL.Add(record.Key, record.Value + foredayWL);
                    }

                    sb.AppendLine("Bnd Date\tWL");
                    foreach (DateTime dates in hindcastWL.Keys)
                    {
                        sb.AppendLine(dates.ToString("yyyy-MM-dd") + "\t" + hindcastWL[dates].ToString("0.00"));
                        Console.WriteLine(dates.ToString("yyyy-MM-dd") + "," + hindcastWL[dates].ToString("0.00"));
                    }
                    File.WriteAllText(correctHDir + element + "_" + foreDate.ToString("yyyy-MM-dd") + ".txt", sb.ToString());
                    sb.Clear();
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Hardinge Bridge and depended station's forecasts generated successfully.");
                tracker.AppendLine("Hardinge Bridge and depended station's forecasts generated successfully.");
                Console.ResetColor();
            }
            catch (Exception error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Hardinge Bridge and depended station's forecasts cannot be generated due to an error. Error: " + error);
                tracker.AppendLine("Hardinge Bridge and depended station's forecasts cannot be generated due to an error. Error: " + error);
                Console.ResetColor();
            }
            File.WriteAllText(logfilepath, tracker.ToString());
        }
    }
}
