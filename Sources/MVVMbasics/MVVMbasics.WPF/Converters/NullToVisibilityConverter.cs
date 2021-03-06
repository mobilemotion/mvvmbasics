﻿/*
 * (c) 2014-2018 Andreas Kuntner
 */

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MVVMbasics.Converters
{
	/// <summary>
	/// Converter class that converts values to the Visibility.Collapsed value if they are NULL, and to Visibility.Visible otherwise.
	/// </summary>
	public class NullToVisibilityConverter : IValueConverter
	{
		/// <summary>
		/// Converts values to the Visibility.Collapsed value if they are NULL, and to Visibility.Visible otherwise.
		/// </summary>
		/// <param name="value">Input value</param>
		/// <param name="targetType">(not used)</param>
		/// <param name="parameter">(not used)</param>
		/// <param name="culture">(not used)</param>
		/// <returns>Visibility.Collapsed if the input value is NULL, Visibility.Visible otherwise</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return Visibility.Collapsed;
			else
				return Visibility.Visible;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			// Not implemented
			return DependencyProperty.UnsetValue;
		}
	}
}
