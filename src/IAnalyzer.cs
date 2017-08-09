using System;
using System.Collections.Generic;
using System.Text;

namespace AA
{
    interface IAnalyzer
    {
        void Analyze(string dllPath, string outputPath);
    }
}
