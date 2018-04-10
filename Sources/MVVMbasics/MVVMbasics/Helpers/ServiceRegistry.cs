/*
 * (c) 2013-2016 Andreas Kuntner
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MVVMbasics.Attributes;
using MVVMbasics.Exceptions;
using MVVMbasics.Services;

namespace MVVMbasics.Helpers
{
	/// <summary>
	/// Simple IoC container that allows services to be registered and retrieved. All services must
	/// implement the <see cref="MVVMbasics.Services.IService">IService</see> interface.
	/// </summary>
	public class ServiceRegistry
	{
		#region Members

		/// <summary>
		/// Collection of all registered service instances.
		/// </summary>
		private readonly List<IService> _instances = new List<IService>();

		/// <summary>
		/// Collection of all registered service types that have not yet been registered.
		/// </summary>
		private readonly List<TypeInfo> _types = new List<TypeInfo>();

		#endregion

		#region Register Methods

		/// <summary>
		/// Registers an instance of a certain service.
		/// </summary>
		/// <param name="service">Service to be registered.</param>
		public void Register(IService service)
		{
			// If service of the same kind has already been registered,
			// throw an exception
			if (_instances.Any(i => i.GetType() == service.GetType())
				|| _types.Any(t => t.Equals(service.GetType().GetTypeInfo())))
			{
				throw new ServiceRegistrationException("Service of type '" + service.GetType().FullName + "' is already registered.");
			}
			// If not, register the given service
			_instances.Add(service);
		}

		/// <summary>
		/// Registers a reference to a certain service type, without instantly instantiating it.
		/// </summary>
		/// <typeparam name="T">Type of service to be registered (must be a subclass of 
		/// <see cref="MVVMbasics.Services.IService">IService</see>).</typeparam>
		public void Register<T>() where T : IService
		{
			// If service of the same kind has already been registered,
			// throw an exception
			if (_instances.Any(i => i.GetType() == typeof(T))
				|| _types.Any(t => t.Equals(typeof(T).GetTypeInfo())))
			{
				throw new ServiceRegistrationException("Service of type '" + typeof(T).FullName + "' is already registered.");
			}
			// If not, register the given service
			_types.Add(typeof(T).GetTypeInfo());
		}

		/// <summary>
		/// Traverses all services that are located in a given assembly and registers their types, without instantly
		/// instantiating them.
		/// </summary>
		/// <param name="assembly">The assembly to be scanned. If NULL or not specified, the calling  assembly will be 
		/// scanned.</param>
		public void RegisterAll(Assembly assembly = null)
		{
			RegisterAll(null, assembly);
		}

		/// <summary>
		/// Traverses all services that are located in a given namespace inside a given assembly and registers their 
		/// types, without instantly instantiating them.
		/// </summary>
		/// <param name="ns">Namespace to be scanned. If ends with '.*', also sub-directories will be scanned. If NULL,
		/// the whole assembly will be scanned.</param>
		/// <param name="assembly">The assembly to be scanned. If NULL, the calling assembly will be scanned.</param>
		public void RegisterAll(string ns, Assembly assembly = null)
		{
			// List all classes that fulfill the following criteria:
			// (1) extend IService
			// (2) are located in the given namespace
		    if (assembly == null)
		        assembly = Assembly.GetEntryAssembly();
			IEnumerable<TypeInfo> services;
			if (ns != null)
				services = from t in assembly.DefinedTypes
						   where
							   t.Namespace != null && t.IsClass && typeof(IService).GetTypeInfo().IsAssignableFrom(t) &&
							   t.GetCustomAttributes(typeof(MvvmServiceAttribute), true).Any() &&
							   (t.Namespace.Equals(ns) || (ns.EndsWith(".*") && t.Namespace.StartsWith(ns.Replace(".*", String.Empty))))
						   select t;
			else
				services = from t in assembly.DefinedTypes
						   where t.IsClass && typeof(IService).GetTypeInfo().IsAssignableFrom(t) &&
							t.GetCustomAttributes(typeof(MvvmServiceAttribute), true).Any()
						   select t;

			// Register all detected service types, if not already registered
			foreach (var service in services)
			{
				if (_instances.All(i => !i.GetType().GetTypeInfo().Equals(service))
					&& _types.All(t => !t.Equals(service)))
				{
					_types.Add(service);
				}
			}
		}

		#endregion

		#region Contains Methods

		/// <summary>
		/// Returns TRUE if a certain kind of service has been registered, FALSE otherwise.
		/// </summary>
		/// <typeparam name="T">Type of service to be retrieved (must be a subclass of 
		/// <see cref="MVVMbasics.Services.IService">IService</see>.</typeparam>
		/// <returns>TRUE if a certain kind of service has been registered, FALSE otherwise.</returns>
		public bool Contains<T>()
		{
			return (_instances.Any(i => i is T) || _types.Any(t => typeof(T).GetTypeInfo().IsAssignableFrom(t)));
		}

		/// <summary>
		/// Returns TRUE if a certain kind of service has been registered, FALSE otherwise.
		/// </summary>
		/// <param name="type">Type of service to be retrieved (must be a subclass of 
		/// <see cref="MVVMbasics.Services.IService">IService</see>).</param>
		/// <returns>TRUE if a certain kind of service has been registered, FALSE otherwise.</returns>
		public bool Contains(Type type)
		{
			return (_instances.Any(i => i.GetType() == type)
				|| _instances.Any(i => type.GetTypeInfo().IsAssignableFrom(i.GetType().GetTypeInfo()))
				|| _types.Any(t => type.GetTypeInfo().IsAssignableFrom(t)));
		}

		#endregion

		#region Resolve Methods

		/// <summary>
		/// Returns a matching service type if a service type is passed as parameter and a matching service has been
		/// registered. Otherwise, instantiates the given type be resolving all its constructor parameters, and
		/// returns the created instance.
		/// </summary>
		/// <param name="type">Type of service to be retrieved (must be a subclass of 
		/// <see cref="MVVMbasics.Services.IService">IService</see>), or of the class to instantiate.</param>
		/// <returns>Instance of the requested service or class.</returns>
		public object Resolve(Type type)
		{
			if (Contains(type))
				return Retrieve(type);
			else
				return Instantiate(type);
		}

		/// <summary>
		/// Returns a matching service type if a service type is passed as parameter and a matching service has been
		/// registered. Otherwise, instantiates the given type be resolving all its constructor parameters, and
		/// returns the created instance.
		/// </summary>
		/// <typeparam name="T">Type of service to be retrieved (must be a subclass of 
		/// <see cref="MVVMbasics.Services.IService">IService</see>), or of the class to instantiate.</typeparam>
		/// <returns>Instance of the requested service or class.</returns>
		public T Resolve<T>()
		{
			return (T)Resolve(typeof(T));
		}

		#endregion

		#region Helper Methods

		/// <summary>
		/// Creates a service instance, stores it, and returns it if desired. If no matching service has been
		/// registered, an exception is thrown.
		/// </summary>
		/// <param name="type">Type of service to be retrieved (must be a subclass of 
		/// <see cref="MVVMbasics.Services.IService">IService</see>.</param>
		/// <returns>Instance of the requested service.</returns>
		private object Retrieve(Type type)
		{
			// Loop through all registered services that have already been instantiated to find the requested one
			foreach (var instance in _instances)
			{
				if (instance.GetType() == type || type.GetTypeInfo().IsAssignableFrom(instance.GetType().GetTypeInfo()))
				{
					return instance;
				}
			}

			// If not found, loop through all registered services that are not yet instantiated to find the requested 
			// type
			foreach (var t in _types)
			{
				if (type.GetTypeInfo().IsAssignableFrom(t))
				{
					// If a matching type is found, instantiate it and pass a reference to the ServiceLocator if
					// requested
					var instance = Instantiate(t.AsType());

					// Delete the type reference and store the instance
					_instances.Add((IService)instance);
					_types.Remove(t);

					// Finally return the instance
					return instance;
				}
			}

			// If no matching service is found, try to instantiate the passed type
			return Instantiate(type);
		}

		/// <summary>
		/// Creates an instance of a given type by resolving and instantiating all its parameter constructors and
		/// calling the constructor to create an instance.
		/// </summary>
		/// <param name="type">Type of the class to be instantiated.</param>
		/// <returns>Instance of the requested class.</returns>
		private object Instantiate(Type type)
		{
			if (type.GetTypeInfo().DeclaredConstructors.Any(c => !c.GetParameters().Any()))
			{
				// If the type has a parameterless constructor, instantiate it directly...
				return Activator.CreateInstance(type);
			}
			else
			{
				// ...otherwise, we take the first available constructor and try to retrieve and instantiate each
				// of its parameters
				ConstructorInfo ctor = type.GetTypeInfo().DeclaredConstructors.FirstOrDefault();
				if (ctor != null)
				{
					var parameters = ctor.GetParameters().Select(parameter => Retrieve(parameter.ParameterType)).ToArray();
					var instance = Activator.CreateInstance(type, parameters);
					return instance;
				}
			}

			// If the type could not be instantiated, throw an exception
			throw new ServiceResolutionException();
		}

		#endregion
	}
}