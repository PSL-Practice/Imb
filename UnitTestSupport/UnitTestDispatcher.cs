using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace UnitTestSupport
{
    public class UnitTestDispatcher
    {
        private DispatcherFrame _frame = new DispatcherFrame();
        public Dispatcher Dispatcher {get { return _frame.Dispatcher; }}

        public void RunDispatcher()
        {
            for (int x = 0; x < 10; ++x) //repetition sometimes needed to sweep up lingering scrap from previous tests. I think.
            {
                try
                {
                    _frame.Dispatcher.BeginInvoke(new Action(() => _frame.Continue = false));

                    _frame.Continue = true;
                    Dispatcher.PushFrame(_frame);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Invocation {0} threw an exception:", x);
                    Console.WriteLine(e);
                }
            }
        }

    }
}
