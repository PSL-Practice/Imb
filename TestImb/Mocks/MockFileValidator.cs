using Imb.Utils;

namespace TestImb.Mocks
{
    public class MockFileValidator : IFileValidator
    {
        public bool Validate(string path)
        {
            return true;
        }

        public bool Validate(byte[] data)
        {
            return true;
        }
    }
}