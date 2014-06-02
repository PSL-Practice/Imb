using System;

namespace Imb.Data
{
    public class OnError : IDisposable
    {
        private Action _reverseAction;
        private bool _committed;

        public OnError(Action reverseAction)
        {
            _reverseAction = reverseAction;
        }

        public void Commit()
        {
            _committed = true;
        }

        public void Dispose()
        {
            if (!_committed)
                _reverseAction();
        }
    }
}