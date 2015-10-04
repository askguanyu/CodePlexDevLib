namespace DevLib.Text.UnitTest
{
    using System;
    using System.Text;
    using DevLib.Diagnostics;
    using DevLib.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///This is a test class for Given_StringBuilderCache and is intended
    ///to contain all Given_StringBuilderCache Unit Tests
    ///</summary>
    [TestClass()]
    public class Given_StringBuilderCache
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        /// <summary>
        /// Usage scenario 1: how to use StringBuilderCache
        ///</summary>
        [TestMethod()]
        public void When_Scenario1()
        {
            StringBuilder source = StringBuilderCache.Acquire();
            source.Append("hello");
            source.Append("string");
            string actual = StringBuilderCache.GetStringAndRelease(source);
            string expected = "hellostring";

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        /// Benchmark: compare with StringBuilder
        /// </summary>
        [TestMethod()]
        public void When_Scenario2()
        {
            int loop = 100000;

            Benchmark.Initialize();

            Benchmark.Run(i =>
            {
                StringBuilder sb = new StringBuilder();

                for (int j = 0; j < 1; j++)
                {
                    sb.Append(DateTime.Now.ToLongDateString());
                    sb.Append(DateTime.Now.ToLongTimeString());
                    sb.AppendLine(j.ToString());
                }

                string result = sb.ToString();
            }, "StringBuilder", loop);

            Benchmark.Initialize();

            Benchmark.Run(i =>
            {
                StringBuilder sb = StringBuilderCache.Acquire();

                for (int j = 0; j < 1; j++)
                {
                    sb.Append(DateTime.Now.ToLongDateString());
                    sb.Append(DateTime.Now.ToLongTimeString());
                    sb.AppendLine(j.ToString());
                }

                string result = StringBuilderCache.GetStringAndRelease(sb);
            }, "StringBuilderCache", loop);
        }
    }
}
