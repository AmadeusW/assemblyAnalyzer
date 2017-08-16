using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace AA
{
    class Program
    {
        static IAnalyzer dllAnalyzer = new ReflectionAnalyzer();
        static IAnalyzer resourceAnalyzer = new ResourceAnalyzer();
        static IEnumerable<string> dllsToAnalyze;
        static IEnumerable<string> moreDlls = new List<string>();

        // Argument 1: Path to a single DLL to analyze or to a directory whose all DLLs will be analyzed
        // Argument 2: Path to directory that will be scanned for dependent assemblies
        // Argument 3: Path to output directory where artifacts will be written to
        // 
        // Sample args for debugging:
        // C:\git\platform\insertion D:\output\insertion C:\git\platform\src "C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.0\"
        // "C:\Program Files (x86)\Microsoft Visual Studio\Dog153\Enterprise\Common7\IDE\PrivateAssemblies\Microsoft.VisualStudio.Text.Internal.dll" C:\git\platform\output\programfiles "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE" "C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.0\UIAutomationProvider.dll"
        // D:\assemblies D:\output\programfiles "C:\Program Files (x86)\Microsoft Visual Studio\Dog153\Enterprise\Common7\IDE" "C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.0\UIAutomationProvider.dll"
        // \\scratch2\scratch\olegtk\insertion  D:\output\olegs "C:\Program Files (x86)\Microsoft Visual Studio\Dog153\Enterprise\Common7\IDE" "C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.0\UIAutomationProvider.dll"
        static void Main(string[] args)
        {
            if (args.Length < 3) throw new ArgumentException("Usage: AA AnalyzePath OutputPath [AssemblySearchPath1] [AssemblySearchPath2] ...");
            var sourcePath = args[0];
            var outputPath = args[1];
            for (int i = 2; i < args.Length; i++)
            {
                var moreDllsPath = args[i];
                if (!Directory.Exists(moreDllsPath)) throw new DirectoryNotFoundException($"Directory {moreDllsPath} does not exist");
                moreDlls = moreDlls.Union(Directory.EnumerateFiles(moreDllsPath, "*.dll", SearchOption.AllDirectories));
            }

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

            
            AppDomain.CurrentDomain.AssemblyResolve += OnReflectionOnlyAssemblyResolve;

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
                    //File.WriteAllText(savePath, dllAnalyzer.Analyze(dll));
                    File.WriteAllText(savePath, resourceAnalyzer.Analyze(dll));
                    Console.WriteLine($"OK: {name}");
                }
                catch (ReflectionTypeLoadException ex)
                {
                    foreach (var e in ex.LoaderExceptions)
                    {
                        Console.WriteLine($"Error: {name}: {e.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {name}: {ex.Message}");
                }
            }
        }

        private static Assembly OnReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyFullName = args.Name;
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            var shortName = assemblyFullName.Substring(0, assemblyFullName.IndexOf(','));

            foreach (var assembly in loadedAssemblies)
            {
                if (assembly.FullName == assemblyFullName)
                {
                    return Assembly.LoadFile(assemblyFullName);
                }
            }

            var assemblyCandidateFileName = assemblyFullName.Substring(0, assemblyFullName.IndexOf(',')) + ".dll";
            // First, scan the provided directory
            foreach (var dll in dllsToAnalyze)
            {
                if (dll.EndsWith(assemblyCandidateFileName))
                {
                    var loadedAssembly = Assembly.LoadFile(dll);
                    return loadedAssembly;
                }
            }
            // If this fails, scan the fallback location
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
