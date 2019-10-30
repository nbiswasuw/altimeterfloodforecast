namespace Aviso_J2
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series4 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title1 = new System.Windows.Forms.DataVisualization.Charting.Title();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btnDownloadFFWC = new System.Windows.Forms.Button();
            this.btnHDSimulate = new System.Windows.Forms.Button();
            this.btnGenerateForecast = new System.Windows.Forms.Button();
            this.chartStationForecast = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.btnUpdatehdMnd = new System.Windows.Forms.Button();
            this.btnDownloadSat = new System.Windows.Forms.Button();
            this.btnProcessSatellite = new System.Windows.Forms.Button();
            this.listForecastStationBox = new System.Windows.Forms.ListBox();
            this.btnImportFFWC = new System.Windows.Forms.Button();
            this.btndevInfo = new System.Windows.Forms.Button();
            this.btnForecastHeight = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.reportBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.chartStationForecast)).BeginInit();
            this.SuspendLayout();
            // 
            // btnDownloadFFWC
            // 
            this.btnDownloadFFWC.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDownloadFFWC.Location = new System.Drawing.Point(10, 55);
            this.btnDownloadFFWC.Name = "btnDownloadFFWC";
            this.btnDownloadFFWC.Size = new System.Drawing.Size(200, 40);
            this.btnDownloadFFWC.TabIndex = 1;
            this.btnDownloadFFWC.Text = "Download FFWC WL";
            this.btnDownloadFFWC.UseVisualStyleBackColor = true;
            this.btnDownloadFFWC.Click += new System.EventHandler(this.btnDownloadFFWC_Click);
            // 
            // btnHDSimulate
            // 
            this.btnHDSimulate.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnHDSimulate.Location = new System.Drawing.Point(10, 235);
            this.btnHDSimulate.Name = "btnHDSimulate";
            this.btnHDSimulate.Size = new System.Drawing.Size(200, 40);
            this.btnHDSimulate.TabIndex = 2;
            this.btnHDSimulate.Text = "Simulate Model";
            this.btnHDSimulate.UseVisualStyleBackColor = true;
            this.btnHDSimulate.Click += new System.EventHandler(this.btnHDSimulate_Click);
            // 
            // btnGenerateForecast
            // 
            this.btnGenerateForecast.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGenerateForecast.Location = new System.Drawing.Point(10, 280);
            this.btnGenerateForecast.Name = "btnGenerateForecast";
            this.btnGenerateForecast.Size = new System.Drawing.Size(200, 40);
            this.btnGenerateForecast.TabIndex = 3;
            this.btnGenerateForecast.Text = "Generate Forecast";
            this.btnGenerateForecast.UseVisualStyleBackColor = true;
            this.btnGenerateForecast.Click += new System.EventHandler(this.btnGenerateForecast_Click);
            // 
            // chartStationForecast
            // 
            this.chartStationForecast.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            chartArea1.AxisX.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisX.Title = "Date";
            chartArea1.AxisY.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisY.Title = "Water Level (mPWD)";
            chartArea1.Name = "ChartArea1";
            this.chartStationForecast.ChartAreas.Add(chartArea1);
            this.chartStationForecast.Location = new System.Drawing.Point(344, 12);
            this.chartStationForecast.Name = "chartStationForecast";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.Name = "Series1";
            series1.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.DateTime;
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series2.Name = "Series2";
            series2.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.DateTime;
            series3.ChartArea = "ChartArea1";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series3.Name = "Series3";
            series3.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.DateTime;
            series4.ChartArea = "ChartArea1";
            series4.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series4.Name = "Series4";
            series4.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.DateTime;
            this.chartStationForecast.Series.Add(series1);
            this.chartStationForecast.Series.Add(series2);
            this.chartStationForecast.Series.Add(series3);
            this.chartStationForecast.Series.Add(series4);
            this.chartStationForecast.Size = new System.Drawing.Size(500, 360);
            this.chartStationForecast.TabIndex = 4;
            this.chartStationForecast.Text = "chartStationForecast";
            title1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            title1.Name = "Title1";
            this.chartStationForecast.Titles.Add(title1);
            // 
            // btnUpdatehdMnd
            // 
            this.btnUpdatehdMnd.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnUpdatehdMnd.Location = new System.Drawing.Point(10, 190);
            this.btnUpdatehdMnd.Name = "btnUpdatehdMnd";
            this.btnUpdatehdMnd.Size = new System.Drawing.Size(200, 40);
            this.btnUpdatehdMnd.TabIndex = 5;
            this.btnUpdatehdMnd.Text = "Generate Boundary";
            this.btnUpdatehdMnd.UseVisualStyleBackColor = true;
            this.btnUpdatehdMnd.Click += new System.EventHandler(this.btnUpdatehdMnd_Click);
            // 
            // btnDownloadSat
            // 
            this.btnDownloadSat.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDownloadSat.Location = new System.Drawing.Point(10, 10);
            this.btnDownloadSat.Name = "btnDownloadSat";
            this.btnDownloadSat.Size = new System.Drawing.Size(200, 40);
            this.btnDownloadSat.TabIndex = 7;
            this.btnDownloadSat.Text = "Download Virtual Station File";
            this.btnDownloadSat.UseVisualStyleBackColor = true;
            this.btnDownloadSat.Click += new System.EventHandler(this.btnDownloadSat_Click);
            // 
            // btnProcessSatellite
            // 
            this.btnProcessSatellite.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnProcessSatellite.Location = new System.Drawing.Point(10, 100);
            this.btnProcessSatellite.Name = "btnProcessSatellite";
            this.btnProcessSatellite.Size = new System.Drawing.Size(200, 40);
            this.btnProcessSatellite.TabIndex = 8;
            this.btnProcessSatellite.Text = "Extract VS Height";
            this.btnProcessSatellite.UseVisualStyleBackColor = true;
            this.btnProcessSatellite.Click += new System.EventHandler(this.btnProcessSatellite_Click);
            // 
            // listForecastStationBox
            // 
            this.listForecastStationBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listForecastStationBox.FormattingEnabled = true;
            this.listForecastStationBox.ItemHeight = 20;
            this.listForecastStationBox.Items.AddRange(new object[] {
            "Pankha",
            "Noonkhawa",
            "Faridpur",
            "Rohanpur",
            "Badarganj",
            "Panchagarh",
            "Dalia",
            "Gaibandha",
            "Comilla",
            "Dinajpur",
            "Kurigram"});
            this.listForecastStationBox.Location = new System.Drawing.Point(219, 47);
            this.listForecastStationBox.Name = "listForecastStationBox";
            this.listForecastStationBox.Size = new System.Drawing.Size(120, 284);
            this.listForecastStationBox.TabIndex = 11;
            this.listForecastStationBox.TabStop = false;
            this.listForecastStationBox.Visible = false;
            this.listForecastStationBox.SelectedIndexChanged += new System.EventHandler(this.listForecastStationBox_SelectedIndexChanged);
            // 
            // btnImportFFWC
            // 
            this.btnImportFFWC.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnImportFFWC.Location = new System.Drawing.Point(10, 326);
            this.btnImportFFWC.Name = "btnImportFFWC";
            this.btnImportFFWC.Size = new System.Drawing.Size(200, 39);
            this.btnImportFFWC.TabIndex = 12;
            this.btnImportFFWC.Text = "Import FFWCData";
            this.btnImportFFWC.UseVisualStyleBackColor = true;
            this.btnImportFFWC.Click += new System.EventHandler(this.btnImportFFWC_Click);
            // 
            // btndevInfo
            // 
            this.btndevInfo.Location = new System.Drawing.Point(219, 336);
            this.btndevInfo.Name = "btndevInfo";
            this.btndevInfo.Size = new System.Drawing.Size(120, 29);
            this.btndevInfo.TabIndex = 13;
            this.btndevInfo.Text = "About the Tool";
            this.btndevInfo.UseVisualStyleBackColor = true;
            this.btndevInfo.Click += new System.EventHandler(this.btndevInfo_Click);
            // 
            // btnForecastHeight
            // 
            this.btnForecastHeight.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnForecastHeight.Location = new System.Drawing.Point(10, 145);
            this.btnForecastHeight.Name = "btnForecastHeight";
            this.btnForecastHeight.Size = new System.Drawing.Size(200, 40);
            this.btnForecastHeight.TabIndex = 14;
            this.btnForecastHeight.Text = "Forecast Heights";
            this.btnForecastHeight.UseVisualStyleBackColor = true;
            this.btnForecastHeight.Click += new System.EventHandler(this.btnForecastHeight_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(219, 12);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(120, 30);
            this.button2.TabIndex = 15;
            this.button2.Text = "Visualize Heights";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // reportBox
            // 
            this.reportBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.reportBox.Location = new System.Drawing.Point(13, 375);
            this.reportBox.Multiline = true;
            this.reportBox.Name = "reportBox";
            this.reportBox.Size = new System.Drawing.Size(831, 58);
            this.reportBox.TabIndex = 16;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(861, 438);
            this.Controls.Add(this.reportBox);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.btnForecastHeight);
            this.Controls.Add(this.btndevInfo);
            this.Controls.Add(this.btnImportFFWC);
            this.Controls.Add(this.listForecastStationBox);
            this.Controls.Add(this.btnProcessSatellite);
            this.Controls.Add(this.btnDownloadSat);
            this.Controls.Add(this.btnUpdatehdMnd);
            this.Controls.Add(this.btnGenerateForecast);
            this.Controls.Add(this.btnHDSimulate);
            this.Controls.Add(this.btnDownloadFFWC);
            this.Controls.Add(this.chartStationForecast);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "SASWE-FFWC Jason-2 Flood Forecasting System";
            ((System.ComponentModel.ISupportInitialize)(this.chartStationForecast)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnDownloadFFWC;
        private System.Windows.Forms.Button btnHDSimulate;
        private System.Windows.Forms.Button btnGenerateForecast;
        private System.Windows.Forms.DataVisualization.Charting.Chart chartStationForecast;
        private System.Windows.Forms.Button btnUpdatehdMnd;
        private System.Windows.Forms.Button btnDownloadSat;
        private System.Windows.Forms.Button btnProcessSatellite;
        private System.Windows.Forms.ListBox listForecastStationBox;
        private System.Windows.Forms.Button btnImportFFWC;
        private System.Windows.Forms.Button btndevInfo;
        private System.Windows.Forms.Button btnForecastHeight;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox reportBox;
    }
}

