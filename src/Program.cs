using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace AA
{
    class Program
    {
        static IAnalyzer dllAnalyzer = new ReflectionAnalyzer();
        static IEnumerable<string> dllsToAnalyze;
        static IEnumerable<string> moreDlls;

        // Sample commandline args:
        // C:\git\platform\insertion C:\git\platform\src C:\git\platform\output
        static void Main(string[] args)
        {
            if (args.Length != 3) throw new ArgumentException("Usage: AA AnalyzePath AssemblySearchPath OutputPath");
            var sourcePath = args[0].Trim();
            var moreDllsPath = args[1].Trim();
            var outputPath = args[2].Trim();

            if (!Directory.Exists(sourcePath)) throw new DirectoryNotFoundException($"Directory {sourcePath} does not exist");
            dllsToAnalyze = Directory.EnumerateFiles(sourcePath, "*.dll", SearchOption.AllDirectories);
            moreDlls = Directory.EnumerateFiles(moreDllsPath, "*.dll", SearchOption.AllDirectories);

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            ProcessAllAssemblies(outputPath);
            Console.WriteLine("Done.");
            Console.ReadLine();
        }

        static void ProcessAllAssemblies(string outputPath)
        {
            foreach (var dll in dllsToAnalyze)
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

            var assemblyCandidateFileName = assemblyFullName.Substring(0, assemblyFullName.IndexOf(',')) + ".dll";

            foreach (var dll in moreDlls)
            {
                if (dll.EndsWith(assemblyCandidateFileName))
                {
                    var loadedAssembly = Assembly.LoadFile(dll);
                    return loadedAssembly;
                }
            }

            return null;
        }
    }
}
