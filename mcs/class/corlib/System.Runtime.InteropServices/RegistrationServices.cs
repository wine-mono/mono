//
// System.Runtime.InteropServices.RegistrationServices.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
// Copyright (c) Microsoft Corporation
// Copyright 2026 YJBeetle
//
// Portions adapted from the MIT-licensed Microsoft Reference Source:
// mscorlib/system/runtime/interopservices/registrationservices.cs.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Microsoft.Win32;

namespace System.Runtime.InteropServices
{
	
#if !MOBILE	
	[ComVisible(true)]
	[Guid ("475e398f-8afa-43a7-a3be-f4ef8d6787c9")]
	[ClassInterface (ClassInterfaceType.None)]
	public class RegistrationServices : IRegistrationServices
	{
		const string managedCategory = "{62C8FE65-4EBB-45E7-B440-6E39B2CDBF29}";
		const string managedCategoryDescription = ".NET Category";
		const string managedTypeThreadingModel = "Both";
		const string runtimeServer = "mscoree.dll";

		static readonly Guid guidManagedCategory = new Guid (managedCategory);

		public RegistrationServices ()
		{
		}

		public virtual Guid GetManagedCategoryGuid ()
		{
			return guidManagedCategory;
		}

		public virtual string GetProgIdForType (Type type)
		{
			return Marshal.GenerateProgIdForType (type);
		}

		public virtual Type[] GetRegistrableTypesInAssembly (Assembly assembly)
		{
			if (assembly == null)
				throw new ArgumentNullException ("assembly");

			List<Type> types = new List<Type> ();
			foreach (Type type in assembly.GetExportedTypes ()) {
				if (TypeRequiresRegistration (type))
					types.Add (type);
			}
			return types.ToArray ();
		}

		public virtual bool RegisterAssembly (Assembly assembly, AssemblyRegistrationFlags flags)
		{
			if (assembly == null)
				throw new ArgumentNullException ("assembly");
			if (assembly.ReflectionOnly)
				throw new InvalidOperationException ("A reflection-only assembly cannot be registered.");

			string assemblyName = assembly.FullName;
			if (assemblyName == null)
				throw new InvalidOperationException ("The assembly does not have a name.");

			string codeBase = null;
			if ((flags & AssemblyRegistrationFlags.SetCodeBase) != 0) {
				codeBase = assembly.CodeBase;
				if (codeBase == null)
					throw new InvalidOperationException ("The assembly does not have a code base.");
			}

			Type[] types = GetRegistrableTypesInAssembly (assembly);
			object[] primaryInteropAttributes = assembly.GetCustomAttributes (typeof (PrimaryInteropAssemblyAttribute), false);
			ThrowIfPrimaryInteropAssembly (assembly, primaryInteropAttributes);

			MethodInfo[] callbacks = GetUserDefinedRegistrationMethods (types, true);
			string version = assembly.GetName ().Version.ToString ();
			string runtimeVersion = assembly.ImageRuntimeVersion;
			for (int i = 0; i < types.Length; ++i) {
				Type type = types[i];
				if (type.IsValueType)
					RegisterValueType (type, assemblyName, version, codeBase, runtimeVersion);
				else if (TypeRepresentsComType (type))
					RegisterComImportedType (type, assemblyName, version, codeBase, runtimeVersion);
				else
					RegisterManagedType (type, assemblyName, version, codeBase, runtimeVersion);

				CallUserDefinedRegistrationMethod (type, callbacks[i]);
			}

			return types.Length != 0 || primaryInteropAttributes.Length != 0;
		}

		[MonoTODO ("implement")]
		public virtual void RegisterTypeForComClients (Type type, ref Guid g)
		{
			throw new NotImplementedException ();
		}

		public virtual bool TypeRepresentsComType (Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (!type.IsCOMObject)
				return false;
			if (type.IsImport)
				return true;

			Type importedBase = type.BaseType;
			while (importedBase != null && !importedBase.IsImport)
				importedBase = importedBase.BaseType;

			return importedBase != null &&
				Marshal.GenerateGuidForType (type) == Marshal.GenerateGuidForType (importedBase);
		}

		public virtual bool TypeRequiresRegistration (Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if ((!type.IsClass && !type.IsValueType) || type.IsAbstract)
				return false;
			if (!type.IsValueType && type.GetConstructor (Type.EmptyTypes) == null)
				return false;
			return Marshal.IsTypeVisibleFromCom (type);
		}

