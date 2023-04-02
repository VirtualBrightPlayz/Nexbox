using Jint;
using Jint.Runtime.Interop;

namespace Nexbox.Interpreters;

public class JavaScriptInterpreter : IInterpreter
{
    private Engine engine;
        
    public void StartSandbox(Action<object> print)
    {
        engine = new Engine(options =>
        {
            options.LimitMemory(4_000_000);
            options.TimeoutInterval(TimeSpan.FromMinutes(1));
        });
        engine.SetValue("print", new Action<object>(print));
    }

    public void CreateGlobal(string name, object global) => engine.SetValue(name, global);

    public void ForwardType(string module, Type type) =>
        engine.SetValue(module, TypeReference.CreateTypeReference(engine, type));

    public void RunScript(string script, Action<Exception> OnException = null)
    {
        try
        {
            engine.Execute(script);
        }
        catch(Exception e){ OnException?.Invoke(e); }
    }

    public void Stop() => engine.Dispose();
}