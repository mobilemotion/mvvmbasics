/*
 * (c) 2015 Frank Albert
 */

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace MVVMbasics.Converters
{
	/// <summary>
	/// Converter class that inserts line breaks into a given text at predefined locations.
	/// </summary>
	public class TextWrapConverter : IValueConverter
	{
		/// <summary>
		/// Converter class that inserts line breaks into a given text at predefined locations.
		/// </summary>
		/// <example>
		/// <code language="XAML" title="XAML">
		/// <![CDATA[
		/// <local:TextWrapConverter x:Key="textWrapConverter"/>
		/// 
		/// <!-- Tooltip für CommandButton-->
		/// <ToolTip
		///    x:Key="CommandToolTip"
		///    x:Shared="False">
		///    <StackPanel>
		///        <TextBlock
		///            FontWeight="Bold"
		///            Text="{Binding DisplayName}" />
		///        <TextBlock
		///            Text="{Binding ToolTip, 
		///                Converter={StaticResource textWrapConverter},
		///                ConverterParameter=40}" />
		///    </StackPanel>
		/// </ToolTip>]]>
		/// </code>
		/// </example>
		/// <param name="value">Text to be wrapped.</param>
		/// <param name="targetType">The type of the binding target property.</param>
		/// <param name="parameter">The desired length of each text line.</param>
		/// <param name="language">The language of the conversion.</param>
		/// <returns>The wrapped text, or an empty string if the input text was NULL or empty.</returns>
		public object Convert(object value, Type targetType, object parameter, string language)
		{
			string inp = value as string;
			if (string.IsNullOrEmpty(inp))
				return string.Empty;
			int wrapLength = 80;
			if (parameter != null)
				int.TryParse(parameter.ToString(), out wrapLength);
			return inp.WordWrap(wrapLength);
		}

		/// <summary>
		/// Converts a value. 
		/// </summary>
		/// <param name="value">The value that is produced by the binding target.</param>
		/// <param name="targetType">The type to convert to.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="language">The language of the conversion.</param>
		/// <returns>A converted value. If the method returns null, the valid null value is used.</returns>
		public object ConvertBack(object value, Type targetType, object parameter, string language)
		{
			// Not implemented
			return DependencyProperty.UnsetValue;
		}
	}
	/// <summary>
	/// Helper class, used for text wrapping.
	/// Based on Tyler Mercier's TextHelper library:
	/// https://github.com/tylermercier/TextHelper/blob/master/TextHelper/WordWrapper.cs
	/// </summary>
	public static class Text
	{

		/// <summary>
		/// Truncates a given text after a given length.
		/// </summary>
		/// <param name="text">Text to be wrapped.</param>
		/// <param name="lineWidth">Length of truncated text (default is 80).</param>
		/// <returns>The wrapped text.</returns>
		public static string WordWrap(this string text, int lineWidth = 80)
		{
			if (text.Length <= lineWidth)
				return text;

			var pattern = string.Format(@"(.{{1,{0}}})(\s+|$)", lineWidth);

			var lines = new List<string>();
			foreach (var line in text.Split('\n'))
			{
				lines.Add(Regex.Replace(line.Trim(), pattern, "$1\n").Trim());
			}
			return string.Join("\n", lines);
		}
	}
}
