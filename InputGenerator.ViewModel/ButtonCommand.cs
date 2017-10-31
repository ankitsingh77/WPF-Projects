using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace InputGenerator.ViewModel
{
    public class ButtonCommand : ICommand
    {
        private Action _execute;
        private Func<bool> _canExecute;

        public ButtonCommand(Action execute,Func<bool> canExecute)
        {
            this._execute = execute;
            this._canExecute = canExecute;
        }

        public void Refresh()
        {
            if(CanExecuteChanged!=null)
                CanExecuteChanged(this, EventArgs.Empty);
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return this._canExecute();
        }

        public void Execute(object parameter)
        {
            this._execute();
        }
    }
}
