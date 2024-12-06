namespace Nexbox;

public interface IInterpreterModules
{
    public void RunModule(string module, string script, Action<Exception> OnException = null);
    public string[] GetModuleGlobals(string module);
    public object GetModuleProperty(string module, string name);
    public void SetModuleProperty(string module, string name, object value);
    public object RunModuleFunction(string module, string name, params object[] args);
}