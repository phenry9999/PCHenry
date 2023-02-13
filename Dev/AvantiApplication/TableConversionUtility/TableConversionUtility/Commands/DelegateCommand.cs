﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TableConversionUtility.Commands
{
	public class DelegateCommand : ICommand
	{
		private readonly Action<object?> execute;
		private Func<object?, bool>? canExecute;

		public event EventHandler? CanExecuteChanged;

		public DelegateCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
		{
			ArgumentNullException.ThrowIfNull(execute);
			this.execute = execute;
			this.canExecute = canExecute;
		}

		public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

		public bool CanExecute(object? parameter) => canExecute is null || canExecute(parameter);

		public void Execute(object? parameter) => execute(parameter);
	}
}
