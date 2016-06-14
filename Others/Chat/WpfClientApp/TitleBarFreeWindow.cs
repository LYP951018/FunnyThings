using Prism.Commands;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfClientApp
{
    public class IsShownVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isShown = (bool)value;
            return isShown ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class WindowStateToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (WindowState)value;
            return state == WindowState.Maximized ? "\u0032" : "\u0031";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TitleBarFreeWindow : Window
    {        
        public TitleBarFreeWindow()
        {
            MouseLeftButtonDown += (o, e) =>
            {
                DragMove();
            };
        }

        public Brush TitleBarBackground
        {
            get { return (Brush)GetValue(TitleBarBackgroundProperty); }
            set { SetValue(TitleBarBackgroundProperty, value); }
        }
       
        public static readonly DependencyProperty TitleBarBackgroundProperty =
            DependencyProperty.Register("TitleBarBackground", typeof(Brush), typeof(TitleBarFreeWindow), new PropertyMetadata(Brushes.White));

        public Brush TitleBarForeground
        {
            get { return (Brush)GetValue(TitleBarForegroundProperty); }
            set { SetValue(TitleBarForegroundProperty, value); }
        }
        
        public static readonly DependencyProperty TitleBarForegroundProperty =
            DependencyProperty.Register("TitleBarForeground", typeof(Brush), typeof(TitleBarFreeWindow), new PropertyMetadata(Brushes.Black));

        public bool IsMinimalButtonShown
        {
            get { return (bool)GetValue(IsMinimalButtonShownProperty); }
            set { SetValue(IsMinimalButtonShownProperty, value); }
        }

        public static readonly DependencyProperty IsMinimalButtonShownProperty =
            DependencyProperty.Register("IsMinimalButtonShown", typeof(bool), typeof(TitleBarFreeWindow), new PropertyMetadata(true));

        public bool IsMaxiumButtonShown
        {
            get { return (bool)GetValue(IsMaxiumButtonShownProperty); }
            set { SetValue(IsMaxiumButtonShownProperty, value); }
        }

        public static readonly DependencyProperty IsMaxiumButtonShownProperty =
            DependencyProperty.Register("IsMaxiumButtonShown", typeof(bool), typeof(TitleBarFreeWindow), new PropertyMetadata(true));

        public bool IsCloseButtonShown
        {
            get { return (bool)GetValue(IsCloseButtonShownProperty); }
            set { SetValue(IsCloseButtonShownProperty, value); }
        }
       
        public static readonly DependencyProperty IsCloseButtonShownProperty =
            DependencyProperty.Register("IsCloseButtonShown", typeof(bool), typeof(TitleBarFreeWindow), new PropertyMetadata(true));
       
        static TitleBarFreeWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TitleBarFreeWindow), new FrameworkPropertyMetadata(typeof(TitleBarFreeWindow)));
        }

        public ICommand CloseWindowCommand
        {
            get;
            private set;
        } = new DelegateCommand<Window>(w => w.Close());

        public ICommand MinOrMaxWindowCommand
        {
            get;
            private set;
        } = new DelegateCommand<Window>(w =>
            {
                var state = w.WindowState;
                if (state == WindowState.Maximized)
                    w.WindowState = WindowState.Normal;
                else if (state == WindowState.Normal)
                    w.WindowState = WindowState.Maximized;
            }
        );
    }
}
