using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ServiceDeskToolsApp
{
    public class RelayCommand : ICommand
    {
        #region Private Members

        private readonly Action _action;

        #endregion

        #region Public Events

        public event EventHandler CanExecuteChanged = (sender, e) => { }; 
        
        #endregion

        #region Constructor

        public RelayCommand(Action mAction)
        {
            _action = mAction;
        } 

        #endregion

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            _action();
        }
    }

    public class DelegateCommand<T> : ICommand
    {
        #region Private Members

        private readonly Action<T> _action;

        #endregion

        #region Public Events

        public event EventHandler CanExecuteChanged = (sender, e) => { };

        #endregion

        #region Constructor

        public DelegateCommand(Action<T> mAction)
        {
            _action = mAction;
        }

        #endregion

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            _action((T)parameter);
        }
    }
}
