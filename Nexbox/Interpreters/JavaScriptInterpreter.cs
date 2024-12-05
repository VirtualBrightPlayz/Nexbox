using Jint;
using Jint.Native;
using Jint.Runtime.Interop;
using Nexbox.Internals;

namespace Nexbox.Interpreters;

public class JavaScriptInterpreter : IInterpreter, IInterpreterGlobals
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

    public void Stop()
    {
        if (stop)
            return;
        stop = true;
        engine.Dispose();
    }

    public string[] GetGlobals()
    {
        string[] keys = engine.Global.GetOwnProperties().Where(x => x.Value.Value.Type != Jint.Runtime.Types.Object && x.Value.Value.Type != Jint.Runtime.Types.Undefined).Select(x => x.Key.ToString()).ToArray();
        return keys;
    }

    public object GetProperty(string name)
    {
        JsValue val = engine.GetValue(name);
        return val.ToObject();
    }

    public void SetProperty(string name, object value)
    {
        engine.SetValue(name, JsValue.FromObject(engine, value));
    }
}