/*
using System;

class A
{
	// In the il, remove "init" in the ".locals" of this function:
	// There might be a way to write this in C# directly, but it is not
	// straightforward.
	static unsafe byte* NoInit (int size, byte value, bool init)
	{
		byte* data = stackalloc byte [0x10000 + size];
		if (init)
		{
			for (int i = 0; i < size; ++i)
				data [i] = value;
		}
		else
		{
			for (int i = 0; i < size; ++i)
				data [i] += value;
		}
		return data;
	}

	static void assert (int a, int b, int c, bool f)
	{
		if (f)
			return;
		Console.WriteLine ("assert failed {0} {1} {2}", a, b, c);
		Environment.Exit (1);
	}

	unsafe public static void Main ()
	{
		int count = 123;
		byte value = 31;
		byte * data;
		byte * data2;

		data = NoInit (count, value, true);
		for (int i = 0; i < 100; ++i)
			assert (1, i, data [i], data [i] == value);

		data2 = NoInit (count, 0, false);
		// Previously this would work:
		// for (int i = 0; i < 100; ++i)
			// assert (2, i, data [i], data [i] == 0);

		assert (2, 0, (int)(data2 - data), data2 == data);

		// Now this works:
		for (int i = 0; i < 100; ++i)
			assert (3, i, data [i], data [i] == value);
	}
}
*/

