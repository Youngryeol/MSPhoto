using System;
using System.IO;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MSPhoto.Model;
using MSPhoto.View;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace MSPhoto.ViewModel
{
    public class MainPageViewModel : ViewModelBase
    {
        #region Property

        #region IsBeginCount

        private bool isBeginCount;
        public bool IsBeginCount
        {
            get { return this.isBeginCount; }
            set
            {
                if (this.isBeginCount != value)
                {
                    this.isBeginCount = value;
                    this.RaisePropertyChanged("IsBeginCount");
                }
            }
        }

        #endregion

        #region IsCapture

        private bool isCapture;
        public bool IsCapture
        {
            get { return this.isCapture; }
            set
            {
                if (this.isCapture != value)
                {
                    this.isCapture = value;
                    this.RaisePropertyChanged("IsCapture");
                }
            }
        }

        #endregion

        #region CapturePreviewState

        private PreviewState capturePreviewState = PreviewState.Start;
        public PreviewState CapturePreviewState
        {
            get { return this.capturePreviewState; }
            set
            {
                if (this.capturePreviewState != value)
                {
                    this.capturePreviewState = value;
                    this.RaisePropertyChanged("CapturePreviewState");
                }
            }
        }

        #endregion

        #region CaptureImage

        private BitmapImage captureImage;
        public BitmapImage CaptureImage
        {
            get { return this.captureImage; }
            set
            {
                if (this.captureImage != value)
                {
                    this.captureImage = value;
                    this.RaisePropertyChanged("CaptureImage");
                }
            }
        }

        #endregion

        #region CaptureImageVisibility

        private Visibility captureImageVisibility = Visibility.Collapsed;
        public Visibility CaptureImageVisibility
        {
            get { return this.captureImageVisibility; }
            set
            {
                if (this.captureImageVisibility != value)
                {
                    this.captureImageVisibility = value;
                    this.RaisePropertyChanged("CaptureImageVisibility");
                }
            }
        }

        #endregion

        #endregion

        #region Command

        #region CaptureCommand

        private ICommand captureCommand;
        public ICommand CaptureCommand
        {
            get { return this.captureCommand ?? (this.captureCommand = new RelayCommand(OnCapture)); }
        }

        private void OnCapture()
        {
            this.IsBeginCount = true;
        }

        #endregion

        #region CompletedCountCommand

        private ICommand completedCountCommand;
        public ICommand CompletedCountCommand
        {
            get { return this.completedCountCommand ?? (this.completedCountCommand = new RelayCommand(OnCompletedCount)); }
        }

        private void OnCompletedCount()
        {
            this.IsBeginCount = false;
            App.Navigate(typeof(CompletePage));
        }

        #endregion

        #region CapturedCommand

        private ICommand capturedCommand;
        public ICommand CapturedCommand
        {
            get { return this.capturedCommand ?? (this.capturedCommand = new RelayCommand<IRandomAccessStream>(OnCaptured)); }
        }

        private async void OnCaptured(IRandomAccessStream stream)
        {
            SendImage(stream);

            using (var streamClone = stream.CloneStream())
            {
                BitmapImage bitmap = new BitmapImage();

                await bitmap.SetSourceAsync(streamClone);
                this.CaptureImage = bitmap;
                this.CaptureImageVisibility = Visibility.Visible;
                App.Locator.Complete.CaptureImage = bitmap;
            }

            this.IsCapture = false;
        }

        #endregion

        #region FlashCommand

        private ICommand flashCommand;
        public ICommand FlashCommand
        {
            get { return this.flashCommand ?? (this.flashCommand = new RelayCommand(OnFlash)); }
        }

        private void OnFlash()
        {
            this.IsCapture = true;
            this.CapturePreviewState = PreviewState.Stop;
        }

        #endregion

        #endregion

        #region Constructor

        public MainPageViewModel()
        {
        }

        #endregion

        #region Private Method

        private async void SendImage(IRandomAccessStream stream)
        {
            HostName hostName = new HostName("127.0.0.1");

            using (StreamSocket socket = new StreamSocket())
            {
                try
                {
                    await socket.ConnectAsync(hostName, "5000");

                    using (DataWriter writer = new DataWriter(socket.OutputStream))
                    {
                        writer.ByteOrder = ByteOrder.LittleEndian;

                        var readStream = stream.AsStreamForRead();

                        /*
                        byte[] buffer = new byte[1024];

                        writer.WriteInt32(0);
                        await writer.StoreAsync();

                        while (readStream.CanRead)
                        {
                            var readCount = await stream.AsStream().ReadAsync(buffer, 0, buffer.Length);

                            if (readCount < buffer.Length)
                            {
                                break;
                            }

                            byte[] bytes = new byte[readCount];

                            Array.Copy(buffer, bytes, readCount);

                            writer.WriteInt32(1);
                            await writer.StoreAsync();

                            writer.WriteInt32(bytes.Length);
                            await writer.StoreAsync();

                            writer.WriteBytes(bytes);
                            await writer.StoreAsync();
                        }

                        writer.WriteInt32(2);
                        await writer.StoreAsync();

                        await writer.FlushAsync();
                        writer.DetachStream();
                        */
                        
                        byte[] bytes = new byte[readStream.Length];
                        await readStream.ReadAsync(bytes, 0, bytes.Length);

                        writer.WriteInt32(bytes.Length);
                        await writer.StoreAsync();

                        writer.WriteBytes(bytes);
                        await writer.StoreAsync();

                        await writer.FlushAsync();
                        writer.DetachStream();
                    }
                }
                catch
                {

                }
            }
        }

        #endregion
    }
}