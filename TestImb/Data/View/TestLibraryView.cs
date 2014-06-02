using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using ApprovalTests;
using ApprovalTests.Reporters;
using Imb.Data;
using Imb.Data.View;
using Imb.EventAggregation;
using NUnit.Framework;
using TestImb.Mocks;
using UnitTestSupport;

namespace TestImb.Data.View
{
    [TestFixture]
    [UseReporter(typeof(CustomReporter))]
    public class TestLibraryView
    {
        private LibraryView _view;
        private MockTagsCache _tags;
        private MockPathsCache _paths;
        private MockBinaryDataCache _data;
        private MockBinariesCache _binaries;
        private UnitTestDispatcher _frame;
        private MockErrorHandler _errorHandler;
        private MockFileValidator _fileValidator;
        private UnitTestEventAggregator _eventAggregator;

        [SetUp]
        public void SetUp()
        {
            _frame = new UnitTestDispatcher();
            _tags = new MockTagsCache();
            _paths = new MockPathsCache(_tags);
            _data = new MockBinaryDataCache();
            _binaries = new MockBinariesCache(_data);
            _errorHandler = new MockErrorHandler();
            _fileValidator = new MockFileValidator();
            _eventAggregator = new UnitTestEventAggregator();

            CreateData();

            _view = new LibraryView(_tags, _paths, _binaries, _frame.Dispatcher, null, _errorHandler, _fileValidator, _eventAggregator);
            _view.AttachDisplay(new MockDisplay());
            _frame.RunDispatcher();
        }

        [TearDown]
        public void TearDown()
        {
            _view.Dispose();
        }

        [Test]
        public void BinariesAreLoadedFromCacheInConstruction()
        {
            WaitLoad();
            Assert.That(_view.Items.Count(i => i is LibraryFileNode), Is.EqualTo(3));
        }

        [Test]
        public void PathNodesAreCreatedOnConstructionLoad()
        {
            WaitLoad();

            var result = PrintView();
            Console.WriteLine(result);
            Approvals.Verify(result);
        }

        [Test]
        public void AllChildrenOfAGivenNodeCanBeRetrievedRecusively()
        {
            WaitLoad();

            _frame.RunDispatcher();

            var sb = new StringBuilder();

            var folderD = _view.Items.FirstOrDefault(i => i.Name == "FolderD");
            if (folderD != null)
            {
                var folderId = folderD.Id;
                foreach (var node in _view.GetAllChildren(folderId))
                {
                    sb.AppendLine(string.Format("{0} {1}", FormatPath(node), node.Name));
                }
            }

            var result = sb.ToString();

            Console.WriteLine(result);
            Approvals.Verify(result);
        }

        [Test]
        public void AddedNodeIsSelected()
        {
            WaitLoad();

            _frame.RunDispatcher();

            var firstPath = _paths.AllPathIds.Any() ? _paths.AllPathIds.First() : -1;
            var pathStrings = _paths.GetPathStrings(firstPath);

            var libraryFileNode = new LibraryFileNode(_frame.Dispatcher, _eventAggregator)
            {
                Path = pathStrings
            };
            _view.AddNode(libraryFileNode);

            Assert.That(_view.SelectedItem, Is.SameAs(libraryFileNode));
        }

        private string FormatPath(LibraryViewNode node)
        {
            return node.Parent != null ? FormatPath(node.Parent) + "\\" + node.Name : node.Name;
        }

        private string PrintView()
        {
            var sb = new StringBuilder();
            foreach (var node in _view.Items)
            {
                PrintNode(sb, node, 0);
            }

            return sb.ToString();
        }

        private static void PrintNode(StringBuilder sb, LibraryViewNode node, int indent)
        {
            var indentString = new string(' ', indent*4);
            sb.AppendFormat("{0}{1}", indentString, node);
            sb.AppendLine();
            foreach (var child in node.ChildrenView)
            {
                PrintNode(sb, child, indent + 1);
            }
        }

        private void CreateData()
        {
            var random = new Random();
            foreach (var path in new[]
            {
                null,
                new[] {"FolderA", "FolderA1"},
                new[] {"FolderB", "FolderB1", "FolderB2"},
                new[] {"FolderC", "FolderC1", "FolderC2", "FolderC3"},
                new[] {"FolderD", "FolderD1"},
                new[] {"FolderD", "FolderD1", "FolderD2", "FolderD3", "FolderD4"},
            })
            {
                foreach (var binName in new[] {"Bin1", "Bin2", "Bin3"})
                {
                    var data = new byte[100];
                    random.NextBytes(data);
                    var nameTag = _tags.AddOrGet(binName);
                    var binaryStorageClass = new BinaryStorageClass {Id = Guid.NewGuid(), NameTag = nameTag};
                    if (path != null)
                        binaryStorageClass.PathId = _paths.AddOrGet(path);
                    _binaries.Add(binaryStorageClass, data);
                }
            }

        }

        private void WaitLoad()
        {
            var sw = new Stopwatch();
            sw.Start();
            while (sw.ElapsedMilliseconds < 1000 && !_view.Loaded) Thread.Sleep(30);
            _frame.RunDispatcher();
        }
    }
}
