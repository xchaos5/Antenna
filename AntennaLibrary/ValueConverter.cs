using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using AntennaLibCore;

namespace AntennaLibrary
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Do the conversion from bool to visibility
            var bVal = value as bool? ?? false;
            if (bVal)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Do the conversion from visibility to bool
            throw new NotImplementedException();
        }
    }

    public class BoolToOppositeVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Do the conversion from bool to visibility
            var bVal = value as bool? ?? false;
            if (bVal)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Do the conversion from visibility to bool
            throw new NotImplementedException();
        }
    }

    public class BoolToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bVal = value as bool? ?? false;
            if (bVal)
            {
                return 1.0;
            }
            return 0.25;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FileNameWithoutExtensionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var name = Path.GetFileNameWithoutExtension(value as string);
            return name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FreqUnitConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = value is FreqUnit ? (FreqUnit) value : FreqUnit.MHz;
            switch (item)
            {
                case FreqUnit.Hz:
                    return "Hz";
                case FreqUnit.KHz:
                    return "KHz";
                case FreqUnit.MHz:
                    return "MHz";
                case FreqUnit.GHz:
                    return "GHz";
            }
            return "MHz";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value as string)
            {
                case "Hz":
                    return FreqUnit.Hz;
                case "KHz":
                    return FreqUnit.KHz;
                case "MHz":
                    return FreqUnit.MHz;
                case "GHz":
                    return FreqUnit.GHz;

            }
            return FreqUnit.MHz;
        }
    }

    public class InvertVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bVal = (Visibility)value;
            if (bVal == Visibility.Visible)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DimensionScaleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double original = 0.0, scale = 0.0;
            double.TryParse(values[0].ToString(), out original);
            double.TryParse(values[1].ToString(), out scale);
            return (Math.Round(original * scale, 2)).ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var uri = new Uri(Environment.CurrentDirectory + "/" + value, UriKind.Absolute);
            var source = new BitmapImage();
            source.BeginInit();
            source.UriSource = uri;
            source.EndInit();
            return source;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
