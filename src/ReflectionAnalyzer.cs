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
            var sb = new IndentingStringBuilder();
            foreach (var type in dll.DefinedTypes.OrderBy(n => n.Name))
            {
                Analyze(sb, type);
            }

            sb.AppendLine("");
            sb.AppendLine("-----");
            sb.AppendLine("Resources:");
            sb.IncreaseIndentation();
            dll.GetManifestResourceNames().OrderBy(n => n).All(n => { sb.AppendLine(n); return true; });
            sb.DecreaseIndentation();

            return sb.ToString();
        }

        private void Analyze(IndentingStringBuilder sb, MemberInfo member)
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

        private void Analyze(IndentingStringBuilder sb, IEnumerable<MemberInfo> members)
        {
            members.OrderBy(n => n.Name).All(n => { Analyze(sb, n); return true; });
        }

        private void Analyze(IndentingStringBuilder sb, IEnumerable<TypeInfo> nestedTypes)
        {
            nestedTypes.OrderBy(n => n.Name).All(n => { Analyze(sb, n); return true; });
        }

        private void Analyze(IndentingStringBuilder sb, TypeInfo type)
        {
            sb.AppendLine(new MemberData(type).ToString());
            sb.IncreaseIndentation();

            Analyze(sb, type.DeclaredConstructors);
            Analyze(sb, type.DeclaredMethods);
            Analyze(sb, type.DeclaredEvents);
            Analyze(sb, type.DeclaredFields);
            // sb.AppendLine("--- members:"); Analyze(sb, type.DeclaredMembers); sb.AppendLine("---"); // for debugging, reveal all members
            if (type.DeclaredNestedTypes.Any())
            {
                Analyze(sb, type.DeclaredNestedTypes);
            }

            sb.DecreaseIndentation();
        }
    }
}
