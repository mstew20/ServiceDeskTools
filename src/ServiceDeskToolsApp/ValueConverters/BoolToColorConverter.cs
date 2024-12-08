using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace ServiceDeskToolsApp.ValueConverters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (bool)value;

            if (!val)
            {
                return new SolidColorBrush(Color.FromRgb(100, 100, 100));
            }
            
            return Application.Current.FindResource("MahApps.Brushes.Text");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
