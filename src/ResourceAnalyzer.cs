using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace AA
{
    class ResourceAnalyzer : IAnalyzer
    {
        const string ildasmLocation = @"C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0A\Bin\ildasm.exe";

        public string Analyze(string dllPath)
        {
            var dllName = Path.GetFileNameWithoutExtension(dllPath);
            var tempLocation = Path.Combine(Path.GetTempPath(), "AssemblyAnalyzer", dllName);
            CleanAndCreateDirectory(tempLocation);

            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = ildasmLocation;
            p.StartInfo.Arguments = $"{dllPath} /out={dllName}.txt /nobar";
            p.StartInfo.WorkingDirectory = tempLocation;
            p.Start();

            // To avoid deadlocks, always read the output stream first and then wait.
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            var sb = new IndentingStringBuilder();

            var availableResources = Directory.EnumerateFiles(tempLocation, "*.resources");
            if (!availableResources.Any())
            {
                sb.AppendLine("No embedded resources.");
                return sb.ToString();
            }

            sb.AppendLine("Resources:");
            foreach (var resource in availableResources)
            {
                sb.IncreaseIndentation();
                sb.AppendLine(Path.GetFileName(resource));

                var rr = new ResourceReader(resource);
                foreach (var r in rr)
                {
                    var d = (DictionaryEntry)r;
                    if (d.Value is Stream s)
                    {
                        var sr = new StreamReader(s);
                        sb.AppendLine($"{d.Key}: {s.Length}B Stream");
                        //sb.IncreaseIndentation();
                        //sb.AppendLine(sr.ReadToEnd());
                        //sb.DecreaseIndentation();
                    }
                    else
                    {
                        sb.AppendLine($"{d.Key}: {d.Value}");
                    }
                }
                sb.DecreaseIndentation();
            }

            return sb.ToString();
        }

        private void CleanDirectory(string path)
        {
            if (!Directory.Exists(path))
                return;

            var files = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    Console.WriteLine($"Unable to delete {file}");
                }
            }
            try
            {
                Directory.Delete(path);
            }
            catch
            {
                Console.WriteLine($"Unable to delete {path}");
            }
        }

        private void CleanAndCreateDirectory(string path)
        {
            CleanDirectory(path);
            Directory.CreateDirectory(path);
        }
    }
}
