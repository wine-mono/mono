using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Runtime.InteropServices
{
	partial class Marshal
	{
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static IntPtr AllocCoTaskMem (int cb);
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static IntPtr AllocHGlobal (IntPtr cb);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static void FreeBSTR (IntPtr ptr);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static void FreeCoTaskMem (IntPtr ptr);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static void FreeHGlobal (IntPtr hglobal);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public static extern int GetLastWin32Error ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static void DestroyStructure (IntPtr ptr, Type structuretype);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static IntPtr OffsetOf (Type t, string fieldName);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static string PtrToStringBSTR (IntPtr ptr);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static IntPtr ReAllocCoTaskMem (IntPtr pv, int cb);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static IntPtr ReAllocHGlobal (IntPtr pv, IntPtr cb);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static void StructureToPtr (object structure, IntPtr ptr, bool fDeleteOld);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static IntPtr UnsafeAddrOfPinnedArrayElement (Array arr, int index);

		public static IntPtr UnsafeAddrOfPinnedArrayElement<T> (T[] arr, int index)
		{
			if (arr == null)
				throw new ArgumentNullException (nameof(arr));

			return UnsafeAddrOfPinnedArrayElement ((Array)arr, index);
		}

		internal static IntPtr AllocBSTR (int length)
		{
			throw new NotImplementedException ();
		}

		internal static bool IsPinnable (object obj)
		{
			throw new NotImplementedException ();
		}

		// TODO: Should be called from Windows only code
		internal static void SetLastWin32Error (int error)
		{
		}

		static Exception GetExceptionForHRInternal (int errorCode, IntPtr errorInfo)
		{
			throw new NotImplementedException ();
		}

		static void PrelinkCore (MethodInfo m)
		{
			if (!(m is MonoMethod))
			{
				throw new ArgumentException(SR.Argument_MustBeRuntimeMethodInfo, nameof(m));
			}

			PrelinkInternal (m);
		}

		static void PtrToStructureHelper (IntPtr ptr, object structure, bool allowValueClasses)
		{
			throw new NotImplementedException ();
		}

		static object PtrToStructureHelper (IntPtr ptr, Type structureType)
		{
			throw new NotImplementedException ();
		}

		[MethodImpl (MethodImplOptions.InternalCall)]
		static extern Delegate GetDelegateForFunctionPointerInternal (IntPtr ptr, Type t);

		[MethodImpl (MethodImplOptions.InternalCall)]
		static extern IntPtr GetFunctionPointerForDelegateInternal (Delegate d);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		extern static void PrelinkInternal (MethodInfo m);

		[MethodImpl (MethodImplOptions.InternalCall)]
		static extern int SizeOfHelper (Type t, bool throwIfNotMarshalable);

		static int GetSystemMaxDBCSCharSize ()
		{
			// TODO: Windows specific
			return 2;
		}

		public static IntPtr GetExceptionPointers ()
		{
			throw new NotImplementedException ();
		}

		// TODO: Implement in shared
		public static void ZeroFreeBSTR (IntPtr s)
		{
			throw new NotImplementedException ();
		}

		public static void ZeroFreeCoTaskMemAnsi (IntPtr s)
		{
			throw new NotImplementedException ();
		}

		public static void ZeroFreeCoTaskMemUnicode (IntPtr s)
		{
			throw new NotImplementedException ();
		}

		public static void ZeroFreeCoTaskMemUTF8 (IntPtr s)
		{
			throw new NotImplementedException ();
		}
		
		public static void ZeroFreeGlobalAllocAnsi (IntPtr s)
		{
			throw new NotImplementedException ();
		}

		public static void ZeroFreeGlobalAllocUnicode (IntPtr s)
		{
			throw new NotImplementedException ();
		}

		public static IntPtr StringToBSTR(string s) { throw null; }
		public static IntPtr StringToCoTaskMemAnsi(string s) { throw null; }
		public static IntPtr StringToCoTaskMemAuto(string s) { throw null; }
		public static IntPtr StringToCoTaskMemUni(string s) { throw null; }
		public static IntPtr StringToCoTaskMemUTF8(string s) { throw null; }
		public static IntPtr StringToHGlobalAnsi(string s) { throw null; }
		public static IntPtr StringToHGlobalAuto(string s) { throw null; }
		public static IntPtr StringToHGlobalUni(string s) { throw null; }

		//

		#region PlatformNotSupported

		public static int GetExceptionCode()
		{
			// Obsolete
			throw new PlatformNotSupportedException ();
		}

		public static byte ReadByte(Object ptr, int ofs)
		{
			// Obsolete
			throw new PlatformNotSupportedException ();
		}

		public static short ReadInt16(Object ptr, int ofs)
		{
			// Obsolete
			throw new PlatformNotSupportedException ();
		}

		public static int ReadInt32(Object ptr, int ofs)
		{
			// Obsolete
			throw new PlatformNotSupportedException ();
		}

		public static long ReadInt64(Object ptr, int ofs)
		{
			// Obsolete
			throw new PlatformNotSupportedException ();
		}

		public static void WriteByte(Object ptr, int ofs, byte val)
		{
			// Obsolete
			throw new PlatformNotSupportedException ();
		}

		public static void WriteInt16(Object ptr, int ofs, short val)
		{
			// Obsolete
			throw new PlatformNotSupportedException ();
		}

		public static void WriteInt32(Object ptr, int ofs, int val)
		{
			// Obsolete
			throw new PlatformNotSupportedException ();
		}

		public static void WriteInt64(Object ptr, int ofs, long val)
		{
			// Obsolete
			throw new PlatformNotSupportedException ();
		}

		#endregion
	}
}