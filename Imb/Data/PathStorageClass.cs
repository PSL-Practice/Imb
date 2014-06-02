namespace Imb.Data
{
    public class PathStorageClass
    {
        public int[] Tags;

        public static implicit operator PathStorageClass(int[] values)
        {
            return new PathStorageClass {Tags = values};
        }
    }
}