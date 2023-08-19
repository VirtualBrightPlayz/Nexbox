using MoonSharp.Interpreter;
using Nexbox.Internals;

namespace Nexbox.Interpreters;

public class LuaInterpreter : IInterpreter
{
    internal bool stop;
    private Script _script;
    
    public void StartSandbox(Action<object> print)
    {
        if (stop || _script != null)
            return;
        _script = new Script(CoreModules.Preset_SoftSandbox);
        _script.Options.DebugPrint = print;
        UserData.RegisterType(typeof(SandboxFunc), InteropAccessMode.BackgroundOptimized);
        _script.Globals["SandboxFunc"] = new Func<object>(() => new SandboxFunc(new LuaEngine(this)));
    }

    public void CreateGlobal(string name, object global)
    {
        if (stop)
            return;
        _script.Globals[name] = global;
    }

    public void ForwardType(string module, Type type)
    {
        if (stop)
            return;
        if (type.IsEnum || (type.IsAbstract && type.IsSealed))
        {
            UserData.RegisterType(type, InteropAccessMode.BackgroundOptimized);
            _script.Globals[module] = UserData.CreateStatic(type);
            return;
        }
        UserData.RegisterType(type, InteropAccessMode.BackgroundOptimized);
        _script.Globals[module] = new Func<object>(() => Activator.CreateInstance(type));
    }

    public void RunScript(string script, Action<Exception> OnException = null)
    {
        if (stop)
            return;
        try
        {
            _script.DoString(script);
        }
        catch(Exception e){ OnException?.Invoke(e); }
    }

    public void Stop()
    {
        if (stop)
            return;
        stop = true;
        _script = null;
    }

    internal static ScriptFunctionDelegate ClosureToDelegate(Closure c) => c.GetDelegate();
}