/*
 * (c) 2013-2015 Andreas Kuntner
 */
using System;

namespace MVVMbasics.Attributes
{
	/// <summary>
	/// Attribute to be applied to View classes. Allows the binding of a given Viewmodel type to this View class.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, 
		AllowMultiple = false, 
		Inherited = false)]
	public class MvvmNavigationTargetAttribute : Attribute
	{
		/// <summary>
		/// Type of the Viewmodel this View class should be bound to.
		/// </summary>
		private readonly Type _viewmodel;

		/// <summary>
		/// Path to the XAML file representing this View class. If not specified, the path will be retrieved from the
		/// namespace during viewmodel registration.
		/// </summary>
		private readonly string _path = null;

		/// <summary>
		/// Constructor that allows to specify Viewmodel. The path to the XAML file representing this View class will
		/// be retrieved from the View's namespace.
		/// </summary>
		/// <param name="viewmodel">Type of the Viewmodel this View class should be bound to (must be a subclass of 
		/// <see cref="MVVMbasics.Viewmodels.BaseViewmodel">BaseViewmodel</see>.</param>
		public MvvmNavigationTargetAttribute(Type viewmodel)
		{
			_viewmodel = viewmodel;
		}

		/// <summary>
		/// Constructor that allows to specify Viewmodel and path. This constructor should be used, if the path of the
		/// XAML file representing this View class does not correspond to the View's namespace.
		/// </summary>
		/// <param name="viewmodel">Type of the Viewmodel this View class should be bound to.</param>
		/// <param name="path">Path to the XAML file representing this View class.</param>
		public MvvmNavigationTargetAttribute(Type viewmodel, string path)
		{
			_viewmodel = viewmodel;
			_path = path;
		}

		/// <summary>
		/// Returns the type of the Viewmodel bound to this View class.
		/// </summary>
		/// <returns>Type of the Viewmodel bound to this View class.</returns>
		public Type GetViewmodel()
		{
			return _viewmodel;
		}

		/// <summary>
		/// Checks if a path to the XAML file representing this View class was specified.
		/// </summary>
		/// <returns>TRUE if a path was specified for this View class, FALSE otherwise</returns>
		public bool HasPath()
		{
			return (_path != null);
		}

		/// <summary>
		/// Returns the path to the XAML file representing this View class, if specified.
		/// </summary>
		/// <returns>Path to the XAML file representing this View class.</returns>
		public string GetPath()
		{
			return _path;
		}
	}
}
