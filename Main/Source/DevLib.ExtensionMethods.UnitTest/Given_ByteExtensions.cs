namespace DevLib.ExtensionMethods.UnitTest
{
    using System.Collections;
    using System.Drawing;
    using DevLib.ExtensionMethods;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    ///This is a test class for Given_ByteExtensions and is intended
    ///to contain all Given_ByteExtensions Unit Tests
    ///</summary>
    [TestClass()]
    public class Given_ByteExtensions
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
        ///A test for BitStringToBitArray
        ///</summary>
        [TestMethod()]
        public void When_BitStringToBitArray()
        {
            string source = "0101";
            BitArray expected = new BitArray(new[] { false, true, false, true });
            BitArray actual;
            actual = ByteExtensions.BitStringToBitArray(source);

            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Compress
        ///</summary>
        [TestMethod()]
        public void When_Compress()
        {
            byte[] source = "hello".ToByteArray();
            byte[] expected = source.Compress().Decompress();
            byte[] actual = source;

            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Decompress
        ///</summary>
        [TestMethod()]
        public void When_Decompress()
        {
            byte[] source = "hello".ToByteArray();
            byte[] expected = source.Compress().Decompress();
            byte[] actual = source;

            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for HexToByteArray
        ///</summary>
        [TestMethod()]
        public void When_HexToByteArray()
        {
            string source = "00FF";
            byte[] expected = new byte[] { 0, 255 };
            byte[] actual;
            actual = ByteExtensions.HexToByteArray(source);

            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ToBitArray
        ///</summary>
        [TestMethod()]
        public void When_ToBitArray()
        {
            byte[] source = new byte[] { 0, 0 };
            BitArray expected = new BitArray(16, false);
            BitArray actual;
            actual = ByteExtensions.ToBitArray(source);

            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ToBitArray
        ///</summary>
        [TestMethod()]
        public void When_ToBitArray1()
        {
            int source = 0;
            BitArray expected = new BitArray(32, false);
            BitArray actual;
            actual = ByteExtensions.ToBitArray(source);

            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ToBitArray
        ///</summary>
        [TestMethod()]
        public void When_ToBitArray2()
        {
            byte source = 0;
            BitArray expected = new BitArray(8, false);
            BitArray actual;
            actual = ByteExtensions.ToBitArray(source);

            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ToBitIntArray
        ///</summary>
        [TestMethod()]
        public void When_ToBitIntArray()
        {
            BitArray source = "11".BitStringToBitArray();
            int[] expected = new[] { 3, 0 };
            int[] actual;
            actual = ByteExtensions.ToBitIntArray(source);

            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ToBitString
        ///</summary>
        [TestMethod()]
        public void When_ToBitString()
        {
            BitArray source = new BitArray(8, true);
            string expected = "11111111";
            string actual;
            actual = ByteExtensions.ToBitString(source);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ToBoolArray
        ///</summary>
        [TestMethod()]
        public void When_ToBoolArray()
        {
            BitArray source = new BitArray(8, true);
            bool[] expected = new[] { true, true, true, true, true, true, true, true };
            bool[] actual;
            actual = ByteExtensions.ToBoolArray(source);

            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ToByteArray
        ///</summary>
        [TestMethod()]
        public void When_ToByteArray()
        {
            BitArray source = new BitArray(8, false);
            byte[] expected = new byte[] { 0 };
            byte[] actual;
            actual = ByteExtensions.ToByteArray(source);

            CollectionAssert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ToEncodingString
        ///</summary>
        [TestMethod()]
        public void When_ToEncodingString()
        {
            byte[] source = "hello".ToByteArray();
            string expected = "hello";
            string actual;
            actual = ByteExtensions.ToEncodingString(source);

            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ToHexString
        ///</summary>
        [TestMethod()]
        public void When_ToHexString()
        {
            byte source = 255;
            string expected = "FF";
            string actual;
            actual = ByteExtensions.ToHexString(source);

            Assert.AreEqual(expected, actual, true);
        }

        /// <summary>
        ///A test for ToHexString
        ///</summary>
        [TestMethod()]
        public void When_ToHexString1()
        {
            byte[] source = new byte[] { 0, 255, 255, 0 };
            string expected = "00-FF-FF-00";
            string actual;
            actual = ByteExtensions.ToHexString(source, '-');

            Assert.AreEqual(expected, actual, true);
        }

        /// <summary>
        ///A test for ToImage
        ///</summary>
        [TestMethod()]
        public void When_ToImage()
        {
            Bitmap expected = DrawFilledRectangle(Brushes.Blue, 100, 100);
            byte[] source = expected.ImageToByteArray();
            Bitmap actual;
            actual = (Bitmap)ByteExtensions.ToImage(source);

            for (int i = 0; i < 100; i++)
            {
                for (int j = 0; j < 100; j++)
                {
                    Assert.AreEqual(expected.GetPixel(i, j), actual.GetPixel(i, j));
                }
            }
        }

        private Bitmap DrawFilledRectangle(Brush brush, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);

            using (Graphics graph = Graphics.FromImage(result))
            {
                Rectangle imageSize = new Rectangle(0, 0, width, height);
                graph.FillRectangle(brush, imageSize);
            }

            return result;
        }
    }
}
