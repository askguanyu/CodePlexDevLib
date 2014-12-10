namespace DevLib.Samples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;


    public class WcfTest : IWcfTest
    {
        List<Animal> animals = new List<Animal>();

        public string MyOperation1(string arg1, int arg2)
        {
            return arg1 + arg2.ToString();
        }

        public void MyOperation2(string value)
        {
        }


        public object Foo(string value)
        {
            return value;
        }


        public void AddAnimal(Animal value)
        {
            Console.WriteLine("call AddAnimal");
            animals.Add(value);
            Console.WriteLine(value.Name);
        }
    }
}