		public virtual bool UnregisterAssembly (Assembly assembly)
		{
			if (assembly == null)
				throw new ArgumentNullException ("assembly");
			if (assembly.ReflectionOnly)
				throw new InvalidOperationException ("A reflection-only assembly cannot be unregistered.");

			Type[] types = GetRegistrableTypesInAssembly (assembly);
			object[] primaryInteropAttributes = assembly.GetCustomAttributes (typeof (PrimaryInteropAssemblyAttribute), false);
			ThrowIfPrimaryInteropAssembly (assembly, primaryInteropAttributes);

			MethodInfo[] callbacks = GetUserDefinedRegistrationMethods (types, false);
			string version = assembly.GetName ().Version.ToString ();
			for (int i = 0; i < types.Length; ++i) {
				Type type = types[i];
				CallUserDefinedRegistrationMethod (type, callbacks[i]);
				if (type.IsValueType)
					UnregisterValueType (type, version);
				else if (TypeRepresentsComType (type))
					UnregisterComImportedType (type, version);
				else
					UnregisterManagedType (type, version);
			}

			return types.Length != 0 || primaryInteropAttributes.Length != 0;
		}

		[ComVisible(false)]
		[MonoTODO ("implement")]
		public virtual int RegisterTypeForComClients(Type type, RegistrationClassContext classContext, RegistrationConnectionType flags)
		{
			throw new NotImplementedException ();
		}
		
		[ComVisible(false)]
		[MonoTODO ("implement")]
		public virtual void UnregisterTypeForComClients(int cookie)
		{
			throw new NotImplementedException ();
		}

		static string GetGuidString (Type type)
		{
			return "{" + Marshal.GenerateGuidForType (type).ToString ().ToUpper (CultureInfo.InvariantCulture) + "}";
		}

		static void SetRegistrationValues (RegistryKey key, Type type, string assemblyName,
			string codeBase, string runtimeVersion)
		{
			key.SetValue ("Class", type.FullName);
			key.SetValue ("Assembly", assemblyName);
			key.SetValue ("RuntimeVersion", runtimeVersion);
			if (codeBase != null)
				key.SetValue ("CodeBase", codeBase);
		}

		void RegisterManagedType (Type type, string assemblyName, string version,
			string codeBase, string runtimeVersion)
		{
			string classId = GetGuidString (type);
			string progId = GetProgIdForType (type);

			if (progId.Length != 0) {
				using (RegistryKey progIdKey = Registry.ClassesRoot.CreateSubKey (progId)) {
					progIdKey.SetValue (String.Empty, type.FullName);
					using (RegistryKey classIdKey = progIdKey.CreateSubKey ("CLSID"))
						classIdKey.SetValue (String.Empty, classId);
				}
			}

			using (RegistryKey classIdKey = Registry.ClassesRoot.CreateSubKey ("CLSID\\" + classId)) {
				classIdKey.SetValue (String.Empty, type.FullName);
				using (RegistryKey serverKey = classIdKey.CreateSubKey ("InprocServer32")) {
					serverKey.SetValue (String.Empty, runtimeServer);
					serverKey.SetValue ("ThreadingModel", managedTypeThreadingModel);
					SetRegistrationValues (serverKey, type, assemblyName, codeBase, runtimeVersion);
					using (RegistryKey versionKey = serverKey.CreateSubKey (version))
						SetRegistrationValues (versionKey, type, assemblyName, codeBase, runtimeVersion);
				}
				if (progId.Length != 0) {
					using (RegistryKey progIdKey = classIdKey.CreateSubKey ("ProgId"))
						progIdKey.SetValue (String.Empty, progId);
				}
				using (classIdKey.CreateSubKey ("Implemented Categories\\" + managedCategory)) {
				}
			}

			EnsureManagedCategoryExists ();
		}

		static void RegisterValueType (Type type, string assemblyName, string version,
			string codeBase, string runtimeVersion)
		{
			using (RegistryKey key = Registry.ClassesRoot.CreateSubKey ("Record\\" + GetGuidString (type) + "\\" + version))
				SetRegistrationValues (key, type, assemblyName, codeBase, runtimeVersion);
		}

		static void RegisterComImportedType (Type type, string assemblyName, string version,
			string codeBase, string runtimeVersion)
		{
			using (RegistryKey key = Registry.ClassesRoot.CreateSubKey ("CLSID\\" + GetGuidString (type) + "\\InprocServer32")) {
				SetRegistrationValues (key, type, assemblyName, codeBase, runtimeVersion);
				using (RegistryKey versionKey = key.CreateSubKey (version))
					SetRegistrationValues (versionKey, type, assemblyName, codeBase, runtimeVersion);
			}
		}

