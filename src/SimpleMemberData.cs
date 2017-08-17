using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AA
{
    class SimpleMemberData
    {
        public string Kind { get; set; }
        public string Content { get; set; }
        public List<SimpleMemberData> Nodes = new List<SimpleMemberData>();
        
        internal static SimpleMemberData FromIL(string buffer)
        {
            var data = FromBuffer(buffer);
            var niceKind = data.First().Substring(1, 1).ToUpper() + data.First().Substring(2);

            // Skip these attributes. They are associated with default constructors.
            if (data.Any(n => n.Contains("DebuggerBrowsableAttribute")))
                return null;

            var everythingElse = data.Skip(1).Cleanup();
            return new SimpleMemberData
            {
                Kind = niceKind,
                Content = String.Join(" ", everythingElse)
            };
        }

        static string[] FromBuffer(string buffer) => buffer.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        public override string ToString() => $"{Kind} {Content}";
    }

    public static class LinqExtensions
    {
        static string pattern = @">\S+__\S+'";
        static Regex r = new Regex(pattern, RegexOptions.IgnoreCase);

        public static IEnumerable<string> Cleanup(this IEnumerable<string> e)
        {
            foreach (var n in e)
                yield return  r.Replace(n, ">___'");
        }
    }
}
