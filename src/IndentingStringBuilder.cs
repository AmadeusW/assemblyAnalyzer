using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AA
{
    class IndentingStringBuilder
    {
        private int indentation;
        private StringBuilder builder = new StringBuilder();
        private char indent = '\t';

        public void IncreaseIndentation()
        {
            indentation++;
        }

        public void DecreaseIndentation()
        {
            if (indentation == 0) return;
            indentation--;
        }

        public void Append(string text)
        {
            builder.Append(indent, indentation);
            builder.Append(text);
        }

        public void AppendWithoutIndent(string text)
        {
            builder.Append(text);
        }

        public void AppendLine(string text)
        {
            builder.Append(indent, indentation);
            builder.AppendLine(text);
        }

        public void AppendLineWithoutIndent(string text)
        {
            builder.AppendLine(text);
        }

        public override string ToString()
        {
            return builder.ToString();
        }
    }
}
