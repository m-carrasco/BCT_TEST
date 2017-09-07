using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH
{
    class Program
    {
        static void Main(string[] args)
        {
        }

        /*
          Translation error in body of
                'BCT_TEST.Program.oldSubindex':
                    BitwiseAnd called in a non-boolean context, but at least one argument is
             not an integer! 
        */

        public static int oldSubindex(int n, int l, int cell_nsub)
        {
            int i = 0;
            for (int k = 0; k < n; k++)
            {
                int icValue = value(k);
                if ((icValue & l) != 0)
                    i += cell_nsub >> (k + 1);
                }
            return i;
        }

        public static int value(int k) { return k; }
    }
}
