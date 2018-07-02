using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Windows.Devices.Enumeration;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MSPhoto.Model
{
    public enum PreviewState { Start, Stop }

    public class CaptureControl : ContentPresenter, INotifyPropertyChanged
    {
        #region Property

        #region DependencyProperty

        #region PreviewState

        public static readonly DependencyProperty PreviewStateProperty =
            DependencyProperty.Register("PreviewState",
            typeof(PreviewState),
            typeof(CaptureControl),
            new PropertyMetadata(PreviewState.Stop, ChangedPreviewStateProperty));

        private static void ChangedPreviewStateProperty(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var capture = obj as CaptureControl;

            switch ((PreviewState)e.NewValue)
            {
                case PreviewState.Start:
                    capture.Start();
                    break;

                case PreviewState.Stop:
                    capture.Stop();
                    break;
            }
        }

        public PreviewState PreviewState
        {
            get { return (PreviewState)this.GetValue(PreviewStateProperty); }
            set { this.SetValue(PreviewStateProperty, value); }
        }

        #endregion

        #region IsCapture

        public static readonly DependencyProperty IsCaptureProperty =
            DependencyProperty.Register("IsCapture",
            typeof(bool),
            typeof(CaptureControl),
            new PropertyMetadata(false, ChangedIsCaptureProperty));

        private static void ChangedIsCaptureProperty(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                var capture = obj as CaptureControl;
                capture.Capture();
            }
        }

        public bool IsCapture
        {
            get { return (bool)this.GetValue(IsCaptureProperty); }
            set { this.SetValue(IsCaptureProperty, value); }
        }

        #endregion

        #region CapturedCommand

        public static readonly DependencyProperty CapturedCommandProperty =
            DependencyProperty.Register("CapturedCommand",
            typeof(ICommand),
            typeof(CaptureControl),
            new PropertyMetadata(null));

        public ICommand CapturedCommand
        {
            get { return (ICommand)this.GetValue(CapturedCommandProperty); }
            set { this.SetValue(CapturedCommandProperty, value); }
        }
        
        #endregion

        #endregion

        public MediaCapture Media { get; private set; }
        public CaptureElement Element { get; private set; }

        #endregion

        public CaptureControl()
        {
            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                Initialize();
            }
        }

        #region Private Method

        private async void Initialize()
        {
            var devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            this.Media = new MediaCapture();
            
            if (devices.Count > 0)
            {
                MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings
                {
                    VideoDeviceId = devices[0].Id,
                    PhotoCaptureSource = PhotoCaptureSource.VideoPreview
                };

                await this.Media.InitializeAsync(settings);

                var resolutions = this.Media.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview);
                var resolution = (from r in resolutions
                                  orderby (r as VideoEncodingProperties).Width descending
                                  select r).FirstOrDefault();
                
                await this.Media.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, resolution);

                this.Element = new CaptureElement();
                this.Element.Source = this.Media;
                this.Content = this.Element;
            }

            if (this.PreviewState == Model.PreviewState.Start)
            {
                Start();
            }

            Window.Current.VisibilityChanged += Current_VisibilityChanged;
        }

        private async void Start()
        {
            try
            {
                if (this.Media != null)
                {
                    await this.Media.StartPreviewAsync();
                }
            }
            catch
            {
            }
        }

        private async void Stop()
        {
            try
            {
                if (this.Media != null)
                {
                    await this.Media.StopPreviewAsync();
                }
            }
            catch
            {
            }
        }

        private async void Capture()
        {
            try
            {
                using (var stream = new InMemoryRandomAccessStream())
                {
                    await this.Media.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);
                    await stream.FlushAsync();

                    if (this.CapturedCommand != null)
                    {
                        this.CapturedCommand.Execute(stream.CloneStream());
                    }
                }
            }
            catch
            {
            }
        }

        #endregion

        #region Event Handler

        private void Current_VisibilityChanged(object sender, VisibilityChangedEventArgs e)
        {
            if (e.Visible)
            {
                this.PreviewState = Model.PreviewState.Start;
            }
            else
            {
                this.PreviewState = Model.PreviewState.Stop;
            }
        }

        #endregion

        #region RaisePropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propertyName)
        {
            var OnPropertyChanged = PropertyChanged;
            if (OnPropertyChanged != null)
            {
                OnPropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}