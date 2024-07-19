using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TML.SpriteCollectionEditor {
	public class RelayCommand<T>(Action<T?> execute, Func<T?, bool>? canExecute = null) : ICommand {
		private readonly Action<T?> _execute = execute;
		private readonly Func<T?, bool>? _canExecute = canExecute;

		public bool CanExecute(object? parameter) {
			return _canExecute?.Invoke((T?)parameter) ?? true;
		}

		public void Execute(object? parameter) {
			_execute((T?)parameter);
		}

		public event EventHandler? CanExecuteChanged;
		public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, new EventArgs());
	}
}
