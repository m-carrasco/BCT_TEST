using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NLucene
{
    class Program
    {
        static void Main(string[] args)
        {
        }

        public static string baseToString()
        {
            return "";
        }


        public static string ToString()
        {
            bool isStored = false;
            bool isIndexed = false;
            bool isTokenized = false;
            String name = "";
            String stringValue = "";
            String readerValue = "";

            if (isStored && isIndexed && !isTokenized)
                return "Keyword<" + name + ":" + stringValue + ">";

            //else if (isStored && !isIndexed && !isTokenized)
            //    return "Unindexed<" + name + ":" + stringValue + ">";
            //else if (isStored && isIndexed && isTokenized && stringValue != null)
            //    return "Text<" + name + ":" + stringValue + ">";
            //else if (!isStored && isIndexed && isTokenized && readerValue != null)
            //    return "Text<" + name + ":" + readerValue + ">";
            //else
            //    return baseToString();

            return baseToString();
        }
    }
}
