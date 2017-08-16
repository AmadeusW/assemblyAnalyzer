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
        int assemblyId = 0;

        public string Analyze(string dllPath)
        {
            var dllName = Path.GetFileNameWithoutExtension(dllPath);
            //var tempLocation = Path.Combine(Path.GetTempPath(), "AssemblyAnalyzer", dllName);
            var tempLocation = Path.Combine(Path.GetTempPath(), "aa", (assemblyId++).ToString());
            CleanAndCreateDirectory(tempLocation);

            using (var p = new Process())
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = ildasmLocation;
                p.StartInfo.Arguments = $"{dllPath} /out={dllName}.txt /nobar";
                p.StartInfo.WorkingDirectory = tempLocation+"\\";
                p.Start();

                // To avoid deadlocks, always read the output stream first and then wait.
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
            }

            var sb = new IndentingStringBuilder();

            processIL(sb, tempLocation);
            processResources(sb, tempLocation);

            return sb.ToString();
        }

        private void processIL(IndentingStringBuilder sb, String tempLocation)
        {
            var ilFile = Directory.EnumerateFiles(tempLocation, "*.txt").Single();

            bool relevantContent = false;
            string previousLine = String.Empty;

            foreach (var rawLine in File.ReadLines(ilFile))
            {
                var line = rawLine.Trim();
                if (!String.IsNullOrEmpty(previousLine))
                {
                    if (previousLine.StartsWith(".class"))
                    {
                        var data = MemberData.ClassFromIL(previousLine, line);
                        sb.AppendLine(data.ToString());

                        relevantContent = true;
                        sb.IncreaseIndentation();
                    }
                    if (previousLine.StartsWith(".method"))
                    {
                        var data = MemberData.MethodFromIL(previousLine, line);
                        sb.AppendLine(data.ToString());
                    }
                    previousLine = String.Empty;
                }
                else
                {
                    if (line.StartsWith("}"))
                    {
                        if (relevantContent)
                        {
                            sb.DecreaseIndentation();
                            relevantContent = false;
                        }
                    }
                    else if (line.StartsWith(".assembly"))
                    {
                        sb.AppendLine($"Reference {line.Split(' ').Last()}");
                    }
                    else if (line.StartsWith(".class"))
                    {
                        previousLine = line;
                    }
                    else if (line.StartsWith(".method"))
                    {
                        previousLine = line;
                    }
                    else if (line.StartsWith(".property"))
                    {
                        previousLine = line;
                    }
                    else if (line.StartsWith(".field"))
                    {
                        var data = MemberData.FieldFromIL(line);
                        sb.AppendLine(data.ToString());
                    }
                    // todo: add constructor: .custom
                }
            }
        }

        private void processResources(IndentingStringBuilder sb, String tempLocation)
        {
            var availableResources = Directory.EnumerateFiles(tempLocation, "*.resources");
            if (!availableResources.Any())
            {
                sb.AppendLine("No embedded resources.");
                return;
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
