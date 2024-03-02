using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VortexApp.UI.Helpers
{

    public class IsClickableToTextBoxStyleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isClickable = (bool)value;
            return isClickable ? Application.Current.FindResource("TextBoxHighlight") : "{x:Null}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}