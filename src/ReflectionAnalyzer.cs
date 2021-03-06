﻿using System;
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
            var dll = Assembly.LoadFrom(dllPath);
            var sb = new IndentingStringBuilder();
            foreach (var type in dll.DefinedTypes.OrderBy(n => n.Name))
            {
                Analyze(sb, type);
            }
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
                throw new NotSupportedException($"Unknown member kind: {member.Name}");
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
            try
            {
                Analyze(sb, type.DeclaredConstructors);
                Analyze(sb, type.DeclaredMethods);
                Analyze(sb, type.DeclaredEvents);
                Analyze(sb, type.DeclaredFields);
                // sb.AppendLine("--- members:"); Analyze(sb, type.DeclaredMembers); sb.AppendLine("---"); // for debugging, reveal all members
                if (type.DeclaredNestedTypes.Any())
                {
                    Analyze(sb, type.DeclaredNestedTypes);
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                foreach (var e in ex.LoaderExceptions)
                {
                    sb.AppendLine($"Error: {type.Name}: {e.Message}");
                }
            }
            finally
            {
                sb.DecreaseIndentation();
            }
        }
    }
}
