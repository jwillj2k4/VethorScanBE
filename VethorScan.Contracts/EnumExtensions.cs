using System;
using System.Collections.Generic;
using System.Linq;

namespace VethorScan.Contracts
{
    public static class EnumExtensions
    {
        public static bool ExactMatch(this NodeType op, params NodeType[] checkflags)
        {
            foreach (NodeType checkflag in checkflags)
            {
                if ((op & checkflag) != checkflag)
                    return false;
            }
            return true;
        }

        public static IEnumerable<Enum> ToList(this Enum e)
        {
            return Enum.GetValues(e.GetType()).Cast<Enum>().Where(e.HasFlag);
        }

        public static void ForEach(this Enum e, Action<Enum> action)
        {
            foreach (var a in e.ToList().AsParallel())
            {
                action(a);

            }
        }
    }
}