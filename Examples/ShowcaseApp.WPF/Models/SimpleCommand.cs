using System;
using System.Windows.Input;

namespace ShowcaseApp.WPF.Models
{
    public class SimpleCommand : ICommand
   {
       public Predicate<object> CanExecuteDelegate { get; set; }
       public Action<object> ExecuteDelegate { get; set; }

       public SimpleCommand(Predicate<object> can, Action<object> ex)
       {
           CanExecuteDelegate = can;
           ExecuteDelegate = ex;
       }

       #region ICommand Members
    
       public bool CanExecute(object parameter)
       {
           if (CanExecuteDelegate != null)
               return CanExecuteDelegate(parameter);
           return true;// if there is no can execute default to true
       }
    
       public event EventHandler CanExecuteChanged
       {
           add { CommandManager.RequerySuggested += value; }
           remove { CommandManager.RequerySuggested -= value; }
       }
    
       public void Execute(object parameter)
       {
           if (ExecuteDelegate != null)
               ExecuteDelegate(parameter);
       }
    
       #endregion
   }
}
