namespace DevLib.ExtensionMethods.UnitTest
{
    using System.Linq;
    using DevLib.ExtensionMethods;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class Given_ArrayExtensions
    {
        [TestMethod]
        public void When_ForEach()
        {
            var testArray = new[] { "a", "b", "c", "d" };

            testArray.ForEach(i => Assert.IsTrue(testArray.Contains(i)), true);
        }

        [TestMethod]
        public void When_AddRangeTo()
        {
            var sourceArray = new[] { "a", "b", "c", "d" };
            var targetArray = new[] { "e", "f", "g", "h" };

            var expectedArray = new[] { "a", "b", "c", "d", "e", "f", "g", "h" };

            targetArray.AddRangeTo(ref sourceArray);

            CollectionAssert.AreEqual(expectedArray, sourceArray);
        }

        [TestMethod]
        public void When_FindArray()
        {
            var sourceArray = new[] { "a", "b", "c", "d" };
            var subArray1 = new[] { "b", "c" };
            var subArray2 = new[] { "d", "f" };

            Assert.AreEqual(1, sourceArray.FindArrayIndex(subArray1));
            Assert.AreEqual(-1, sourceArray.FindArrayIndex(subArray2));
        }
    }
}
