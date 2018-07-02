using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml;

namespace MSPhoto.ViewModel
{
    public class CompletePageViewModel : ViewModelBase
    {
        #region Property

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

        #endregion

        #region Command

        #region PointerPressedCommand

        private ICommand pointerPressedCommand;
        public ICommand PointerPressedCommand
        {
            get { return this.pointerPressedCommand ?? (this.pointerPressedCommand = new RelayCommand(OnPointerPressed)); }
        }

        private void OnPointerPressed()
        {
            App.Current.Exit();
        }

        #endregion

        #endregion

        #region Constructor

        public CompletePageViewModel()
        {
        }

        #endregion
    }
}