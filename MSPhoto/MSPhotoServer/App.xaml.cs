using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Windows;
using TCP;

namespace MSPhotoServer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Mutex _mutex;
        private TCPManager _tcpManager;
        private SerialPort _serialPort;
        private byte[] _imgBytes;

        public App()
        {
            bool appExists;
            this._mutex = new Mutex(true, "MSPhotoServer", out appExists);

            if (appExists)
            {
                this.Startup += App_Startup;
                this.Exit += App_Exit;
            }
            else
            {
                Application.Current.Shutdown();
            } 
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            this._mutex.ReleaseMutex();

            if (this._tcpManager != null)
            {
                this._tcpManager.Clean();
            }

            this.Close();
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            this._tcpManager = new TCPManager();
            this._tcpManager.Received += _tcpManager_Received;
            this._tcpManager.StartServer(5000);
        }

        private void Close()
        {
            if (this._serialPort != null)
            {
                this._serialPort.Close();
                this._serialPort.Dispose();
                this._serialPort = null;
            }
        }

        private void SendSerialPort()
        {
            try
            {
                if (this._serialPort == null)
                {
                    string[] portNames = SerialPort.GetPortNames();
                    if (portNames.Length > 0)
                    {
                        string str = ConfigurationManager.AppSettings.Get("PortName");
                        if (portNames.Contains<string>(str))
                        {
                            this._serialPort = new SerialPort(str, 0x2580);
                        }
                    }
                }
                if (this._serialPort != null)
                {
                    if (!this._serialPort.IsOpen)
                    {
                        this._serialPort.Open();
                    }
                    this._serialPort.WriteLine("S\n");
                }
            }
            catch (Exception exception)
            {
                this.WriteLog("[ Serial ] " + exception.ToString());
                this.Close();
            }
        }

        private void SendCaptureImage(byte[] bytes)
        {
            try
            {
                var folder = ConfigurationManager.AppSettings.Get("ImageFolder");

                if (Directory.Exists(folder))
                {
                    string fileName = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string fullPath = string.Format("{0}\\{1}.jpg", folder, fileName);

                    using(MemoryStream stream = new MemoryStream(bytes))
                    {
                        using (var bitmap = Bitmap.FromStream(stream))
                        {
                            bitmap.RotateFlip(RotateFlipType.Rotate180FlipY);
                            bitmap.Save(fullPath, ImageFormat.Jpeg);
                            bitmap.Dispose();
                        }

                        stream.Close();
                    }                    
                }
            }
            catch(Exception ex)
            {
                WriteLog("[ Image ] " + ex.ToString());
            }
        }

        private void _tcpManager_Received(object sender, TCPReceiveEventArgs e)
        {
            /*
            if (e.Message == "0")
            {
                SendSerialPort();
                this._imgBytes = new byte[0];
            }
            else if (e.Message == "1")
            {
                Array.Resize(ref this._imgBytes, this._imgBytes.Length + e.Bytes.Length);
                int di = this._imgBytes.Length - e.Bytes.Length;
                Array.Copy(e.Bytes, 0, this._imgBytes, di, e.Bytes.Length);
            }
            else
            {
                SendCaptureImage(this._imgBytes);
            }*/

            SendSerialPort();
            SendCaptureImage(e.Bytes);
        }

        private void WriteLog(string message)
        {
            string log = string.Format("{0:yyyyMMdd:HHmmss:fff}:{1}", DateTime.Now, message);
            System.Diagnostics.Trace.WriteLine(log);
        }
    }
}