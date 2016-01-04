using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Wtalk.MvvM
{
    public class RelayCommand : ICommand
    {
        Action<object> _execute;
        Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute)
        {
            _execute = execute;
        }

        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
            : this(execute)
        {
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {

            return _canExecute == null ? true : _canExecute(parameter);
        }

        public virtual event EventHandler CanExecuteChanged;
        //{
        //    add { CommandManager.RequerySuggested += value; }
        //    remove { CommandManager.RequerySuggested -= value; }
        //}

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
