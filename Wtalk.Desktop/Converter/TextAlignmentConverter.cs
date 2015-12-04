using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Wtalk.Desktop.Converter
{
    public class TextAlignmentConverter : IMultiValueConverter
    {
        System.Windows.FrameworkElement myControl;
        object theValue;
        public object Convert(object[] values, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            myControl = values[0] as System.Windows.FrameworkElement;            
            if(myControl.Tag.ToString() == values[1].ToString())
                return System.Windows.TextAlignment.Left;
            else
                return System.Windows.TextAlignment.Right;
        }

        public object[] ConvertBack(object value, System.Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
