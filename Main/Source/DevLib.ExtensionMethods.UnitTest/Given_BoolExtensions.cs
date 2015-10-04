namespace DevLib.ExtensionMethods.UnitTest
{
    using System;
    using DevLib.ExtensionMethods;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///This is a test class for Given_BoolExtensions and is intended
    ///to contain all Given_BoolExtensions Unit Tests
    ///</summary>
    [TestClass()]
    public class Given_BoolExtensions
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
        ///A test for BitIntToBool
        ///</summary>
        [TestMethod()]
        public void When_BitIntToBool()
        {
            int source = 0; // TODO: Initialize to an appropriate value
            bool expected = false; // TODO: Initialize to an appropriate value
            bool actual;
            actual = BoolExtensions.BitIntToBool(source);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for BitStringToBool
        ///</summary>
        [TestMethod()]
        public void When_BitStringToBool()
        {
            string source = "1";
            bool expected = true;
            bool actual;
            actual = BoolExtensions.BitStringToBool(source);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IfFalse
        ///</summary>
        [TestMethod()]
        public void When_IfFalse()
        {
            bool source = false;
            bool expected = false;
            bool actual;
            actual = BoolExtensions.IfFalse(source, () => Assert.AreEqual(expected, source));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for IfTrue
        ///</summary>
        [TestMethod()]
        public void When_IfTrue()
        {
            bool source = true;
            bool expected = true;
            bool actual;
            actual = BoolExtensions.IfTrue(source, () => Assert.AreEqual(expected, source));
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ToBitInt
        ///</summary>
        [TestMethod()]
        public void When_ToBitInt()
        {
            bool source = false;
            int expected = 0;
            int actual;
            actual = BoolExtensions.ToBitInt(source);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ToBitString
        ///</summary>
        [TestMethod()]
        public void When_ToBitString()
        {
            bool source = true;
            string expected = "1";
            string actual;
            actual = BoolExtensions.ToBitString(source);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ToYesNoString
        ///</summary>
        [TestMethod()]
        public void When_ToYesNoString()
        {
            bool source = false;
            string expected = "No";
            string actual;
            actual = BoolExtensions.ToYesNoString(source);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for YesNoToBool
        ///</summary>
        [TestMethod()]
        public void When_YesNoToBool()
        {
            string source = "Yes";
            bool ignoreCase = false;
            bool expected = true;
            bool actual;
            actual = BoolExtensions.YesNoToBool(source, ignoreCase);
            Assert.AreEqual(expected, actual);
        }
    }
}
