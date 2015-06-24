/*
 * (c) 2013-2015 Andreas Kuntner
 */
using System;
using System.Collections.Generic;
using System.Reflection;
using MVVMbasics.Viewmodels;

namespace MVVMbasics.Services
{
	/// <summary>
	/// Interface specifying all methods for a service which provides page navigation functionality. Includes both 
	/// "standard" navigation methods which navigate backwards, as well as a View-/Viewmodel-Locator that stores 
	/// mappings of View to Viewmodel and allows to directly navigate to a View that is associated to a given Viewmodel.
	/// </summary>
	public interface INavigatorService : IService
	{
		void Register<T>(object view) where T : BaseViewmodel;
		void RegisterAll(Assembly assembly = null);
		void RegisterAll(string ns, Assembly assembly = null);
		object Retrieve<T>() where T : BaseViewmodel;
		void NavigateTo<T>(params Parameter[] parameters) where T : BaseViewmodel;
		void NavigateTo<T>(string key, object value) where T : BaseViewmodel;
		void NavigateTo<T>(ParameterList parameters) where T : BaseViewmodel;
		void NavigateBack();
		void SetBackParameters(params Parameter[] parameters);
		void SetBackParameters(string key, object value);
		void SetBackParameters(ParameterList parameters);
		void ClearBackParameters();
		bool CanGoBack();
		void RemoveBackEntry();
		void ClearBackStack();
	}

	/// <summary>
	/// Specifies the lifecycle states a View can reach:
	/// <list type="bullet">
	///		<item>
	///			<term><c>Activated</c></term>
	///			<description>View is loaded for the first time</description>
	///		</item>
	///		<item>
	///			<term><c>Reactivated</c></term>
	///			<description>View has been inactive and is reactivated</description>
	///		</item>
	///		<item>
	///			<term><c>Deactivated</c></term>
	///			<description>Another View is loaded, the current one is not unloaded but remains in the background
	///			</description>
	///		</item>
	///		<item>
	///			<term><c>Hibernated</c></term>
	///			<description>The whole App is hibernated (occurs only on Phone and Tablet platforms)</description>
	///		</item>
	///		<item>
	///			<term><c>Awakened</c></term>
	///			<description>The whole App returns from tombstoning (occurs only on Phone and Tablet platforms)</description>
	///		</item>
	///		<item>
	///			<term><c>Closed</c></term>
	///			<description>View is completely unloaded and removed from the backstack</description>
	///		</item>
	/// </list>
	/// </summary>
	public enum ViewState
	{
		Activated,   // View is loaded for the first time
		Reactivated, // View has been inactive and is reactivated
		Deactivated, // Another View is loaded, the current one is not unloaded but remains in the background
		Hidden,      // The whole App is moved to the background (e.g., because another App becomes active on Phone and
		             // Tablet platforms, or because the App loses focus on the Desktop platform)
		Shown,       // The whole App is reactivated from the background (on Phone and Tablet platforms) or receives back
		             // focus (on the Desktop platform)
		Hibernated,  // The whole App is hibernated (occurs only on Phone and Tablet platforms)
		Awakened,    // The whole App returns from tombstoning (occurs only on Phone and Tablet platforms)
		Closed,      // View is completely unloaded and removed from the backstack
	}

	/// <summary>
	/// Helper class that represents a parameter to be passed to a view during navigation.
	/// </summary>
	public class Parameter
	{
		#region Members

		/// <summary>
		/// The parameter's unique key
		/// </summary>
		public string Key { get; set; }
		/// <summary>
		/// The parameter#s content value
		/// </summary>
		public object Value { get; set; }

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor for use with parameter value of type <c>object</c>.
		/// </summary>
		/// <param name="key">Unique key</param>
		/// <param name="value">Content value</param>
		public Parameter(string key, object value)
		{
			Key = key;
			Value = value;
		}

		/// <summary>
		/// Constructor for use with parameter value of type <c>string</c>.
		/// </summary>
		/// <param name="key">Unique key</param>
		/// <param name="value">Content value</param>
		public Parameter(string key, string value)
		{
			Key = key;
			Value = value;
		}

		/// <summary>
		/// Constructor for use with parameter value of type <c>int</c>.
		/// </summary>
		/// <param name="key">Unique key</param>
		/// <param name="value">Content value</param>
		public Parameter(string key, int value)
		{
			Key = key;
			Value = value;
		}

