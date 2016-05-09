using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace _2048
{
    public class NumberColorConverter : IValueConverter
    {
        public NumberColorConverter()
        {
            _resourceDictionary.Source = new Uri("Themes/GridCell.xaml", UriKind.RelativeOrAbsolute);
        }

        public object Convert(object value, Type targetType, object parameter,
                System.Globalization.CultureInfo culture)
        {
            var brushName = $"Brush{value}";
            var ret = (SolidColorBrush)_resourceDictionary[brushName];
            Debug.Assert(ret != null);
            return ret;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private ResourceDictionary _resourceDictionary = new ResourceDictionary();
    }

    public class NumberTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
                System.Globalization.CultureInfo culture)
        {
            var number = (uint)value;
            if (number == 0) return "";
            else return number.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter,
                System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NumberForegroundConverter : IValueConverter
    {
        public NumberForegroundConverter()
        {
            _resourceDictionary.Source = new Uri("Themes/GridCell.xaml", UriKind.RelativeOrAbsolute);
        }

        public object Convert(object value, Type targetType, object parameter,
                System.Globalization.CultureInfo culture)
        {
            var number = (uint)value;
            if (number <= 8) return _resourceDictionary["TextBrush0"];
            else return _resourceDictionary["TextBrush1"];
        }

        public object ConvertBack(object value, Type targetType, object parameter,
               System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private ResourceDictionary _resourceDictionary = new ResourceDictionary();
    }

    [DebuggerDisplay("Number = {Number}")]
    public class GridCell : Control
    {
        static GridCell()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GridCell), new FrameworkPropertyMetadata(typeof(GridCell)));
            NumberProperty = DependencyProperty.Register("Number", typeof(uint), typeof(GridCell));
        }

        public static DependencyProperty NumberProperty;

        public uint Number
        {
            get { return (uint)GetValue(NumberProperty); }
            set { SetValue(NumberProperty, value);}
        }
    }
}
