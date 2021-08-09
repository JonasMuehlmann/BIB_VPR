#Benutzte Pakete
System.Threading.Tasks
#Exportschnittstellen
public abstract bool CanHandle(object args);
public override bool CanHandle(object args)
public abstract Task HandleAsync(object args);
public override async Task HandleAsync(object args)
