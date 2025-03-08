using System.Reflection;
using Jint.Native;
using MoonSharp.Interpreter;
using Nexbox.Interpreters;

namespace Nexbox;

public static class SandboxFuncTools
{
    public static void InvokeSandboxFunc(SandboxFunc f, params object[] args)
    {
        foreach (MethodInfo methodInfo in f.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
        {
            if(methodInfo.Name != "Invoke") continue;
            object[] p = new object[1];
            p[0] = args;
            methodInfo.Invoke(f, p);
        }
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
                        LuaInterpreter.ClosureToDelegate(lua1).Invoke((object[])args);
                    }),
                };
            case Func<JsValue, JsValue[], JsValue> js1:
                return new SandboxFunc()
                {
                    a = new Action<object>(args =>
                    {
                        JsValue[] vals = new JsValue[((object[]) args).Length];
                        for (int i = 0; i < vals.Length; i++)
                        {
                            vals[i] = JsValue.FromObject(JavaScriptInterpreter.activeEngine, ((object[]) args)[i]);
                        }
                        js1.Invoke(JsValue.Null, vals);
                    }),
                };
            default:
                return new SandboxFunc().SetAction(func);
        }
    }
}