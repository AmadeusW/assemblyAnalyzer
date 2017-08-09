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
            Kind = "Field";
            Type = info.FieldType.Name;
            if (info.IsStatic) Modifiers.Add("static");
            if (info.IsPrivate) Modifiers.Add("private");
            if (info.IsPublic) Modifiers.Add("public");
        }

        public MemberData(TypeInfo info)
        {
            Name = info.Name;
            Kind = "Type";
            if (info.IsClass) Modifiers.Add("class");
            if (info.IsGenericType) Modifiers.Add("generic");
            if (info.IsPublic) Modifiers.Add("public");
        }

        public MemberData(MethodInfo info)
        {
            Name = info.Name;
            Kind = "Method";
            Type = info.ReturnType.Name;
            if (info.IsStatic) Modifiers.Add("static");
            if (info.IsPrivate) Modifiers.Add("private");
            if (info.IsPublic) Modifiers.Add("public");
        }

        public MemberData(PropertyInfo info)
        {
            Name = info.Name;
            Kind = "Property";
            Type = info.PropertyType.Name;
            if (info.GetMethod?.IsStatic == true || info.SetMethod?.IsStatic == true) Modifiers.Add("static");
            if (info.GetMethod?.IsPublic == true) Modifiers.Add("publicGet");
            if (info.SetMethod?.IsPublic == true) Modifiers.Add("publicSet");
            if (info.GetMethod?.IsPrivate == true) Modifiers.Add("privateGet");
            if (info.SetMethod?.IsPrivate == true) Modifiers.Add("privateSet");
        }
    }
}
