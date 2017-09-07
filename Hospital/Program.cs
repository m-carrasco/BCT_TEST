using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Health
{
    class Program
    {
        static void Main(string[] args)
        {
        }
    }

    /*
        BitwiseAnd called in a non-boolean context, but at least one argument is not an integer!
    */
    class Hospital
    {
        private int personnel;

        Hospital(int level)
        {
            personnel = 1 << (level - 1);
        }
    }
}