.assembly extern mscorlib
{
  .ver 4:0:0:0
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 ) // .z\V.4..
}
.assembly 'localloc-noinit'
{
  .custom instance void class [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::'.ctor'() =  (
                01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
                63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01       ) // ceptionThrows.

  .permissionset reqmin = {[mscorlib]System.Security.Permissions.SecurityPermissionAttribute = {property bool 'SkipVerification' = bool(true)}}
  .hash algorithm 0x00008004
  .ver  0:0:0:0
}
.module 'localloc-noinit.exe' // GUID = {4900D196-730B-46C4-B656-74B1ADC00FAD}

.custom instance void class [mscorlib]System.Security.UnverifiableCodeAttribute::'.ctor'() =  (01 00 00 00 ) // ....


  .class private auto ansi beforefieldinit A
        extends [mscorlib]System.Object
  {

    // method line 1
    .method public hidebysig specialname rtspecialname 
           instance default void '.ctor' ()  cil managed 
    {
        // Method begins at RVA 0x2050
        // Code size 7 (0x7)
        .maxstack 8
        IL_0000:  ldarg.0 
        IL_0001:  call instance void object::'.ctor'()
        IL_0006:  ret 
    } // end of method A::.ctor

    // method line 2
    .method private static hidebysig 
           default unsigned int8* NoInit (int32 size, unsigned int8 'value', bool 'init')  cil managed 
    {
        // Method begins at RVA 0x2058
        // Code size 77 (0x4d)
        .maxstack 3
        .locals (
                unsigned int8*  V_0,
                int32   V_1,
                int32   V_2)
        IL_0000:  ldc.i4 65536
        IL_0005:  ldarg.0 
        IL_0006:  add 
        IL_0007:  ldc.i4.1 
        IL_0008:  mul.ovf.un 
        IL_0009:  localloc 
        IL_000b:  stloc.0 
        IL_000c:  ldarg.2 
        IL_000d:  brfalse IL_002f

        IL_0012:  ldc.i4.0 
        IL_0013:  stloc.1 
        IL_0014:  br IL_0023

        IL_0019:  ldloc.0 
        IL_001a:  ldloc.1 
        IL_001b:  conv.i 
        IL_001c:  add 
        IL_001d:  ldarg.1 
        IL_001e:  stind.i1 
        IL_001f:  ldloc.1 
        IL_0020:  ldc.i4.1 
        IL_0021:  add 
        IL_0022:  stloc.1 
        IL_0023:  ldloc.1 
        IL_0024:  ldarg.0 
        IL_0025:  blt IL_0019

        IL_002a:  br IL_004b

        IL_002f:  ldc.i4.0 
        IL_0030:  stloc.2 
        IL_0031:  br IL_0044

        IL_0036:  ldloc.0 
        IL_0037:  ldloc.2 
        IL_0038:  conv.i 
        IL_0039:  add 
        IL_003a:  dup 
        IL_003b:  ldind.u1 
        IL_003c:  ldarg.1 
        IL_003d:  add 
        IL_003e:  conv.u1 
        IL_003f:  stind.i1 
        IL_0040:  ldloc.2 
        IL_0041:  ldc.i4.1 
        IL_0042:  add 
        IL_0043:  stloc.2 
        IL_0044:  ldloc.2 
        IL_0045:  ldarg.0 
        IL_0046:  blt IL_0036

        IL_004b:  ldloc.0 
        IL_004c:  ret 
    } // end of method A::NoInit

    // method line 3
    .method private static hidebysig 
           default void 'assert' (int32 a, int32 b, int32 c, bool f)  cil managed 
    {
        // Method begins at RVA 0x20b1
        // Code size 42 (0x2a)
        .maxstack 8
        IL_0000:  ldarg.3 
        IL_0001:  brfalse IL_0007

        IL_0006:  ret 
        IL_0007:  ldstr "assert failed {0} {1} {2}"
        IL_000c:  ldarg.0 
        IL_000d:  box [mscorlib]System.Int32
        IL_0012:  ldarg.1 
        IL_0013:  box [mscorlib]System.Int32
        IL_0018:  ldarg.2 
        IL_0019:  box [mscorlib]System.Int32
        IL_001e:  call void class [mscorlib]System.Console::WriteLine(string, object, object, object)
        IL_0023:  ldc.i4.1 
        IL_0024:  call void class [mscorlib]System.Environment::Exit(int32)
        IL_0029:  ret 
    } // end of method A::assert

    // method line 4
    .method public static hidebysig 
           default void Main ()  cil managed 
    {
        // Method begins at RVA 0x20dc
        .entrypoint
        // Code size 133 (0x85)
        .maxstack 5
        .locals init (
                int32   V_0,
                unsigned int8   V_1,
                unsigned int8*  V_2,
                unsigned int8*  V_3,
                int32   V_4,
                int32   V_5)
        IL_0000:  ldc.i4.s 0x7b
        IL_0002:  stloc.0 
        IL_0003:  ldc.i4.s 0x1f
        IL_0005:  stloc.1 
        IL_0006:  ldloc.0 
        IL_0007:  ldloc.1 
        IL_0008:  ldc.i4.1 
        IL_0009:  call unsigned int8* class A::NoInit(int32, unsigned int8, bool)
        IL_000e:  stloc.2 
        IL_000f:  ldc.i4.0 
        IL_0010:  stloc.s 4
        IL_0012:  br IL_0034

        IL_0017:  ldc.i4.1 
        IL_0018:  ldloc.s 4
        IL_001a:  ldloc.2 
        IL_001b:  ldloc.s 4
        IL_001d:  conv.i 
        IL_001e:  add 
        IL_001f:  ldind.u1 
        IL_0020:  ldloc.2 
        IL_0021:  ldloc.s 4
        IL_0023:  conv.i 
        IL_0024:  add 
        IL_0025:  ldind.u1 
        IL_0026:  ldloc.1 
        IL_0027:  ceq 
        IL_0029:  call void class A::'assert'(int32, int32, int32, bool)
        IL_002e:  ldloc.s 4
        IL_0030:  ldc.i4.1 
        IL_0031:  add 
        IL_0032:  stloc.s 4
        IL_0034:  ldloc.s 4
        IL_0036:  ldc.i4.s 0x64
        IL_0038:  blt IL_0017

        IL_003d:  ldloc.0 
        IL_003e:  ldc.i4.0 
        IL_003f:  ldc.i4.0 
        IL_0040:  call unsigned int8* class A::NoInit(int32, unsigned int8, bool)
        IL_0045:  stloc.3 
        IL_0046:  ldc.i4.2 
        IL_0047:  ldc.i4.0 
        IL_0048:  ldloc.3 
        IL_0049:  ldloc.2 
        IL_004a:  sub 
        IL_004b:  conv.i8 
        IL_004c:  conv.i4 
        IL_004d:  ldloc.3 
        IL_004e:  ldloc.2 
        IL_004f:  ceq 
        IL_0051:  call void class A::'assert'(int32, int32, int32, bool)
        IL_0056:  ldc.i4.0 
        IL_0057:  stloc.s 5
        IL_0059:  br IL_007b

        IL_005e:  ldc.i4.3 
        IL_005f:  ldloc.s 5
        IL_0061:  ldloc.2 
        IL_0062:  ldloc.s 5
        IL_0064:  conv.i 
        IL_0065:  add 
        IL_0066:  ldind.u1 
        IL_0067:  ldloc.2 
        IL_0068:  ldloc.s 5
        IL_006a:  conv.i 
        IL_006b:  add 
        IL_006c:  ldind.u1 
        IL_006d:  ldloc.1 
        IL_006e:  ceq 
        IL_0070:  call void class A::'assert'(int32, int32, int32, bool)
        IL_0075:  ldloc.s 5
        IL_0077:  ldc.i4.1 
        IL_0078:  add 
        IL_0079:  stloc.s 5
        IL_007b:  ldloc.s 5
        IL_007d:  ldc.i4.s 0x64
        IL_007f:  blt IL_005e

        IL_0084:  ret 
    } // end of method A::Main

  } // end of class A

