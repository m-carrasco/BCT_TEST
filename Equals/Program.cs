using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equals
{
    class Program
    {
        static void Main(string[] args)
        {
            Z.testZ();
            X.testX();
            Console.ReadLine();
        }
    }

    class Z
    {
        public static void testZ()
        {
            Z z1 = new Z();
            z1.a = 10;

            Z z2 = new Z();
            z2.a = 10;

            if (z1.Equals(z2))
                Console.WriteLine("They are equal");
            else
                Console.WriteLine("They are not equal"); // I replace this line witha an assert false

            // if you execute this code "They are equal" is printed
        }

        public override bool Equals(object obj)
        {
            Z zObj = obj as Z;
            if (zObj == null)
                return false;

            // bct & corral works fails!
            // docs: Equals on value types (stirng excluded) gives bitwise equality by default.
            //          Bitwise equality means the objects that are compared have the same binary representation.
            return zObj.a.Equals(a);
        }

        public override int GetHashCode()
        {
            return this.a;
        }

        int a = 0;
    }

    class X
    {
        public static void testX()
        {
            X x1 = new X();
            x1.a = 10;

            X x2 = new X();
            x2.a = 10;

            if (x1.Equals(x2))
                Console.WriteLine("They are equal");
            else
                Console.WriteLine("They are not equal"); // I replace this line witha an assert false

            // if you execute this code "They are equal" is printed
        }

        public override bool Equals(object obj)
        {
            X xObj = obj as X;
            if (xObj == null)
                return false;

            // bct & corral works good here
            return xObj.a == this.a;
        }

        public override int GetHashCode()
        {
            return this.a;
        }

        int a = 0;
    }

    /*
        code used to understand how comparison should work.

        class A
        {
            public int a;
        }

        static void testEqualVirtualFunction()
        {
            // The default implementation of Equals supports reference equality for reference types, and bitwise equality for value types. 
            // Reference equality means the object references that are compared refer to the same object.
            // Bitwise equality means the objects that are compared have the same binary representation.
            // For reference types other than string, == returns true if its two operands refer to the same object. 

            A a1 = new A();
            A a2 = new A();

            a1.a = 0;
            a2.a = 0;

            if (!a1.Equals(a2))
                Console.WriteLine("a1 != a2");

            if (a1.Equals(a1))
                Console.WriteLine("a1 == a1");

            A a3 = a1;
            if (a3.Equals(a1))
                Console.WriteLine("a3 == a1");
        }

        static void testEqualOperator()
        {
            // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/equality-comparison-operator
            // For predefined value types, the equality operator (==) returns true if the values of its operands are equal, false otherwise. 

            int a = 5;
            int b = 5;

            if (a == b)
                Console.WriteLine("a == b");

            int c = 6;
            if (!(a == c))
                Console.WriteLine("a != c");

            // For reference types other than string, == returns true if its two operands refer to the same object. 
            A a1 = new A();
            A a2 = new A();

            a1.a = 0;
            a2.a = 0;

            if (!(a1 == a2))
                Console.WriteLine("a1 != a2");

            A a3 = a1;
            if (a1 == a3)
                Console.WriteLine("a1 == a3");

            // For the string type, == compares the values of the strings.

            string a4 = "Hola";
            string a5 = "Hola";
            string a6 = "Hol";
            string a7 = a4;

            if (a4 == a5)
                Console.WriteLine("a4 == a5");

            if (!(a6 == a4))
                Console.WriteLine("a4 != a6");

            if (a4 == a7)
                Console.WriteLine("a4 == a7");
        }
        static void test1(A a1, A a2)
        {
            if (a1 == a2)
                Console.WriteLine("Test 1 true"); // esto lo remplazo por un assert
        }

        static void test2(A a1, A a2)
        {
            if (a1.Equals(a2))
                Console.WriteLine("Test 2 true"); // esto lo remplazo por un assert
        }

        static void test3(A a1, A a2)
        {
            if (Object.ReferenceEquals(a1, a2))
                Console.WriteLine("Test 3 true");
        }
    }*/

}