		static void EnsureManagedCategoryExists ()
		{
			if (ManagedCategoryExists ())
				return;

			using (RegistryKey key = Registry.ClassesRoot.CreateSubKey ("Component Categories\\" + managedCategory)) {
				key.SetValue ("0", managedCategoryDescription);
			}
		}

		static bool ManagedCategoryExists ()
		{
			using (RegistryKey key = Registry.ClassesRoot.OpenSubKey ("Component Categories\\" + managedCategory)) {
				if (key == null)
					return false;
				return String.Equals (key.GetValue ("0") as string, managedCategoryDescription,
					StringComparison.Ordinal);
			}
		}

		static void ThrowIfPrimaryInteropAssembly (Assembly assembly, object[] attributes)
		{
			if (attributes.Length == 0)
				return;

			byte[] publicKey = assembly.GetName ().GetPublicKey ();
			if (publicKey == null || publicKey.Length == 0)
				throw new InvalidOperationException ("A primary interop assembly must be strong named.");

			// PIA registration depends on Marshal.GetTypeLibGuidForAssembly, whose
			// fallback GUID generation is not implemented by Mono. Fail before any
			// registry changes or callbacks instead of leaving a partial registration.
			throw new NotImplementedException ("Primary interop assembly registration and unregistration are not implemented.");
		}

		void UnregisterManagedType (Type type, string version)
		{
			string classId = GetGuidString (type);
			string progId = GetProgIdForType (type);
			bool allVersionsRemoved = true;

			using (RegistryKey classIdKey = Registry.ClassesRoot.OpenSubKey ("CLSID\\" + classId, true)) {
				if (classIdKey != null) {
					using (RegistryKey serverKey = classIdKey.OpenSubKey ("InprocServer32", true)) {
						if (serverKey != null) {
							RemoveVersion (serverKey, version);
							allVersionsRemoved = serverKey.SubKeyCount == 0;
							// The .NET Framework removes these unversioned values even when
							// another version subkey remains. Preserve that compatibility behavior.
							DeleteRegistrationValues (serverKey);
							if (allVersionsRemoved) {
								serverKey.DeleteValue (String.Empty, false);
								serverKey.DeleteValue ("ThreadingModel", false);
							}
						}
					}
					DeleteSubKeyIfEmpty (classIdKey, "InprocServer32");

					if (allVersionsRemoved) {
						classIdKey.DeleteValue (String.Empty, false);
						if (progId.Length != 0)
							DeleteValueAndEmptySubKey (classIdKey, "ProgId", String.Empty);
						DeleteEmptySubKeyPath (classIdKey, "Implemented Categories", managedCategory);
					}
				}
			}

			DeleteSubKeyIfEmpty (Registry.ClassesRoot, "CLSID\\" + classId);
			if (allVersionsRemoved && progId.Length != 0) {
				using (RegistryKey progIdKey = Registry.ClassesRoot.OpenSubKey (progId, true)) {
					if (progIdKey != null) {
						progIdKey.DeleteValue (String.Empty, false);
						DeleteValueAndEmptySubKey (progIdKey, "CLSID", String.Empty);
					}
				}
				DeleteSubKeyIfEmpty (Registry.ClassesRoot, progId);
			}
		}

		static void UnregisterValueType (Type type, string version)
		{
			string path = "Record\\" + GetGuidString (type);
			bool empty;
			using (RegistryKey key = Registry.ClassesRoot.OpenSubKey (path, true)) {
				if (key == null)
					return;
				RemoveVersion (key, version);
				empty = key.SubKeyCount == 0;
			}
			if (empty)
				DeleteSubKeyIfEmpty (Registry.ClassesRoot, path);
		}

		static void UnregisterComImportedType (Type type, string version)
		{
			string classPath = "CLSID\\" + GetGuidString (type);
			string serverPath = classPath + "\\InprocServer32";
			bool empty;
			using (RegistryKey key = Registry.ClassesRoot.OpenSubKey (serverPath, true)) {
				if (key == null)
					return;
				RemoveVersion (key, version);
				DeleteRegistrationValues (key);
				empty = key.SubKeyCount == 0;
			}
			if (empty) {
				DeleteSubKeyIfEmpty (Registry.ClassesRoot, serverPath);
				DeleteSubKeyIfEmpty (Registry.ClassesRoot, classPath);
			}
		}

