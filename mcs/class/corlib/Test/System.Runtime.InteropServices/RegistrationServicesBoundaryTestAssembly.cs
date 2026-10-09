//
// RegistrationServicesBoundaryTestAssembly.cs - COM registration boundary fixtures
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
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: ComVisible (false)]
#if VERSION_ONE
[assembly: AssemblyVersion ("1.0.0.0")]
#elif VERSION_TWO
[assembly: AssemblyVersion ("2.0.0.0")]
#endif

namespace MonoTests.RegistrationServices {
#if VERSION_ONE || VERSION_TWO
	[ComVisible (true)]
	[Guid ("5C9782F8-3BAA-42C7-A461-B8C94F2FA438")]
	public class VersionedObject {
		public VersionedObject ()
		{
		}
	}
#elif INVALID_CALLBACK
	[ComVisible (true)]
	[Guid ("F81E4AE1-917F-43C8-BD29-A56B182287AA")]
	public class InvalidCallbackObject {
		public InvalidCallbackObject ()
		{
		}

		[ComRegisterFunction]
		public static int InvalidRegister (Type type)
		{
			return 0;
		}
	}
#elif GENERIC_CALLBACK
	[ComVisible (true)]
	[Guid ("86F43A7E-B430-4F56-9C6C-DD61BA460217")]
	public class GenericCallbackObject {
		public GenericCallbackObject ()
		{
		}

		[ComRegisterFunction]
		public static void GenericRegister<T> (Type type)
		{
		}
	}
#endif
}

#endif
