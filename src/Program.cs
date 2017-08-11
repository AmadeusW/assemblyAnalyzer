using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Resources;

namespace AA
{
    class Program
    {
        static IAnalyzer dllAnalyzer = new ReflectionAnalyzer();
        static IAnalyzer resourceAnalyzer = new ResourceAnalyzer();
        static IEnumerable<string> dllsToAnalyze;
        static IEnumerable<string> moreDlls;

        // Argument 1: Path to a single DLL to analyze or to a directory whose all DLLs will be analyzed
        // Argument 2: Path to directory that will be scanned for dependent assemblies
        // Argument 3: Path to output directory where artifacts will be written to
        //
        // Sample args for debugging:
        // C:\git\platform\insertion C:\git\platform\src C:\git\platform\output
        // "C:\Program Files (x86)\Microsoft Visual Studio\Dog153\Enterprise\Common7\IDE\PrivateAssemblies\Microsoft.VisualStudio.Text.Internal.dll" "C:\Program Files (x86)\Microsoft Visual Studio\Dog153\Enterprise\Common7\IDE" C:\git\platform\output\programfiles
        // D:\assemblies "C:\Program Files (x86)\Microsoft Visual Studio\Dog153\Enterprise\Common7\IDE" D:\output\programfiles
        // \\scratch2\scratch\olegtk\insertion "C:\Program Files (x86)\Microsoft Visual Studio\Dog153\Enterprise\Common7\IDE" D:\output\olegs
        static void Main(string[] args)
        {
            if (args.Length != 3) throw new ArgumentException("Usage: AA AnalyzePath AssemblySearchPath OutputPath");
            var sourcePath = args[0].Trim();
            var moreDllsPath = args[1].Trim();
            var outputPath = args[2].Trim();

            if (!Directory.Exists(moreDllsPath)) throw new DirectoryNotFoundException($"Directory {moreDllsPath} does not exist");
            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

            if (File.Exists(sourcePath))
            {
                // Process single DLL
                dllsToAnalyze = new string[] { sourcePath };
            }
            else
            {
                // Process all DLLs in a directory
                if (!Directory.Exists(sourcePath)) throw new DirectoryNotFoundException($"Directory {sourcePath} does not exist");
                dllsToAnalyze = Directory.EnumerateFiles(sourcePath, "*.dll", SearchOption.AllDirectories);
            }

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
                    var savePath = Path.Combine(outputPath, name + ".txt");
                    File.WriteAllText(savePath, dllAnalyzer.Analyze(dll));
                    File.AppendAllText(savePath, resourceAnalyzer.Analyze(dll));
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
