using Codout.Framework.Common.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Codout.Framework.Common.Test.Extensions
{
    [TestClass]
    public class IOTest
    {
        [TestMethod]
        [DataRow(".JPG", "image/jpeg")]
        [DataRow(".ogg", "video/ogg")]
        [DataRow(".webm", "video/webm")]
        [DataRow(null, null)]
        [DataRow("", null)]
        [DataRow("noFileExtension", null)]
        [DataRow(".invalidExtension", "application/octet-stream")]
        public void GetMimeTypeTest(string fileName, string expectedMimeType)
        {
            Assert.AreEqual(expectedMimeType, IO.GetMimeType(fileName));
        }
    }
}