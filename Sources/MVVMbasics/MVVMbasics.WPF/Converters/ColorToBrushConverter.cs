/*
 * (c) 2014-2015 Andreas Kuntner
 */

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MVVMbasics.Converters
{
	/// <summary>
	/// Converter class that converts Color values to objects of type SolidColorBrush to be directly used in XAML.
	/// </summary>
	public class ColorToBrushConverter : IValueConverter
	{
		/// <summary>
		/// Converts a Color value to an object of type SolidColorBrush.
		/// </summary>
		/// <param name="value">Color input value</param>
		/// <param name="targetType">(not used)</param>
		/// <param name="parameter">(not used)</param>
		/// <param name="culture">(not used)</param>
		/// <returns>SolidColorBrush containing the given color value</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Color)
			{
				Color input = (Color) value;
				return new SolidColorBrush(input);
			}
			return DependencyProperty.UnsetValue;
		}

		/// <summary>
		/// Converts a SolidColorBrush object to a Color value, by retrieving the brushes color.
		/// </summary>
		/// <param name="value">SolidColorBrush input object</param>
		/// <param name="targetType">(not used)</param>
		/// <param name="parameter">(not used)</param>
		/// <param name="culture">(not used)</param>
		/// <returns>Color value representig the given brush</returns>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is SolidColorBrush)
			{
				SolidColorBrush input = (SolidColorBrush)value;
				return input.Color;
			}
			return DependencyProperty.UnsetValue;
		}
	}
}
