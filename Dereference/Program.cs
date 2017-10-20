using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dereference
{
    class Program
    {
        static void Main(string[] args)
        {
            //test(3,5);
            // test1();
            //test2();
            //testList();
            // comment out the test you want to run
        }
        public static void testList()
        {
            List l = new List();
            l.Add(1);
            l.Add(2);
            l.Add(3);
            l.Add(4);
            l.Add(5);
            l.Delete(5);
            l.SumAll(); // boom - normal execution crashes here
            // fixing translation bugs of BCT corral works as expected.
        }
        public static void test(int x, int y)
        {
            A aRef = getNull();

            // always true
            if ((x + y) * (x + y) == x * x + y * y + x * y * 2)
                aRef = new A();

            // works good
            aRef.id = aRef.id + 1;
        }

        public static void test1()
        {
            A aRef = getNull();

            //for (int i = 0; i < 4; i++)
            //{

            //}
            int i = 0;
            while (true){
                i++;
                if (i%10==0)
                {
                    aRef = new A();
                    break;
                }
            }

            // detects null pointer exception
            aRef.id = aRef.id + 1;
        }

        public static void test2()
        {
            A aRef = getNull();

            complexMethod();

            // detects null pointer exception
            aRef.id = aRef.id + 1;
        }

        public static void complexMethod()
        {

        }

        public static void test3(int x, int y)
        {
            A aRef = getNull();

            if ((x + y) * (x + y) == x * x + y * y + x * y * 2 + 5) // it is not always true, +5
                aRef = new A();

            // works good - detects null pointer exception
            aRef.id = aRef.id + 1;
        }

        // method used to trick the compiler
        public static A getNull()
        {
            return null;
        }
    }

    class A
    {
        public int id;
    }


    public class List
    {
        Node head;

        public void Add(int x)
        {
            Node n = new Node();
            n.value = new Value();
            n.value.value = x;

            if (head == null)
            {
                head = n;
                return;
            }

            n.next = head;
            head = n;
        }

        // deletes the first node that has the specified value
        // this method will have an intended bug
        public void Delete(int value)
        {
            Node n = head;
            while (n != null)
            {
                if (n.value.value == value)
                {
                    // here is a "bug" 
                    // the node is not removed
                    // the node's value object is set to null
                    n.value = null;
                    return;
                }

                n = n.next;
            }
        }

        public int SumAll()
        {
            int i = 0;

            Node n = head;
            while (n != null)
            {
                i = i + n.value.value;
                n = n.next;
            }

            return i;
        }
    }

    public class Value
    {
        public int value;
    }
    public class Node
    {
        public Node next;
        public Value value;
    }
}
