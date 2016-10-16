namespace DevLib.Web.UnitTest
{
    using DevLib.Web;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    ///This is a test class for Given_UrlBuilder and is intended
    ///to contain all Given_UrlBuilder Unit Tests
    ///</summary>
    [TestClass()]
    public class Given_UrlBuilder
    {
        private TestContext testContextInstance;

        private const string TestUrlFull = "abc://username:password@example.com:123/path/data?key=value&key2=value2#fragid1";
        private const string TestUrlNoPort = "abc://username:password@example.com/path/data?key=value&key2=value2#fragid1";
        private const string TestUrlNoPath = "abc://username:password@example.com?key=value&key2=value2#fragid1";
        private const string TestUrlNoScheme = "username:password@example.com?key=value&key2=value2#fragid1";

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

        [TestMethod()]
        public void When_BuildUrl()
        {
            string expected = "abc://username:password@example.com:123/path/data?key=value&key2=value2#fragid1";
            string actual;

            UrlBuilder target = new UrlBuilder();

            target
                .SetScheme(() => "abc")
                .SetAuthority(() => "username", () => "password")
                .SetHost(() => "example.com")
                .SetPort(() => 123)
                .AppendPath("path", "data")
                .AppendQuery("key", "value")
                .AppendQuery("key2", "value2")
                .SetFragment(() => "fragid1");

            actual = target.ToString();

            Assert.AreEqual(expected, actual);
        }
    }
}
