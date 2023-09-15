using System;
using System.Reflection;
using System.IO;

namespace Test
{
    internal class Program
    {
        static int Main()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            var asm = Assembly.Load("assemblyresolve_event8_helper, Version=0.0.0.0, Culture=neutral");
            var asm2 = Assembly.Load("assemblyresolve_event8_helper, Version=0.0.0.0, Culture=neutral");

            var instance = asm.CreateInstance("NullLibrary.Class1", true);

            var method = asm.GetType("NullLibrary.Class1").GetMethod("DoThing");
            method.Invoke(instance, null);

            return asm == asm2 ? 0 : 1;
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name);
            if (name.Name == "assemblyresolve_event8_helper")
            {
                var bytes = File.ReadAllBytes("assemblyresolve_deps/assemblyresolve_event8_helper2.dll");
                var asm = Assembly.Load(bytes);
                Console.WriteLine("requested " + name);
                Console.WriteLine("loaded GetName " + asm.GetName());
                Console.WriteLine("loaded ToString " + asm);
                return asm;
            }

            return null;
        }
    }
}
