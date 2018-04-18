/*
 * (c) 2014-2018 Andreas Kuntner
 */

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MVVMbasics.Converters
{
	/// <summary>
	/// Converter class that converts boolean values to Visibility values and vice versa.
	/// </summary>
	public class BooleanToVisibilityConverter : IValueConverter
	{
		/// <summary>
		/// Converts a boolean value to a Visibility value.
		/// </summary>
		/// <param name="value">Boolean input value</param>
		/// <param name="targetType">(not used)</param>
		/// <param name="parameter">(not used)</param>
		/// <param name="culture">(not used)</param>
		/// <returns>Visibility.Visible if the input value is TRUE, Visibility.Collapsed otherwise</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool)
			{
				bool input = (bool) value;
				return input ? Visibility.Visible : Visibility.Collapsed;
			}
			return DependencyProperty.UnsetValue;
		}

		/// <summary>
		/// Converts a Visibility value to a boolean value.
		/// </summary>
		/// <param name="value">Visibility value</param>
		/// <param name="targetType">(not used)</param>
		/// <param name="parameter">(not used)</param>
		/// <param name="culture">(not used)</param>
		/// <returns>TRUE if the input value equals Visibility.Visible, FALSE otherwise</returns>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Visibility)
			{
				Visibility input = (Visibility)value;
				return input == Visibility.Visible;
			}
			return DependencyProperty.UnsetValue;
		}
	}
}
