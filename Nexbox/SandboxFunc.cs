using System.Reflection;
using Jint.Native;
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

    public SandboxFunc SetAction(object func)
    {
        a = new Action<object>(args =>
        {
            foreach (MethodInfo methodInfo in func.GetType().GetMethods())
            {
                if (methodInfo.Name.Contains("Call"))
                {
                    ScriptFunctionDelegate s =
                        LuaInterpreter.ClosureToDelegate((Closure) func);
                    ((Closure) func).Call(((List<object>) args).ToArray());
                    break;
                }
                if (methodInfo.Name.Contains("Invoke"))
                {
                    List<JsValue> vals = new List<JsValue>();
                    foreach (object o in (List<object>) args)
                        vals.Add(JsValue.FromObject(e.engine, o));
                    object[] p = new object[2];
                    p[0] = null;
                    p[1] = vals.ToArray();
                    methodInfo.Invoke(func, p);
                    break;
                }
            }
        });
        return this;
    }

    private void Invoke(object[] args)
    {
        if (e != null && e.stop)
            return;
        if (s != null && s.stop)
            return;
        a.DynamicInvoke(args.ToList());
    }
}