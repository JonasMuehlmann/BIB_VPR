#Benutzte Pakete
System.Windows.Input
#Importschnittstellen
System.Windows.Input
#Importschnittstellen
System.Action.Invoke()
System.Action<T>.Invoke(T)
System.EventHandler.Invoke(object?, System.EventArgs)
System.Func<bool>.Invoke()
System.Func<T, bool>.Invoke(T)
#Exportschnittstellen
public void OnCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
public void OnCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
public RelayCommand(Action execute)
public RelayCommand(Action execute, Func<bool> canExecute)
public RelayCommand(Action<T> execute)
public RelayCommand(Action<T> execute, Func<T, bool> canExecute)
System.Action.Invoke()
System.Action<T>.Invoke(T)
System.EventHandler.Invoke(object?, System.EventArgs)
System.Func<bool>.Invoke()
System.Func<T, bool>.Invoke(T)
#Exportschnittstellen
public void OnCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
public void OnCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
public RelayCommand(Action execute)
public RelayCommand(Action execute, Func<bool> canExecute)
public RelayCommand(Action<T> execute)
public RelayCommand(Action<T> execute, Func<T, bool> canExecute)
