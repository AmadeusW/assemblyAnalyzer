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
            else if (member is ConstructorInfo c)
                sb.AppendLine(new MemberData(c).ToString());
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

        private void Analyze(StringBuilder sb, IEnumerable<TypeInfo> nestedTypes)
        {
            nestedTypes.All(n => { Analyze(sb, n); return true; });
        }

        private void Analyze(StringBuilder sb, TypeInfo type)
        {
            sb.AppendLine(new MemberData(type).ToString());

            Analyze(sb, type.DeclaredConstructors);
            Analyze(sb, type.DeclaredMethods);
            Analyze(sb, type.DeclaredEvents);
            Analyze(sb, type.DeclaredFields);
            // sb.AppendLine("--- members:"); Analyze(sb, type.DeclaredMembers); sb.AppendLine("---"); // for debugging, reveal all members
            if (type.DeclaredNestedTypes.Any())
            {
                sb.AppendLine("--- nested types:");
                Analyze(sb, type.DeclaredNestedTypes);
            }
        }
    }
}
