using System;
using System.Runtime.InteropServices;

public class Test {

	[DllImport ("libtest", PreserveSig=false)]
	public static extern int mono_preserve_sig_test (int hr);

	[DllImport ("libtest", PreserveSig=false)]
	public static extern void mono_preserve_sig_test (int hr, out int result);

	public static int Main () {
		int result = -1;

		if (mono_preserve_sig_test (0) != 6)
			return 1;

		mono_preserve_sig_test (0, out result);
		if (result != 6)
			return 2;

		try {
			mono_preserve_sig_test (unchecked((int)0x80004001) /* E_NOTIMPL */);
			// should throw NotImplementedException
			return 3;
		}
		catch (NotImplementedException e) { }

		try {
			result = -1;
			mono_preserve_sig_test (unchecked((int)0x80004001) /* E_NOTIMPL */, out result);
			// should throw NotImplementedException
			return 4;
		}
		catch (NotImplementedException e) { }

		if (result != 6)
			return 5;

		return 0;
	}
}
