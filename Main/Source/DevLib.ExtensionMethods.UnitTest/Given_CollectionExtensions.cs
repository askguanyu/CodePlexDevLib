namespace DevLib.ExtensionMethods.UnitTest
{
    using DevLib.ExtensionMethods;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///This is a test class for Given_CollectionExtensions and is intended
    ///to contain all Given_CollectionExtensions Unit Tests
    ///</summary>
    [TestClass()]
    public class Given_CollectionExtensions
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
        ///A test for FindAllIndex
        ///</summary>
        [TestMethod()]
        public void When_FindAllIndex()
        {
            string source = "abcdefgabcdefg";
            List<int> expected = new List<int> { 0, 5, 7, 12 };
            List<int> actual;
            actual = CollectionExtensions.FindAllIndex(source, i => i == 'a' || i == 'f');

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void When_FindAllIndex1()
        {
            List<string> source = new List<string> { "abc", "def", "ghi", "abcde" };
            List<int> expected = new List<int> { 0, 3 };
            List<int> actual;
            actual = CollectionExtensions.FindAllIndex(source, i => i.Contains("a"));

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void When_ForEach()
        {
            Dictionary<int, string> source = new Dictionary<int, string>();
            source.Add(1, "a");
            source.Add(2, "B");

            source.ForEach((k, v) =>
            {
                Assert.IsInstanceOfType(k, typeof(int));
                Assert.IsInstanceOfType(v, typeof(string));
            });
        }

        /// <summary>
        ///A test for GetEnumerableElementType
        ///</summary>
        [TestMethod()]
        public void When_GetEnumerableElementType()
        {
            Type source = typeof(List<DateTime>);
            Type expected = typeof(DateTime);
            Type actual;
            actual = CollectionExtensions.GetEnumerableElementType(source);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetEnumerableElementType
        ///</summary>
        [TestMethod()]
        public void When_GetEnumerableElementType1()
        {
            Type source = typeof(char[]);
            Type expected = typeof(char);
            Type actual;
            actual = CollectionExtensions.GetEnumerableElementType(source);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsDictionary
        ///</summary>
        [TestMethod()]
        public void When_IsDictionary()
        {
            Type source = typeof(SortedDictionary<string, string>);
            bool expected = true;
            bool actual;
            actual = CollectionExtensions.IsDictionary(source);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IsEnumerable
        ///</summary>
        [TestMethod()]
        public void When_IsEnumerable()
        {
            Type source = typeof(LinkedList<int>);
            bool expected = true;
            bool actual;
            actual = CollectionExtensions.IsEnumerable(source);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void When_IsNotEmpty()
        {
            List<string> source = null;
            bool expected = false;
            bool actual;
            actual = CollectionExtensions.IsNotEmpty(source);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void When_IsNullOrEmpty()
        {
            List<string> source = null;
            bool expected = true;
            bool actual;
            actual = CollectionExtensions.IsNullOrEmpty(source);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void When_Safe()
        {
            List<string> source = null;
            var expected = Enumerable.Empty<string>();
            var actual = CollectionExtensions.Safe(source);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void When_SplitByEndsWith()
        {
        }

        [TestMethod()]
        public void When_SplitByStartsWith()
        {
        }

        [TestMethod()]
        public void When_Update()
        {
            Dictionary<int, string> source = new Dictionary<int, string>();
            source.Add(1, "a");
            source.Add(2, "b");

            string expected = "c";
            source.Update(2, "c");
            string actual=source[2];

            Assert.AreEqual(expected, actual);
        }
    }
}
