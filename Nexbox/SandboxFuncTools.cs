using System.Reflection;
using Jint.Native;
using MoonSharp.Interpreter;
using Nexbox.Interpreters;

namespace Nexbox;

public static class SandboxFuncTools
{
    private static readonly MethodInfo invokeMethod = typeof(SandboxFunc).GetMethod("Invoke", BindingFlags.Instance | BindingFlags.NonPublic);

    public static object InvokeSandboxFunc(this SandboxFunc f, params object[] args)
    {
        object[] p = new object[1];
        p[0] = args;
        return invokeMethod.Invoke(f, p);
    }

    public static SandboxFunc TryConvert(object func)
    {
        switch (func)
        {
            case SandboxFunc _:
                return func as SandboxFunc;
            case Closure lua1:
                return new SandboxFunc()
                {
                    a = new Action<object>(args =>
                    {
                        LuaInterpreter.ClosureToDelegate(lua1).Invoke(((List<object>)args).ToArray());
                    }),
                };
            case Func<JsValue, JsValue[], JsValue> js1:
                return new SandboxFunc()
                {
                    a = new Action<object>(args =>
                    {
                        List<JsValue> vals = new List<JsValue>();
                        foreach (object o in (List<object>)args)
                            vals.Add(JsValue.FromObject(JavaScriptInterpreter.activeEngine, o));
                        js1.Invoke(JsValue.Null, vals.ToArray());
                    }),
                };
            default:
                return null;
        }
    }
}