		/// <summary>
		/// Constructor for use with parameter value of type <c>uint</c>.
		/// </summary>
		/// <param name="key">Unique key</param>
		/// <param name="value">Content value</param>
		public Parameter(string key, uint value)
		{
			Key = key;
			Value = value;
		}

		/// <summary>
		/// Constructor for use with parameter value of type <c>double</c>.
		/// </summary>
		/// <param name="key">Unique key</param>
		/// <param name="value">Content value</param>
		public Parameter(string key, double value)
		{
			Key = key;
			Value = value;
		}

		/// <summary>
		/// Constructor for use with parameter value of type <c>float</c>.
		/// </summary>
		/// <param name="key">Unique key</param>
		/// <param name="value">Content value</param>
		public Parameter(string key, float value)
		{
			Key = key;
			Value = value;
		}

		/// <summary>
		/// Constructor for use with parameter value of type <c>long</c>.
		/// </summary>
		/// <param name="key">Unique key</param>
		/// <param name="value">Content value</param>
		public Parameter(string key, long value)
		{
			Key = key;
			Value = value;
		}

		/// <summary>
		/// Constructor for use with parameter value of type <c>short</c>.
		/// </summary>
		/// <param name="key">Unique key</param>
		/// <param name="value">Content value</param>
		public Parameter(string key, short value)
		{
			Key = key;
			Value = value;
		}

		/// <summary>
		/// Constructor for use with parameter value of type <c>ulong</c>.
		/// </summary>
		/// <param name="key">Unique key</param>
		/// <param name="value">Content value</param>
		public Parameter(string key, ulong value)
		{
			Key = key;
			Value = value;
		}

		/// <summary>
		/// Constructor for use with parameter value of type <c>ushort</c>.
		/// </summary>
		/// <param name="key">Unique key</param>
		/// <param name="value">Content value</param>
		public Parameter(string key, ushort value)
		{
			Key = key;
			Value = value;
		}

		/// <summary>
		/// Constructor for use with parameter value of type <c>byte</c>.
		/// </summary>
		/// <param name="key">Unique key</param>
		/// <param name="value">Content value</param>
		public Parameter(string key, byte value)
		{
			Key = key;
			Value = value;
		}

		/// <summary>
		/// Constructor for use with parameter value of type <c>sbyte</c>.
		/// </summary>
		/// <param name="key">Unique key</param>
		/// <param name="value">Content value</param>
		public Parameter(string key, sbyte value)
		{
			Key = key;
			Value = value;
		}

		/// <summary>
		/// Constructor for use with parameter value of type <c>bool</c>.
		/// </summary>
		/// <param name="key">Unique key</param>
		/// <param name="value">Content value</param>
		public Parameter(string key, bool value)
		{
			Key = key;
			Value = value;
		}

		/// <summary>
		/// Constructor for use with parameter value of type <c>char</c>.
		/// </summary>
		/// <param name="key">Unique key</param>
		/// <param name="value">Content value</param>
		public Parameter(string key, char value)
		{
			Key = key;
			Value = value;
		}

		/// <summary>
		/// Constructor for use with parameter value of type <c>DateTime</c>.
		/// </summary>
		/// <param name="key">Unique key</param>
		/// <param name="value">Content value</param>
		public Parameter(string key, DateTime value)
		{
			Key = key;
			Value = value;
		}

		#endregion
	}

	/// <summary>
	/// Helper class that holds several <see cref="MVVMbasics.Services.Parameter">Parameters</see> which have been 
	/// passed to a View during navigation.
	/// </summary>
	public class ParameterList : Dictionary<string, object>
	{
		#region Constructors

		/// <summary>
		/// Empty Constructor.
		/// </summary>
		public ParameterList()
		{
		}

		/// <summary>
		/// Constructor that expects one or several <see cref="MVVMbasics.Services.Parameter">Parameter</see> objects
		/// and creates a <c>ParameterList</c> of those.
		/// </summary>
		/// <param name="parameters">One or several <see cref="MVVMbasics.Services.Parameter">Parameter</see> objects
		/// to be contained in the <c>ParameterList</c>.</param>
		public ParameterList(params Parameter[] parameters)
		{
			this.Add(parameters);
		}

		#endregion

