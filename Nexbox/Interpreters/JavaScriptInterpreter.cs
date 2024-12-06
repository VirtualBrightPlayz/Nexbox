using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime.Interop;
using Jint.Runtime.Modules;
using Nexbox.Internals;

namespace Nexbox.Interpreters;

public class JavaScriptInterpreter : IInterpreter, IInterpreterGlobals, IInterpreterModules
{
    internal bool stop;
    internal Engine engine;
    [ThreadStatic]
    internal static Engine activeEngine;

    internal readonly Dictionary<string, ObjectInstance> modules = new Dictionary<string, ObjectInstance>();

    public void StartSandbox(Action<object> print)
    {
        if (stop || engine != null)
            return;
        modules.Clear();
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
        if (stop)
            return Array.Empty<string>();
        string[] keys = engine.Global.GetOwnProperties().Select(x => x.Key.ToString()).ToArray();
        return keys;
    }

    public object GetProperty(string name)
    {
        if (stop)
            return null;
        JsValue val = engine.GetValue(name);
        return val.ToObject();
    }

    public void SetProperty(string name, object value)
    {
        if (stop)
            return;
        engine.SetValue(name, JsValue.FromObject(engine, value));
    }

    public void RunModule(string module, string script, Action<Exception> OnException = null)
    {
        if (stop)
            return;

        activeEngine = engine;
        try
        {
            engine.Modules.Add(module, script);
            modules[module] = engine.Modules.Import(module);
        }
        catch(Exception e){ OnException?.Invoke(e); }
        activeEngine = null;
    }

    public string[] GetModuleGlobals(string module)
    {
        if (stop)
            return Array.Empty<string>();
        if (modules.TryGetValue(module, out ObjectInstance instance))
        {
            return instance.GetOwnPropertyKeys(Jint.Runtime.Types.String).Select(x => x.ToString()).ToArray();
        }
        return Array.Empty<string>();
    }

    public object GetModuleProperty(string module, string name)
    {
        if (stop)
            return null;
        if (modules.TryGetValue(module, out ObjectInstance instance))
        {
            return instance[name].ToObject();
        }
        return null;
    }

    public void SetModuleProperty(string module, string name, object value)
    {
        if (stop)
            return;
        if (modules.TryGetValue(module, out ObjectInstance instance))
        {
            instance[name] = JsValue.FromObject(engine, value);
        }
    }

    public object RunModuleFunction(string module, string name, params object[] args)
    {
        if (stop)
            return null;
        if (modules.TryGetValue(module, out ObjectInstance instance))
        {
            activeEngine = engine;
            SandboxFunc func = SandboxFuncTools.TryConvert(instance[name]);
            return SandboxFuncTools.InvokeSandboxFunc(func, args);
        }
        return null;
    }
}