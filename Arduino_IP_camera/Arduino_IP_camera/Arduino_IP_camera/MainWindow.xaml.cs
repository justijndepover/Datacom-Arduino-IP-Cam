﻿using System;
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
using System.IO.Ports;
using System.Security.AccessControl;
using System.Threading;
using System.Windows.Threading;

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
        SerialPort port;
        private bool enableControls = true;

        BackgroundWorker bw = new BackgroundWorker();

        string ReceivedMessage;

        public BitmapImage Image
        {
            get { return _image; }
            set { _image = value; }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.Focus();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += bw_DoWork;
            bw.RunWorkerAsync();

            try
            {
                port = new SerialPort("COM4");
                port.BaudRate = 9600;
                port.Open();
                port.DataReceived += port_DataReceived;
            }
            catch (Exception)
            {

            }
            this.Focus();
        }


        void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                ReceivedMessage = "";
                ReceivedMessage = port.ReadLine();
                ReceivedMessage = ReceivedMessage.Replace("\r", "");

                if (ReceivedMessage == "1")
                {
                    chksScan.IsChecked = false;
                    MoveCamera("home");
                    MessageBoxResult mr = MessageBox.Show("Er staat iemand aan de deur, Wilt u de persoon binnenlaten?", "bel", MessageBoxButton.YesNo);

                    if (mr == MessageBoxResult.Yes)
                    {
                        port.WriteLine("2");
                    }
                    else if (mr == MessageBoxResult.No)
                    {
                        port.WriteLine("3");
                    }
                    port.DiscardInBuffer();
                }
            }));

        }

        #region Backgroundworker Image
        private void GetImage()
        {
            while (bw.IsBusy)
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

        #region move camera
        private void btnArrow_Click(object sender, RoutedEventArgs e)
        {
            MoveCamera(((Button)sender).Tag.ToString());
        }

        private void MoveCamera(string direction)
        {
            string link = string.Format("http://{0}/axis-cgi/com/ptz.cgi?move=" + direction, IPADDRESS);
            PostRequest(link);
        }

        private void ZoomCamera()
        {
            string link = string.Format("http://{0}/axis-cgi/com/ptz.cgi?zoom=" + Zoom, IPADDRESS);
            PostRequest(link);
        }

        private void ZoomCameraRelative(int value)
        {
            string link = string.Format("http://{0}/axis-cgi/com/ptz.cgi?rzoom=" + value, IPADDRESS);
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

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (enableControls)
            {
                if (!Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    switch (e.Key)
                    {
                        case Key.Up:
                            MoveCamera("up");
                            break;
                        case Key.Down:
                            MoveCamera("down");
                            break;
                        case Key.Left:
                            MoveCamera("left");
                            break;
                        case Key.Right:
                            MoveCamera("right");
                            break;
                    }
                }
                else if (Keyboard.IsKeyDown(Key.LeftCtrl))
                {
                    if (e.Key == Key.Up)
                    {
                        ZoomCameraRelative(500);
                    }

                    if (e.Key == Key.Down)
                    {
                        ZoomCameraRelative(-500);
                    }
                }
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string link = "";
            switch (((Button)sender).Tag.ToString())
            {
                case "zoomin":
                    link = string.Format("http://{0}/axis-cgi/com/ptz.cgi?rzoom=500", IPADDRESS);
                    break;
                case "zoomuit":
                    link = string.Format("http://{0}/axis-cgi/com/ptz.cgi?rzoom=-500", IPADDRESS);
                    break;
                case "focusin":
                    link = string.Format("http://{0}/axis-cgi/com/ptz.cgi?rfocus=200", IPADDRESS);
                    break;
                case "focusuit":
                    link = string.Format("http://{0}/axis-cgi/com/ptz.cgi?rfocus=-200", IPADDRESS);
                    break;
                case "autofocus":
                    link = string.Format("http://{0}/axis-cgi/com/ptz.cgi?autofocus=on", IPADDRESS);
                    break;
                case "helderheidin":
                    link = string.Format("http://{0}/axis-cgi/com/ptz.cgi?rbrightness=200", IPADDRESS);
                    break;
                case "helderheiduit":
                    link = string.Format("http://{0}/axis-cgi/com/ptz.cgi?rbrightness=-200", IPADDRESS);
                    break;
                case "zwartwit":
                    link = string.Format("http://{0}/axis-cgi/com/ptz.cgi?ircutfilter=off", IPADDRESS);
                    break;
                case "kleur":
                    link = string.Format("http://{0}/axis-cgi/com/ptz.cgi?ircutfilter=on", IPADDRESS);
                    break;
                default:
                    break;
            }
            if (link != "")
            {
                PostRequest(link);
            }
        }

        private void scanCamera(int pan, int tilt)
        {
            string link = string.Format("http://{0}/axis-cgi/com/ptz.cgi?continuouspantiltmove=" + pan + "," + tilt, IPADDRESS);
            PostRequest(link);
        }

        private void tilt(float value)
        {
            string link = string.Format("http://{0}/axis-cgi/com/ptz.cgi?tilt=" + value, IPADDRESS);
            PostRequest(link);
        }

        #endregion

        private void chksScan_Checked(object sender, RoutedEventArgs e)
        {
            if (chksScan.IsChecked == true)
            {
                enableControls = false;
                grdControl.IsEnabled = false;
                stpControl.IsEnabled = false;
                tilt(-30);
                Zoom = 1;
                ZoomCamera();
                scanCamera(5, 0);
            }
            else
            {
                enableControls = true;
                grdControl.IsEnabled = true;
                stpControl.IsEnabled = true;
                scanCamera(0, 0);
            }
        }

    }
}