		#region Helper methods

		/// <summary>
		/// Checks whether a <see cref="MVVMbasics.Services.Parameter">Parameter</see> with specified key exists within
		/// this <c>ParameterList</c>.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be searched.
		/// </param>
		/// <returns>TRUE if a <see cref="MVVMbasics.Services.Parameter">Parameter</see> with the given key exists,
		/// FALSE otherwise.</returns>
		public bool Contains(string key)
		{
			return this.ContainsKey(key);
		}

		//public void Remove(string key)
		// needs not be implemented since it is available in the base Dictionary class

		#endregion

		#region Get methods

		/// <summary>
		/// Getter for <see cref="MVVMbasics.Services.Parameter">Parameter</see> values of type <c>object</c>.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </param>
		/// <returns>Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if specified.
		/// </returns>
		[Obsolete(@"Use Get<T>(string key) instead")]
		public object GetObject(string key)
		{
			return this[key];
		}

		/// <summary>
		/// Getter for <see cref="MVVMbasics.Services.Parameter">Parameter</see> values of custom types.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </param>
		/// <typeparam name="T">Type of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </typeparam>
		/// <returns>Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if specified.
		/// </returns>
		public T Get<T>(string key)
		{
			object resultObj = GetObject(key);
			T result = default(T);
			try
			{
				result = (T)resultObj;
			}
			catch (InvalidCastException)
			{
				// Do nothing - just return the type's default value
			}
			return result;
		}

		/// <summary>
		/// Getter for <see cref="MVVMbasics.Services.Parameter">Parameter</see> values of type <c>object</c> that
		/// checks whether a parameter with the specified key exists.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </param>
		/// <param name="result">Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if
		/// specified.</param>
		/// <returns>TRUE if a <see cref="MVVMbasics.Services.Parameter">Parameter</see> with the given key has been
		/// specified, FALSE otherwise.</returns>
		[Obsolete(@"Use TryGet<T>(string key, out T result) instead")]
		public bool TryGetObject(string key, out object result)
		{
			object val;
			if (this.TryGetValue(key, out val))
			{
				result = val;
				return true;
			}
			result = null;
			return false;
		}

		/// <summary>
		/// Getter for <see cref="MVVMbasics.Services.Parameter">Parameter</see> values of custom types that checks
		/// whether a parameter with the specified key exists.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </param>
		/// <param name="result">Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if
		/// specified.</param>
		/// <typeparam name="T">Type of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </typeparam>
		/// <returns>Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if specified.
		/// </returns>
		public bool TryGet<T>(string key, out T result)
		{
			result = default(T);
			object val;
			if (!this.TryGetValue(key, out val))
			{
				return false;
			}
			try
			{
				result = (T)val;
				return true;
			}
			catch (InvalidCastException)
			{
				return false;
			}
		}

		/// <summary>
		/// Getter for <see cref="MVVMbasics.Services.Parameter">Parameter</see> values of type <c>string</c>.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </param>
		/// <returns>Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if specified.
		/// </returns>
		public string GetString(string key)
		{
			return this[key].ToString();
		}

		/// <summary>
		/// Getter for <see cref="MVVMbasics.Services.Parameter">Parameter</see> values of type <c>string</c> that
		/// checks whether a parameter with the specified key exists.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </param>
		/// <param name="result">Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if
		/// specified.</param>
		/// <returns>TRUE if a <see cref="MVVMbasics.Services.Parameter">Parameter</see> with the given key has been
		/// specified, FALSE otherwise.</returns>
		public bool TryGetString(string key, out string result)
		{
			object val;
			if (this.TryGetValue(key, out val))
			{
				result = val.ToString();
				return true;
			}
			result = String.Empty;
			return false;
		}

		/// <summary>
		/// Getter for <see cref="MVVMbasics.Services.Parameter">Parameter</see> values of type <c>int</c>.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </param>
		/// <returns>Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if specified.
		/// </returns>
		public int GetInt(string key)
		{
			return Convert.ToInt32(this[key]);
		}

