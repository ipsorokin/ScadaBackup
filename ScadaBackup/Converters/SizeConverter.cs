using System;
using System.Globalization;
using System.Windows.Data;

namespace ScadaBackup.Converters
{
    public class SizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter != null)
            {
                switch (parameter.ToString().ToLower())
                {
                    case "b":
                        return value;
                    case "kb":
                        return (long)value / 1024;
                    case "mb":
                        return ((long)value / 1024 / 1024).ToString("0.0");
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
