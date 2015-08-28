using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDomain.Tests.CQRS
{
    public class DoSimple
    {
        public string Value { get; set; }
    }

    public class DoComplex
    {
        public int IntValue { get; set; }
        public string StringValue { get; set; }
    }

    public class DoSimpleStuff
    {
        public string Name { get; set; }
        public string Who { get; set; }
        public int Age { get; set; }
        public long Random { get; set; }
        public DateTime DateValue { get; set; }

        public static DoSimpleStuff Create()
        {
            return new DoSimpleStuff
            {
                Name = "The name of the simple stuff",
                Who = "Who did this stuff",
                Age = 15,
                Random = long.MaxValue,
                DateValue = new DateTime(2012, 12, 12, 12, 12, 12, 12)
            };
        }
    }

    public class DoComplexStuff
    {
        public string Name { get; set; }
        public DoSimpleStuff[] SimpleStuffs { get; set; }
        public int ZeroInt { get; set; }
        public long ZeroLong { get; set; }

        public static DoComplexStuff Create()
        {
            return new DoComplexStuff
            {
                Name = "The name of the complex stuff",
                ZeroInt = 0,
                ZeroLong = 0,
                SimpleStuffs = Enumerable.Range(0, 5).Select(i => DoSimpleStuff.Create()).ToArray()
            };
        }
    }

    public class DoGenericStuff<T>
    {
        public T Stuff { get; set; }
    }

    public class DoNonGenericStuff
    {
        public object Stuff { get; set; }
    }
}
