using System;
using System.Collections.Generic;

namespace DirectAgents.Domain.Concrete
{
    static class Extensions
    {
        public static string[] SplitCSV(this string str)
        {
            return str.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
