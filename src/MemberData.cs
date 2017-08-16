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

        public string Name { get; set; }
        public List<string> Modifiers { get; set; } = new List<string>();
        public string Type { get; set; }
        public string Kind { get; set; }

        public override string ToString()
        {
            return $"{Kind} {Name} {Type} {String.Join(" ", Modifiers.OrderBy(n => n))}";
        }

        public MemberData() { }

        public static MemberData ClassFromIL(string ildasmLine1, string ildasmLine2)
        {
            var data = ildasmLine1.Split(' ');
            var kind = data.First();
            var name = data.Last();
            string type;
            IEnumerable<string> modifiers;
            if (data.Skip(1).First() == "interface")
            {
                kind = "interface";
                type = String.Empty;
                modifiers = data.Skip(1).Reverse().Skip(1);
            }
            else
            {
                var typeData = ildasmLine2.Split(' ');
                type = typeData.Last();
                modifiers = data.Skip(2).Reverse().Skip(1);
            }

            return new MemberData
            {
                Name = name,
                Kind = kind,
                Type = type,
                Modifiers = modifiers.ToList(),
            };
        }

        public static MemberData MethodFromIL(string ildasmLine1, string ildasmLine2)
        {
            var data = ildasmLine1.Split(' ');
            var kind = "Method";
            var name = data.Last();
            var typeData = ildasmLine2.Split(' ');
            var type = typeData.Last();
            var modifiers = data.Skip(1);

            return new MemberData
            {
                Name = name,
                Kind = kind,
                Type = type,
                Modifiers = modifiers.ToList(),
            };
        }

        public static MemberData PropertyFromIL(string ildasmLine1)
        {
            var data = ildasmLine1.Split(' ');
            var kind = "Property";
            var name = data.Last();
            var type = data.Skip(2).First();
            var modifiers = data.Skip(1).First();

            return new MemberData
            {
                Name = name,
                Kind = kind,
                Type = type,
                Modifiers = new List<string> { modifiers },
            };
        }

        public static MemberData FieldFromIL(string ildasmLine1)
        {
            var data = ildasmLine1.Split(' ');
            var kind = "Field";
            var name = data.Last();
            var type = data.Skip(2).First();
            var modifiers = data.Skip(1).First();

            return new MemberData
            {
                Name = name,
                Kind = kind,
                Type = type,
                Modifiers = new List<string> { modifiers },
            };
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
