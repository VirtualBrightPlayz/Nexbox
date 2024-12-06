using System.Reflection;
using Jint;
using Jint.Native;
using MoonSharp.Interpreter;
using Nexbox.Interpreters;

namespace Nexbox;

public static class SandboxFuncTools
{
    public static object InvokeSandboxFunc(SandboxFunc f, params object[] args)
    {
        foreach (MethodInfo methodInfo in f.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            if (methodInfo.Name != "Invoke") continue;
            object[] p = new object[1];
            p[0] = args;
            return methodInfo.Invoke(f, p);
        }
        return null;
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
                    a = new Func<object, object>(args =>
                    {
                        return LuaInterpreter.ClosureToDelegate(lua1).Invoke(((List<object>)args).ToArray());
                    }),
                };
            case Func<JsValue, JsValue[], JsValue> js1:
                return new SandboxFunc()
                {
                    a = new Func<object, object>(args =>
                    {
                        List<JsValue> vals = new List<JsValue>();
                        foreach (object o in (List<object>)args)
                            vals.Add(JsValue.FromObject(JavaScriptInterpreter.activeEngine, o));
                        return js1.Invoke(JsValue.Null, vals.ToArray()).ToObject();
                    }),
                };
            case JsValue js2:
                return new SandboxFunc()
                {
                    a = new Func<object, object>(args =>
                    {
                        List<JsValue> vals = new List<JsValue>();
                        foreach (object o in (List<object>)args)
                            vals.Add(JsValue.FromObject(JavaScriptInterpreter.activeEngine, o));
                        return js2.Call(vals.ToArray()).ToObject();
                    }),
                };
            default:
                return null;
        }
    }
}