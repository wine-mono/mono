//
// This test merely creates a Win32Exception that triggers the
// code in mono/io-layer/message.c that validates that the
// error table is propertly sorted
using System;
using System.ComponentModel;

class X {
	static string msg (int c)
	{
		return new Win32Exception (c).Message;
	}

	static bool check (int c, params string[] s)
	{
		string actual = msg (c);
		foreach (string expected in s) {
			if (actual == expected)
				return true;
		}
		Console.Error.WriteLine ("For {0} expected {1} got {2}", c, s[0], actual);
		return false;
	}
	
	static int Main ()
	{
		//
		// All this test does is instantiate two Win32Exceptions
		// one with no known text, so it triggers a linear search
		// And one with a known message, to trigger a validation
		//
		// If stderr gets any output, there is a sorting error
		// in mono/io-layer/messages.c
		//
		Exception a = new Win32Exception (99999);
		a = new Win32Exception (9805);

		if (!check (2, "Cannot find the specified file", "The system cannot find the file specified.\r\n"))
			return 1;

		return 0;
	}
	
}
