namespace DevLib.Samples
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;


    public class WcfTest : IWcfTest
    {
        public string MyOperation1(string arg1, int arg2)
        {
            return arg1 + arg2.ToString();
        }

        public void MyOperation2(string value)
        {
        }
    }
}
