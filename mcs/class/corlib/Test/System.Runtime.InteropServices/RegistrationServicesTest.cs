//
// RegistrationServicesTest.cs - NUnit tests for RegistrationServices
//
// Copyright 2026 YJBeetle
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

#if !MOBILE

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32;

using NUnit.Framework;

namespace MonoTests.System.Runtime.InteropServices {

	[TestFixture]
	public class RegistrationServicesTest {
		[ComVisible (true)]
		public class VisibleClass {
			public VisibleClass ()
			{
			}
		}

		[ComVisible (true)]
		public abstract class AbstractClass {
		}

		[ComVisible (true)]
		public class ClassWithoutPublicConstructor {
			ClassWithoutPublicConstructor ()
			{
			}
		}

		[ComVisible (false)]
		public class InvisibleClass {
			public InvisibleClass ()
			{
			}
		}

		[ComVisible (true)]
		public class GenericClass<T> {
			public GenericClass ()
			{
			}
		}

		[ComVisible (true)]
		class PrivateClass {
			public PrivateClass ()
			{
			}
		}

		[ComImport]
		[ComVisible (false)]
		[Guid ("7392E7B9-5C04-4B9C-A1D0-85CF83B289BB")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		public interface ImportedInterface {
		}

		[ComImport]
		[ComVisible (false)]
		[Guid ("52E2055C-5C19-42A6-9C5B-7CC3CE9D03B2")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		internal interface InternalImportedInterface {
		}

		[ComImport]
		[ComVisible (false)]
		[Guid ("90E4CF83-5AE0-48C6-A3D0-8B61F3969D27")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		public interface GenericImportedInterface<T> {
		}

		[Test]
		public void TypeRequiresRegistration ()
		{
			RegistrationServices services = new RegistrationServices ();
			Assert.IsTrue (services.TypeRequiresRegistration (typeof (VisibleClass)), "visible");
			Assert.IsFalse (services.TypeRequiresRegistration (typeof (AbstractClass)), "abstract");
			Assert.IsFalse (services.TypeRequiresRegistration (typeof (ClassWithoutPublicConstructor)), "constructor");
			Assert.IsFalse (services.TypeRequiresRegistration (typeof (InvisibleClass)), "invisible");
			Assert.IsFalse (services.TypeRequiresRegistration (typeof (IDisposable)), "interface");
		}

		[Test]
		public void TypeVisibility ()
		{
			Assert.IsTrue (Marshal.IsTypeVisibleFromCom (typeof (VisibleClass)), "visible");
			Assert.IsFalse (Marshal.IsTypeVisibleFromCom (typeof (InvisibleClass)), "attribute");
			Assert.IsFalse (Marshal.IsTypeVisibleFromCom (typeof (PrivateClass)), "private");
			Assert.IsFalse (Marshal.IsTypeVisibleFromCom (typeof (GenericClass<int>)), "generic");
			Assert.IsTrue (Marshal.IsTypeVisibleFromCom (typeof (ImportedInterface)), "imported interface");
			Assert.IsTrue (Marshal.IsTypeVisibleFromCom (typeof (InternalImportedInterface)), "internal imported interface");
			Assert.IsFalse (Marshal.IsTypeVisibleFromCom (typeof (GenericImportedInterface<>)), "generic imported interface");
			Assert.IsFalse (Marshal.IsTypeVisibleFromCom (typeof (GenericImportedInterface<int>)), "constructed generic imported interface");
			Assert.IsFalse (Marshal.IsTypeVisibleFromCom (typeof (VisibleClass[])), "array");
			Assert.Throws<ArgumentNullException> (() => Marshal.IsTypeVisibleFromCom (null), "null");

			string path = Path.Combine (Path.GetDirectoryName (typeof (RegistrationServicesTest).Assembly.Location),
				"RegistrationServicesTestAssembly.dll");
			Assembly assembly = Assembly.LoadFrom (path);
			Assert.IsTrue (Marshal.IsTypeVisibleFromCom (assembly.GetType (
				"MonoTests.RegistrationServices.TestObject", true)), "type overrides assembly");
			Assert.IsFalse (Marshal.IsTypeVisibleFromCom (assembly.GetType (
				"MonoTests.RegistrationServices.CallbackBase", true)), "assembly and type hidden");
		}
	}

	[TestFixture]
	public class RegistrationServicesRegistryTest {
		const string ClassId = "{5E4466A3-2BA4-414E-B70B-317D91BE57CC}";
		const string ProgId = "MonoTests.RegistrationServices.TestObject";
		const string DerivedClassId = "{641D963E-EA94-4E4F-B6EF-1DFEB43FB697}";
		const string DerivedProgId = "MonoTests.RegistrationServices.DerivedTestObject";
		const string EmptyProgIdClassId = "{F0439499-B07C-4FA5-BC3B-8402B70B3AFF}";
		const string CallbackPath = "MonoTests.RegistrationServices.CallbackState";
		const string ManagedCategoryPath = "Component Categories\\{62C8FE65-4EBB-45E7-B440-6E39B2CDBF29}";
		const string ManagedCategoryDescription = ".NET Category";
		const string ThirdPartyValue = "MonoRegistrationServicesTest";
		const string DynamicClassId = "{5C9782F8-3BAA-42C7-A461-B8C94F2FA438}";
		const string DynamicProgId = "MonoTests.RegistrationServices.VersionedObject";
		const string InvalidCallbackClassId = "{F81E4AE1-917F-43C8-BD29-A56B182287AA}";
		const string InvalidCallbackProgId = "MonoTests.RegistrationServices.InvalidCallbackObject";
		const string GenericCallbackClassId = "{86F43A7E-B430-4F56-9C6C-DD61BA460217}";
		const string GenericCallbackProgId = "MonoTests.RegistrationServices.GenericCallbackObject";

		ManagedCategoryState managedCategoryState;
		bool registrySetupSucceeded;

		[SetUp]
		public void SetUp ()
		{
			if (Environment.OSVersion.Platform != PlatformID.Win32NT)
				return;

			try {
				RemoveTestKeys ();
				managedCategoryState = new ManagedCategoryState ();
				registrySetupSucceeded = true;
			} catch (UnauthorizedAccessException) {
				Assert.Ignore ("COM registry tests require write access to their test keys and category fixture.");
			} catch (SecurityException) {
				Assert.Ignore ("COM registry tests require write access to their test keys and category fixture.");
			}
		}

		[TearDown]
		public void TearDown ()
		{
			if (Environment.OSVersion.Platform != PlatformID.Win32NT)
				return;
			if (!registrySetupSucceeded)
				return;

			try {
				RemoveTestKeys ();
			} finally {
				if (managedCategoryState != null) {
					managedCategoryState.Dispose ();
					managedCategoryState = null;
				}
				registrySetupSucceeded = false;
			}
		}

		[Test]
		public void RegisterAndUnregisterAssembly ()
		{
			if (Environment.OSVersion.Platform != PlatformID.Win32NT)
				Assert.Ignore ("COM registration is only supported on Windows.");

			string path = Path.Combine (Path.GetDirectoryName (typeof (RegistrationServicesRegistryTest).Assembly.Location),
				"RegistrationServicesTestAssembly.dll");
			Assembly assembly = Assembly.LoadFrom (path);
			Type type = assembly.GetType ("MonoTests.RegistrationServices.TestObject", true);
			RegistrationServices services = new RegistrationServices ();
			Assert.IsTrue (services.RegisterAssembly (assembly, AssemblyRegistrationFlags.None), "register");

			using (RegistryKey progIdKey = Registry.ClassesRoot.OpenSubKey (ProgId)) {
				Assert.IsNotNull (progIdKey, "ProgID key");
				Assert.AreEqual (type.FullName, progIdKey.GetValue (String.Empty), "ProgID description");
				using (RegistryKey classIdKey = progIdKey.OpenSubKey ("CLSID"))
					Assert.AreEqual (ClassId, classIdKey.GetValue (String.Empty), "ProgID CLSID");
			}

			using (RegistryKey serverKey = Registry.ClassesRoot.OpenSubKey ("CLSID\\" + ClassId + "\\InprocServer32")) {
				Assert.IsNotNull (serverKey, "InprocServer32 key");
				Assert.AreEqual ("mscoree.dll", serverKey.GetValue (String.Empty), "server");
				Assert.AreEqual ("Both", serverKey.GetValue ("ThreadingModel"), "threading model");
				Assert.AreEqual (type.FullName, serverKey.GetValue ("Class"), "class");
				Assert.AreEqual (assembly.FullName, serverKey.GetValue ("Assembly"), "assembly");
				Assert.AreEqual (assembly.ImageRuntimeVersion, serverKey.GetValue ("RuntimeVersion"), "runtime version");
			}

			using (RegistryKey callbackKey = Registry.ClassesRoot.OpenSubKey (CallbackPath)) {
				Assert.IsNotNull (callbackKey, "callback key");
				Assert.AreEqual (type.FullName, callbackKey.GetValue ("Register"), "register callback");
				Assert.AreEqual ("HKEY_CLASSES_ROOT\\CLSID\\" + DerivedClassId,
					callbackKey.GetValue ("DerivedRegister"), "derived callback overrides base callback");
			}
			using (RegistryKey categoryKey = Registry.ClassesRoot.OpenSubKey (ManagedCategoryPath)) {
				Assert.AreEqual (ManagedCategoryDescription, categoryKey.GetValue ("0"), "existing managed category description");
				Assert.AreEqual ("preserve", categoryKey.GetValue (ThirdPartyValue), "existing managed category data");
			}

			Assert.IsTrue (services.UnregisterAssembly (assembly), "unregister");
			Assert.IsNull (Registry.ClassesRoot.OpenSubKey (ProgId), "removed ProgID");
			Assert.IsNull (Registry.ClassesRoot.OpenSubKey ("CLSID\\" + ClassId), "removed CLSID");
			Assert.IsNull (Registry.ClassesRoot.OpenSubKey (DerivedProgId), "removed derived ProgID");
			Assert.IsNull (Registry.ClassesRoot.OpenSubKey ("CLSID\\" + DerivedClassId), "removed derived CLSID");
			using (RegistryKey callbackKey = Registry.ClassesRoot.OpenSubKey (CallbackPath)) {
				Assert.IsNotNull (callbackKey, "callback key after unregister");
				Assert.AreEqual (type.FullName, callbackKey.GetValue ("Unregister"), "unregister callback");
				Assert.AreEqual ("HKEY_CLASSES_ROOT\\CLSID\\" + DerivedClassId,
					callbackKey.GetValue ("DerivedUnregister"), "derived unregister callback overrides base callback");
			}
		}

		[Test]
		public void RegisterCreatesMissingManagedCategoryDescription ()
		{
			if (Environment.OSVersion.Platform != PlatformID.Win32NT)
				Assert.Ignore ("COM registration is only supported on Windows.");

			using (RegistryKey categoryKey = Registry.ClassesRoot.OpenSubKey (ManagedCategoryPath, true))
				categoryKey.DeleteValue ("0");

			Assembly assembly = LoadTestAssembly ();
			RegistrationServices services = new RegistrationServices ();
			Assert.IsTrue (services.RegisterAssembly (assembly, AssemblyRegistrationFlags.None), "register");

			using (RegistryKey categoryKey = Registry.ClassesRoot.OpenSubKey (ManagedCategoryPath)) {
				Assert.AreEqual (ManagedCategoryDescription, categoryKey.GetValue ("0"), "created category description");
				Assert.AreEqual ("preserve", categoryKey.GetValue (ThirdPartyValue), "existing category data");
			}
		}

		[Test]
		public void RegisterRepairsIncorrectManagedCategoryDescription ()
		{
			if (Environment.OSVersion.Platform != PlatformID.Win32NT)
				Assert.Ignore ("COM registration is only supported on Windows.");

			using (RegistryKey categoryKey = Registry.ClassesRoot.OpenSubKey (ManagedCategoryPath, true))
				categoryKey.SetValue ("0", "Incorrect Category");

			Assembly assembly = LoadTestAssembly ();
			RegistrationServices services = new RegistrationServices ();
			Assert.IsTrue (services.RegisterAssembly (assembly, AssemblyRegistrationFlags.None), "register");

			using (RegistryKey categoryKey = Registry.ClassesRoot.OpenSubKey (ManagedCategoryPath)) {
				Assert.AreEqual (ManagedCategoryDescription, categoryKey.GetValue ("0"), "repaired category description");
				Assert.AreEqual ("preserve", categoryKey.GetValue (ThirdPartyValue), "existing category data");
			}
		}

		[Test]
		public void EmptyProgIdPreservesForeignClassProgId ()
		{
			if (Environment.OSVersion.Platform != PlatformID.Win32NT)
				Assert.Ignore ("COM registration is only supported on Windows.");

			const string foreignProgId = "Foreign.Component.ProgId";
			string path = Path.Combine (Path.GetDirectoryName (typeof (RegistrationServicesRegistryTest).Assembly.Location),
				"RegistrationServicesTestAssembly.dll");
			Assembly assembly = Assembly.LoadFrom (path);
			RegistrationServices services = new RegistrationServices ();
			using (RegistryKey progIdKey = Registry.ClassesRoot.CreateSubKey (
				"CLSID\\" + EmptyProgIdClassId + "\\ProgId"))
				progIdKey.SetValue (String.Empty, foreignProgId);

			Assert.IsTrue (services.RegisterAssembly (assembly, AssemblyRegistrationFlags.None), "register");
			using (RegistryKey progIdKey = Registry.ClassesRoot.OpenSubKey (
				"CLSID\\" + EmptyProgIdClassId + "\\ProgId"))
				Assert.AreEqual (foreignProgId, progIdKey.GetValue (String.Empty), "register preserves foreign ProgID");

			Assert.IsTrue (services.UnregisterAssembly (assembly), "unregister");
			using (RegistryKey progIdKey = Registry.ClassesRoot.OpenSubKey (
				"CLSID\\" + EmptyProgIdClassId + "\\ProgId")) {
				Assert.IsNotNull (progIdKey, "foreign ProgID key retained");
				Assert.AreEqual (foreignProgId, progIdKey.GetValue (String.Empty), "unregister preserves foreign ProgID");
			}
		}

		[Test]
		public void PrimaryInteropAssemblyFailsBeforeRegistryChanges ()
		{
			string path = Path.Combine (Path.GetDirectoryName (typeof (RegistrationServicesRegistryTest).Assembly.Location),
				"RegistrationServicesPIATestAssembly.dll");
			Assembly assembly = Assembly.LoadFrom (path);
			RegistrationServices services = new RegistrationServices ();

			NotImplementedException registerError = Assert.Throws<NotImplementedException> (() =>
				services.RegisterAssembly (assembly, AssemblyRegistrationFlags.None));
			StringAssert.Contains ("Primary interop assembly", registerError.Message, "register message");

			NotImplementedException unregisterError = Assert.Throws<NotImplementedException> (() =>
				services.UnregisterAssembly (assembly));
			StringAssert.Contains ("Primary interop assembly", unregisterError.Message, "unregister message");

			if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
				Assert.IsNull (Registry.ClassesRoot.OpenSubKey (ProgId), "no partial ProgID");
				Assert.IsNull (Registry.ClassesRoot.OpenSubKey ("CLSID\\" + ClassId), "no partial CLSID");
				Assert.IsNull (Registry.ClassesRoot.OpenSubKey (CallbackPath), "no callback");
			}
		}

		[Test]
		public void UnregisterPreservesOtherVersionsAndForeignData ()
		{
			if (Environment.OSVersion.Platform != PlatformID.Win32NT)
				Assert.Ignore ("COM registration is only supported on Windows.");

			string testDirectory = Path.GetDirectoryName (typeof (RegistrationServicesRegistryTest).Assembly.Location);
			string assemblyName = "RegistrationServicesVersionedTestAssembly.dll";
			Assembly first = Assembly.LoadFile (Path.Combine (testDirectory, "RegistrationVersion1", assemblyName));
			Assembly second = Assembly.LoadFile (Path.Combine (testDirectory, "RegistrationVersion2", assemblyName));
			RegistrationServices services = new RegistrationServices ();
			Assert.AreEqual (new Version (1, 0, 0, 0), first.GetName ().Version, "first assembly version");
			Assert.AreEqual (new Version (2, 0, 0, 0), second.GetName ().Version, "second assembly version");

			Assert.IsTrue (services.RegisterAssembly (first, AssemblyRegistrationFlags.None), "register first");
			Assert.IsTrue (services.RegisterAssembly (second, AssemblyRegistrationFlags.None), "register second");

			string serverPath = "CLSID\\" + DynamicClassId + "\\InprocServer32";
			using (RegistryKey serverKey = Registry.ClassesRoot.OpenSubKey (serverPath, true)) {
				serverKey.SetValue ("ForeignValue", "preserve");
				using (RegistryKey versionKey = serverKey.OpenSubKey ("1.0.0.0", true))
					versionKey.SetValue ("ForeignVersionValue", "preserve");
			}

			Assert.IsTrue (services.UnregisterAssembly (first), "unregister first");
			using (RegistryKey serverKey = Registry.ClassesRoot.OpenSubKey (serverPath)) {
				Assert.IsNotNull (serverKey, "server retained");
				Assert.AreEqual ("preserve", serverKey.GetValue ("ForeignValue"), "foreign server value");
				Assert.IsNull (serverKey.GetValue ("Assembly"), "top-level assembly removed for compatibility");
				Assert.IsNotNull (serverKey.OpenSubKey ("1.0.0.0"), "foreign data keeps old version key");
				Assert.IsNotNull (serverKey.OpenSubKey ("2.0.0.0"), "other version retained");
			}

			Assert.IsTrue (services.UnregisterAssembly (second), "unregister second");
			using (RegistryKey serverKey = Registry.ClassesRoot.OpenSubKey (serverPath)) {
				Assert.IsNotNull (serverKey, "foreign data keeps server key");
				Assert.AreEqual ("preserve", serverKey.GetValue ("ForeignValue"), "foreign server value after all versions");
			}
		}

		[Test]
		public void InvalidCallbackReportsAssemblyTypeAndMethodBeforeRegistryChanges ()
		{
			if (Environment.OSVersion.Platform != PlatformID.Win32NT)
				Assert.Ignore ("COM registration is only supported on Windows.");

			const string typeName = "MonoTests.RegistrationServices.InvalidCallbackObject";
			const string methodName = "InvalidRegister";
			Assembly assembly = LoadBoundaryTestAssembly ("RegistrationServicesInvalidCallbackTestAssembly.dll");
			RegistrationServices services = new RegistrationServices ();

			InvalidOperationException error = Assert.Throws<InvalidOperationException> (() =>
				services.RegisterAssembly (assembly, AssemblyRegistrationFlags.None));
			StringAssert.Contains (assembly.FullName, error.Message, "assembly");
			StringAssert.Contains (typeName, error.Message, "type");
			StringAssert.Contains (methodName, error.Message, "method");
			Assert.IsNull (Registry.ClassesRoot.OpenSubKey (InvalidCallbackProgId), "no partial ProgID");
			Assert.IsNull (Registry.ClassesRoot.OpenSubKey ("CLSID\\" + InvalidCallbackClassId), "no partial CLSID");
		}

		[Test]
		public void GenericCallbackFailsBeforeRegistryChanges ()
		{
			if (Environment.OSVersion.Platform != PlatformID.Win32NT)
				Assert.Ignore ("COM registration is only supported on Windows.");

			const string typeName = "MonoTests.RegistrationServices.GenericCallbackObject";
			const string methodName = "GenericRegister";
			Assembly assembly = LoadBoundaryTestAssembly ("RegistrationServicesGenericCallbackTestAssembly.dll");
			RegistrationServices services = new RegistrationServices ();

			InvalidOperationException error = Assert.Throws<InvalidOperationException> (() =>
				services.RegisterAssembly (assembly, AssemblyRegistrationFlags.None));
			StringAssert.Contains (assembly.FullName, error.Message, "assembly");
			StringAssert.Contains (typeName, error.Message, "type");
			StringAssert.Contains (methodName, error.Message, "method");
			StringAssert.Contains ("generic parameters", error.Message, "requirement");
			Assert.IsNull (Registry.ClassesRoot.OpenSubKey (GenericCallbackProgId), "no partial ProgID");
			Assert.IsNull (Registry.ClassesRoot.OpenSubKey ("CLSID\\" + GenericCallbackClassId), "no partial CLSID");
		}

		static Assembly LoadBoundaryTestAssembly (string name)
		{
			return Assembly.LoadFrom (Path.Combine (
				Path.GetDirectoryName (typeof (RegistrationServicesRegistryTest).Assembly.Location), name));
		}

		static Assembly LoadTestAssembly ()
		{
			return LoadBoundaryTestAssembly ("RegistrationServicesTestAssembly.dll");
		}

		static void RemoveTestKeys ()
		{
			Registry.ClassesRoot.DeleteSubKeyTree (ProgId, false);
			Registry.ClassesRoot.DeleteSubKeyTree ("CLSID\\" + ClassId, false);
			Registry.ClassesRoot.DeleteSubKeyTree (DerivedProgId, false);
			Registry.ClassesRoot.DeleteSubKeyTree ("CLSID\\" + DerivedClassId, false);
			Registry.ClassesRoot.DeleteSubKeyTree ("CLSID\\" + EmptyProgIdClassId, false);
			Registry.ClassesRoot.DeleteSubKeyTree (CallbackPath, false);
			Registry.ClassesRoot.DeleteSubKeyTree ("CLSID\\" + DynamicClassId, false);
			Registry.ClassesRoot.DeleteSubKeyTree (DynamicProgId, false);
			Registry.ClassesRoot.DeleteSubKeyTree (InvalidCallbackProgId, false);
			Registry.ClassesRoot.DeleteSubKeyTree ("CLSID\\" + InvalidCallbackClassId, false);
			Registry.ClassesRoot.DeleteSubKeyTree (GenericCallbackProgId, false);
			Registry.ClassesRoot.DeleteSubKeyTree ("CLSID\\" + GenericCallbackClassId, false);
		}

		sealed class ManagedCategoryState : IDisposable {
			readonly bool keyExisted;
			readonly RegistryValueState description;
			readonly RegistryValueState thirdPartyValue;
			bool restored;

			public ManagedCategoryState ()
			{
				using (RegistryKey key = Registry.ClassesRoot.OpenSubKey (ManagedCategoryPath)) {
					keyExisted = key != null;
					description = RegistryValueState.Capture (key, "0");
					thirdPartyValue = RegistryValueState.Capture (key, ThirdPartyValue);
				}

				try {
					using (RegistryKey key = Registry.ClassesRoot.CreateSubKey (ManagedCategoryPath)) {
						key.SetValue ("0", ManagedCategoryDescription);
						key.SetValue (ThirdPartyValue, "preserve");
					}
				} catch {
					Restore ();
					throw;
				}
			}

			public void Dispose ()
			{
				Restore ();
			}

			void Restore ()
			{
				if (restored)
					return;
				restored = true;

				RegistryKey key = Registry.ClassesRoot.OpenSubKey (ManagedCategoryPath, true);
				if (key == null && (description.Exists || thirdPartyValue.Exists))
					key = Registry.ClassesRoot.CreateSubKey (ManagedCategoryPath);
				if (key != null) {
					using (key) {
						description.Restore (key, "0");
						thirdPartyValue.Restore (key, ThirdPartyValue);
					}
				}

				if (!keyExisted)
					DeleteManagedCategoryIfEmpty ();
			}

			static void DeleteManagedCategoryIfEmpty ()
			{
				bool empty = false;
				using (RegistryKey key = Registry.ClassesRoot.OpenSubKey (ManagedCategoryPath)) {
					if (key != null)
						empty = key.SubKeyCount == 0 && key.ValueCount == 0;
				}
				if (empty)
					Registry.ClassesRoot.DeleteSubKey (ManagedCategoryPath, false);
			}
		}

		struct RegistryValueState {
			readonly bool exists;
			readonly object value;
			readonly RegistryValueKind kind;

			RegistryValueState (bool exists, object value, RegistryValueKind kind)
			{
				this.exists = exists;
				this.value = value;
				this.kind = kind;
			}

			public bool Exists {
				get { return exists; }
			}

			public static RegistryValueState Capture (RegistryKey key, string name)
			{
				if (key == null)
					return new RegistryValueState (false, null, RegistryValueKind.None);

				foreach (string valueName in key.GetValueNames ()) {
					if (String.Equals (valueName, name, StringComparison.OrdinalIgnoreCase))
						return new RegistryValueState (true, key.GetValue (name, null,
							RegistryValueOptions.DoNotExpandEnvironmentNames), key.GetValueKind (name));
				}
				return new RegistryValueState (false, null, RegistryValueKind.None);
			}

			public void Restore (RegistryKey key, string name)
			{
				if (exists)
					key.SetValue (name, value, kind);
				else
					key.DeleteValue (name, false);
			}
		}
	}
}

#endif