		/// <summary>
		/// Getter for <see cref="MVVMbasics.Services.Parameter">Parameter</see> values of type <c>int</c> that
		/// checks whether a parameter with the specified key exists.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </param>
		/// <param name="result">Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if
		/// specified.</param>
		/// <returns>TRUE if a <see cref="MVVMbasics.Services.Parameter">Parameter</see> with the given key has been
		/// specified, FALSE otherwise.</returns>
		public bool TryGetInt(string key, out int result)
		{
			object val;
			if (this.TryGetValue(key, out val))
			{
				try
				{
					result = Convert.ToInt32(val);
					return true;
				}
				catch (Exception)
				{
					result = 0;
					return false;
				}
			}
			result = 0;
			return false;
		}

		/// <summary>
		/// Getter for <see cref="MVVMbasics.Services.Parameter">Parameter</see> values of type <c>uint</c>.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </param>
		/// <returns>Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if specified.
		/// </returns>
		public uint GetUInt(string key)
		{
			return Convert.ToUInt32(this[key]);
		}

		/// <summary>
		/// Getter for <see cref="MVVMbasics.Services.Parameter">Parameter</see> values of type <c>uint</c> that
		/// checks whether a parameter with the specified key exists.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </param>
		/// <param name="result">Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if
		/// specified.</param>
		/// <returns>TRUE if a <see cref="MVVMbasics.Services.Parameter">Parameter</see> with the given key has been
		/// specified, FALSE otherwise.</returns>
		public bool TryGetUInt(string key, out uint result)
		{
			object val;
			if (this.TryGetValue(key, out val))
			{
				try
				{
					result = Convert.ToUInt32(val);
					return true;
				}
				catch (Exception)
				{
					result = 0;
					return false;
				}
			}
			result = 0;
			return false;
		}

		/// <summary>
		/// Getter for <see cref="MVVMbasics.Services.Parameter">Parameter</see> values of type <c>double</c>.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </param>
		/// <returns>Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if specified.
		/// </returns>
		public double GetDouble(string key)
		{
			return Convert.ToInt32(this[key]);
		}

		/// <summary>
		/// Getter for <see cref="MVVMbasics.Services.Parameter">Parameter</see> values of type <c>double</c> that
		/// checks whether a parameter with the specified key exists.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </param>
		/// <param name="result">Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if
		/// specified.</param>
		/// <returns>TRUE if a <see cref="MVVMbasics.Services.Parameter">Parameter</see> with the given key has been
		/// specified, FALSE otherwise.</returns>
		public bool TryGetDouble(string key, out double result)
		{
			object val;
			if (this.TryGetValue(key, out val))
			{
				try
				{
					result = Convert.ToDouble(val);
					return true;
				}
				catch (Exception)
				{
					result = 0;
					return false;
				}
			}
			result = 0;
			return false;
		}

		/// <summary>
		/// Getter for <see cref="MVVMbasics.Services.Parameter">Parameter</see> values of type <c>long</c>.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </param>
		/// <returns>Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if specified.
		/// </returns>
		public long GetLong(string key)
		{
			return Convert.ToInt64(this[key]);
		}

		/// <summary>
		/// Getter for <see cref="MVVMbasics.Services.Parameter">Parameter</see> values of type <c>long</c> that
		/// checks whether a parameter with the specified key exists.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </param>
		/// <param name="result">Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if
		/// specified.</param>
		/// <returns>TRUE if a <see cref="MVVMbasics.Services.Parameter">Parameter</see> with the given key has been
		/// specified, FALSE otherwise.</returns>
		public bool TryGetLong(string key, out long result)
		{
			object val;
			if (this.TryGetValue(key, out val))
			{
				try
				{
					result = Convert.ToInt64(val);
					return true;
				}
				catch (Exception)
				{
					result = 0;
					return false;
				}
			}
			result = 0;
			return false;
		}

		/// <summary>
		/// Getter for <see cref="MVVMbasics.Services.Parameter">Parameter</see> values of type <c>short</c>.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </param>
		/// <returns>Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if specified.
		/// </returns>
		public short GetShort(string key)
		{
			return Convert.ToInt16(this[key]);
		}

		/// <summary>
		/// Getter for <see cref="MVVMbasics.Services.Parameter">Parameter</see> values of type <c>short</c> that
		/// checks whether a parameter with the specified key exists.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </param>
		/// <param name="result">Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if
		/// specified.</param>
		/// <returns>TRUE if a <see cref="MVVMbasics.Services.Parameter">Parameter</see> with the given key has been
		/// specified, FALSE otherwise.</returns>
		public bool TryGetShort(string key, out short result)
		{
			object val;
			if (this.TryGetValue(key, out val))
			{
				try
				{
					result = Convert.ToInt16(val);
					return true;
				}
				catch (Exception)
				{
					result = 0;
					return false;
				}
			}
			result = 0;
			return false;
		}

