namespace Nexbox;

public interface IInterpreter
{
    public bool IsStopped { get; }
    public void StartSandbox(Action<object> print);
    public void CreateGlobal(string name, object global);
    public void ForwardType(string module, Type type);
    public void RunScript(string script, Action<Exception> OnException = null);
    public void Stop();
    public IScriptEngine GetEngine();
    public void CallFunction(object func, object args);
}