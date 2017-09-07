using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Perimeter
{
    class Program
    {
        private static int levels = 12;

        /*
         * Translation error in body of
            'Perimeter.Main':
                BitwiseAnd called in a non-boolean context, but at least one argument is not an integer!
         *  SOLO PASA SI COMPILO EN DEBUG!
         */
        static void Main(string[] args)
        {
            int size = 1 << levels;
            int msize = 1 << (levels - 1);
        }
    }
}
