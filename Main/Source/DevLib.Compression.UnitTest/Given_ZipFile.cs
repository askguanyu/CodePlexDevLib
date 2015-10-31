namespace DevLib.Compression.UnitTest
{
    using DevLib.Compression;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;

    [TestClass()]
    public class Given_ZipFile
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
        public void When_CreateFromDirectory()
        {
            string destinationArchiveFileName = @"c:\test_zip.zip";
            string sourceDirectoryName = @"c:\test_zip";
            bool includeBaseDirectory = true;
            bool includeSubDirectories = true;

            ZipFile.CreateFromDirectory(destinationArchiveFileName, sourceDirectoryName, includeBaseDirectory, includeSubDirectories);

            Assert.Inconclusive();
        }

        [TestMethod()]
        public void When_ExtractToDirectory()
        {
            string sourceArchiveFileName = @"c:\test_zip.zip";
            string destinationDirectoryName = @"c:\test_zip_1";
            bool overwrite = true;
            bool throwOnError = true;
            ZipFile.ExtractToDirectory(sourceArchiveFileName, destinationDirectoryName, overwrite, throwOnError);

            Assert.Inconclusive();
        }
    }
}
