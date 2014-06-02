using System;
using System.Windows.Input;

namespace Utils.UISupport
{
	public class BlockableCommand<T> : SimpleCommand<T>
	{
		private bool _blocked;
		public bool Blocked
		{
			get { return _blocked; }
			set 
			{ 
				_blocked = value;
				CommandManager.InvalidateRequerySuggested();
			}
		}

		public BlockableCommand(bool blocked, Action<T> executeDelegate )
		{
			CanExecuteDelegate = CanExecuteImpl;
			Blocked = blocked;
			ExecuteDelegate = executeDelegate;
		}

		private bool CanExecuteImpl(T obj)
		{
			return !_blocked;
		}
	}
}