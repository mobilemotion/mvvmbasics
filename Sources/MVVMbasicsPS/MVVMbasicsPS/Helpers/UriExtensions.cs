/*
 * (c) 2013-2015 Andreas Kuntner
 */

using System;
using MVVMbasics.Services;

namespace MVVMbasics.Helpers
{
    /// <summary>
    /// Helper class that contains extension functions to the <c>Uri</c> class which add parameter handling
    /// functionality.
    /// </summary>
	public static class UriExtensions
    {
        /// <summary>
        /// Retrieves all parameters from a given URI and returns them as 
        /// <see cref="MVVMbasics.Services.ParameterList">ParameterList</see>.
        /// </summary>
        /// <param name="uri">URI to be parsed.</param>
        /// <returns>List of all retrieved parameters.</returns>
		public static ParameterList RetrieveParameters(this Uri uri)
		{
			ParameterList result = new ParameterList();
			string paramStr = uri.ToString().Substring(uri.ToString().IndexOf('?') + 1);
			string[] allParams = paramStr.Split('&');
			foreach (string paramPair in allParams)
			{
				string[] keyValuePair = paramPair.Split('=');
				if (keyValuePair.Length == 2)
				{
					result.Add(new Parameter(Uri.UnescapeDataString(keyValuePair[0]), Uri.UnescapeDataString(keyValuePair[1])));
				}
			}
			return result;
		}
	}
}
