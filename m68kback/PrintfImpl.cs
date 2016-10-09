using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace m68kback
{
    public class PrintfImpl : IPrintf
    {
        public IList<string> PrintedStrings { get; set; }  = new List<string>();

        private uint n = 0;
        private IStackAccess stack;

        public uint printf(string str, IStackAccess stack)
        {
            this.stack = stack;
            n = 0;

            var rx = "%(s|d|\\d\\dX|\\d\\dx)";
            var regex = new Regex(rx);

            var matchEvaluator = new MatchEvaluator(ReplaceMatch);

            var newstr = regex.Replace(str, matchEvaluator);

            Console.WriteLine(newstr);

            PrintedStrings.Add(newstr);

            return n;
        }

        string ReplaceMatch(Match match)
        {
            int i = (int)n;
            n++;
            if (match.Value.Length > 2)
            {
                if (char.IsDigit(match.Value[1]))
                {
                    return stack.GetUint(i).ToString("X");
                }
            }

            switch (match.Value[1])
            {
                case 's':
                    return stack.GetString(i);
                case 'd':
                    return stack.GetUint(i).ToString();
            }

            throw new NotSupportedException();
        }
    }
}