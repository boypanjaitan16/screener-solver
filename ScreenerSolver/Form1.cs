using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenerSolver
{
    public partial class Form1 : Form
    {
        private Boolean isStart = false;
        private Timer timer;
        public Form1()
        {
            InitializeComponent();

            this.label2.Height = 2;
            this.label3.Height = 2;
            this.label6.Height = 2;
            this.label9.Height = 2;
            this.label11.Height = 2;
            this.label14.Height = 2;
            this.label15.Height = 2;

            timer = new Timer();
        }
        

        private void runTimer() {
            string selang = this.loopTime.Text;
            int value;

            if (int.TryParse(selang, out value))
            {
                timer.Interval = value * 60000;//one minute 60000
                timer.Tick += new EventHandler(moveImages);
                timer.Start();

                this.resultMoveImages.Text  = "Layanan sedang berjalan ...";
                this.startAction.Text = "STOP";
                isStart = true;
            }
            else
            {
                MessageBox.Show("Nilai timer harus berupa angka!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void moveImages(object sender, EventArgs e) {
            string path         = this.imageLocation.Text;
            string[] allImages  = Directory.GetFiles(@path);
            string preName      = this.preName.Text;
            string cleanName    = Regex.Replace(preName, @"\s", "_");
            string currentTime  = DateTime.Now.ToString("_yyyyMMdd_HHmmss");

            string textMundur   = this.jamMundur.Text;

            if (textMundur != "" || textMundur != "0")
            {
                int mundur;
                if (int.TryParse(textMundur, out mundur))
                {
                    double tmpstmp = DateTimeToUnixTimestamp(DateTime.Now) - (mundur * 3600);
                    currentTime = UnixTimestampToDateTime(tmpstmp).ToString("_yyyyMMdd_HHmmss");
                    
                }
            }
            

            string fullName     = String.Concat(cleanName, currentTime, ".png");
            string ftpUser      = this.ftpUsername.Text;
            string ftpPassword  = this.ftpPassword.Text;
            string ftpAddress   = this.ftpAddress.Text;

            Random rnd  = new Random();
            int r       = rnd.Next(allImages.Length);

            using (WebClient client = new WebClient())
            {
                try
                {

                    string stopTime = this.stopTime.Text;
                    if(stopTime != "")
                    {
                        string dateString   = DateTime.Now.ToString("dd-MM-yyyy,"+stopTime);
                        DateTime myDate     = DateTime.ParseExact(dateString, "dd-MM-yyyy,HH:mm",System.Globalization.CultureInfo.CurrentCulture);
                        
                        
                        if(DateTimeToUnixTimestamp(DateTime.Now) > DateTimeToUnixTimestamp(myDate))
                        {
                            stopAction();
                            return;
                        }
                        
                    }
                    
                    client.Credentials = new NetworkCredential(ftpUser, ftpPassword);
                    client.UploadFile(String.Concat(ftpAddress, fullName), WebRequestMethods.Ftp.UploadFile, allImages[r]);

                    this.resultMoveImages.Text = String.Concat(fullName, " berhasil di unggah");
                    
                }
                catch(Exception ex)
                {
                    this.resultMoveImages.Text = ex.Message;
                }
            }
            
        }

        private void btnChooseLocation_Click(object sender, EventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                this.locationPath.Text = dialog.SelectedPath;
            }
        }

        private async void btnCapture_Click(object sender, EventArgs e)
        {
            if (this.locationPath.Text == String.Empty || this.locationPath.Text == "")
            {
                MessageBox.Show("Silahkan pilih lokasi penyimpanan terlebih dahulu", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ScreenCapture sc = new ScreenCapture();
            string currentTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileName = String.Concat(currentTime, ".png");
            string fullPath = String.Concat(this.locationPath.Text, "\\", fileName);

            try
            {
                /*
                //How to capture the whole screen
                var image = ScreenCapture.CaptureDesktop();
                image.Save(@fullPath, ImageFormat.Jpeg);
                */
                string delayText = this.delayTime.Text;
                int delay;
                if(int.TryParse(delayText, out delay))
                {
                    await Task.Delay(delay*1000);
                    var image = ScreenCapture.CaptureWithoutTaskbar();
                    image.Save(@fullPath, ImageFormat.Png);
                }
                else
                {
                    MessageBox.Show("Error parse delay time", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                


                /*
                //How to capture the whole Screen
                var image = ScreenCapture.CaptureActiveWindow();
                image.Save(@"C:\tmp\snippetsource.jpg", ImageFormat.Jpeg);
                */

                this.textResult.Text = String.Concat(fileName, " berhasil disimpan");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Terjadi kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }

        private void btnChooseImgLocation_Click(object sender, EventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                this.imageLocation.Text = dialog.SelectedPath;
            }
        }

        private void startAction_Click(object sender, EventArgs e)
        {
            
            if (!isStart)
            {
                if (this.imageLocation.Text != "")
                {
                    if (this.preName.Text != "")
                    {
                        string path = this.imageLocation.Text;
                        if (Directory.Exists(path))
                        {
                            runTimer();
                        }
                        else
                        {
                            MessageBox.Show("Lokasi pengambilan gambar tidak valid, silahkan periksa kembali", "Persiapan belum lengkap", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        }
                    }
                    else
                    {
                        MessageBox.Show("Nama tambahan awal belum di isikan", "Persiapan belum lengkap", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    }

                }
                else
                {
                    MessageBox.Show("Anda belum memilih lokasi pengambilan gambar", "Persiapan belum lengkap", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                stopAction();
            }
            
        }

        private void stopAction() {
            timer.Stop();
            this.startAction.Text = "START";
            this.resultMoveImages.Text = "Layanan berhenti";
            this.isStart = false;
        }

        public static DateTime UnixTimestampToDateTime(double unixTime)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTime).ToLocalTime();
            return dtDateTime;
        }

        public static double DateTimeToUnixTimestamp(DateTime dateTime)
        {
            DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Local);
            long unixTimeStampInTicks = (dateTime.ToUniversalTime() - unixStart).Ticks;
            return (double)unixTimeStampInTicks / TimeSpan.TicksPerSecond;
        }
    }
}
