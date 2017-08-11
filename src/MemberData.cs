using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AA
{
    class MemberData
    {
        private EventInfo e;
        private ConstructorInfo c;

        public string Name { get; }
        public List<string> Modifiers { get; } = new List<string>();
        public string Type { get; }
        public string Kind { get; }

        public override string ToString()
        {
            return $"{Kind} {Name} {Type} {String.Join(" ", Modifiers.OrderBy(n => n))}";
        }

        public MemberData(string name)
        {
            Name = name;
        }

        public MemberData(FieldInfo info)
        {
            Name = info.Name;
            Kind = info.MemberType.ToString();
            Type = info.FieldType.Name;
            if (info.IsStatic) Modifiers.Add("static");
            if (info.IsPrivate) Modifiers.Add("private");
            if (info.IsPublic) Modifiers.Add("public");
            if (info.IsInitOnly) Modifiers.Add("readonly");
        }

        public MemberData(TypeInfo info)
        {
            Name = info.Name;
            Kind = info.MemberType.ToString();
            if (info.IsClass) Modifiers.Add("class");
            if (info.CustomAttributes.Any(n => n.ToString() == "[System.Runtime.CompilerServices.CompilerGeneratedAttribute()]")) Modifiers.Add("compilerGenerated");
            if (info.IsGenericType) Modifiers.Add("generic");
            if (info.IsPublic) Modifiers.Add("public");
        }

        public MemberData(MethodInfo info)
        {
            Name = info.Name + " (" + String.Join(", ", info.GetParameters().Select(n => n.ToString())) + " )";
            Kind = info.MemberType.ToString();
            Type = info.ReturnType.Name;
            if (info.IsStatic) Modifiers.Add("static");
            if (info.IsPrivate) Modifiers.Add("private");
            if (info.IsPublic) Modifiers.Add("public");
            if (info.IsFinal) Modifiers.Add("final");
            if (info.IsVirtual) Modifiers.Add("virtual");
            if (info.IsAbstract) Modifiers.Add("abstract");
        }

        public MemberData(EventInfo info)
        {
            Name = info.Name;
            Kind = info.MemberType.ToString();
            Type = info.EventHandlerType.Name;
            if (info.IsMulticast) Modifiers.Add("multicast");
        }

        public MemberData(ConstructorInfo info)
        {
            Name = info.Name + " (" + String.Join(", ", info.GetParameters().Select(n => n.ToString())) + ")";
            Kind = info.MemberType.ToString();
            if (info.IsStatic) Modifiers.Add("static");
            if (info.IsPublic) Modifiers.Add("public");
            if (info.IsPrivate) Modifiers.Add("private");
            if (info.IsFinal) Modifiers.Add("final");
            if (info.IsVirtual) Modifiers.Add("virtual");
            if (info.IsAbstract) Modifiers.Add("abstract");
        }
    }
}
