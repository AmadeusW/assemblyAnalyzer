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

        static string[] FromBuffer(string buffer) => buffer.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        public static MemberData ClassFromIL(string buffer)
        {
            var data = FromBuffer(buffer);
            string type = String.Empty;
            if (data.Reverse().Skip(1).First() == "extends")
            {
                type = data.Reverse().Last();
                data = data.Reverse().Skip(2).Reverse().ToArray();
            }
            var kind = data.First();
            var name = data.Last();
            IEnumerable<string> modifiers;
            // TODO interface
            if (data.Skip(1).First() == "interface")
            {
                kind = "interface";
                type = String.Empty;
                modifiers = data.Skip(1).Reverse().Skip(1);
            }
            else
            {
                kind = "class";
                var typeData = buffer.Split(' ');
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

        public static MemberData MethodFromIL(string buffer)
        {
            var data = FromBuffer(buffer);
            var kind = "Method";
            //var signature = data.SkipWhile(n => n.Contains('('))
            int index = 0;
            int signatureStart = 0;
            int signatureEnd = 0;
            foreach (var dataPiece in data)
            {
                if (dataPiece.Contains('('))
                    signatureStart = index;
                if (dataPiece.Contains(')'))
                    signatureEnd = index;
                index++;
            }
            var type = data.ElementAt(signatureStart - 1);
            var name = String.Join(" ", data.Skip(signatureStart).Take(signatureEnd - signatureStart + 1));
            var modifiers = data.Skip(1).Take(signatureStart - 2);

            return new MemberData
            {
                Name = name,
                Kind = kind,
                Type = type,
                Modifiers = modifiers.ToList(),
            };
        }

        public static MemberData PropertyFromIL(string buffer)
        {
            var data = FromBuffer(buffer);
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

        private static MemberData AssemblyFromIL(string buffer)
        {
            return new MemberData
            {
                Name = FromBuffer(buffer).Last(),
                Kind = "Reference"
            };
        }

        private static MemberData ConstructorFromIL(string buffer)
        {
            return new MemberData
            {
                Name = FromBuffer(buffer).Reverse().Skip(2).First(),
                Kind = "Constructor"
            };
        }

        public static MemberData FieldFromIL(string buffer)
        {
            var data = FromBuffer(buffer);
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

        internal static MemberData FromIL(string buffer)
        {
            if (buffer.StartsWith(".assembly"))
            {
                return MemberData.AssemblyFromIL(buffer);
            }
            else if (buffer.StartsWith(".class"))
            {
                return MemberData.ClassFromIL(buffer);
            }
            else if (buffer.StartsWith(".method"))
            {
                return MemberData.MethodFromIL(buffer);
            }
            else if (buffer.StartsWith(".property"))
            {
                return MemberData.PropertyFromIL(buffer);
            }
            else if (buffer.StartsWith(".field"))
            {
                return MemberData.FieldFromIL(buffer);
            }
            return null;
            /*
            else if (buffer.StartsWith(".custom"))
            {
                return MemberData.ConstructorFromIL(buffer);
            }
            return new MemberData
            {
                Kind = "Unknown",
                Name = buffer,
            };
            */
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
