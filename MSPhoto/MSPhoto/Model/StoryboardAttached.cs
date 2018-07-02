using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace MSPhoto.Model
{
    public class StoryboardAttached : DependencyObject
    {
        #region IsBegin

        public static readonly DependencyProperty IsBeginPropety =
            DependencyProperty.RegisterAttached("IsBegin",
            typeof(bool),
            typeof(StoryboardAttached),
            new PropertyMetadata(false, ChangedIsBeginProperty));

        public static void SetIsBegin(DependencyObject obj, bool value)
        {
            obj.SetValue(IsBeginPropety, value);
        }

        public static bool GetIsBegin(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsBeginPropety);
        }

        private static void ChangedIsBeginProperty(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var sb = obj as Storyboard;

            if ((bool)e.NewValue)
            {
                sb.Begin();
            }
            else
            {
                sb.Stop();
            }
        }

        #endregion

        #region CompletedCommand

        public static readonly DependencyProperty CompletedCommandProperty =
            DependencyProperty.RegisterAttached("CompletedCommand",
            typeof(ICommand),
            typeof(StoryboardAttached),
            new PropertyMetadata(null, ChangedCompletedCommandProperty));

        public static void SetCompletedCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(CompletedCommandProperty, value);
        }

        public static ICommand GetCompletedCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(CompletedCommandProperty);
        }

        private static void ChangedCompletedCommandProperty(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var sb = obj as Storyboard;
            var command = e.NewValue as ICommand;

            sb.Completed += (s, args) =>
            {
                command.Execute(null);
            };
        }

        #endregion
    }
}