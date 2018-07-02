using System;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MSPhoto.Model
{
    public class CountSound : ContentPresenter
    {
        #region Property

        #region CountSoundPath

        public string CountSoundPath { get; set; }

        #endregion

        #region CaptureSoundPath

        public string CaptureSoundPath { get; set; }

        #endregion

        #region IsPlay

        public static readonly DependencyProperty IsPlayProperty =
            DependencyProperty.Register("IsPlay",
            typeof(bool),
            typeof(CountSound),
            new PropertyMetadata(false, ChangedIsPlayProperty));

        private static void ChangedIsPlayProperty(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var sound = obj as CountSound;

            if ((bool)e.NewValue)
            {
                sound.Start();
            }
            else
            {
                sound.Stop();
            }
        }

        public bool IsPlay
        {
            get { return (bool)this.GetValue(IsPlayProperty); }
            set { this.SetValue(IsPlayProperty, value); }
        }

        #endregion

        #region FlashCommand

        private static readonly DependencyProperty FlashCommandProperty =
            DependencyProperty.Register("FlashCommand",
            typeof(ICommand),
            typeof(CountSound),
            new PropertyMetadata(null));

        public ICommand FlashCommand
        {
            get { return (ICommand)this.GetValue(FlashCommandProperty); }
            set { this.SetValue(FlashCommandProperty, value); }
        }

        #endregion

        #endregion

        #region Variable

        private MediaElement _media;
        private DispatcherTimer _timer;
        private int _count;

        #endregion

        public CountSound()
        {
            this._media = new MediaElement();
            this._media.AudioCategory = Windows.UI.Xaml.Media.AudioCategory.BackgroundCapableMedia;
            this.Content = this._media;

            this._timer = new DispatcherTimer();
            this._timer.Interval = TimeSpan.FromSeconds(1);
            this._timer.Tick += _timer_Tick;
        }

        private void Start()
        {
            this._count = 1;
            this._media.Source = new Uri(this.CountSoundPath, UriKind.RelativeOrAbsolute);
            this._timer.Start();
        }

        private void Stop()
        {
            this._timer.Stop();
        }

        private void _timer_Tick(object sender, object e)
        {
            this._media.Stop();
            this._timer.Stop();

            if (this._count == 6)
            {
                if (this.FlashCommand != null)
                {
                    this.FlashCommand.Execute(null);                
                }

                this._media.Source = new Uri(this.CaptureSoundPath, UriKind.RelativeOrAbsolute);
            }
            else
            {
                this._media.Play();
                this._timer.Start();
            }

            this._count++;
        }
    }
}