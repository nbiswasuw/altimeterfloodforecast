using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using HtmlAgilityPack;
using System.Globalization;
using System.IO;
using Microsoft.Research.Science.Data.Imperative;
using Microsoft.Research.Science.Data;
using System.IO.Compression;
using System.Data.SqlClient;
using DHI.Generic.MikeZero.DFS;
using DHI.Generic.MikeZero;
using System.Diagnostics;
using System.Windows.Forms.DataVisualization.Charting;


namespace Aviso_J2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        string directory = @"M:\NASAFF\";
        SqlConnection con = new SqlConnection(@"Data Source = .\SQLEXPRESS; AttachDbFilename=M:\NASAFF\Database\NASAffwcFF.mdf; Integrated Security=True; User Instance=True");
        SqlCommand cmd = new SqlCommand();
        System.Data.DataSet ds = new System.Data.DataSet();
        SqlDataAdapter ad = new SqlDataAdapter();

        public static void UnZip(string zipFile, string folderPath)
        {
            if (!File.Exists(zipFile))
                throw new FileNotFoundException();

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            Shell32.Shell objShell = new Shell32.Shell();
            Shell32.Folder destinationFolder = objShell.NameSpace(folderPath);
            Shell32.Folder sourceFile = objShell.NameSpace(zipFile);

            foreach (var file in sourceFile.Items())
            {
                destinationFolder.CopyHere(file, 4 | 16);
            }
        }
        private void btnDownloadFFWC_Click(object sender, EventArgs e)
        {
            listForecastStationBox.Visible = false;
            chartStationForecast.Visible = false; 
            try
            {
                WebClient client = new WebClient();
                string htmlCode = client.DownloadString("http://www.ffwc.gov.bd/ffwc_charts/waterlevel.php");
                HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(htmlCode);
                con.Open();
                try
                {
                    HtmlNodeCollection tables = doc.DocumentNode.SelectNodes("//table");
                    HtmlNodeCollection rows = tables[0].SelectNodes(".//tr");
                    HtmlNodeCollection col = rows[1].SelectNodes(".//td");
                    string dateTimeText = "2016-" + col[4].InnerText.Trim();
                    DateTime dataDate = DateTime.ParseExact(dateTimeText, "yyyy-dd-MM", CultureInfo.InvariantCulture).AddHours(6);
                    string noStation = (rows.Count-3).ToString();
                    for (int i = 0; i < rows.Count - 3; ++i)
                    {
                        HtmlNodeCollection cols = rows[i + 3].SelectNodes(".//td");
                        if (cols.Count > 4 && cols[4].InnerText != "NP")
                        {
                            try
                            {
                                cmd = new SqlCommand("INSERT INTO FFWCData VALUES(@dataDate, @individual, @individual2)", con);
                                cmd.Parameters.AddWithValue("@dataDate", dataDate);
                                cmd.Parameters.AddWithValue("@individual", cols[1].InnerText);
                                cmd.Parameters.AddWithValue("@individual2", cols[4].InnerText.Trim());
                                cmd.ExecuteNonQuery();
                            }
                            catch (SqlException)
                            {
                                cmd = new SqlCommand("Update FFWCData SET WL = @value Where Date = @dataDate AND Station= @station", con);
                                cmd.Parameters.AddWithValue("@dataDate", dataDate);
                                cmd.Parameters.AddWithValue("@station", cols[1].InnerText);
                                cmd.Parameters.AddWithValue("@value", cols[4].InnerText.Trim());
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    con.Close();
                    MessageBox.Show("WL Data from FFWC Website downloaded successfully.", "FFWC WL Download", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    reportBox.Text = "WL Data from FFWC Website for the Date: " + dataDate.ToString("yyyyMMdd") + " successfully downloaded. Total number of stations: " + noStation;
                }
                catch (Exception error)
                {
                    MessageBox.Show("WL Data from FFWC Website cannot be downloaded due to an error. Error: " + error.Message, "FFWC WL Download Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    con.Close();
                    reportBox.Text = "WL Data from FFWC Website cannot be downloaded due to an error. This could be due to interruption in internet connection or FFWC website is not updated with WL Data. FFWC CSV file can be imported to skip this step. Error: " + error.ToString();
                }
            }
            catch (Exception errr)
            {
                con.Close();
                MessageBox.Show("FFWC Website cannot be retrieved due to error. Error: " + errr.Message);
                reportBox.Text = "WL Data from FFWC Website cannot be downloaded due to an error. This could be due to interruption in internet connection or FFWC website is not updated with WL Data. FFWC CSV file can be imported to skip this step. Error: " + errr.ToString();
            }
        }
        private void btnHDSimulate_Click(object sender, EventArgs e)
        {
            listForecastStationBox.Visible = false;
            chartStationForecast.Visible = false;
            //Change .sim File end date
            DateTime today = DateTime.Now;
            string startdate = "         start = " + today.AddDays(-7).Year + ", " + today.AddDays(-7).Month + ", " + today.AddDays(-7).Day + ", 6, 0, 0";
            string enddate = "         end = " + today.Year + ", " + today.Month + ", " + today.Day + ", 6, 0, 0";

            string[] alllines = File.ReadAllLines(directory + @"Model\M11\FF2003.sim11");
            alllines[38] = startdate;
            alllines[39] = enddate;
            alllines[72] = @"         hd = 2, 'Results\FF2003-HD.RES11', false, " + today.AddDays(-7).Year + ", " + today.AddDays(-7).Month + ", " + today.AddDays(-7).Day + ", 6, 0, 0";
            alllines[75] = @"         rr = 1, 'Results\FF2003-RR.RES11', false, " + today.AddDays(-7).Year + ", " + today.AddDays(-7).Month + ", " + today.AddDays(-7).Day + ", 6, 0, 0";
            File.WriteAllLines(directory + @"Model\M11\FF2003.sim11", alllines);
            Process.Start(directory + @"Programs\NecessaryFiles\FF_Model.BAT");
        }
        private void btnDownloadSat_Click(object sender, EventArgs e)
        {
            listForecastStationBox.Visible = false;
            chartStationForecast.Visible = false;
            ///--------------------------------------------For JSON2 Data Downloading----------------------------------------------//////////////////
            try
            {
                //---------------Creating directory if not exists ---------------------------------------
                if (Directory.Exists(@directory + @"SatelliteData\GranuleData") != true)
                {
                    Directory.CreateDirectory(directory + @"SatelliteData\GranuleData");
                }

                //-------------------------------------Checking all the files in ftp server --------------------------------------------------------
                List<string> sb = new List<string>();
                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://avisoftp.cnes.fr/AVISO/pub/jason-2/igdr/latest_data/");
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();
                StreamReader streamReader = new StreamReader(response.GetResponseStream());
                string line = streamReader.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    sb.Add(line);
                    line = streamReader.ReadLine();
                }
                streamReader.Close();

                //---------------------------------------------Latest file finding and downloading --------------------------------------------
                // ---------------------------------------------- For Ganges River -------------------------------------------------------
                string gangFile = "";
                for (int i = sb.Count - 1; i >= 0; i--)
                {
                    if (sb[i].Substring(16, 3) == "014" || sb[i].Substring(16, 3) == "079" || sb[i].Substring(16, 3) == "155" || sb[i].Substring(16, 3) == "192")
                    {
                        string filename = "ftp://avisoftp.cnes.fr/AVISO/pub/jason-2/igdr/latest_data/" + sb[i];
                        gangFile = sb[i];
                        string savefilename = directory + @"SatelliteData\GranuleData\" + sb[i];
                        WebClient ftpClient = new WebClient();
                        ftpClient.DownloadFile(filename, savefilename);
                        break;
                    }
                }


                //-------------------------------------- For Brahmaputra River ---------------------------------------------------------------
                string brahmaFile = "";
                for (int i = sb.Count - 1; i >= 0; i--)
                {
                    if (sb[i].Substring(16, 3) == "053" || sb[i].Substring(16, 3) == "166" || sb[i].Substring(16, 3) == "242")
                    {
                        string filename = "ftp://avisoftp.cnes.fr/AVISO/pub/jason-2/igdr/latest_data/" + sb[i];
                        brahmaFile = sb[i];
                        string savefilename = directory + @"SatelliteData\GranuleData\" + sb[i];
                        WebClient ftpClient = new WebClient();
                        ftpClient.DownloadFile(filename, savefilename);
                        break;
                    }
                }

                ///------------------------------- Check with PODAAC Ftp site for more latest Data ----------------------------------
                double ageGanges = (DateTime.Today - DateTime.ParseExact(gangFile.Substring(20, 8), "yyyyMMdd", CultureInfo.InvariantCulture)).TotalDays;
                double ageBrahma = (DateTime.Today - DateTime.ParseExact(brahmaFile.Substring(20, 8), "yyyyMMdd", CultureInfo.InvariantCulture)).TotalDays;
                if (ageGanges > 3.0 || ageBrahma > 3.0)
                {
                    DialogResult result = MessageBox.Show("Age of one or both of the Virtual Station files are more than 3 days. System want to check with Po.DAAC server. Do you want to continue?", "PO.DAAC Server Browsing Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.Yes)
                    {
                        int cycleNo = int.Parse(gangFile.Substring(12, 3));
                        List<string> podaacList = new List<string>();
                        FtpWebRequest podaacRequest = (FtpWebRequest)WebRequest.Create("ftp://data.nodc.noaa.gov/pub/data.nodc/jason2/igdr/igdr/cycle" + cycleNo + "/");
                        podaacRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                        FtpWebResponse podaacResponse = (FtpWebResponse)podaacRequest.GetResponse();
                        StreamReader podaacStreams = new StreamReader(podaacResponse.GetResponseStream());
                        string podaacLine = podaacStreams.ReadLine();
                        while (!string.IsNullOrEmpty(podaacLine))
                        {
                            podaacList.Add(podaacLine);
                            podaacLine = podaacStreams.ReadLine();
                        }
                        podaacStreams.Close();

                        //---------------------------------------------Latest file finding and downloading --------------------------------------------
                        // ---------------------------------------------- For Ganges River -------------------------------------------------------
                        for (int i = podaacList.Count - 1; i >= 0; i--)
                        {
                            if (podaacList[i].Substring(16, 3) == "014" || podaacList[i].Substring(16, 3) == "079" || podaacList[i].Substring(16, 3) == "155" || podaacList[i].Substring(16, 3) == "192")
                            {
                                double agePodaac = (DateTime.Today - DateTime.ParseExact(podaacList[i].Substring(20, 8), "yyyyMMdd", CultureInfo.InvariantCulture)).TotalDays;
                                if (agePodaac < ageGanges)
                                {
                                    string filename = "ftp://data.nodc.noaa.gov/pub/data.nodc/jason2/igdr/igdr/cycle" + cycleNo + "/" + podaacList[i];
                                    gangFile = podaacList[i];
                                    ageGanges = agePodaac;
                                    string savefilename = directory + @"SatelliteData\GranuleData\" + podaacList[i];
                                    WebClient ftpClient = new WebClient();
                                    ftpClient.DownloadFile(filename, savefilename);
                                    break;
                                }
                            }
                        }


                        //-------------------------------------- For Brahmaputra River ---------------------------------------------------------------
                        for (int i = podaacList.Count - 1; i >= 0; i--)
                        {
                            if (podaacList[i].Substring(16, 3) == "053" || podaacList[i].Substring(16, 3) == "166" || podaacList[i].Substring(16, 3) == "242")
                            {
                                double agePodaac = (DateTime.Today - DateTime.ParseExact(podaacList[i].Substring(20, 8), "yyyyMMdd", CultureInfo.InvariantCulture)).TotalDays;
                                if (agePodaac < ageBrahma)
                                {
                                    string filename = "ftp://data.nodc.noaa.gov/pub/data.nodc/jason2/igdr/igdr/cycle" + cycleNo + "/" + podaacList[i];
                                    brahmaFile = podaacList[i];
                                    ageBrahma = agePodaac;
                                    string savefilename = directory + @"SatelliteData\GranuleData\" + podaacList[i];
                                    WebClient ftpClient = new WebClient();
                                    ftpClient.DownloadFile(filename, savefilename);
                                    break;
                                }
                            }
                        }
                    }
                    
                }
                
                MessageBox.Show("Virtual Station files successfully downloaded.", "Virtual Station File Download", MessageBoxButtons.OK, MessageBoxIcon.Information);
                reportBox.Text = "Virtual Station files downloaded successfully." + "\r\nFor Ganges, Latest Virtual Station Pass Number: " + gangFile.Substring(16, 3) + ", Date of fly: " + gangFile.Substring(20, 8) + ", Age of file: " + ageGanges.ToString("0") + " Days.\r\nFor Brahmaputra, Latest Virtual Station Pass Number: " + brahmaFile.Substring(16, 3) + ", Date of fly: " + brahmaFile.Substring(20, 8) + ", Age of file: " + ageBrahma.ToString() + " Days.";
            }
            catch (Exception error)
            {
                MessageBox.Show("Virtual Station files cannot be downloaded. Error: " + error.Message, "Data Download Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                reportBox.Text = "Virtual Station files cannot be downloaded. It could be due to interruption in internet connection. Detailed error: " + error.ToString();
            }
            
            /*
            /////------------------------------------------------For Altika Data Downloading----------------------------------------///////
            try
            {
                
                //-----------------------------------------    BWDB    ----------------------------------------------------------------------//
                List<string> sb = new List<string>();
                FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create("ftp://avisoftp.cnes.fr/AVISO/pub/saral/igdr_t/latest_data/");
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();
                StreamReader streamReader = new StreamReader(response.GetResponseStream());
                string line = streamReader.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    sb.Add(line);
                    line = streamReader.ReadLine();
                }
                streamReader.Close();

                for (int i = sb.Count - 1; i >= 0; i--)
                {
                    if (sb[i].Substring(16, 4) == "0079" || sb[i].Substring(16, 4) == "0251" || sb[i].Substring(16, 4) == "0266" || sb[i].Substring(16, 4) == "0352" || sb[i].Substring(16, 4) == "0438" || sb[i].Substring(16, 4) == "0537" || sb[i].Substring(16, 4) == "0623" || sb[i].Substring(16, 4) == "0709" || sb[i].Substring(16, 4) == "0724" || sb[i].Substring(16, 4) == "0810" || sb[i].Substring(16, 4) == "0982" || sb[i].Substring(16, 4) == "0995")
                    {
                        string filename = "ftp://avisoftp.cnes.fr/AVISO/pub/saral/igdr_t/latest_data/" + sb[i];
                        string savefilename = directory + @"SatelliteData\GranuleData\" + sb[i];
                        WebClient ftpClient = new WebClient();
                        ftpClient.DownloadFile(filename, savefilename);
                        break;
                    }
                }

                for (int i = sb.Count - 1; i >= 0; i--)
                {
                    if (sb[i].Substring(16, 4) == "0051" || sb[i].Substring(16, 4) == "0137" || sb[i].Substring(16, 4) == "0223" || sb[i].Substring(16, 4) == "0238" || sb[i].Substring(16, 4) == "0324" || sb[i].Substring(16, 4) == "0423" || sb[i].Substring(16, 4) == "0509" || sb[i].Substring(16, 4) == "0595" || sb[i].Substring(16, 4) == "0681" || sb[i].Substring(16, 4) == "0782" || sb[i].Substring(16, 4) == "0868" || sb[i].Substring(16, 4) == "0954" || sb[i].Substring(16, 4) == "0967")
                    {
                        string filename = "ftp://avisoftp.cnes.fr/AVISO/pub/saral/igdr_t/latest_data/" + sb[i];
                        string savefilename = directory + @"SatelliteData\GranuleData\" + sb[i];
                        WebClient ftpClient = new WebClient();
                        ftpClient.DownloadFile(filename, savefilename);
                        break;
                    }
                }
                 MessageBox.Show("Data successfully downloaded.", "Data Download", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }*/
        }
        private void btnProcessSatellite_Click(object sender, EventArgs e)
        {
            listForecastStationBox.Visible = false;
            chartStationForecast.Visible = false;
            StringBuilder reportContent = new StringBuilder();
            /////-------------------------------------------------Decompressing Files---------------------------------//////////////
            try
            {
                string dirpath = directory + @"SatelliteData\GranuleData";
                DirectoryInfo di = new DirectoryInfo(dirpath);

                foreach (FileInfo fi in di.GetFiles("*.zip"))
                {
                    UnZip(fi.FullName, dirpath);
                }
            }
            catch (Exception error)
            {
                MessageBox.Show("Error occured in decompressing virtual station zipped files." + error.Message);
                reportContent.AppendLine("Error occured in decompressing virtual station zipped files. Please check the contents of zipped files. Redownload of files can solve decompression error." + error.Message);
            }
            ////-------------------------------------------- Data Processing and Referencing to EGM08------------------------------/////////
            try
            {
                reportContent.AppendLine("Virtual Station Heights are extracting...");
                DirectoryInfo di = new DirectoryInfo(directory + @"SatelliteData\GranuleData");
                foreach (FileInfo fi in di.GetFiles("*.nc"))
                {
                    double maxlat = 0;
                    double minlat = 0;
                    double hcorr = 0;
                    string filename = "";
                    string folderName = "";
                    
                    if (fi.Name.Substring(0, 3) == "JA2")
                    {
                               if (fi.Name.Substring(16, 3) == "192") { maxlat = 25.22; minlat = 25.2; hcorr = 63.173; filename = "192"; folderName = "Ganges"; }
                        else if (fi.Name.Substring(16, 3) == "079") { maxlat = 25.85; minlat = 25.825; hcorr = 66.151; filename = "079"; folderName = "Ganges"; }
                        else if (fi.Name.Substring(16, 3) == "014") { maxlat = 25.52; minlat = 25.5; hcorr = 63.464; filename = "014"; folderName = "Ganges"; }
                        else if (fi.Name.Substring(16, 3) == "155") { maxlat = 25.31; minlat = 25.27; hcorr = 59.967; filename = "155"; folderName = "Ganges"; }
                        else if (fi.Name.Substring(16, 3) == "242") { maxlat = 26.75; minlat = 26.7; hcorr = 55.139; filename = "242"; folderName = "Brahmaputra"; }
                        else if (fi.Name.Substring(16, 3) == "053") { maxlat = 26.78; minlat = 26.75; hcorr = 54.731; filename = "053"; folderName = "Brahmaputra"; }
                        else if (fi.Name.Substring(16, 3) == "166") { maxlat = 26.23; minlat = 26.2; hcorr = 49.315; filename = "166"; folderName = "Brahmaputra"; }

                        var dataset = Microsoft.Research.Science.Data.DataSet.Open(fi.FullName + "?openMode=readOnly");
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
                        sob.AppendLine("Lat\tLon\tH(m)\tBS(dB)");
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
                        File.WriteAllText(directory + @"SatelliteData\VSHeight\" + folderName + @"\JA2_VS_0" + filename + "_" + dataDate.ToString("yyyy-MM-dd") + ".txt", sob.ToString());
                        reportContent.AppendLine("For " + folderName + " River, Virtual Station No. " + filename + ", Date:" + dataDate.ToString("yyyy-MM-dd") + ", Age: " + (DateTime.Today-dataDate.Date).TotalDays.ToString("0") + " days, Maximum BS: " + bsValues.Max() + " dB, Height at Maximum BS: " + heights[Array.IndexOf(bsValues.ToArray(), bsValues.ToArray().Max())].ToString("0.00") + " m, Average height: " + heights.Average().ToString("0.00") + " m");
                        sob.Clear();
                        bsValues.Clear();
                        heights.Clear();
                    }
                    /*
                    if (fi.Name.Substring(0, 3) == "SRL")
                    {
                        if (fi.Name.Substring(16, 4) == "0724") { maxlat = 27.59; minlat = 27.58; hcorr = 54.987; filename = "0724"; folderName = "Brahmaputra"; }
                        else if (fi.Name.Substring(16, 4) == "0266") { maxlat = 26.86; minlat = 26.85; hcorr = 57.648; filename = "0266"; folderName = "Brahmaputra"; }
                        else if (fi.Name.Substring(16, 4) == "0810") { maxlat = 26.7; minlat = 26.68; hcorr = 53.338; filename = "0810"; folderName = "Brahmaputra"; }
                        else if (fi.Name.Substring(16, 4) == "0352") { maxlat = 26.6; minlat = 26.58; hcorr = 49.932; filename = "0352"; folderName = "Brahmaputra"; }
                        else if (fi.Name.Substring(16, 4) == "0438") { maxlat = 26.21; minlat = 26.2; hcorr = 49.341; filename = "0438"; folderName = "Brahmaputra"; }
                        else if (fi.Name.Substring(16, 4) == "0982") { maxlat = 26.14; minlat = 26.12; hcorr = 49.979; filename = "0982"; folderName = "Brahmaputra"; }
                        else if (fi.Name.Substring(16, 4) == "0995") { maxlat = 27.03; minlat = 27.02; hcorr = 57.882; filename = "0995"; folderName = "Brahmaputra"; }
                        else if (fi.Name.Substring(16, 4) == "0537") { maxlat = 26.82; minlat = 26.8; hcorr = 56.440; filename = "0537"; folderName = "Brahmaputra"; }
                        else if (fi.Name.Substring(16, 4) == "0079") { maxlat = 26.68; minlat = 26.66; hcorr = 52.417; filename = "0079"; folderName = "Brahmaputra"; }
                        else if (fi.Name.Substring(16, 4) == "0623") { maxlat = 26.52; minlat = 26.5; hcorr = 49.7; filename = "0623"; folderName = "Brahmaputra"; }
                        else if (fi.Name.Substring(16, 4) == "0709") { maxlat = 26.22; minlat = 26.2; hcorr = 49.25; filename = "0709"; folderName = "Brahmaputra"; }
                        else if (fi.Name.Substring(16, 4) == "0251") { maxlat = 26.16; minlat = 26.15; hcorr = 49.814; filename = "0251"; folderName = "Brahmaputra"; }
                        else if (fi.Name.Substring(16, 4) == "0954") { maxlat = 26.68; minlat = 26.67; hcorr = 67.757; filename = "0954"; folderName = "Ganges"; }
                        else if (fi.Name.Substring(16, 4) == "0324") { maxlat = 26.2; minlat = 26.19; hcorr = 67.472; filename = "0324"; folderName = "Ganges"; }
                        else if (fi.Name.Substring(16, 4) == "0782") { maxlat = 25.67; minlat = 25.66; hcorr = 64.774; filename = "0782"; folderName = "Ganges"; }
                        else if (fi.Name.Substring(16, 4) == "0238") { maxlat = 25.52; minlat = 25.51; hcorr = 63.326; filename = "0238"; folderName = "Ganges"; }
                        else if (fi.Name.Substring(16, 4) == "0423") { maxlat = 25.285; minlat = 25.275; hcorr = 58.262; filename = "0423"; folderName = "Ganges"; }
                        else if (fi.Name.Substring(16, 4) == "0967") { maxlat = 25.32; minlat = 25.31; hcorr = 60.553; filename = "0967"; folderName = "Ganges"; }
                        else if (fi.Name.Substring(16, 4) == "0509") { maxlat = 25.34; minlat = 25.33; hcorr = 61.797; filename = "0509"; folderName = "Ganges"; }
                        else if (fi.Name.Substring(16, 4) == "0051") { maxlat = 25.5; minlat = 25.49; hcorr = 63.715; filename = "0051"; folderName = "Ganges"; }
                        else if (fi.Name.Substring(16, 4) == "0595") { maxlat = 25.735; minlat = 25.725; hcorr = 65.625; filename = "0595"; folderName = "Ganges"; }
                        else if (fi.Name.Substring(16, 4) == "0137") { maxlat = 26.125; minlat = 26.115; hcorr = 67.316; filename = "0137"; folderName = "Ganges"; }
                        else if (fi.Name.Substring(16, 4) == "0681") { maxlat = 26.37; minlat = 26.36; hcorr = 67.608; filename = "0681"; folderName = "Ganges"; }
                        else if (fi.Name.Substring(16, 4) == "0223") { maxlat = 26.74; minlat = 26.73; hcorr = 67.629; filename = "0223"; folderName = "Ganges"; }

                        var dataset = Microsoft.Research.Science.Data.DataSet.Open(fi.FullName + "?openMode=readOnly");
                        int[] lat = dataset.GetData<int[]>("lat");
                        int[] lon = dataset.GetData<int[]>("lon");
                        sbyte[] meas_ind = dataset.GetData<sbyte[]>("meas_ind");
                        double[] time = dataset.GetData<double[]>("time");
                        short[] model_dry_tropo_corr = dataset.GetData<short[]>("model_dry_tropo_corr");
                        short[] model_wet_tropo_corr = dataset.GetData<short[]>("model_wet_tropo_corr");
                        short[] iono_corr_gim = dataset.GetData<short[]>("iono_corr_gim");
                        short[] solid_earth_tide = dataset.GetData<short[]>("solid_earth_tide");
                        short[] pole_tide = dataset.GetData<short[]>("pole_tide");
                        int[,] lon_40hz = dataset.GetData<int[,]>("lon_40hz");
                        int[,] lat_40hz = dataset.GetData<int[,]>("lat_40hz");
                        sbyte[,] ice1_qual_flag_40hz = dataset.GetData<sbyte[,]>("ice1_qual_flag_40hz");
                        double[,] time_40hz = dataset.GetData<double[,]>("time_40hz");
                        int[,] alt_40hz = dataset.GetData<int[,]>("alt_40hz");
                        int[,] ice1_range_40hz = dataset.GetData<int[,]>("ice1_range_40hz");
                        short[,] ice1_sig0_40hz = dataset.GetData<short[,]>("ice1_sig0_40hz");

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
                        double s_ice1_sig0_40 = 0.01;
                        double media_corr;
                        double height;
                        double latitude;
                        double longitude;
                        DateTime dataDate = new DateTime();
                        StringBuilder sob = new StringBuilder();

                        for (int i = 0; i < lat.Length; i++)
                        {
                            if (model_dry_tropo_corr[i] != 32767 && model_wet_tropo_corr[i] != 32767 && iono_corr_gim[i] != 32767 && solid_earth_tide[i] != 32767 && pole_tide[i] != 32767)
                            {
                                media_corr = model_dry_tropo_corr[i] * s_model_dry + model_wet_tropo_corr[i] * s_model_wet + iono_corr_gim[i] * s_iono_corr + solid_earth_tide[i] * s_solid_earth_tide + pole_tide[i] * s_pole_tide;
                                for (int j = 0; j < meas_ind.Length; j++)
                                {
                                    if (ice1_qual_flag_40hz[i, j] != 1 && lat_40hz[i, j] != 2147483647 && lat_40hz[i, j] * s_latlon >= minlat && lat_40hz[i, j] * s_latlon <= maxlat)
                                    {
                                        height = alt_40hz[i, j] * s_alt - (media_corr + ice1_range_40hz[i, j] * s_icerange_ku) - 0.7 + hcorr;
                                        longitude = lon_40hz[i, j] * s_latlon;
                                        latitude = lat_40hz[i, j] * s_latlon;
                                        dataDate = refDate.AddSeconds(time_40hz[i, j]);
                                        sob.AppendLine(latitude + "," + longitude + "," + Math.Round(height, 2) + "," + ice1_sig0_40hz[i, j] * s_ice1_sig0_40);
                                    }
                                }
                            }
                        }
                        File.WriteAllText(directory + @"SatelliteData\Output\" + folderName + @"\SRL_VS_" + filename + "_" + dataDate.ToString("yyyy-MM-dd") + ".txt", sob.ToString());
                        sob.Clear();
                    }*/
                }
                reportBox.Text = reportContent.ToString();
                reportContent.Clear();
                MessageBox.Show("Virtual Station Height extraction completed.", "Virtual Height Extraction", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception error)
            {
                MessageBox.Show("Virtual Station Height cannot be extracted due to an error. Error: " + error.Message, "VS Height Extraction Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                reportBox.Text = "Virtual Station Height cannot be extracted due to an error. This can be due to problem with the .nc files. Please check the files. Deatiled error: " + error.ToString();
            }
        }
        private void btnUpdatehdMnd_Click(object sender, EventArgs e)
        {
            listForecastStationBox.Visible = false;
            chartStationForecast.Visible = false;
            StringBuilder reportContent = new StringBuilder();
            try
            {
                string[] ratingCurves = File.ReadAllLines(directory + @"Programs\NecessaryFiles\RatingCurves.txt");
                string[] bndName = new string[ratingCurves.Length-1];
                int[] curveSegment = new int[ratingCurves.Length-1];
                float[,] curveValues = new float[ratingCurves.Length - 1, 7];
                for (int i = 0; i < ratingCurves.Length - 1; i++)
                {
                    var dispTxt = ratingCurves[i + 1].Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);
                    bndName[i] = dispTxt[0];
                    curveSegment[i] = int.Parse(dispTxt[1]);
                    for (int j = 2; j < dispTxt.Length; j++)
                    {
                        curveValues[i, j - 2] = float.Parse(dispTxt[j]);
                    }

                }
                int bndCount = 0;
                foreach (string element in bndName)
                {
                    string[] fileLines = File.ReadAllLines(directory + @"SatelliteData\ForecastHeights\" + element + ".txt");
                    DateTime[] dataDate = new DateTime[fileLines.Length - 1];
                    float[] bndData = new float[fileLines.Length-1];
                    for (int i = 1; i < fileLines.Length; i++ )
                    {
                        var data = fileLines[i].Split('\t');
                        dataDate[i - 1] = DateTime.ParseExact(data[0], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        bndData[i-1] =float.Parse(data[1]);
                    }
                    if (curveSegment[bndCount] == 1)
                    {
                        float constant = curveValues[bndCount, 1];
                        float thresholdH = curveValues[bndCount, 2];
                        float powerN = curveValues[bndCount, 3];
                        for(int i=0; i<bndData.Length; i++)
                        {
                            bndData[i] = Convert.ToSingle(constant*Math.Pow((bndData[i]-thresholdH), powerN));
                        }
                    }
                    else if (curveSegment[bndCount] == 2)
                    {
                        float criticalH = curveValues[bndCount, 0];
                        float constant1 = curveValues[bndCount, 1];
                        float thresholdH1 = curveValues[bndCount, 2];
                        float powerN1 = curveValues[bndCount, 3];
                        float constant2 = curveValues[bndCount, 4];
                        float thresholdH2 = curveValues[bndCount, 5];
                        float powerN2 = curveValues[bndCount, 6];
                        for (int i = 0; i < bndData.Length; i++)
                        {
                            if (bndData[i] <= criticalH)
                            {
                                bndData[i] = Convert.ToSingle(constant1 * Math.Pow((bndData[i] - thresholdH1), powerN1));
                            }
                            else 
                            {
                                bndData[i] = Convert.ToSingle(constant2 * Math.Pow((bndData[i] - thresholdH2), powerN2)); 
                            }
                        }
                    }
                    string filename = directory + @"Model\HDBounds\"+ element + ".dfs0";
                    DfsFactory factory = new DfsFactory();
                    DfsBuilder filecreator = DfsBuilder.Create(element, element, 2012);
                    filecreator.SetDataType(1);
                    filecreator.SetGeographicalProjection(factory.CreateProjectionUndefined());
                    filecreator.SetTemporalAxis(factory.CreateTemporalNonEqCalendarAxis(eumUnit.eumUsec, new DateTime(dataDate[0].Year, dataDate[0].Month, dataDate[0].Day, dataDate[0].Hour, 00, 00)));
                    filecreator.SetItemStatisticsType(StatType.RegularStat);
                    DfsDynamicItemBuilder item = filecreator.CreateDynamicItemBuilder();
                    item.Set(element, eumQuantity.Create(eumItem.eumIDischarge, eumUnit.eumUm3PerSec), DfsSimpleType.Float);
                    item.SetValueType(DataValueType.Instantaneous);
                    item.SetAxis(factory.CreateAxisEqD0());
                    item.SetReferenceCoordinates(1f, 2f, 3f);
                    filecreator.AddDynamicItem(item.GetDynamicItemInfo());
                    filecreator.CreateFile(filename);

                    IDfsFile file = filecreator.GetFile();
                    for (int i = 0; i < dataDate.Length; i++)
                    {
                        double secondInterval = (dataDate[i] - dataDate[0]).TotalSeconds;
                        file.WriteItemTimeStepNext(secondInterval, new float[] { bndData[i] });
                    }
                    file.Close();
                    bndCount = bndCount + 1;
                    reportContent.AppendLine("Discahrge Boundary DFS0 of " + element + " created.");
                }

                string[] bndH = new string[] { "Faridpur", "Rohanpur", "Gaibandha", "Kurigram", "Dinajpur" };
                foreach(string element in bndH)
                {
                    string[] fileLines = File.ReadAllLines(directory + @"SatelliteData\ForecastHeights\" + element + ".txt");
                    DateTime[] dataDate = new DateTime[fileLines.Length - 1];
                    float[] bndData = new float[fileLines.Length - 1];
                    for (int i = 1; i < fileLines.Length; i++)
                    {
                        var data = fileLines[i].Split('\t');
                        dataDate[i - 1] = DateTime.ParseExact(data[0], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        bndData[i - 1] = float.Parse(data[1]);
                    }

                    string filename = directory + @"Model\HDBounds\" + element +"-H.dfs0";
                    DfsFactory factory = new DfsFactory();
                    DfsBuilder filecreator = DfsBuilder.Create(element, element, 2012);
                    filecreator.SetDataType(1);
                    filecreator.SetGeographicalProjection(factory.CreateProjectionUndefined());
                    filecreator.SetTemporalAxis(factory.CreateTemporalNonEqCalendarAxis(eumUnit.eumUsec, new DateTime(dataDate[0].Year, dataDate[0].Month, dataDate[0].Day, dataDate[0].Hour, 00, 00)));
                    filecreator.SetItemStatisticsType(StatType.RegularStat);
                    DfsDynamicItemBuilder item = filecreator.CreateDynamicItemBuilder();
                    item.Set(element, eumQuantity.Create(eumItem.eumIWaterLevel, eumUnit.eumUmeter), DfsSimpleType.Float);
                    item.SetValueType(DataValueType.Instantaneous);
                    item.SetAxis(factory.CreateAxisEqD0());
                    item.SetReferenceCoordinates(1f, 2f, 3f);
                    filecreator.AddDynamicItem(item.GetDynamicItemInfo());
                    filecreator.CreateFile(filename);

                    IDfsFile file = filecreator.GetFile();
                    for (int i = 0; i < dataDate.Length; i++)
                    {
                        double secondInterval = (dataDate[i] - dataDate[0]).TotalSeconds;
                        file.WriteItemTimeStepNext(secondInterval, new float[] { bndData[i]});
                    }
                    file.Close();
                    reportContent.AppendLine("Water level Boundary DFS0 of " + element + " created.");
                }
                reportBox.Text = reportContent.ToString();
                reportContent.Clear();
                MessageBox.Show("Boundary Generation successfully completed.", "Boundary Generation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception error)
            {
                MessageBox.Show("Error in Boundary generation." + error.Message, "Boundary generation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                reportContent.AppendLine("Error in boundary generation. Scroll to see the mssing boundary.");
                reportBox.Text = reportContent.ToString();
                reportContent.Clear();
            }
        }
        
        private void btnGenerateForecast_Click(object sender, EventArgs e)
        {
            listForecastStationBox.Visible = false;
            chartStationForecast.Visible = false;
            string[] stationName = new string[] { "Elashinghat", "Bahadurabad", "Sariakandi", "Serajganj", "Aricha", "Hardinge-RB", "Gorai-RB", "Goalondo", "Bhagyakul" };
            listForecastStationBox.Visible = true;
            chartStationForecast.Visible = true;
            foreach (string element in stationName)
            {
                listForecastStationBox.Items.Add(element);
            }

            int index = 0;
            foreach (string element in stationName)
            {
                string[] forecastOutfile = File.ReadAllLines(directory + @"forecastInfo/OUT.txt");
                List<DateTime> foreDate = new List<DateTime>();
                float[,] forecastWL = new float[9, 9];
                int x = 0;
                for (int i = 22; i < forecastOutfile.Length - 3; i++)
                {
                    var separatedText = forecastOutfile[i].Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);
                    DateTime date = new DateTime(int.Parse(separatedText[0]), int.Parse(separatedText[1]), int.Parse(separatedText[2]), int.Parse(separatedText[3]), int.Parse(separatedText[4]), int.Parse(separatedText[5]));
                    if (date >= DateTime.Today.AddHours(6) && date.Hour == 6 && date.Minute == 0 && date.Second == 0)
                    {
                        foreDate.Add(date);
                        for (int j = 6; j < separatedText.Length; j++)
                        {
                            forecastWL[x, j - 6] = float.Parse(separatedText[j]);
                        }
                        x = x + 1;
                    }
                }

                con.Open();
                cmd = new SqlCommand("Select River, DangerLevel, RHWL from ForecastLocation Where Station = @station", con);
                cmd.Parameters.AddWithValue("@station", element);
                DataTable infotable = new DataTable();
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = cmd;
                adapter.Fill(infotable);
                adapter.Dispose();
                cmd.Dispose();
                con.Close();

                DataTable dt = infotable;
                string riverName = "";
                float dangerlevel = 0f;
                float rhwl = 0f;
                MessageBox.Show(dt.Rows.Count.ToString());
                foreach (DataRow dr in dt.Rows)
                {
                    riverName = dr[0].ToString();
                    dangerlevel = Convert.ToSingle(dr[1]);
                    rhwl = Convert.ToSingle(dr[2]);
                }


                con.Open();
                cmd = new SqlCommand("Select Date, WL from FFWCData Where Station = @station AND Date>= @startDate AND Date<=@endDate", con);
                cmd.Parameters.AddWithValue("@startDate", DateTime.Today.AddDays(-7).AddHours(6));
                cmd.Parameters.AddWithValue("@endDate", DateTime.Today.AddHours(6));
                cmd.Parameters.AddWithValue("@station", element);
                DataTable obsTable = new DataTable();
                adapter.SelectCommand = cmd;
                adapter.Fill(obsTable);
                adapter.Dispose();
                cmd.Dispose();
                con.Close();

                dt = obsTable;
                List<DateTime> obsDate = new List<DateTime>();
                List<float> obsWL = new List<float>();
                MessageBox.Show(dt.Rows.Count.ToString());
                foreach (DataRow dr in dt.Rows)
                {
                    obsDate.Add(Convert.ToDateTime(dr[0]));
                    obsWL.Add(Convert.ToSingle(dr[1]));
                }

                chartStationForecast.Series[0].Points.Clear();
                chartStationForecast.Series[1].Points.Clear();
                chartStationForecast.Series[2].Points.Clear();
                chartStationForecast.Series[3].Points.Clear();

                chartStationForecast.Titles[0].Text = "8 Day Forecast" + "\r\n" + "Station Name: " + element + "  River Name: " + riverName + "\r\n" + "Forecast Date: " + DateTime.Today.AddHours(6);

                List<DateTime> chartDate = new List<DateTime>();
                List<float> chartWL = new List<float>();

                for (int i = 0; i < obsDate.Count; i++)
                {
                    chartDate.Add(obsDate[i]);
                    chartWL.Add(obsWL[i]);
                    chartStationForecast.Series[0].Points.AddXY(obsDate[i], obsWL[i]);
                }

                float correction = forecastWL[0, index] - obsWL[obsWL.Count - 1];
                for (int i = 0; i < 9; i++)
                {
                    chartDate.Add(foreDate[i]);
                    chartWL.Add(forecastWL[i, index]);
                    chartStationForecast.Series[1].Points.AddXY(foreDate[i], (forecastWL[i, index] - correction));
                }

                for (int i = 0; i < chartDate.Count; i++)
                {
                    chartStationForecast.Series[2].Points.AddXY(chartDate[i], dangerlevel);
                    chartStationForecast.Series[3].Points.AddXY(chartDate[i], rhwl);
                }

                chartWL.Add(dangerlevel);
                chartWL.Add(rhwl);
                chartStationForecast.Series[0].ChartType = SeriesChartType.Line;  // Set chart type like Bar chart, Pie chart
                chartStationForecast.Series[0].IsValueShownAsLabel = false;
                chartStationForecast.Series[0].BorderWidth = 3;
                chartStationForecast.Series[0].Color = Color.Green;

                chartStationForecast.Series[1].ChartType = SeriesChartType.Point;
                chartStationForecast.Series[1].IsValueShownAsLabel = false;
                chartStationForecast.Series[1].MarkerSize = 3;
                chartStationForecast.Series[1].Color = Color.Blue;

                chartStationForecast.Series[2].ChartType = SeriesChartType.Line;  // Set chart type like Bar chart, Pie chart
                chartStationForecast.Series[2].IsValueShownAsLabel = false;
                chartStationForecast.Series[2].BorderWidth = 2;

                chartStationForecast.Series[3].ChartType = SeriesChartType.Line;  // Set chart type like Bar chart, Pie chart
                chartStationForecast.Series[3].IsValueShownAsLabel = false;
                chartStationForecast.Series[3].BorderWidth = 2;

                //chartStationForecast.ChartAreas[0].AxisX.Maximum = Convert.ToDouble(chartDate.Max());
                //chartStationForecast.ChartAreas[0].AxisX.Minimum = Convert.ToDouble(chartDate.Min());
                chartStationForecast.ChartAreas[0].AxisY.Maximum = Math.Ceiling(chartWL.Max());
                chartStationForecast.ChartAreas[0].AxisY.Minimum = Math.Floor(chartWL.Min());

                chartStationForecast.ChartAreas[0].AxisY.MajorTickMark.Interval = (Math.Ceiling(chartWL.Max()) - Math.Floor(chartWL.Min())) / 5.0;
                chartStationForecast.ChartAreas[0].AxisY.LabelStyle.Format = "0.0";

                index = index + 1;
                chartDate.Clear();
                chartWL.Clear();
                obsWL.Clear();
                obsDate.Clear();
            }
        }
       /* private void btnClearAllData_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure to delete previous process data?\nThe operation cannot be rolled back. ", "Confirm File Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                DirectoryInfo di = new DirectoryInfo(directory + @"SatelliteData\GranuleData");
                foreach (FileInfo fi in di.GetFiles("*.*"))
                {
                    File.Delete(fi.FullName);
                }
                di = new DirectoryInfo(directory + @"SatelliteData\Output\Ganges");
                foreach (FileInfo fi in di.GetFiles("*.*"))
                {
                    File.Delete(fi.FullName);
                }
                di = new DirectoryInfo(directory + @"SatelliteData\Output\Brahmaputra");
                foreach (FileInfo fi in di.GetFiles("*.*"))
                {
                    File.Delete(fi.FullName);
                }
                MessageBox.Show("All previous records cleared successfully.", "Operation Completion", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }*/
        private void listForecastStationBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string element = listForecastStationBox.SelectedItem.ToString();
            string[] fileLines = File.ReadAllLines(directory + @"SatelliteData\ForecastHeights\" + element + ".txt");
            DateTime[] dataDate = new DateTime[fileLines.Length - 1];
            float[] bndData = new float[fileLines.Length - 1];
            for (int i = 1; i < fileLines.Length; i++)
            {
                var data = fileLines[i].Split('\t');
                dataDate[i - 1] = DateTime.ParseExact(data[0], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                bndData[i - 1] = float.Parse(data[1]);
            }

            chartStationForecast.Series[0].Points.Clear();
            chartStationForecast.Series[1].Points.Clear();

            chartStationForecast.Titles[0].Text = "8 Day Forecast" + "\r\n" + "Boundary Station Name: " + element + "\r\n" + "Forecast Date: " + DateTime.Today.AddHours(6);

            List<DateTime> chartDate = new List<DateTime>();
            List<float> chartWL = new List<float>();
            for (int i = 0; i < bndData.Length; i++)
            {
                chartDate.Add(dataDate[i]);
                chartWL.Add(bndData[i]);
                if ((DateTime.Today.AddHours(6) - dataDate[i]).TotalSeconds >= 0)
                {
                    chartStationForecast.Series[0].Points.AddXY(dataDate[i], bndData[i]);
                }
                else
                {
                    chartStationForecast.Series[1].Points.AddXY(dataDate[i], bndData[i]);
                }
            }


            chartStationForecast.Series[0].ChartType = SeriesChartType.Line;  // Set chart type like Bar chart, Pie chart
            chartStationForecast.Series[0].IsValueShownAsLabel = false;
            chartStationForecast.Series[0].BorderWidth = 3;

            chartStationForecast.Series[1].ChartType = SeriesChartType.Point;
            chartStationForecast.Series[1].IsValueShownAsLabel = false;
            chartStationForecast.Series[1].MarkerColor = Color.Red;
            chartStationForecast.Series[1].MarkerSize = 5;

            //chartStationForecast.ChartAreas[0].AxisX.Maximum = Convert.ToDouble(chartDate.Max());
            //chartStationForecast.ChartAreas[0].AxisX.Minimum = Convert.ToDouble(chartDate.Min());
            chartStationForecast.ChartAreas[0].AxisY.Maximum = Math.Ceiling(chartWL.Max());
            chartStationForecast.ChartAreas[0].AxisY.Minimum = Math.Floor(chartWL.Min());

            chartStationForecast.ChartAreas[0].AxisY.MajorTickMark.Interval = (Math.Ceiling(chartWL.Max()) - Math.Floor(chartWL.Min())) / 5.0;
            chartStationForecast.ChartAreas[0].AxisY.LabelStyle.Format = "0.0";

            chartDate.Clear();
            chartWL.Clear();

            /*
            string element = listForecastStationBox.SelectedItem.ToString();
            int index = listForecastStationBox.SelectedIndex;
            string[] forecastOutfile = File.ReadAllLines(directory + @"Model/Output/OUT.txt");
            List<DateTime> foreDate = new List<DateTime>();
            float[,] forecastWL = new float[9, 9];
            int x = 0;
            for (int i = 22; i < forecastOutfile.Length - 3; i++)
            {
                var separatedText = forecastOutfile[i].Split(default(string[]), StringSplitOptions.RemoveEmptyEntries);
                DateTime date = new DateTime(int.Parse(separatedText[0]), int.Parse(separatedText[1]), int.Parse(separatedText[2]), int.Parse(separatedText[3]), int.Parse(separatedText[4]), int.Parse(separatedText[5]));
                if (date >= DateTime.Today.AddHours(6) && date.Hour == 6 && date.Minute == 0 && date.Second == 0)
                {
                    foreDate.Add(date);
                    for (int j = 6; j < separatedText.Length; j++)
                    {
                        forecastWL[x, j - 6] = float.Parse(separatedText[j]);
                    }
                    x = x + 1;
                }
            }

            con.Open();
            cmd = new SqlCommand("Select River, DangerLevel, RHWL from ForecastLocation Where Station = @station", con);
            cmd.Parameters.AddWithValue("@station", element);
            DataTable infotable = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.SelectCommand = cmd;
            adapter.Fill(infotable);
            adapter.Dispose();
            cmd.Dispose();
            con.Close();

            DataTable dt = infotable;
            string riverName = "";
            float dangerlevel = 0f;
            float rhwl = 0f;
            MessageBox.Show(dt.Rows.Count.ToString());
            foreach (DataRow dr in dt.Rows)
            {
                riverName = dr[0].ToString();
                dangerlevel = Convert.ToSingle(dr[1]);
                rhwl = Convert.ToSingle(dr[2]);
            }


            con.Open();
            cmd = new SqlCommand("Select Date, WL from FFWCData Where Station = @station AND Date>= @startDate AND Date<=@endDate", con);
            cmd.Parameters.AddWithValue("@startDate", DateTime.Today.AddDays(-7).AddHours(6));
            cmd.Parameters.AddWithValue("@endDate", DateTime.Today.AddHours(6));
            cmd.Parameters.AddWithValue("@station", element);
            DataTable obsTable = new DataTable();
            adapter.SelectCommand = cmd;
            adapter.Fill(obsTable);
            adapter.Dispose();
            cmd.Dispose();
            con.Close();

            dt = obsTable;
            List<DateTime> obsDate = new List<DateTime>();
            List<float> obsWL = new List<float>();
            MessageBox.Show(dt.Rows.Count.ToString());
            foreach (DataRow dr in dt.Rows)
            {
                obsDate.Add(Convert.ToDateTime(dr[0]));
                obsWL.Add(Convert.ToSingle(dr[1]));
            }

            chartStationForecast.Series[0].Points.Clear();
            chartStationForecast.Series[1].Points.Clear();
            chartStationForecast.Series[2].Points.Clear();
            chartStationForecast.Series[3].Points.Clear();

            chartStationForecast.Titles[0].Text = "8 Day Forecast" + "\r\n" + "Station Name: " + element + "  River Name: " + riverName + "\r\n" + "Forecast Date: " + DateTime.Today.AddHours(6);

            List<DateTime> chartDate = new List<DateTime>();
            List<float> chartWL = new List<float>();

            for (int i = 0; i < obsDate.Count; i++)
            {
                chartDate.Add(obsDate[i]);
                chartWL.Add(obsWL[i]);
                chartStationForecast.Series[0].Points.AddXY(obsDate[i], obsWL[i]);
            }

            float correction = forecastWL[0, index] - obsWL[obsWL.Count - 1];
            for (int i = 0; i < 9; i++)
            {
                chartDate.Add(foreDate[i]);
                chartWL.Add(forecastWL[i, index]);
                chartStationForecast.Series[1].Points.AddXY(foreDate[i], (forecastWL[i, index]-correction));
            }

            for (int i = 0; i < chartDate.Count; i++)
            {
                chartStationForecast.Series[2].Points.AddXY(chartDate[i], dangerlevel);
                chartStationForecast.Series[3].Points.AddXY(chartDate[i], rhwl);
            }

            chartWL.Add(dangerlevel);
            chartWL.Add(rhwl);
            chartStationForecast.Series[0].ChartType = SeriesChartType.Line;  // Set chart type like Bar chart, Pie chart
            chartStationForecast.Series[0].IsValueShownAsLabel = false;
            chartStationForecast.Series[0].BorderWidth = 3;

            chartStationForecast.Series[1].ChartType = SeriesChartType.Point;
            chartStationForecast.Series[1].IsValueShownAsLabel = false;
            chartStationForecast.Series[1].MarkerSize = 3;

            chartStationForecast.Series[2].ChartType = SeriesChartType.Line;  // Set chart type like Bar chart, Pie chart
            chartStationForecast.Series[2].IsValueShownAsLabel = false;
            chartStationForecast.Series[2].BorderWidth = 2;

            chartStationForecast.Series[3].ChartType = SeriesChartType.Line;  // Set chart type like Bar chart, Pie chart
            chartStationForecast.Series[3].IsValueShownAsLabel = false;
            chartStationForecast.Series[3].BorderWidth = 2;

            //chartStationForecast.ChartAreas[0].AxisX.Maximum = Convert.ToDouble(chartDate.Max());
            //chartStationForecast.ChartAreas[0].AxisX.Minimum = Convert.ToDouble(chartDate.Min());
            chartStationForecast.ChartAreas[0].AxisY.Maximum = Math.Ceiling(chartWL.Max());
            chartStationForecast.ChartAreas[0].AxisY.Minimum = Math.Floor(chartWL.Min());

            chartStationForecast.ChartAreas[0].AxisY.MajorTickMark.Interval = (Math.Ceiling(chartWL.Max()) - Math.Floor(chartWL.Min())) / 5.0;
            chartStationForecast.ChartAreas[0].AxisY.LabelStyle.Format = "0.0";

            chartDate.Clear();
            chartWL.Clear();
            obsWL.Clear();
            obsDate.Clear();*/

        }
        private void btndevInfo_Click(object sender, EventArgs e)
        {
            MessageBox.Show("The Program is developed in SASWE Research Group.\n\nFor details:\nDr. Faisal Hossain\nDepartment of Civil and Environment Engineering\nUniversity of Washington\nSeattle, Washington\nUSA\n\nE-mail: fhossain@uw.edu\nWebsite: www.saswe.net", "Developer Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void btnImportFFWC_Click(object sender, EventArgs e)
        {
            listForecastStationBox.Visible = false;
            chartStationForecast.Visible = false;
            try
            {
                con.Open();
                OpenFileDialog csvOpener = new OpenFileDialog();
                csvOpener.Title = "Select CSV File to import in database";
                csvOpener.Filter = "ff-data-IWM CSV| *.csv";
                DialogResult result = csvOpener.ShowDialog();
                if (result == DialogResult.OK)
                {

                    string[] ffDataFile = File.ReadAllLines(csvOpener.FileName);
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
                                    try
                                    {
                                        //MessageBox.Show(jaggedString[0][j] + "  -  " + jaggedString[i][1].Substring(0, jaggedString[i][1].Length - 6) + "  -  " + jaggedString[i][j]);
                                        cmd = new SqlCommand("INSERT INTO FFWCData VALUES(@dataDate, @individual, @individual2)", con);
                                        cmd.Parameters.AddWithValue("@dataDate", jaggedString[0][j]);
                                        cmd.Parameters.AddWithValue("@individual", station);
                                        cmd.Parameters.AddWithValue("@individual2", jaggedString[i][j]);
                                        cmd.ExecuteNonQuery();
                                    }
                                    catch (SqlException)
                                    {
                                        cmd = new SqlCommand("Update FFWCData SET WL = @rfValue Where Date = @dataDate AND Station= @station", con);
                                        cmd.Parameters.AddWithValue("@dataDate", jaggedString[0][j]);
                                        cmd.Parameters.AddWithValue("@station", station);
                                        cmd.Parameters.AddWithValue("@rfValue", jaggedString[i][j]);
                                        cmd.ExecuteNonQuery();
                                    }
                                    //sb.AppendLine(jaggedString[0][j] + "," + jaggedString[i][1].Substring(0, jaggedString[i][1].Length - 6) + "," + jaggedString[i][j]);
                                }
                                else { continue; }
                            }
                        }
                        else { continue; }
                    }
                    MessageBox.Show("All WL Data have been imported Successfully.", "Data Import", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception error)
            {
                con.Close();
                MessageBox.Show("Data can not be inserted due to an error. Error: " + error.Message);
            }
        }

        private void btnForecastHeight_Click(object sender, EventArgs e)
        {
            listForecastStationBox.Visible = false;
            chartStationForecast.Visible = false;
            StringBuilder reportContent = new StringBuilder();
            bool frcFile = File.Exists(directory + @"Programs\NecessaryFiles\FRCInfo.txt");

            if (frcFile == false)
            {
                reportBox.Text = "Forecast Rating Curve file does not exists! Keep the file in right path and try again.";
            }
            else
            {
                string[] frcFileInfo = File.ReadAllLines(directory + @"Programs\NecessaryFiles\FRCInfo.txt");
                string[,] frcInfo = new string[32, 17];
                for (int i = 0; i < frcFileInfo.Length; i++)
                {
                    var dispersedText = frcFileInfo[i].Split(',');
                    for (int j = 0; j < dispersedText.Length; j++)
                    {
                        frcInfo[i, j] = dispersedText[j];
                    }
                }

                float[] BndwlPankha = new float[8];
                float[] pankhaDIff = new float[8];
                try
                {
                    con.Open();
                    cmd = new SqlCommand("Select Date, WL FROM FFWCData Where Station = @Title AND Date >= @startDate AND Date <= @endDate ORDER By Date ASC", con);
                    cmd.Parameters.AddWithValue("@Title", "Hardinge-RB");
                    cmd.Parameters.AddWithValue("@startDate", DateTime.Today.AddDays(-7).AddHours(6));
                    cmd.Parameters.AddWithValue("@endDate", DateTime.Today.AddHours(6));
                    ad.SelectCommand = cmd;
                    DataTable table = new DataTable();
                    ad.Fill(ds, "table");
                    ad.Dispose();
                    cmd.Dispose();
                    con.Close();
                    List<DateTime> dataDate = new List<DateTime>();
                    List<float> bndData = new List<float>();
                    foreach (DataRow dr in ds.Tables["table"].Rows)
                    {
                        dataDate.Add(Convert.ToDateTime(dr[0]));
                        bndData.Add(Convert.ToSingle(dr[1]));
                    }

                    float aveDiff = ((bndData[bndData.Count - 1] - bndData[bndData.Count - 2]) + (bndData[bndData.Count - 2] - bndData[bndData.Count - 3]) + (bndData[bndData.Count - 3] - bndData[bndData.Count - 4])) / 3.0f;
                    float obsrvdValue = bndData[bndData.Count - 1];
                    ds.Tables.Clear();

                    DirectoryInfo di = new DirectoryInfo(directory + @"SatelliteData\VSHeight\Ganges");
                    string selectedFilePath = "";
                    string selectedFileName = "";
                    FileInfo[] files = di.GetFiles("*.txt");
                    if (files.Length == 1)
                    {
                        selectedFilePath = files[0].FullName;
                        selectedFileName = files[0].Name;
                    }
                    if (files.Length > 1)
                    {
                        selectedFilePath = files[0].FullName;
                        selectedFileName = files[0].Name;
                        DateTime minDate = DateTime.ParseExact(files[0].Name.Substring(12, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        foreach (FileInfo fi in files)
                        {
                            DateTime fileDate = DateTime.ParseExact(fi.Name.Substring(12, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            if ((fileDate - minDate).TotalDays > 0)
                            {
                                selectedFilePath = fi.FullName;
                                selectedFileName = fi.Name;
                                minDate = fileDate;
                            }
                        }
                    }
                    double DateInterval = (DateTime.Today - DateTime.ParseExact(selectedFileName.Substring(12, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture)).TotalDays;
                    string[] satFileInfo = File.ReadAllLines(selectedFilePath);
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
                        MessageBox.Show("No Heights found in Ganges with Backscatter greater than 30 dB, taken height corrsponding to Maximum BS.", "Ganges Backscatter Value warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                    for (int i = 0; i < 32; i++)
                    {
                        if (selectedFileName.Substring(0, 11) == frcInfo[i, 0])
                        {
                            for (int j = 0; j < 8; j++)
                            {
                                BndwlPankha[j] = height * float.Parse(frcInfo[i, j * 2 + 1]) - float.Parse(frcInfo[i, j * 2 + 2]);
                            }
                            break;
                        }
                    }


                    if (DateInterval > 3.0)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            float chkWL = BndwlPankha[0] - (obsrvdValue + aveDiff);
                            dataDate.Add(DateTime.Today.AddHours(6).AddDays(j + 1));
                            bndData.Add(BndwlPankha[j] - chkWL);
                        }
                    }
                    if (DateInterval == 3.0)
                    {
                        float chkWL = BndwlPankha[0] - (obsrvdValue + 2.0f * aveDiff);
                        dataDate.Add(DateTime.Today.AddHours(6).AddDays(1));
                        bndData.Add(obsrvdValue + aveDiff);
                        //bndData.Add((obsrvdValue + BndwlPankha[0]) / 2.0f);
                        for (int j = 1; j < 8; j++)
                        {
                            dataDate.Add(DateTime.Today.AddHours(6).AddDays(j + 1));
                            bndData.Add(BndwlPankha[j - 1] - chkWL);
                        }
                    }
                    else if (DateInterval == 2.0)
                    {
                        float chkWL = BndwlPankha[0] - (obsrvdValue + 3.0f * aveDiff);
                        dataDate.Add(DateTime.Today.AddHours(6).AddDays(1));
                        bndData.Add(obsrvdValue + aveDiff);
                        //bndData.Add(obsrvdValue + (BndwlPankha[0] - obsrvdValue) * 0.333f);
                        dataDate.Add(DateTime.Today.AddHours(6).AddDays(2));
                        bndData.Add(obsrvdValue + 2.0f * aveDiff);
                        //bndData.Add(obsrvdValue + (BndwlPankha[0] - obsrvdValue) * 0.67f);
                        for (int j = 2; j < 8; j++)
                        {
                            dataDate.Add(DateTime.Today.AddHours(6).AddDays(j + 1));
                            bndData.Add(BndwlPankha[j - 2] - chkWL);
                        }
                    }
                    else if (DateInterval == 1.0)
                    {
                        float chkWL = BndwlPankha[0] - (obsrvdValue + 4.0f * aveDiff);
                        dataDate.Add(DateTime.Today.AddHours(6).AddDays(1));
                        bndData.Add(obsrvdValue + aveDiff);
                        //bndData.Add(obsrvdValue + (BndwlPankha[0] - obsrvdValue) * 0.25f);
                        dataDate.Add(DateTime.Today.AddHours(6).AddDays(2));
                        bndData.Add(obsrvdValue + 2.0f * aveDiff);
                        //bndData.Add(obsrvdValue + (BndwlPankha[0] - obsrvdValue) * 0.50f);
                        dataDate.Add(DateTime.Today.AddHours(6).AddDays(3));
                        bndData.Add(obsrvdValue + 3.0f * aveDiff);
                        //bndData.Add(obsrvdValue + (BndwlPankha[0] - obsrvdValue) * 0.75f);
                        for (int j = 3; j < 8; j++)
                        {
                            dataDate.Add(DateTime.Today.AddHours(6).AddDays(j + 1));
                            bndData.Add(BndwlPankha[j - 3] - chkWL);
                        }
                    }
                    else if (DateInterval == 0.0)
                    {
                        float chkWL = BndwlPankha[0] - (obsrvdValue + 5.0f * aveDiff);
                        dataDate.Add(DateTime.Today.AddHours(6).AddDays(1));
                        bndData.Add(obsrvdValue + aveDiff);
                        //bndData.Add(obsrvdValue + (BndwlPankha[0] - obsrvdValue) * 0.2f);
                        dataDate.Add(DateTime.Today.AddHours(6).AddDays(2));
                        bndData.Add(obsrvdValue + 2.0f * aveDiff);
                        //bndData.Add(obsrvdValue + (BndwlPankha[0] - obsrvdValue) * 0.4f);
                        dataDate.Add(DateTime.Today.AddHours(6).AddDays(3));
                        bndData.Add(obsrvdValue + 3.0f * aveDiff);
                        //bndData.Add(obsrvdValue + (BndwlPankha[0] - obsrvdValue) * 0.6f);
                        dataDate.Add(DateTime.Today.AddHours(6).AddDays(4));
                        bndData.Add(obsrvdValue + 4.0f * aveDiff);
                        //bndData.Add(obsrvdValue + (BndwlPankha[0] - obsrvdValue) * 0.8f);
                        for (int j = 4; j < 8; j++)
                        {
                            dataDate.Add(DateTime.Today.AddHours(6).AddDays(j + 1));
                            bndData.Add(BndwlPankha[j - 4] - chkWL);
                        }
                    }


                    int x = 7;
                    for (int i = bndData.Count - 1; i >= bndData.Count - 8; i--)
                    {
                        pankhaDIff[x] = bndData[i] - obsrvdValue;
                        x = x - 1;
                    }

                    StringBuilder bndHBuilder = new StringBuilder();
                    bndHBuilder.AppendLine("Date and Time \t\tHeight(mPWD)");

                    for (int i = 0; i < bndData.Count; i++)
                    {
                        bndHBuilder.AppendLine(dataDate[i].ToString("yyyy-MM-dd HH:mm:ss") + "\t" + bndData[i].ToString("0.000"));
                    }
                    File.WriteAllText(directory + @"SatelliteData\ForecastHeights\Pankha.txt", bndHBuilder.ToString());
                    bndHBuilder.Clear();
                    dataDate.Clear();
                    bndData.Clear();
                    reportContent.AppendLine("Forecasted Heights of Pankha boundary station is calculated.");
                }
                catch (Exception error)
                {
                    MessageBox.Show("Error in Pankha Station height calculation. Error: " + error.Message);
                    reportContent.AppendLine("Error in forecasted Heights calculation at Pankha boundary station. Error: " + error.ToString());
                }

                float[] BndwlNoon = new float[8];
                float[] noonDiff = new float[8];
                try
                {
                    con.Open();
                    cmd = new SqlCommand("Select Date, WL FROM FFWCData Where Station = @Title AND Date >= @startDate AND Date <= @endDate ORDER By Date ASC", con);
                    cmd.Parameters.AddWithValue("@Title", "Bahadurabad");
                    cmd.Parameters.AddWithValue("@startDate", DateTime.Today.AddDays(-7).AddHours(6));
                    cmd.Parameters.AddWithValue("@endDate", DateTime.Today.AddHours(6));
                    ad.SelectCommand = cmd;
                    DataTable table = new DataTable();
                    ad.Fill(ds, "table");
                    ad.Dispose();
                    cmd.Dispose();
                    con.Close();
                    List<DateTime> dataDate = new List<DateTime>();
                    List<float> bndData = new List<float>();
                    foreach (DataRow dr in ds.Tables["table"].Rows)
                    {
                        dataDate.Add(Convert.ToDateTime(dr[0]));
                        bndData.Add(Convert.ToSingle(dr[1]));
                    }
                    float aveDiff = ((bndData[bndData.Count - 1] - bndData[bndData.Count - 2]) + (bndData[bndData.Count - 2] - bndData[bndData.Count - 3]) + (bndData[bndData.Count - 3] - bndData[bndData.Count - 4])) / 3.0f;
                    float obsrvdValue = bndData[bndData.Count - 1];

                    ds.Tables.Clear();

                    DirectoryInfo di = new DirectoryInfo(directory + @"SatelliteData\VSHeight\Brahmaputra");
                    string selectedFilePath = "";
                    string selectedFileName = "";
                    FileInfo[] files = di.GetFiles("*.txt");
                    if (files.Length == 1)
                    {
                        selectedFilePath = files[0].FullName;
                        selectedFileName = files[0].Name;
                    }
                    if (files.Length > 1)
                    {
                        selectedFilePath = files[0].FullName;
                        selectedFileName = files[0].Name;
                        DateTime minDate = DateTime.ParseExact(files[0].Name.Substring(12, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        foreach (FileInfo fi in files)
                        {
                            DateTime fileDate = DateTime.ParseExact(fi.Name.Substring(12, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture);
                            if ((fileDate - minDate).TotalDays > 0)
                            {
                                selectedFilePath = fi.FullName;
                                selectedFileName = fi.Name;
                                minDate = fileDate;
                            }
                        }
                    }
                    double DateInterval = (DateTime.Today - DateTime.ParseExact(selectedFileName.Substring(12, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture)).TotalDays;
                    string[] satFileInfo = File.ReadAllLines(selectedFilePath);
                    List<float> heights = new List<float>();

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
                        MessageBox.Show("No Heights found with Backscatter greater than 30 dB, taking all heights.", "Brahmaputra Backscatter Value warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        for (int i = 1; i < satFileInfo.Length; i++)
                        {
                            var values = satFileInfo[i].Split('\t');
                            heights.Add(float.Parse(values[2].Trim()));
                        }
                    }
                    float height = heights.Average();
                    for (int i = 0; i < 32; i++)
                    {
                        if (selectedFileName.Substring(0, 11) == frcInfo[i, 0])
                        {
                            for (int j = 0; j < 8; j++)
                            {
                                BndwlNoon[j] = height * float.Parse(frcInfo[i, j * 2 + 1]) - float.Parse(frcInfo[i, j * 2 + 2]);
                            }
                            break;
                        }
                    }

                    if (DateInterval > 3.0)
                    {
                        float chkWL = BndwlNoon[0] - (obsrvdValue + aveDiff);
                        for (int j = 0; j < 8; j++)
                        {
                            dataDate.Add(DateTime.Today.AddHours(6).AddDays(j + 1));
                            bndData.Add(BndwlNoon[j] - chkWL);
                        }
                    }
                    if (DateInterval == 3.0)
                    {
                        float chkWL = BndwlNoon[0] - (obsrvdValue + 2.0f * aveDiff);
                        dataDate.Add(DateTime.Today.AddHours(6).AddDays(1));
                        bndData.Add(obsrvdValue + aveDiff);
                        //bndData.Add((obsrvdValue + BndwlNoon[0]) / 2.0f);
                        for (int j = 1; j < 8; j++)
                        {
                            dataDate.Add(DateTime.Today.AddHours(6).AddDays(j + 1));
                            bndData.Add(BndwlNoon[j - 1] - chkWL);
                        }
                    }
                    else if (DateInterval == 2.0)
                    {
                        float chkWL = BndwlNoon[0] - (obsrvdValue + 3.0f * aveDiff);
                        dataDate.Add(DateTime.Today.AddHours(6).AddDays(1));
                        bndData.Add(obsrvdValue + aveDiff);
                        //bndData.Add(obsrvdValue + (BndwlNoon[0] - obsrvdValue) * 0.333f);
                        dataDate.Add(DateTime.Today.AddHours(6).AddDays(2));
                        bndData.Add(obsrvdValue + 2.0f * aveDiff);
                        //bndData.Add(obsrvdValue + (BndwlNoon[0] - obsrvdValue) * 0.67f);
                        for (int j = 2; j < 8; j++)
                        {
                            dataDate.Add(DateTime.Today.AddHours(6).AddDays(j + 1));
                            bndData.Add(BndwlNoon[j - 2] - chkWL);
                        }
                    }
                    else if (DateInterval == 1.0)
                    {
                        float chkWL = BndwlNoon[0] - (obsrvdValue + 4.0f * aveDiff);
                        dataDate.Add(DateTime.Today.AddHours(6).AddDays(1));
                        bndData.Add(obsrvdValue + aveDiff);
                        //bndData.Add(obsrvdValue + (BndwlNoon[0] - obsrvdValue) * 0.25f);
                        dataDate.Add(DateTime.Today.AddHours(6).AddDays(2));
                        bndData.Add(obsrvdValue + 2.0f * aveDiff);
                        //bndData.Add(obsrvdValue + (BndwlNoon[0] - obsrvdValue) * 0.50f);
                        dataDate.Add(DateTime.Today.AddHours(6).AddDays(3));
                        bndData.Add(obsrvdValue + 3.0f * aveDiff);
                        //bndData.Add(obsrvdValue + (BndwlNoon[0] - obsrvdValue) * 0.75f);
                        for (int j = 3; j < 8; j++)
                        {
                            dataDate.Add(DateTime.Today.AddHours(6).AddDays(j + 1));
                            bndData.Add(BndwlNoon[j - 3] - chkWL);
                        }
                    }
                    else if (DateInterval == 0.0)
                    {
                        float chkWL = BndwlNoon[0] - (obsrvdValue + 5.0f * aveDiff);
                        dataDate.Add(DateTime.Today.AddHours(6).AddDays(1));
                        bndData.Add(obsrvdValue + aveDiff);
                        //bndData.Add(obsrvdValue + (BndwlNoon[0] - obsrvdValue) * 0.2f);
                        dataDate.Add(DateTime.Today.AddHours(6).AddDays(2));
                        bndData.Add(obsrvdValue + 2.0f * aveDiff);
                        //bndData.Add(obsrvdValue + (BndwlNoon[0] - obsrvdValue) * 0.4f);
                        dataDate.Add(DateTime.Today.AddHours(6).AddDays(3));
                        bndData.Add(obsrvdValue + 3.0f * aveDiff);
                        //bndData.Add(obsrvdValue + (BndwlNoon[0] - obsrvdValue) * 0.6f);
                        dataDate.Add(DateTime.Today.AddHours(6).AddDays(4));
                        bndData.Add(obsrvdValue + 4.0f * aveDiff);
                        //bndData.Add(obsrvdValue + (BndwlNoon[0] - obsrvdValue) * 0.8f);
                        for (int j = 4; j < 8; j++)
                        {
                            dataDate.Add(DateTime.Today.AddHours(6).AddDays(j + 1));
                            bndData.Add(BndwlNoon[j - 4] - chkWL);
                        }
                    }

                    int x = 7;
                    for (int i = bndData.Count - 1; i >= bndData.Count - 8; i--)
                    {
                        noonDiff[x] = bndData[i] - obsrvdValue;
                        x = x - 1;
                    }

                    StringBuilder bndHBuilder = new StringBuilder();
                    bndHBuilder.AppendLine("Date and Time \t\tHeight(mPWD)");

                    for (int i = 0; i < bndData.Count; i++)
                    {
                        bndHBuilder.AppendLine(dataDate[i].ToString("yyyy-MM-dd HH:mm:ss") + "\t" + bndData[i].ToString("0.000"));
                    }
                    File.WriteAllText(directory + @"SatelliteData\ForecastHeights\Noonkhawa.txt", bndHBuilder.ToString());
                    bndHBuilder.Clear();
                    dataDate.Clear();
                    bndData.Clear();
                    reportContent.AppendLine("");
                    reportContent.AppendLine("Forecasted Heights of Noonkhawa boundary station is calculated.");
                }
                catch (Exception error)
                {
                    MessageBox.Show("Error in Noonkhawa Station height calculation. Error: " + error.Message);
                    reportContent.AppendLine("Error in forecasted Heights calculation at Noonkhawa boundary station. Error: " + error.ToString());
                }

                try
                {
                    con.Open();
                    cmd = new SqlCommand("Select Date, WL FROM FFWCData Where Station = @Title AND Date >= @startDate AND Date <= @endDate", con);
                    cmd.Parameters.AddWithValue("@Title", "Faridpur");
                    cmd.Parameters.AddWithValue("@startDate", DateTime.Today.AddDays(-7).AddHours(6));
                    cmd.Parameters.AddWithValue("@endDate", DateTime.Today.AddHours(6));
                    ad.SelectCommand = cmd;
                    DataTable table = new DataTable();
                    ad.Fill(ds, "table");
                    ad.Dispose();
                    cmd.Dispose();
                    con.Close();
                    List<DateTime> dataDate = new List<DateTime>();
                    List<float> bndData = new List<float>();
                    foreach (DataRow dr in ds.Tables["table"].Rows)
                    {
                        dataDate.Add(Convert.ToDateTime(dr[0]));
                        bndData.Add(Convert.ToSingle(dr[1]));
                    }
                    float obsrvdValue = Convert.ToSingle(ds.Tables["table"].Rows[ds.Tables["table"].Rows.Count - 1].ItemArray[1]);
                    ds.Tables.Clear();

                    for (int i = 0; i < 8; i++)
                    {
                        dataDate.Add(DateTime.Today.AddHours(6).AddDays(i + 1));
                        bndData.Add(obsrvdValue + pankhaDIff[i]);
                    }

                    StringBuilder bndHBuilder = new StringBuilder();
                    bndHBuilder.AppendLine("Date and Time \t\tHeight(mPWD)");

                    for (int i = 0; i < bndData.Count; i++)
                    {
                        bndHBuilder.AppendLine(dataDate[i].ToString("yyyy-MM-dd HH:mm:ss") + "\t" + bndData[i].ToString("0.000"));
                    }
                    File.WriteAllText(directory + @"SatelliteData\ForecastHeights\Faridpur.txt", bndHBuilder.ToString());
                    bndHBuilder.Clear();
                    dataDate.Clear();
                    bndData.Clear();
                    reportContent.AppendLine("Forecasted Heights of Faridpur boundary station is calculated.");
                }
                catch (Exception error)
                {
                    MessageBox.Show("Error in Faridpur Station height calculation. Error: " + error.Message);
                    reportContent.AppendLine("Error in forecasted Heights calculation at Faridpur boundary station. Error: " + error.ToString());
                }

                try
                {
                    con.Open();
                    cmd = new SqlCommand("Select Date, WL FROM FFWCData Where Station = @Title AND Date >= @startDate AND Date <= @endDate", con);
                    cmd.Parameters.AddWithValue("@Title", "Rohanpur");
                    cmd.Parameters.AddWithValue("@startDate", DateTime.Today.AddDays(-7).AddHours(6));
                    cmd.Parameters.AddWithValue("@endDate", DateTime.Today.AddHours(6));
                    ad.SelectCommand = cmd;
                    DataTable table = new DataTable();
                    ad.Fill(ds, "table");
                    ad.Dispose();
                    cmd.Dispose();
                    con.Close();
                    List<DateTime> dataDate = new List<DateTime>();
                    List<float> bndData = new List<float>();
                    foreach (DataRow dr in ds.Tables["table"].Rows)
                    {
                        dataDate.Add(Convert.ToDateTime(dr[0]));
                        bndData.Add(Convert.ToSingle(dr[1]));
                    }
                    float obsrvdValue = Convert.ToSingle(ds.Tables["table"].Rows[ds.Tables["table"].Rows.Count - 1].ItemArray[1]);
                    ds.Tables.Clear();

                    for (int i = 0; i < 8; i++)
                    {
                        dataDate.Add(DateTime.Today.AddHours(6).AddDays(i + 1));
                        bndData.Add(obsrvdValue + pankhaDIff[i]);
                    }

                    StringBuilder bndHBuilder = new StringBuilder();
                    bndHBuilder.AppendLine("Date and Time \t\tHeight(mPWD)");

                    for (int i = 0; i < bndData.Count; i++)
                    {
                        bndHBuilder.AppendLine(dataDate[i].ToString("yyyy-MM-dd HH:mm:ss") + "\t" + bndData[i].ToString("0.000"));
                    }
                    File.WriteAllText(directory + @"SatelliteData\ForecastHeights\Rohanpur.txt", bndHBuilder.ToString());
                    bndHBuilder.Clear();
                    dataDate.Clear();
                    bndData.Clear();
                    reportContent.AppendLine("Forecasted Heights of Rohanpur boundary station is calculated.");
                }
                catch (Exception error)
                {
                    MessageBox.Show("Error in Rohanpur Station height calculation. Error: " + error.Message);
                    reportContent.AppendLine("Error in forecasted Heights calculation at Rohanpur boundary station. Error: " + error.ToString());
                }
                try
                {
                    string[] hdBnd = new string[] { "Gaibandha", "Kurigram", "Badarganj", "Panchagarh", "Dalia", "Comilla", "Dinajpur" };
                    foreach (string element in hdBnd)
                    {

                        con.Open();
                        cmd = new SqlCommand("Select Date, WL FROM FFWCData Where Station = @Title AND Date >= @startDate AND Date <= @endDate", con);
                        cmd.Parameters.AddWithValue("@Title", element);
                        cmd.Parameters.AddWithValue("@startDate", DateTime.Today.AddDays(-7).AddHours(6));
                        cmd.Parameters.AddWithValue("@endDate", DateTime.Today.AddHours(6));
                        ad.SelectCommand = cmd;
                        DataTable table = new DataTable();
                        ad.Fill(ds, "table");
                        ad.Dispose();
                        cmd.Dispose();
                        con.Close();
                        List<DateTime> dataDate = new List<DateTime>();
                        List<float> bndData = new List<float>();
                        foreach (DataRow dr in ds.Tables["table"].Rows)
                        {
                            dataDate.Add(Convert.ToDateTime(dr[0]));
                            bndData.Add(Convert.ToSingle(dr[1]));
                        }
                        float obsrvdValue = Convert.ToSingle(ds.Tables["table"].Rows[ds.Tables["table"].Rows.Count - 1].ItemArray[1]);
                        ds.Tables.Clear();

                        for (int i = 0; i < 8; i++)
                        {
                            dataDate.Add(DateTime.Today.AddHours(6).AddDays(i + 1));
                            bndData.Add(obsrvdValue + noonDiff[i]);
                        }

                        StringBuilder bndHBuilder = new StringBuilder();
                        bndHBuilder.AppendLine("Date and Time \t\tHeight(mPWD)");

                        for (int i = 0; i < bndData.Count; i++)
                        {
                            bndHBuilder.AppendLine(dataDate[i].ToString("yyyy-MM-dd HH:mm:ss") + "\t" + bndData[i].ToString("0.000"));
                        }
                        File.WriteAllText(directory + @"SatelliteData\ForecastHeights\" + element + ".txt", bndHBuilder.ToString());
                        bndHBuilder.Clear();
                        dataDate.Clear();
                        bndData.Clear();
                        reportContent.AppendLine("Forecasted Heights of " + element + " boundary station is calculated.");
                    }
                    MessageBox.Show("Forecasted heights in each boundary successfully calculated.", "Forecast Height Calculation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    reportBox.Text = reportContent.ToString();
                    reportContent.Clear();
                }
                catch (Exception error)
                {
                    MessageBox.Show("Error in Station height calculation. Error: " + error.Message, "Forecast Height Extraction Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    reportContent.AppendLine("Error in one of the stations (Gaibandha, Kurigram, Badarganj, Panchagarh, Dalia, Comilla, Dinajpur) during forecasted Heights calculation. Error: " + error.ToString());
                    reportBox.Text = reportContent.ToString();
                    reportContent.Clear();
                }
                
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listForecastStationBox.Visible == false || chartStationForecast.Visible == false)
            {
                listForecastStationBox.Visible = true;
                chartStationForecast.Visible = true;
            }
            else
            {
                listForecastStationBox.Visible = false;
                chartStationForecast.Visible = false;
            }

        }


    }
}

