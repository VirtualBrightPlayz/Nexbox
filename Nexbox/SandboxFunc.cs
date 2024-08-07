using System.Reflection;
using Jint.Native;
using MoonSharp.Interpreter;
using Nexbox.Internals;
using Nexbox.Interpreters;

namespace Nexbox;

public class SandboxFunc
{
    private IInterpreter e;
    internal Delegate a;
    
    public SandboxFunc(){}

    public SandboxFunc(object script)
    {
        Type t = script.GetType();
        FieldInfo f = t.GetField("e", BindingFlags.Instance | BindingFlags.NonPublic);
        if (script is IScriptEngine && f != null)
            e = f.GetValue(script) as IInterpreter;
        else
            throw new Exception("Invalid engine!");
    }

    public SandboxFunc SetAction(object func)
    {
        a = new Action<object>(args =>
        {
            e.CallFunction(func, args);
            /*
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
            */
        });
        return this;
    }

    private void Invoke(object[] args)
    {
        if (e != null && e.IsStopped)
            return;
        a.DynamicInvoke(args?.ToList() ?? new List<object>());
    }
}