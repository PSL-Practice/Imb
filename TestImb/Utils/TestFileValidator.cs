using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Imb.Utils;
using NUnit.Framework;

namespace TestImb.Utils
{
    [TestFixture]
    public class TestFileValidator
    {
        private FileValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _validator = new FileValidator();
        }

        [Test]
        public void JpegFileIsValid()
        {
            Assert.That(_validator.Validate(@"images\test.jpg"));
        }

        [Test]
        public void PngFileIsValid()
        {
            Assert.That(_validator.Validate(@"images\test.png"));
        }

        [Test]
        public void GifFileIsValid()
        {
            Assert.That(_validator.Validate(@"images\test.gif"));
        }

        [Test]
        public void BadFileIsNotValid()
        {
            Assert.That(_validator.Validate(@"images\bad.jpg"), Is.False);
        }
    }
}
