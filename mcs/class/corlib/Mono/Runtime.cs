//
// Mono Runtime gateway functions
//
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mono {

#if MOBILE || XAMMAC_4_5
	public
#endif
	static class Runtime
	{

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern void mono_runtime_install_handlers ();

#if MOBILE || XAMMAC_4_5
		public
#else
		internal
#endif
		static void InstallSignalHandlers ()
		{
			mono_runtime_install_handlers ();
		}

#if MOBILE || XAMMAC_4_5
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void mono_runtime_cleanup_handlers ();

		public static void RemoveSignalHandlers ()
		{
			mono_runtime_cleanup_handlers ();
		}
#endif

		// Should not be removed intended for external use
		// Safe to be called using reflection
		// Format is undefined only for use as a string for reporting
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
#if MOBILE || XAMMAC_4_5
		public
#else
		internal
#endif
		static extern string GetDisplayName ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern string GetNativeStackTrace (Exception exception);

		public static bool SetGCAllowSynchronousMajor (bool flag)
		{
			// No longer used
			return true;
		}

		static object dump = new object ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern string ExceptionToState_internal (Exception exc, out ulong portable_hash, out ulong unportable_hash);

		static Tuple<String, ulong, ulong>
		ExceptionToState (Exception exc)
		{
			ulong portable_hash;
			ulong unportable_hash;
			string payload_str = ExceptionToState_internal (exc, out portable_hash, out unportable_hash);

			return new Tuple<String, ulong, ulong> (payload_str, portable_hash, unportable_hash);
		}


#if !MOBILE 
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern void DisableMicrosoftTelemetry ();

		static void
		WriteStateToFile (Exception exc)
		{
			throw new PlatformNotSupportedException("Merp is no longer supported.");
		}

		static void SendMicrosoftTelemetry (string payload_str, ulong portable_hash, ulong unportable_hash)
		{
			throw new PlatformNotSupportedException("Merp is no longer supported.");
		}

		// Usage: 
		//
		// catch (Exception exc) {
		//   var monoType = Type.GetType ("Mono.Runtime", false);
		//   var m = monoType.GetMethod("SendExceptionToTelemetry", BindingFlags.NonPublic | BindingFlags.Static);
		//   m.Invoke(null, new object[] { exc });
		// }
		static void SendExceptionToTelemetry (Exception exc)
		{
			throw new PlatformNotSupportedException("Merp is no longer supported.");
		}

		// All must be set except for configDir_str
		static void EnableMicrosoftTelemetry (string appBundleID_str, string appSignature_str, string appVersion_str, string merpGUIPath_str, string unused /* eventType_str */, string appPath_str, string configDir_str)
		{
			throw new PlatformNotSupportedException("Merp is no longer supported.");
		}
#endif

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern string DumpStateSingle_internal (out ulong portable_hash, out ulong unportable_hash);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern string DumpStateTotal_internal (out ulong portable_hash, out ulong unportable_hash);

		static Tuple<String, ulong, ulong>
		DumpStateSingle ()
		{
			ulong portable_hash;
			ulong unportable_hash;
			string payload_str;
			lock (dump)
			{
				payload_str = DumpStateSingle_internal (out portable_hash, out unportable_hash);
			}

			return new Tuple<String, ulong, ulong> (payload_str, portable_hash, unportable_hash);
		}

		static Tuple<String, ulong, ulong>
		DumpStateTotal ()
		{
			ulong portable_hash;
			ulong unportable_hash;
			string payload_str;
			lock (dump)
			{
				payload_str = DumpStateTotal_internal (out portable_hash, out unportable_hash);
			}

			return new Tuple<String, ulong, ulong> (payload_str, portable_hash, unportable_hash);
		}

		static void RegisterReportingForAllNativeLibs ()
		{
			// not supported
		}

		static void RegisterReportingForNativeLib (string modulePathSuffix_str, string moduleName_str)
		{
			// not supported
		}

		static void EnableCrashReportLog (string directory_str)
		{
			// not supported
		}

		enum CrashReportLogLevel : int {
			MonoSummaryNone = 0,
			MonoSummarySetup,
			MonoSummarySuspendHandshake,
			MonoSummaryUnmanagedStacks,
			MonoSummaryManagedStacks,
			MonoSummaryStateWriter,
			MonoSummaryStateWriterDone,
			MonoSummaryMerpWriter,
			MonoSummaryMerpInvoke,
			MonoSummaryCleanup,
			MonoSummaryDone,

			MonoSummaryDoubleFault
		}

		static CrashReportLogLevel CheckCrashReportLog (string directory_str, bool clear)
		{
			// not supported
			return CrashReportLogLevel.MonoSummaryNone;
		}

		static long CheckCrashReportHash (string directory_str, bool clear)
		{
			// not supported
			return 0;
		}

		static string CheckCrashReportReason (string directory_str, bool clear)
		{
			// not supported
			return string.Empty;
		}

		static void AnnotateMicrosoftTelemetry (string key, string val)
		{
			throw new PlatformNotSupportedException("Merp is no longer supported.");
		}
	}
}
