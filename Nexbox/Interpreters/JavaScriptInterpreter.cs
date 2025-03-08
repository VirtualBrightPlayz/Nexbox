﻿using System.Reflection;
using Jint;
using Jint.Native;
using Jint.Runtime.Interop;
using Nexbox.Internals;

namespace Nexbox.Interpreters;

public class JavaScriptInterpreter : IInterpreter
{
    internal bool stop;
    internal Engine engine;
    [ThreadStatic]
    internal static Engine activeEngine;

    public bool IsStopped => stop;

    public void StartSandbox(Action<object> print)
    {
        if (stop || engine != null)
            return;
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

    public IScriptEngine GetEngine()
    {
        return new JsEngine(this);
    }

    public void CallFunction(object func, object args)
    {
        foreach (MethodInfo methodInfo in func.GetType().GetMethods())
        {
            if (methodInfo.Name.Contains("Invoke"))
            {
                JsValue[] vals = new JsValue[((object[]) args).Length];
                for (int i = 0; i < vals.Length; i++)
                {
                    vals[i] = JsValue.FromObject(engine, ((object[]) args)[i]);
                }
                object[] p = new object[2];
                p[0] = null;
                p[1] = vals;
                methodInfo.Invoke(func, p);
                break;
            }
        }
    }
}