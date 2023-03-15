using System;
using System.Windows.Input;

namespace PZ9;

public class Command<T> : ICommand where T : new() {
    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) {
        return canExecute.Invoke();
    }

    public void Execute(object? parameter) {
        action?.Invoke((T)parameter);
    }

    private Action<T> action;
    private Func<bool> canExecute;

    public Command(Action<T> action, Func<bool> canExecute) {
        this.canExecute = canExecute;
        this.action = action;
    }

    public Command(Action<T> action) : this(action, () => true) {

    }
}