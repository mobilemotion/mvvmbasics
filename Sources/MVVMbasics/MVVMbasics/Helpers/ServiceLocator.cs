/*
 * (c) 2013-2018 Andreas Kuntner
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
	/// Service locator that allows services to be registered and retrieved. All services must implement the 
	/// <see cref="MVVMbasics.Services.IService">IService</see> interface in order to be detected by 
	/// <c>ServiceLocator</c>.
	/// </summary>
	public class ServiceLocator
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

		#region Contains Method

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

		#endregion

		#region Retrieve Methods

		/// <summary>
		/// Creates a service instance, stores and returns it, or throws an exception if no matching service has been
		/// registered.
		/// </summary>
		/// <typeparam name="T">Type of service to be retrieved (must be a subclass of 
		/// <see cref="MVVMbasics.Services.IService">IService</see>.</typeparam>
		/// <returns>Instance of the requested service.</returns>
		public T Retrieve<T>()
		{
			return Retrieve<T>(true);
		}

		/// <summary>
		/// Creates and returns a service instance without storing it, or throws an exception if no matching service has
		/// been registered.
		/// </summary>
		/// <typeparam name="T">Type of service to be retrieved (must be a subclass of 
		/// <see cref="MVVMbasics.Services.IService">IService</see>.</typeparam>
		/// <returns>Instance of the requested service.</returns>
		public T RetrieveOnce<T>()
		{
			return Retrieve<T>(false);
		}

		/// <summary>
		/// Creates a service instance, stores it, and returns TRUE, if a matching service has been registered. If no
		/// matching service is found, returns FALSE.
		/// </summary>
		/// <typeparam name="T">Type of service to be retrieved (must be a subclass of 
		/// <see cref="MVVMbasics.Services.IService">IService</see>.</typeparam>
		/// <param name="serviceInstance">Instance of the requested service.</param>
		/// <returns>TRUE if the requested service was found, FALSE otherwise.</returns>
		public bool TryRetrieve<T>(out T serviceInstance)
		{
			return TryRetrieve(out serviceInstance, true);
		}

		/// <summary>
		/// Creates a service instance and returns TRUE, if a matching service has been registered. If no matching
		/// service is found, returns FALSE.
		/// </summary>
		/// <typeparam name="T">Type of service to be retrieved</typeparam>
		/// <param name="serviceInstance">Instance of the requested service</param>
		/// <returns>TRUE if the requested service was found, FALSE otherwise</returns>
		public bool TryRetrieveOnce<T>(out T serviceInstance)
		{
			return TryRetrieve(out serviceInstance, false);
		}

		/// <summary>
		/// Creates a service instance, stores it if desired, and returns TRUE, if a matching service has been
		/// registered. If no matching service is found, returns FALSE.
		/// </summary>
		/// <typeparam name="T">Type of service to be retrieved (must be a subclass of 
		/// <see cref="MVVMbasics.Services.IService">IService</see>.</typeparam>
		/// <param name="serviceInstance">Instance of the requested service.</param>
		/// <param name="instantiateAndStore">Flag indicating whether the service's instance shall be stored or not.
		/// </param>
		/// <returns>TRUE if the requested service was found, FALSE otherwise.</returns>
		private bool TryRetrieve<T>(out T serviceInstance, bool instantiateAndStore)
		{
			if (Contains<T>())
			{
				serviceInstance = Retrieve<T>(instantiateAndStore);
				return true;
			}
			else
			{
				serviceInstance = default(T);
				return false;
			}
		}

		/// <summary>
		/// Creates a service instance and stores and returns it if desired. If no matching service has been registered,
		/// an exception is thrown.
		/// </summary>
		/// <typeparam name="T">Type of service to be retrieved (must be a subclass of 
		/// <see cref="MVVMbasics.Services.IService">IService</see>.</typeparam>
		/// <param name="instantiateAndStore">Flag indicating whether the service's instance shall be stored or not.
		/// </param>
		/// <returns>Instance of the requested service.</returns>
		private T Retrieve<T>(bool instantiateAndStore)
		{
			// Loop through all registered services that have already been instantiated to find the requested one
			foreach (var instance in _instances)
			{
				if (instance is T)
				{
					return (T)instance;
				}
			}

			// If not found, loop through all registered services that are not yet instantiated to find the requested 
			// type
			foreach (var type in _types)
			{
				if (typeof(T).GetTypeInfo().IsAssignableFrom(type))
				{
					// If a matching type is found, instantiate it and pass a reference to the ServiceLocator if
					// requested
					object instance = null;
					foreach (var constructor in typeof(T).GetTypeInfo().DeclaredConstructors)
					{
						if (constructor.GetParameters().Count() == 1)
						{
							if (constructor.GetParameters()[0].ParameterType == typeof(ServiceLocator))
							{
								instance = Activator.CreateInstance(type.AsType(), this);
							}
						}
					}
					if (instance == null)
						instance = (T)Activator.CreateInstance(type.AsType());

					// If desired, delete the type reference and store the instance
					if (instantiateAndStore)
					{
						_instances.Add((IService)instance);
						_types.Remove(type);
					}

					// Finally return the instance
					return (T) instance;
				}
			}

			// If no matching service is found, throw an exception
			throw new ServiceNotFoundException();
		}

		#endregion
	}
}
