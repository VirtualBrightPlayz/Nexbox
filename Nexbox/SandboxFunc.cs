using System.Reflection;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using MoonSharp.Interpreter;
using Nexbox.Internals;
using Nexbox.Interpreters;

namespace Nexbox;

public class SandboxFunc
{
    private LuaInterpreter s;
    private JavaScriptInterpreter e;
    internal Delegate a;
    
    public SandboxFunc(){}

    public SandboxFunc(object script)
    {
        Type t = script.GetType();
        if (t == typeof(JsEngine))
            e = (JavaScriptInterpreter) t.GetField("e", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(script);
        else if (t == typeof(LuaEngine))
            s = (LuaInterpreter) t.GetField("e", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(script);
        else
            throw new Exception("Invalid engine!");
    }

    internal SandboxFunc(IInterpreter interpreter)
    {
        if (interpreter is JavaScriptInterpreter)
            e = (JavaScriptInterpreter)interpreter;
        else if (interpreter is LuaInterpreter)
            s = (LuaInterpreter)interpreter;
        else
            throw new Exception("Invalid engine!");
    }

    public SandboxFunc SetAction(object func)
    {
        a = new Func<object, object>(args =>
        {
            if (e != null && func is JsValue)
            {
                List<JsValue> vals = new List<JsValue>();
                foreach (object o in (List<object>) args)
                    vals.Add(JsValue.FromObject(e.engine, o));
                JsValue ret = ((JsValue)func).Call(null, vals.ToArray());
                return ret?.ToObject();
            }
            foreach (MethodInfo methodInfo in func.GetType().GetMethods())
            {
                if (s != null && methodInfo.Name.Contains("Call"))
                {
                    ScriptFunctionDelegate s =
                        LuaInterpreter.ClosureToDelegate((Closure) func);
                    return ((Closure) func).Call(((List<object>) args).ToArray())?.ToObject();
                }
                if (e != null && methodInfo.Name.Contains("Invoke"))
                {
                    List<JsValue> vals = new List<JsValue>();
                    foreach (object o in (List<object>) args)
                        vals.Add(JsValue.FromObject(e.engine, o));
                    object[] p = new object[2];
                    p[0] = null;
                    p[1] = vals.ToArray();
                    JsValue ret = (JsValue)methodInfo.Invoke(func, p);
                    return ret?.ToObject();
                }
            }
            return null;
        });
        return this;
    }

    private object Invoke(object[] args)
    {
        if (e != null && e.stop)
            return null;
        if (s != null && s.stop)
            return null;
        return a.DynamicInvoke(args.ToList());
    }
}