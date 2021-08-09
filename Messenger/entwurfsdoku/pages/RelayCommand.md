#Benutzte Pakete
System.Windows.Input
#Exportschnittstellen
public void OnCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
public void OnCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
public RelayCommand(Action execute)
public RelayCommand(Action execute, Func<bool> canExecute)
public RelayCommand(Action<T> execute)
public RelayCommand(Action<T> execute, Func<T, bool> canExecute)