		static void RemoveVersion (RegistryKey parent, string version)
		{
			using (RegistryKey key = parent.OpenSubKey (version, true)) {
				if (key != null)
					DeleteRegistrationValues (key);
			}
			DeleteSubKeyIfEmpty (parent, version);
		}

		static void DeleteRegistrationValues (RegistryKey key)
		{
			key.DeleteValue ("Class", false);
			key.DeleteValue ("Assembly", false);
			key.DeleteValue ("RuntimeVersion", false);
			key.DeleteValue ("CodeBase", false);
		}

		static void DeleteValueAndEmptySubKey (RegistryKey parent, string name, string valueName)
		{
			using (RegistryKey key = parent.OpenSubKey (name, true)) {
				if (key != null)
					key.DeleteValue (valueName, false);
			}
			DeleteSubKeyIfEmpty (parent, name);
		}

		static void DeleteEmptySubKeyPath (RegistryKey parent, string name, string childName)
		{
			using (RegistryKey key = parent.OpenSubKey (name, true)) {
				if (key != null)
					DeleteSubKeyIfEmpty (key, childName);
			}
			DeleteSubKeyIfEmpty (parent, name);
		}

		static void DeleteSubKeyIfEmpty (RegistryKey parent, string name)
		{
			bool empty = false;
			using (RegistryKey key = parent.OpenSubKey (name)) {
				if (key != null)
					empty = key.SubKeyCount == 0 && key.ValueCount == 0;
			}
			if (empty)
				parent.DeleteSubKey (name, false);
		}

		static MethodInfo[] GetUserDefinedRegistrationMethods (Type[] types, bool register)
		{
			MethodInfo[] callbacks = new MethodInfo[types.Length];
			for (int i = 0; i < types.Length; ++i)
				callbacks[i] = GetUserDefinedRegistrationMethod (types[i], register);
			return callbacks;
		}

		static MethodInfo GetUserDefinedRegistrationMethod (Type type, bool register)
		{
			Type attributeType = register ? typeof (ComRegisterFunctionAttribute) : typeof (ComUnregisterFunctionAttribute);
			MethodInfo callback = null;

			for (Type current = type; current != null; current = current.BaseType) {
				foreach (MethodInfo method in current.GetMethods (BindingFlags.DeclaredOnly | BindingFlags.Public |
					BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)) {
					if (!method.IsDefined (attributeType, true))
						continue;
					if (callback != null)
						throw new InvalidOperationException (String.Format (CultureInfo.InvariantCulture,
							"Type '{0}' in assembly '{1}' cannot have more than one {2} callback; found methods '{3}' and '{4}'.",
							type.FullName, type.Assembly.FullName, register ? "COM registration" : "COM unregistration",
							callback.Name, method.Name));
					callback = method;
				}
				if (callback != null)
					break;
			}

			if (callback == null)
				return null;
			if (callback.ContainsGenericParameters)
				throw InvalidRegistrationCallback (type, callback, register,
					"must not contain unassigned generic parameters");
			if (!callback.IsStatic)
				throw InvalidRegistrationCallback (type, callback, register, "must be static");

			ParameterInfo[] parameters = callback.GetParameters ();
			if (callback.ReturnType != typeof (void) || parameters.Length != 1 ||
				(parameters[0].ParameterType != typeof (string) && parameters[0].ParameterType != typeof (Type)))
				throw InvalidRegistrationCallback (type, callback, register,
					"must return void and accept a string or Type argument");

			return callback;
		}

		static InvalidOperationException InvalidRegistrationCallback (Type type, MethodInfo callback, bool register,
			string requirement)
		{
			return new InvalidOperationException (String.Format (CultureInfo.InvariantCulture,
				"COM {0} callback '{1}.{2}' selected for type '{3}' in assembly '{4}' {5}.",
				register ? "registration" : "unregistration", callback.DeclaringType.FullName, callback.Name,
				type.FullName, type.Assembly.FullName, requirement));
		}

		static void CallUserDefinedRegistrationMethod (Type type, MethodInfo callback)
		{
			if (callback == null)
				return;

			ParameterInfo[] parameters = callback.GetParameters ();
			object argument = parameters[0].ParameterType == typeof (Type) ? (object) type :
				"HKEY_CLASSES_ROOT\\CLSID\\" + GetGuidString (type);
			callback.Invoke (null, new object[] { argument });
		}
		
	}
#endif
}
