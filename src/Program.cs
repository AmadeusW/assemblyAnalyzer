using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace AA
{
    class Program
    {
        static IAnalyzer dllAnalyzer = new ReflectionAnalyzer();
        static IEnumerable<string> allDlls;

        static void Main(string[] args)
        {
            if (args.Length != 2) throw new ArgumentException("Usage: AA SearchPath OutputPath");
            var sourcePath = args[0].Trim();
            var outputPath = args[1].Trim();

            if (!Directory.Exists(sourcePath)) throw new DirectoryNotFoundException($"Directory {sourcePath} does not exist");
            allDlls = Directory.EnumerateFiles(sourcePath, "*.dll", SearchOption.AllDirectories);

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            ProcessAllAssemblies(outputPath);
            Console.WriteLine("Done.");
            Console.ReadLine();
        }

        static void ProcessAllAssemblies(string outputPath)
        {
            foreach (var dll in allDlls)
            {
                var name = Path.GetFileNameWithoutExtension(dll);
                try
                {
                    var output = dllAnalyzer.Analyze(dll);
                    File.WriteAllText(Path.Combine(outputPath, name + ".txt"), output);
                    Console.WriteLine($"OK: {name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {name}: {ex.Message}");
                }
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyFullName = args.Name;
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var shortName = assemblyFullName.Substring(0, assemblyFullName.IndexOf(','));

            var candidateAssemblies = new List<Assembly>();
            foreach (var assembly in loadedAssemblies)
            {
                if (assembly.FullName == assemblyFullName)
                {
                    return assembly;
                }
            }

            foreach (var dll in allDlls)
            {
                if (dll.EndsWith(assemblyFullName + ".dll"))
                {
                    var loadedAssembly = Assembly.LoadFile(dll);
                    return loadedAssembly;
                }
            }

            return null;
        }
    }
}
