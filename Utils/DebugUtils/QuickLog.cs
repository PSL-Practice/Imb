using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utils.DebugUtils
{
    public interface IQuickLog
    {
        /// <summary>
        /// Call this to add a message to the log.
        /// </summary>
        /// <param name="message">The message format string, or the complete message.</param>
        /// <param name="list">A variable parameter list containing the substitution values
        /// for the format string.</param>
        void Add(string message, params object[] list);

        /// <summary>
        /// A version of add that accepts functions that must be executed to return string format values. These will be
        /// executed in <see cref="QuickLog"/> instances but will not be evaluated in <see cref="DisabledLog"/> instances,
        /// making this the correct call when one of the log message parameters is expensive to derive.
        /// 
        /// It also accepts instances of Lazy objects.  <see cref="QuickLog"/> will evaluate the value, but <see cref="DisabledLog"/>
        /// will not.
        /// </summary>
        /// <param name="message">The message format string, or the complete message.</param>
        /// <param name="list">A variable parameter list containing the substitution values
        /// for the format string.</param>
        void LazyAdd(string message, params object[] list);

        /// <summary>
        /// A simple way to perform some function on each log entry current in the object. In <see cref="QuickLog"/> instances
        /// the <see cref="action"/> will be performed on each log message recorded to date. In <see cref="DisabledLog"/> instances,
        /// this is a no-op.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        int EachEntry(Action<string> action);
    }

    /// <summary>
    /// Use this class to create a quick in-memory log.
    /// </summary>
    public class QuickLog : IQuickLog
    {
        private BlockingCollection<string> _messages = new BlockingCollection<string>();

        public void Add(string message, params object[] list)
        {
            var text = string.Format(message, list);
            _messages.Add(text);
        }

        public int EachEntry(Action<string> action)
        {
            var count = 0;
            foreach (var message in _messages)
            {
                action(message);
                ++count;
            }
            return count;
        }

        public override string ToString()
        {
            var output = new StringBuilder();
            EachEntry(s => output.AppendLine(s));
            return output.ToString();
        }

        public void LazyAdd(string message, params object[] list)
        {
            var evaluated = new List<object>();
            foreach (var nextParam in list)
            {
                var paramType = nextParam.GetType();
                if (paramType.IsGenericType && paramType.GetGenericTypeDefinition() == typeof(Func<>) && paramType.GetGenericArguments().Count() == 1)
                {
                    var invoke = paramType.GetMethod("Invoke");
                    if (invoke != null)
                        evaluated.Add(invoke.Invoke(nextParam, null));
                }
                else if (paramType.IsGenericType && paramType.GetGenericTypeDefinition() == typeof(Lazy<>))
                {
                    var valueProp = paramType.GetProperty("Value");
                    if (valueProp != null)
                        evaluated.Add(valueProp.GetValue(nextParam, null));
                }
                else
                    evaluated.Add(nextParam);
            }

            Add(message, evaluated.ToArray());
        }
    }
    
    /// <summary>
    /// Use this class to stand-in for a QuickLog when you no longer wish to log data, but you want to keep the calls in case you need the output for debugging at a later date.
    /// </summary>
    public class DisabledLog : IQuickLog
    {
        public void Add(string message, params object[] list)
        {
            
        }

        public void LazyAdd(string message, params object[] list)
        {
            
        }

        public int EachEntry(Action<string> action)
        {
            return 0;
        }
    }
}
