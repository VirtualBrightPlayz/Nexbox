using Jint;
using Jint.Runtime.Interop;
using Nexbox.Internals;

namespace Nexbox.Interpreters;

public class JavaScriptInterpreter : IInterpreter
{
    internal bool stop;
    internal Engine engine;
    [ThreadStatic]
    internal static Engine activeEngine;

    public void StartSandbox(Action<object> print)
    {
        if (stop || engine != null)
            return;
        engine = new Engine();
        engine.SetValue("print", print);
        CreateGlobal("engine", new JsEngine(this));
        ForwardType("SandboxFunc", typeof(SandboxFunc));
    }

    public void CreateGlobal(string name, object global)
    {
        if (stop)
            return;
        engine.SetValue(name, global);
    }

    public void ForwardType(string module, Type type)
    {
        if (stop)
            return;
        engine.SetValue(module, TypeReference.CreateTypeReference(engine, type));
    }

    public void RunScript(string script, Action<Exception> OnException = null)
    {
        if (stop)
            return;

        activeEngine = engine;
        try
        {
            engine.Execute(script);
        }
        catch(Exception e){ OnException?.Invoke(e); }
        activeEngine = null;
    }

    public SandboxFunc GetGlobalFunction(string name)
    {
        return new SandboxFunc(this).SetAction(engine.GetValue(name));
    }

    public void Stop()
    {
        if (stop)
            return;
        stop = true;
        engine.Dispose();
    }
}