		/// <summary>
		/// Getter for <see cref="MVVMbasics.Services.Parameter">Parameter</see> values of type <c>ulong</c>.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </param>
		/// <returns>Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if specified.
		/// </returns>
		public ulong GetULong(string key)
		{
			return Convert.ToUInt64(this[key]);
		}

		/// <summary>
		/// Getter for <see cref="MVVMbasics.Services.Parameter">Parameter</see> values of type <c>ulong</c> that
		/// checks whether a parameter with the specified key exists.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </param>
		/// <param name="result">Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if
		/// specified.</param>
		/// <returns>TRUE if a <see cref="MVVMbasics.Services.Parameter">Parameter</see> with the given key has been
		/// specified, FALSE otherwise.</returns>
		public bool TryGetULong(string key, out ulong result)
		{
			object val;
			if (this.TryGetValue(key, out val))
			{
				try
				{
					result = Convert.ToUInt64(val);
					return true;
				}
				catch (Exception)
				{
					result = 0;
					return false;
				}
			}
			result = 0;
			return false;
		}

		/// <summary>
		/// Getter for <see cref="MVVMbasics.Services.Parameter">Parameter</see> values of type <c>ushort</c>.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </param>
		/// <returns>Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if specified.
		/// </returns>
		public ushort GetUShort(string key)
		{
			return Convert.ToUInt16(this[key]);
		}

		/// <summary>
		/// Getter for <see cref="MVVMbasics.Services.Parameter">Parameter</see> values of type <c>ushort</c> that
		/// checks whether a parameter with the specified key exists.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </param>
		/// <param name="result">Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if
		/// specified.</param>
		/// <returns>TRUE if a <see cref="MVVMbasics.Services.Parameter">Parameter</see> with the given key has been
		/// specified, FALSE otherwise.</returns>
		public bool TryGetUShort(string key, out ushort result)
		{
			object val;
			if (this.TryGetValue(key, out val))
			{
				try
				{
					result = Convert.ToUInt16(val);
					return true;
				}
				catch (Exception)
				{
					result = 0;
					return false;
				}
			}
			result = 0;
			return false;
		}

		/// <summary>
		/// Getter for <see cref="MVVMbasics.Services.Parameter">Parameter</see> values of type <c>byte</c>.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </param>
		/// <returns>Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if specified.
		/// </returns>
		public byte GetByte(string key)
		{
			return Convert.ToByte(this[key]);
		}

		/// <summary>
		/// Getter for <see cref="MVVMbasics.Services.Parameter">Parameter</see> values of type <c>byte</c> that
		/// checks whether a parameter with the specified key exists.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </param>
		/// <param name="result">Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if
		/// specified.</param>
		/// <returns>TRUE if a <see cref="MVVMbasics.Services.Parameter">Parameter</see> with the given key has been
		/// specified, FALSE otherwise.</returns>
		public bool TryGetByte(string key, out byte result)
		{
			object val;
			if (this.TryGetValue(key, out val))
			{
				try
				{
					result = Convert.ToByte(val);
					return true;
				}
				catch (Exception)
				{
					result = 0;
					return false;
				}
			}
			result = 0;
			return false;
		}

		/// <summary>
		/// Getter for <see cref="MVVMbasics.Services.Parameter">Parameter</see> values of type <c>sbyte</c>.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </param>
		/// <returns>Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if specified.
		/// </returns>
		public sbyte GetSByte(string key)
		{
			return Convert.ToSByte(this[key]);
		}

		/// <summary>
		/// Getter for <see cref="MVVMbasics.Services.Parameter">Parameter</see> values of type <c>sbyte</c> that
		/// checks whether a parameter with the specified key exists.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </param>
		/// <param name="result">Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if
		/// specified.</param>
		/// <returns>TRUE if a <see cref="MVVMbasics.Services.Parameter">Parameter</see> with the given key has been
		/// specified, FALSE otherwise.</returns>
		public bool TryGetSByte(string key, out sbyte result)
		{
			object val;
			if (this.TryGetValue(key, out val))
			{
				try
				{
					result = Convert.ToSByte(val);
					return true;
				}
				catch (Exception)
				{
					result = 0;
					return false;
				}
			}
			result = 0;
			return false;
		}

