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

            int currentIndentation = 0; // what brace level are we on
            Stack<int> relevantBlocks = new Stack<int>(); // which braces to process
            relevantBlocks.Push(0); // we are always interested in the base level
            string buffer = String.Empty; // here we place processed lines
            bool fillingBuffer = false; // are we adding lines to buffer?

            foreach (var rawLine in File.ReadLines(ilFile))
            {
                var line = rawLine.Trim();
                var top = relevantBlocks.Peek();
                if (line.StartsWith(".") && relevantBlocks.Peek() == currentIndentation)
                {
                    if (fillingBuffer)
                    {   // We reached the next item
                        fillingBuffer = false;
                        // Process buffer:
                        var data = MemberData.FromIL(buffer);
                        buffer = String.Empty;
                        if (data != null)
                            sb.AppendLine(data.ToString());
                    }
                    fillingBuffer = true;
                }
                else if (line.StartsWith("{"))
                {
                    currentIndentation++;
                    if (fillingBuffer)
                    { // We reached the end of the current declaration
                        fillingBuffer = false;
                        // Process buffer:
                        var data = MemberData.FromIL(buffer);
                        buffer = String.Empty;
                        if (data != null)
                        {
                            sb.AppendLine(data.ToString());
                            if (data.Kind == "Class" || data.Kind == "Interface")
                            {
                                sb.IncreaseIndentation();
                                relevantBlocks.Push(currentIndentation); // we are interested in next indentation level
                            }
                        }
                    }
                }
                else if (line.StartsWith("}"))
                {
                    if (relevantBlocks.Peek() == currentIndentation)
                        relevantBlocks.Pop();

                    currentIndentation--;
                    // End of the block

                    if (fillingBuffer)
                    { // We reached the end of the current declaration
                        fillingBuffer = false;
                        // Process buffer:
                        var data = MemberData.FromIL(buffer);
                        buffer = String.Empty;
                        if (data != null)
                            sb.AppendLine(data.ToString());
                    }
                }

                if (fillingBuffer)
                {
                    buffer += line + " ";
                }
            }
        }

        private void processBuffer()
        {
            throw new NotImplementedException();
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
