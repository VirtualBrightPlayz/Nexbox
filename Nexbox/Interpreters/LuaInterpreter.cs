using MoonSharp.Interpreter;

namespace Nexbox.Interpreters;

public class LuaInterpreter : IInterpreter
{
    private Script _script;
    public void StartSandbox(Action<object> print)
    {
        _script = new Script(CoreModules.Preset_SoftSandbox);
        _script.Options.DebugPrint = print;
    }

    public void CreateGlobal(string name, object global) => _script.Globals[name] = global;

    public void ForwardType(string module, Type type)
    {
        UserData.RegisterType(type, InteropAccessMode.BackgroundOptimized);
        _script.Globals[module] = new Func<object>(() => Activator.CreateInstance(type));
    }

    public void RunScript(string script, Action<Exception> OnException = null)
    {
        try
        {
            _script.DoString(script);
        }
        catch(Exception e){ OnException?.Invoke(e); }
    }

    public void Stop() => _script = null;
}