		/// <summary>
		/// Getter for <see cref="MVVMbasics.Services.Parameter">Parameter</see> values of type <c>bool</c>.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </param>
		/// <returns>Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if specified.
		/// </returns>
		public bool GetBoolean(string key)
		{
			return Convert.ToBoolean(this[key]);
		}

		/// <summary>
		/// Getter for <see cref="MVVMbasics.Services.Parameter">Parameter</see> values of type <c>bool</c> that
		/// checks whether a parameter with the specified key exists.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </param>
		/// <param name="result">Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if
		/// specified.</param>
		/// <returns>TRUE if a <see cref="MVVMbasics.Services.Parameter">Parameter</see> with the given key has been
		/// specified, FALSE otherwise.</returns>
		public bool TryGetBoolean(string key, out bool result)
		{
			object val;
			if (this.TryGetValue(key, out val))
			{
				try
				{
					result = Convert.ToBoolean(val);
					return true;
				}
				catch (Exception)
				{
					result = false;
					return false;
				}
			}
			result = false;
			return false;
		}

		/// <summary>
		/// Getter for <see cref="MVVMbasics.Services.Parameter">Parameter</see> values of type <c>char</c>.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </param>
		/// <returns>Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if specified.
		/// </returns>
		public char GetChar(string key)
		{
			return Convert.ToChar(this[key]);
		}

		/// <summary>
		/// Getter for <see cref="MVVMbasics.Services.Parameter">Parameter</see> values of type <c>char</c> that
		/// checks whether a parameter with the specified key exists.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </param>
		/// <param name="result">Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if
		/// specified.</param>
		/// <returns>TRUE if a <see cref="MVVMbasics.Services.Parameter">Parameter</see> with the given key has been
		/// specified, FALSE otherwise.</returns>
		public bool TryGetChar(string key, out char result)
		{
			object val;
			if (this.TryGetValue(key, out val))
			{
				try
				{
					result = Convert.ToChar(val);
					return true;
				}
				catch (Exception)
				{
					result = '\0';
					return false;
				}
			}
			result = '\0';
			return false;
		}

		/// <summary>
		/// Getter for <see cref="MVVMbasics.Services.Parameter">Parameter</see> values of type <c>DateTime</c>.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </param>
		/// <returns>Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if specified.
		/// </returns>
		public DateTime GetDateTime(string key)
		{
			return Convert.ToDateTime(this[key]);
		}

		/// <summary>
		/// Getter for <see cref="MVVMbasics.Services.Parameter">Parameter</see> values of type <c>DateTime</c> that
		/// checks whether a parameter with the specified key exists.
		/// </summary>
		/// <param name="key">Key of the <see cref="MVVMbasics.Services.Parameter">Parameter</see> to be retrieved.
		/// </param>
		/// <param name="result">Value of the desired <see cref="MVVMbasics.Services.Parameter">Parameter</see>, if
		/// specified.</param>
		/// <returns>TRUE if a <see cref="MVVMbasics.Services.Parameter">Parameter</see> with the given key has been
		/// specified, FALSE otherwise.</returns>
		public bool TryGetDateTime(string key, out DateTime result)
		{
			object val;
			if (this.TryGetValue(key, out val))
			{
				try
				{
					result = Convert.ToDateTime(val);
					return true;
				}
				catch (Exception)
				{
					result = DateTime.Now;
					return false;
				}
			}
			result = DateTime.Now;
			return false;
		}

		#endregion

		#region Add methods

		/// <summary>
		/// Adds one or several <see cref="MVVMbasics.Services.Parameter">Parameter</see> objects to this 
		/// <c>ParameterList</c>.
		/// </summary>
		/// <param name="parameters"><see cref="MVVMbasics.Services.Parameter">Parameter</see> objects to be added to
		/// the <c>ParameterList</c>.</param>
		public void Add(params Parameter[] parameters)
		{
			foreach (var param in parameters)
			{
				this.Add(param.Key, param.Value);
			}
		}

		//public void Add(string key, object value)
		//public void Add(string key, string value)
		//public void Add(string key, int value)
		//...
		// need not be implemented since Add(string key, object value) in the base Dictionary
		// class can be used

		#endregion
	}
}