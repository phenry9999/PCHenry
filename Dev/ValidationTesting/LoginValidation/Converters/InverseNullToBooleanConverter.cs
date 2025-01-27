using System.Globalization;
using System.Windows.Data;

namespace LoginValidation.Converters
{
	public class InverseNullToBooleanConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// Convert the value to a boolean based on whether it is null or not
			return value != null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
