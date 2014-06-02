using System.Diagnostics;

namespace Utils.DebugUtils
{
    public static class StackWalker
    {
        public static bool IsInStack(string className)
        {
#if DEBUG
            StackTrace st = new StackTrace(true);
            for (int i = 0; i < st.FrameCount; i++)
            {
                var frame = st.GetFrame(i);
                if (frame.GetMethod().DeclaringType.Name == className)
                    return true;
            }
#endif
            return false;
        }
    }
}
