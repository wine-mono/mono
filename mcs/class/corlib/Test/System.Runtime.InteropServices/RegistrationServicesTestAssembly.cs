//
// RegistrationServicesTestAssembly.cs - COM registration test assembly
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
using System.Runtime.InteropServices;
using Microsoft.Win32;

[assembly: ComVisible (false)]
#if PRIMARY_INTEROP_ASSEMBLY
[assembly: Guid ("9E62C12A-0CB9-4B9A-AE9B-F4318AFD6A71")]
[assembly: PrimaryInteropAssembly (1, 0)]
#endif

namespace MonoTests.RegistrationServices {
	static class CallbackState {
		const string Path = "MonoTests.RegistrationServices.CallbackState";

		public static void Set (string name, string value)
		{
			using (RegistryKey key = Registry.ClassesRoot.CreateSubKey (Path))
				key.SetValue (name, value);
		}
	}

	[ComVisible (true)]
	[Guid ("5E4466A3-2BA4-414E-B70B-317D91BE57CC")]
	[ProgId ("MonoTests.RegistrationServices.TestObject")]
	public class TestObject {
		public TestObject ()
		{
		}

		[ComRegisterFunction]
		public static void Register (Type type)
		{
			CallbackState.Set ("Register", type.FullName);
		}

		[ComUnregisterFunction]
		public static void Unregister (Type type)
		{
			CallbackState.Set ("Unregister", type.FullName);
		}
	}

	[ComVisible (true)]
	[Guid ("F0439499-B07C-4FA5-BC3B-8402B70B3AFF")]
	[ProgId ("")]
	public class EmptyProgIdTestObject {
		public EmptyProgIdTestObject ()
		{
		}
	}

	[ComVisible (false)]
	public abstract class CallbackBase {
		[ComRegisterFunction]
		public static void RegisterBase (Type type)
		{
			CallbackState.Set ("DerivedRegister", "base");
		}

		[ComUnregisterFunction]
		public static void UnregisterBase (Type type)
		{
			CallbackState.Set ("DerivedUnregister", "base");
		}
	}

	[ComVisible (true)]
	[Guid ("641D963E-EA94-4E4F-B6EF-1DFEB43FB697")]
	[ProgId ("MonoTests.RegistrationServices.DerivedTestObject")]
	public class DerivedTestObject : CallbackBase {
		public DerivedTestObject ()
		{
		}

		[ComRegisterFunction]
		public static void RegisterDerived (string key)
		{
			CallbackState.Set ("DerivedRegister", key);
		}

		[ComUnregisterFunction]
		public static void UnregisterDerived (string key)
		{
			CallbackState.Set ("DerivedUnregister", key);
		}
	}
}

#endif
