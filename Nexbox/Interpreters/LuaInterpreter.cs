using System.Reflection;
using MoonSharp.Interpreter;
using Nexbox.Internals;

namespace Nexbox.Interpreters;

public class LuaInterpreter : IInterpreter, IInterpreterGlobals, IInterpreterModules
{
    internal bool stop;
    private Script _script;

    internal readonly Dictionary<string, DynValue> modules = new Dictionary<string, DynValue>();

    
    public void StartSandbox(Action<object> print)
    {
        if (stop || _script != null)
            return;
        _script = new Script(CoreModules.Preset_SoftSandbox);
        _script.Options.DebugPrint = print;
        UserData.RegisterType(typeof(SandboxFunc), InteropAccessMode.BackgroundOptimized);
        _script.Globals["require"] = new Func<string, DynValue>(Require);
        _script.Globals["SandboxFunc"] = new Func<object>(() => new SandboxFunc(new LuaEngine(this)));
        UserData.RegisterType(typeof(LuaNullConstructor), InteropAccessMode.BackgroundOptimized);
        _script.Globals["nilctor"] = new LuaNullConstructor();
    }

    public DynValue Require(string name)
    {
        if (stop)
            return null;
        if (modules.TryGetValue(name, out DynValue val))
            return val;
        return null;
    }

    public void CreateGlobal(string name, object global)
    {
        if (stop)
            return;
        _script.Globals[name] = global;
    }

    private static object[] GetParameters(object[] parameters)
    {
        int nulls = 0;
        List<object> count = new List<object>();
        for (int i = 0; i < parameters.Length; i++)
        {
            object parameter = parameters[i];
            if (parameter != null)
            {
                for (int j = 0; j < nulls; j++)
                    count.Add(null);
                count.Add(parameter);
                nulls = 0;
            }
            else
                nulls++;
        }
        return count.ToArray();
    }
    
    private static object SafeConvert(object o, Type t)
    {
        if (o == null || o.GetType() == typeof(LuaNullConstructor))
            return null;
        return Convert.ChangeType(o, t);
    }

    private static object[] SafeConvert(ParameterInfo[] parameterInfos, object[] objects)
    {
        if (parameterInfos.Length != objects.Length)
            throw new Exception("Parameter Count Mismatch");
        List<object> converted = new List<object>();
        for (int i = 0; i < parameterInfos.Length; i++)
        {
            ParameterInfo parameterInfo = parameterInfos[i];
            object o = objects[i];
            converted.Add(SafeConvert(o, parameterInfo.ParameterType));
        }
        return converted.ToArray();
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
        _script.Globals[module] =
            new Func<object, object, object, object, object, object, object, object, object, object, object, object,
                object, object, object, object, object>(
                (o1, o2, o3, o4, o5, o6, o7, o8, o9, o10, o11, o12, o13, o14, o15, o16) =>
                {
                    object[] parameters = GetParameters(new[]
                        {o1, o2, o3, o4, o5, o6, o7, o8, o9, o10, o11, o12, o13, o14, o15, o16});
                    foreach (ConstructorInfo constructorInfo in type.GetConstructors())
                    {
                        ParameterInfo[] constructorParameters = constructorInfo.GetParameters();
                        if (constructorParameters.Length == parameters.Length)
                            return constructorInfo.Invoke(SafeConvert(constructorParameters, parameters));
                    }
                    // Try and fill in trailing nulls
                    foreach (ConstructorInfo constructorInfo in type.GetConstructors())
                    {
                        try
                        {
                            ParameterInfo[] constructorParameters = constructorInfo.GetParameters();
                            if (constructorParameters.Length > parameters.Length)
                            {
                                int nullsToFill = constructorParameters.Length - parameters.Length;
                                List<object> filledParameters = new List<object>(parameters);
                                for (int i = 0; i < nullsToFill; i++)
                                    filledParameters.Add(null);
                                return constructorInfo.Invoke(SafeConvert(constructorParameters,
                                    filledParameters.ToArray()));
                            }
                        } catch(Exception){}
                    }
                    // Assume constructor-less type
                    return Activator.CreateInstance(type);
                });
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

    public string[] GetGlobals()
    {
        if (stop)
            return Array.Empty<string>();
        return _script.Globals.Keys.Where(x => x.Type == DataType.String).Select(x => x.String).ToArray();
    }

    public object GetProperty(string name)
    {
        if (stop)
            return null;
        return _script.Globals[name];
    }

    public void SetProperty(string name, object value)
    {
        if (stop)
            return;
        _script.Globals[name] = DynValue.FromObject(_script, value);
    }

    public void RunModule(string module, string script, Action<Exception> OnException = null)
    {
        if (stop)
            return;

        try
        {
            modules[module] = _script.DoString(script, codeFriendlyName: module);
        }
        catch(Exception e){ OnException?.Invoke(e); }
    }

    public string[] GetModuleGlobals(string module)
    {
        if (stop)
            return Array.Empty<string>();
        if (modules.TryGetValue(module, out DynValue instance) && instance.Type == DataType.Table)
        {
            return instance.Table.Keys.Select(x => x.String).ToArray();
        }
        return Array.Empty<string>();
    }

    public object GetModuleProperty(string module, string name)
    {
        if (stop)
            return null;
        if (modules.TryGetValue(module, out DynValue instance) && instance.Type == DataType.Table)
        {
            return instance.Table[name];
        }
        return null;
    }

    public void SetModuleProperty(string module, string name, object value)
    {
        if (stop)
            return;
        if (modules.TryGetValue(module, out DynValue instance) && instance.Type == DataType.Table)
        {
            instance.Table[name] = DynValue.FromObject(_script, value);
        }
    }

    public object RunModuleFunction(string module, string name, params object[] args)
    {
        if (stop)
            return null;
        if (modules.TryGetValue(module, out DynValue instance) && instance.Type == DataType.Table)
        {
            SandboxFunc func = SandboxFuncTools.TryConvert(instance.Table[name]);
            return SandboxFuncTools.InvokeSandboxFunc(func, args);
        }
        return null;
    }
}