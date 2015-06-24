/*
 * (c) 2014-2015 Andreas Kuntner
 */
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MVVMbasics.Converters
{
	/// <summary>
	/// Converter class that inverts (negates) boolean values. In addition, it can process an additional IValueConverter
	/// instance that is passed as ConverterParameter and calls this converter's Convert method before or after the
	/// inverting operation, depending on whether the additional converter expects a boolean value as input, or produces
	/// a boolean value as output.
	/// </summary>
	public class BooleanValueInverter : IValueConverter
	{
		/// <summary>
		/// Inverts boolean values, either directly or by applying an additional converter that expects or returns a
		/// boolean value.
		/// </summary>
		/// <param name="value">Input value</param>
		/// <param name="targetType">(not used)</param>
		/// <param name="parameter">Optional additional converter: If provided, is called either before or after the invertion (depending on whether it expects or returns a boolean value). If not provided, the input value must be boolean and is inverted directly.</param>
		/// <param name="culture">(not used)</param>
		/// <returns>Output value</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(parameter is IValueConverter))
			{
				// No second converter is given as parameter:
				// Just invert and return, if boolean input value was provided
				if (value is bool)
					return !(bool)value;
				else
					return DependencyProperty.UnsetValue; // Fallback for non-boolean input values
			}
			else
			{
				// Second converter is provided:
				// Retrieve this converter...
				IValueConverter converter = (IValueConverter)parameter;

				if (value is bool)
				{
					// ...if boolean input value was provided, invert and then convert
					bool input = !(bool)value;
					return converter.Convert(input, targetType, null, culture);
				}
				else
				{
					// ...if input value is not boolean, convert and then invert boolean result
					object convertedValue = converter.Convert(value, targetType, null, culture);
					if (convertedValue is bool)
						return !(bool)convertedValue;
					else
						return DependencyProperty.UnsetValue; // Fallback for non-boolean return values
				}
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// Not implemented
			return DependencyProperty.UnsetValue;
		}
	}
}
