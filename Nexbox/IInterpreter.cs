namespace Nexbox;

public interface IInterpreter
{
    public void StartSandbox(Action<object> print);
    public void CreateGlobal(string name, object global);
    public void ForwardType(string module, Type type);
    public void RunScript(string script, Action<Exception> OnException = null);
    public SandboxFunc GetGlobalFunction(string name);
    public void Stop();
}