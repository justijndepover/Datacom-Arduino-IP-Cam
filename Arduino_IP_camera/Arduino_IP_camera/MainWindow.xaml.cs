using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Arduino_IP_camera
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string IPADDRESS = "172.23.49.1";
        private const string LOGIN = "student";
        private const string PASSWORD = "niets";
        private BitmapImage _image;
        private int Zoom = 0;

        BackgroundWorker bw = new BackgroundWorker();

        public BitmapImage Image
        {
            get { return _image; }
            set { _image = value; }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += bw_DoWork;
            bw.RunWorkerAsync();
        }

        #region Backgroundworker Image
        private void GetImage()
        {
            while(bw.IsBusy)
            {
                try
                {
                    string link = string.Format("http://{0}/axis-cgi/jpg/image.cgi", IPADDRESS);
                    HttpWebRequest req = (HttpWebRequest)WebRequest.Create(link);
                    CredentialCache cc = new CredentialCache();
                    cc.Add(
                        new Uri(link),
                        "Basic",
                        new NetworkCredential(LOGIN, PASSWORD));
                    req.Credentials = cc;
                    HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        using (BinaryReader reader = new BinaryReader(res.GetResponseStream()))
                        {
                            Byte[] lnByte = reader.ReadBytes(1 * 1024 * 1024 * 10);
                            Image = ToImage(lnByte);
                            imgScreen.Source = Image;
                        }
                    }));
                }
                catch (Exception)
                {

                }
                
            }
        }

        private BitmapImage ToImage(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            GetImage();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            bw.CancelAsync();
        }
        #endregion

        private void btnArrow_Click(object sender, RoutedEventArgs e)
        {
            MoveCamera(((Button)sender).Tag.ToString());
        }

        private void MoveCamera(string direction)
        {
            string link = string.Format("http://{0}/axis-cgi/com/ptz.cgi?move="+direction, IPADDRESS);
            PostRequest(link);
        }

        private void ZoomCamera()
        {
            string link = string.Format("http://{0}/axis-cgi/com/ptz.cgi?zoom="+Zoom, IPADDRESS);
            PostRequest(link);
        }

        private void PostRequest(string link)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(link);
            CredentialCache cc = new CredentialCache();
            cc.Add(
                new Uri(link),
                "Basic",
                new NetworkCredential(LOGIN, PASSWORD));
            req.Credentials = cc;
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            res.Close(); 
        }

        private void sldZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Zoom = (int)sldZoom.Value;
            ZoomCamera();
        }

    }
}
