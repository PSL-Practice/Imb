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
    public class TestPathAdjuster
    {
        private static readonly string[] _emptyPath = {};
        private static readonly string[] _root = {"root"};
        private static readonly string[] _alternate = {"alternate"};
        private static readonly string[] _rootChildren = {"root", "child", "grandchild"};
        private static readonly string[] _rootChild = {"root", "child"};
        private static readonly string[] _rootOtherChildren = {"root", "otherchild"};

        [Test]
        public void RootPathIsAdded()
        {
            var adj = new PathAdjuster(_emptyPath, _root);
            var newPath = adj.AdjustPath(_emptyPath);
            Assert.That(PrintPath(newPath), Is.EqualTo("root"));
        }

        [Test]
        public void MoveRootToAlternateChangesOnlyRoot()
        {
            var adj = new PathAdjuster(_root, _alternate);
            var newPath = adj.AdjustPath(_rootChildren);
            Assert.That(PrintPath(newPath), Is.EqualTo(@"alternate\child\grandchild"));
        }

        [Test]
        public void MoveRootChildToOtherChildIsAdjustedCorrectly()
        {
            var adj = new PathAdjuster(_rootChild, _rootOtherChildren);
            var newPath = adj.AdjustPath(_rootChildren);
            Assert.That(PrintPath(newPath), Is.EqualTo(@"root\otherchild\grandchild"));
        }

        private string PrintPath(string[] path)
        {
            if (path == null) return string.Empty;
            return path.Aggregate((t, i) => t + @"\" + i);
        }
    }


}
