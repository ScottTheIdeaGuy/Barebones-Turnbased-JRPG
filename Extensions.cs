using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JRPG
{
    public static class Extensions
    {
        public static string Capitalize(this string word) => word.Substring(0, 1).ToUpper() + word.Substring(1, word.Length - 1);
    }
}
