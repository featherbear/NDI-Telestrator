using System;

public class SimpleCommand : System.Windows.Input.ICommand
{
    readonly Action<object> _execute = null;

    public SimpleCommand(Action<object> execute)
    {
        _execute = execute;
    }

    public bool CanExecute(object parameter) => true;

    public event EventHandler CanExecuteChanged
    {
        add { System.Windows.Input.CommandManager.RequerySuggested += value; }
        remove { System.Windows.Input.CommandManager.RequerySuggested -= value; }
    }

    public void Execute(object parameter)
    {
        _execute(parameter);
    }
}

