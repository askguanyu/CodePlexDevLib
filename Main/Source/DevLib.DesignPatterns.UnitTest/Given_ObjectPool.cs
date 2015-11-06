namespace DevLib.DesignPatterns.UnitTest
{

    using DevLib.DesignPatterns;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Text;

    [TestClass()]
    public class Given_ObjectPool
    {
        private TestContext testContextInstance;

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

        [TestMethod()]
        public void When_Acquire()
        {
            ObjectPool<StringBuilder> SUT = new ObjectPool<StringBuilder>(() => new StringBuilder(), null, ObjectPoolStoreMode.Queue, ObjectPoolLoadingMode.Eager);

            using (var stringBuilder = SUT.Acquire(i => i.Clear()))
            {
                ((StringBuilder)stringBuilder).Append("hello");
            }

            Assert.Inconclusive();
        }
    }
}
