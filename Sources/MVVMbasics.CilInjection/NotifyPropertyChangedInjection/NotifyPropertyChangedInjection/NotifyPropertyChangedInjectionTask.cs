/*
 * (c) 2015-2018 Andreas Kuntner
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace MVVMbasics.CilInjection.NotifyPropertyChangedInjection
{
	public class NotifyPropertyChangedInjectionTask : Task
	{
		[Required]
		public string AssemblyPath { get; set; }

		public override bool Execute()
		{
			// For the decompiler to access MVVMbasics, we need to copy it to the output dir
			var toolsDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			var assemblyDir = Path.GetDirectoryName(AssemblyPath);
			var libName = "MVVMbasics.dll";
			var libToolsPath = Path.Combine(toolsDir, libName);
			var libAssemblyPath = Path.Combine(assemblyDir, libName);
			var libExists = File.Exists(libAssemblyPath);
			if (!libExists)
				File.Copy(libToolsPath, libAssemblyPath);

			// Load assembly
			ModuleDefinition module;
			try
			{
				var resolver = new DefaultAssemblyResolver();
				resolver.AddSearchDirectory(assemblyDir);
				module = ModuleDefinition.ReadModule(AssemblyPath, new ReaderParameters()
				{
					AssemblyResolver = resolver,
					ReadSymbols = true,
					ReadWrite = true
				});
			}
			catch (Exception e)
			{
				Log.LogError(Resources.Resources.Error_LoadAssembly, AssemblyPath, e.Message);
				if (!libExists)
					File.Delete(libAssemblyPath);
				return false;
			}

			bool hasChanged = false;

			foreach (var type in module.Types)
			{
				// Find all classes that inherit from MVVMbasic's BaseModel or BaseViewmodel
				if (InheritsFromBaseType(type, "MVVMbasics.Viewmodels.BaseViewmodel", "MVVMbasics.Models.BaseModel"))
				{
					// Find the NotifyPropertyChanged method within this class
					var notifyMethod = FindNotifyMethod(module, type);

					if (notifyMethod != null)
					{
						// Find methods Object.Equals() and Object.ReferenceEquals()
						var equalsMethod = FindEqualsMethod(module, true);
						var referenceEqualsMethod = FindEqualsMethod(module, false);

						if (equalsMethod != null)
						{
							// This collection will hold all properties that depend on each other
							var dependencies = new Dictionary<string, List<string>>();

							// In the first run, analyze all property's getter methods for other properties they depend on
							foreach (var property in type.Properties.Where(p => p.GetMethod != null))
							{
								// Only analyze properties that are marked bindable
								if (HasBindableAttributes(property, type))
								{
									RetrieveDependenciesFromProperty(property, ref dependencies);
								}
							}

							// In the second run, find all properties that also contain setters and that
							// are marked with the BindableProperty attribute
							foreach (var property in type.Properties.Where(p => p.SetMethod != null))
							{
								if (HasBindableAttributes(property, type))
								{
									hasChanged = ProcessProperty(property, equalsMethod, referenceEqualsMethod, notifyMethod, dependencies);
								}
							}
						}
					}
				}
			}
			var result = true;

			// If any operations have been injected, write them to the assembly
			if (hasChanged)
				result = WriteAssembly(module);

			module.Dispose();

			try
			{
				if (!libExists)
					File.Delete(libAssemblyPath);
			}
			catch (IOException)
			{
				// File might be locked, doesn't matter
			}

			return result;
		}

		/// <summary>
		/// Processes one property by analyzing its getter method and storing all properties it depends on in a collection.
		/// </summary>
		/// <param name="property">Property to be processed</param>
		/// <param name="dependencies">Collection of found dependencies</param>
		private void RetrieveDependenciesFromProperty(PropertyDefinition property, 
			ref Dictionary<string, List<string>> dependencies)
		{
			// Extract the current property's name
			string propertyName = property.Name;

			// Loop through the current property's getter method's operations, and remember all calls
			// to other properties
			var ins = property.GetMethod.Body.Instructions;
			foreach (var instruction in ins)
			{
				// If the current property depends on any other property, it calls the other property's
				// getter. Therefore, analyze all Call operations...
				if (instruction.OpCode.Code == Code.Call || instruction.OpCode.Code == Code.Callvirt)
				{
					var method = instruction.Operand as MethodReference;
					if (method != null)
					{
						// ...and find those that actually call a getter method
						if (method.Name.StartsWith("get_"))
						{
							// Store the current property's name and the name of the property it depends
							// on in the global collection
							var dependsOn = method.Name.Substring(4);

							Log.LogMessage(Resources.Resources.Message_PropertyDependsOn, GetReadableName(property), dependsOn);

							if (dependencies.ContainsKey(dependsOn))
								dependencies[dependsOn].Add(propertyName);
							else
								dependencies.Add(dependsOn, new List<string> {propertyName});
						}
					}
				}
			}
		}

		/// <summary>
		/// Processes one property by injecting equality check and <code>NotifyPropertyChanged</code> invocations into its
		/// setter method.
		/// </summary>
		/// <param name="property">Property to be processed</param>
		/// <param name="equalsMethod">Reference to the <code>System.Object.Equals(Object a, Object b)</code> method</param>
		/// <param name="referenceEqualsMethod">Reference to the <code>System.Object.ReferenceEquals(Object a, Object b)</code> method</param>
		/// <param name="notifyMethod">Reference to the <code>NotifyPropertyChanged</code> method</param>
		/// <param name="dependencies">Collection of other properties the current property depends on</param>
		/// <returns>TRUE if the property's setter method's instructions have been changed, FALSE otherwise</returns>
		private bool ProcessProperty(PropertyDefinition property, MethodReference equalsMethod, MethodReference referenceEqualsMethod,
			MethodReference notifyMethod, Dictionary<string, List<string>> dependencies)
		{
			Log.LogMessage(Resources.Resources.Message_PropertyProcessing, GetReadableName(property));

			bool hasChanged = false;
			// Extract the current property's name
			string propertyName = property.Name;
			// Extract the current property's setter method's contents
			var processor = property.SetMethod.Body.GetILProcessor();
			var instructions = property.SetMethod.Body.Instructions;
			// Find the last instruction in the current property's setter method (the return call)
			var returnInstruction = instructions.LastOrDefault(i => i.OpCode.Code == Code.Ret);
			// Find the backing field's definition
			var setInstruction = instructions.FirstOrDefault(i => i.OpCode.Code == Code.Stfld);
			if (setInstruction != null)
			{
				var backingField = setInstruction.Operand as FieldReference;
				if (backingField != null)
				{
					// Inject an equality check before any other operation in the setter method
					var eqMethod = property.PropertyType.IsValueType ? equalsMethod : referenceEqualsMethod;
					if (InjectEqualsInstructions(ref processor, property, instructions.First(), backingField, equalsMethod))
						hasChanged = true;
				}
			}
			// Inject the RaisePropertyChanged call
			if (InjectNotifyInstructions(ref processor, returnInstruction, notifyMethod, propertyName, GetReadableName(property)))
				hasChanged = true;

			// If other properties depend on this one, also raise PropertyChanged for them
			if (dependencies.ContainsKey(propertyName))
			{
				foreach (var dependentProperty in dependencies[propertyName])
				{
					Log.LogMessage(Resources.Resources.Message_DependentPropertyProcessing, GetReadableName(property), dependentProperty);

					InjectNotifyInstructions(ref processor, returnInstruction, notifyMethod, dependentProperty, GetReadableName(property));
				}
			}
			return hasChanged;
		}

		/// <summary>
		/// Checks if a given type is derived from a certain base type.
		/// </summary>
		/// <param name="type">Type to be checked</param>
		/// <param name="baseTypes">Collection of desired base type names</param>
		/// <returns>TRUE if the given type is derived from one of the provided base types, FALSE otherwise</returns>
		private bool InheritsFromBaseType(TypeDefinition type, params string[] baseTypes)
		{
			if (type == null || type.BaseType == null)
				return false;

			if (type.BaseType.FullName.StartsWith("System") ||
				type.BaseType.FullName.StartsWith("Microsoft") ||
				type.BaseType.FullName.StartsWith("Windows") ||
				type.BaseType.FullName.StartsWith("UIKit") ||
				type.BaseType.FullName.StartsWith("Android"))
				return false;

			if (baseTypes.Any(b => type.BaseType.FullName.Equals(b)))
				return true;

			try
			{
				var baseType = type.BaseType.Resolve();
				return InheritsFromBaseType(baseType, baseTypes);
			}
			catch (Exception e)
			{
				Log.LogWarning(Resources.Resources.Warning_BasetypeResolveFailed, 
					type.FullName, type.BaseType.FullName, e.Message);
				return false;
			}
		}

		/// <summary>
		/// Checks if a given property is marked as bindable property and therefore needs to be processed. This is the case if
		/// it has the <see cref="MVVMbasics.Attributes.MvvmBindableAttribute">MvvmBindable</see> attribute applied, or if it
		/// is defined within a class that has the
		/// <see cref="MVVMbasics.Attributes.MvvmBindablePropertiesAttribute">MvvmBindableProperties</see> attributes applied
		/// and doesn't have the <see cref="MVVMbasics.Attributes.MvvmBindableIgnoreAttribute">MvvmBindableIgnore</see>
		/// attribute applied.
		/// </summary>
		/// <param name="property">Property to be checked</param>
		/// <param name="type">Class the property is defined in</param>
		/// <returns>TRUE if the property shall be processed, FALSE otherwise</returns>
		private static bool HasBindableAttributes(PropertyDefinition property, TypeDefinition type)
		{
			bool processAllProperties = type.CustomAttributes.Any((a =>
				a.AttributeType.FullName.Equals("MVVMbasics.Attributes.MvvmBindablePropertiesAttribute")));
			var attributes = property.CustomAttributes;

			return (attributes.Any(a => a.AttributeType.FullName.Equals("MVVMbasics.Attributes.MvvmBindableAttribute"))
			        || (processAllProperties && !attributes.Any(a =>
				        a.AttributeType.FullName.Equals("MVVMbasics.Attributes.MvvmBindableIgnoreAttribute"))));
		}

		/// <summary>
		/// Returns a reference to the method <code>System.Object.Equals(Object a, Object b)</code> or
		/// <code>System.Object.ReferenceEquals(Object a, Object b)</code>, depending on whether it is to be called for a value
		/// or reference type property.
		/// </summary>
		/// <param name="module">Current module</param>
		/// <returns>A reference to the desired method, or NULL if the method was not found</returns>
		private MethodReference FindEqualsMethod(ModuleDefinition module, bool isValueType)
		{
			TypeDefinition obj;
			try
			{
				obj = module.TypeSystem.Object.Resolve();
			}
			catch (Exception e)
			{
				Log.LogWarning(Resources.Resources.Warning_ResolveEqualsMethodFailed, e.Message);
				return null;
			}
			MethodReference foreignEqualsMethod = obj.Methods.Single(m =>
				m.Name.Equals(isValueType ? "Equals" : "ReferenceEquals") &&
				m.Parameters.Count == 2 &&
				m.Parameters[0].ParameterType.MetadataType == MetadataType.Object &&
				m.Parameters[1].ParameterType.MetadataType == MetadataType.Object);
			MethodReference equalsMethod = module.Import(foreignEqualsMethod);
			return equalsMethod;
		}

		/// <summary>
		/// Searches a given type and all its base types until the method
		/// <code>NotifyPropertyChanged(string propertyName)</code> is found, and returns a reference to this method.
		/// </summary>
		/// <param name="module">Current module</param>
		/// <param name="type">Type in which the desired method is specified</param>
		/// <returns>A reference to the desired method, or NULL if the method was not found</returns>
		private MethodReference FindNotifyMethod(ModuleDefinition module, TypeDefinition type)
		{
			if (type == null || type.BaseType == null)
				return null;

			if (type.BaseType.FullName.StartsWith("System") ||
				type.BaseType.FullName.StartsWith("Microsoft") ||
				type.BaseType.FullName.StartsWith("Windows") ||
				type.BaseType.FullName.StartsWith("UIKit") ||
				type.BaseType.FullName.StartsWith("Android"))
				return null;

			TypeDefinition baseType = type.BaseType.Resolve();
			var foreignRaiseMethod = baseType.Methods.FirstOrDefault(m =>
				m.Name.Equals("NotifyPropertyChanged") &&
				m.Parameters.Count == 1 &&
				m.Parameters[0].ParameterType.MetadataType == MetadataType.String);
			if (foreignRaiseMethod != null)
				return module.Import(foreignRaiseMethod);
			else
				return FindNotifyMethod(module, baseType);
		}

		/// <summary>
		/// Injects a call to the <code>Object.ReferenceEquals</code> method at the beginning of a property's setter
		/// method to quit the method (and not raise the <code>PropertyChanged</code> event) if old and new value are
		/// equal.
		/// </summary>
		/// <param name="processor">IL processor</param>
		/// <param name="property">Property to be processed</param>
		/// <param name="insertBefore">First instruction within the property's setter method</param>
		/// <param name="backingField">Reference to the property's auto-generated backing field</param>
		/// <param name="equalsMethod">Reference to the <code>System.Object.ReferenceEquals(Object a, Object b)</code> method</param>
		private bool InjectEqualsInstructions(ref ILProcessor processor, PropertyDefinition property, Instruction insertBefore,
			FieldReference backingField, MethodReference equalsMethod)
		{
			var isValueType = property.PropertyType.IsValueType;

			// Create instructions that call Object.ReferenceEquals and quit if the backing field equals the new value
			var equalsInstruction1 = processor.Create(OpCodes.Ldarg_0);
			var equalsInstruction2 = processor.Create(OpCodes.Ldfld, backingField);
			Instruction equalsInstruction3 = null;
			if (isValueType)
				equalsInstruction3 = processor.Create(OpCodes.Box, property.PropertyType);
			var equalsInstruction4 = processor.Create(OpCodes.Ldarg_1);
			Instruction equalsInstruction5 = null;
			if (isValueType)
				equalsInstruction5 = processor.Create(OpCodes.Box, property.PropertyType);
			var equalsInstruction6 = processor.Create(OpCodes.Call, equalsMethod);
			var equalsInstruction7 = processor.Create(OpCodes.Brfalse, insertBefore);
			var equalsInstruction8 = processor.Create(OpCodes.Ret);

			// Inject these instructions before the first instruction in the property setter
			try
			{
				processor.InsertBefore(insertBefore, equalsInstruction1);
				processor.InsertBefore(insertBefore, equalsInstruction2);
				if (isValueType && equalsInstruction3 != null)
					processor.InsertBefore(insertBefore, equalsInstruction3);
				processor.InsertBefore(insertBefore, equalsInstruction4);
				if (isValueType && equalsInstruction5 != null)
					processor.InsertBefore(insertBefore, equalsInstruction5);
				processor.InsertBefore(insertBefore, equalsInstruction6);
				processor.InsertBefore(insertBefore, equalsInstruction7);
				processor.InsertBefore(insertBefore, equalsInstruction8);
			}
			catch (Exception e)
			{
				//Log.LogWarning(Resources.Resources.Warning_InjectEqualsInstructionsFailed, GetReadableName(property), e.Message);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Injects a call to the <code>NotifyPropertyChanged</code> method at the end of a property's setter method.
		/// </summary>
		/// <param name="processor">IL processor</param>
		/// <param name="insertBefore">First instruction within the property's setter method</param>
		/// <param name="notifyMethod">Reference to the <code>NotifyPropertyChanged</code> method</param>
		/// <param name="propertyName">The name to be passed to the <code>PropertyChanged</code> event's arguments</param>
		/// <param name="propertyReadableName">Namespace and name of the property currently being processed</param>
		private bool InjectNotifyInstructions(ref ILProcessor processor, Instruction insertBefore,
			MethodReference notifyMethod, string propertyName, string propertyReadableName)
		{
			// Create instructions that call the RaisePropertyChanged with the property's name as argument
			var raiseInstruction1 = processor.Create(OpCodes.Ldarg_0);
			var raiseInstruction2 = processor.Create(OpCodes.Ldstr, propertyName);
			var raiseInstruction3 = processor.Create(OpCodes.Callvirt, notifyMethod);

			// Inject these instructions before the return call
			try
			{
				processor.InsertBefore(insertBefore, raiseInstruction1);
				processor.InsertBefore(insertBefore, raiseInstruction2);
				processor.InsertBefore(insertBefore, raiseInstruction3);
			}
			catch (Exception e)
			{
				Log.LogWarning(Resources.Resources.Warning_InjectNotifyInstructionsFailed, propertyReadableName, e.Message);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Creates a backup of the assembly file, then writes the adapted assembly to the original file.
		/// </summary>
		/// <param name="module">Current module</param>
		/// <returns>TRUE is successful, FALSE on exception</returns>
		private bool WriteAssembly(ModuleDefinition module)
		{
			try
			{
				string backupFile = String.Format("{0}.bak", AssemblyPath);
				File.Delete(backupFile);
				File.Copy(AssemblyPath, backupFile);
			}
			catch (Exception e1)
			{
				Log.LogError(Resources.Resources.Error_CreateBackupFailed, AssemblyPath, e1.Message);
				return false;
			}
			try
			{
				module.Write(new WriterParameters
				{
					WriteSymbols = true
				});
			}
			catch (Exception e2)
			{
				Log.LogErrorFromException(e2, showStackTrace: false);
				return false;
			}
			return true;
		}

		/// <summary>
		/// Concatenates a property's name and the name of its containing type.
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		private static string GetReadableName(PropertyDefinition property)
		{
			return String.Format("{0}.{1}", property.DeclaringType.Name, property.Name);
		}
	}
}
