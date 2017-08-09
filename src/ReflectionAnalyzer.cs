using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            sb.AppendLine("");
            sb.AppendLine("---");
            sb.AppendLine("DefinedTypes");
            sb.AppendLine("---");
            foreach (var type in dll.DefinedTypes)
            {
                Analyze(sb, type);
            }

            sb.AppendLine("");
            sb.AppendLine("---");
            sb.AppendLine("ExportedTypes");
            sb.AppendLine("---");
            foreach (var type in dll.ExportedTypes)
            {
                Analyze(sb, type);
            }

            sb.AppendLine("");
            sb.AppendLine("---");
            sb.AppendLine("GetManifestResourceNames");
            sb.AppendLine("---");
            dll.GetManifestResourceNames().Select(n => sb.AppendLine(n));

            return sb.ToString();
        }

        private void Analyze(StringBuilder sb, MemberInfo member)
        {
            if (member is FieldInfo i)
                sb.AppendLine(new MemberData(i).ToString());
            else if (member is MethodInfo m)
                sb.AppendLine(new MemberData(m).ToString());
            else if (member is EventInfo e)
                sb.AppendLine(new MemberData(e).ToString());
            else
                sb.AppendLine(new MemberData(member.Name).ToString());
        }

        private void Analyze(StringBuilder sb, IEnumerable<MemberInfo> members)
        {
            members.All(n => { Analyze(sb, n); return true; });
        }

        private void Analyze(StringBuilder sb, TypeInfo type)
        {
            sb.AppendLine(new MemberData(type).ToString());

            sb.AppendLine("- DeclaredConstructors");
            Analyze(sb, type.DeclaredConstructors);
            sb.AppendLine("- DeclaredMethods");
            Analyze(sb, type.DeclaredMethods);
            sb.AppendLine("- DeclaredEvents");
            Analyze(sb, type.DeclaredEvents);
            sb.AppendLine("- DeclaredFields");
            Analyze(sb, type.DeclaredFields);
            sb.AppendLine("- DeclaredMembers");
            Analyze(sb, type.DeclaredMembers);
        }
    }
}
