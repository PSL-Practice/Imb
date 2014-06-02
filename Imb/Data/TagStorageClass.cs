namespace Imb.Data
{
    public class TagStorageClass
    {
        public string Tag;

        public static implicit operator TagStorageClass(string value)
        {
            return new TagStorageClass() {Tag = value};
        }
    }
}