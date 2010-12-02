using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace p2p_DNS
{
    public static class Extensions
    {
        public static IEnumerable<string> OmitComments(this IEnumerable<string> strings, params string[] commentCharacters)
        {
            foreach (var line in strings.Where(l => l.Length > 0))
            {
                int i = int.MaxValue;
                foreach (var commentMarker in commentCharacters)
                {
                    int index = line.IndexOf(commentMarker);
                    i = index == -1 ? i : index;
                }

                if (i == int.MaxValue || i == -1)
                    yield return line;
                else if (i > 0)
                    yield return line.Substring(0, i);
            }
        }
    }
}
