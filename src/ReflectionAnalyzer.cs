using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace AA
{
    class ReflectionAnalyzer : IAnalyzer
    {
        public string Analyze(string dllPath)
        {
            var dll = Assembly.LoadFile(dllPath);
            var sb = new StringBuilder();


            return sb.ToString();
        }
    }
}
