﻿namespace Nexbox.Tests;

public class TestClass
{
    public int AddNumbers(int a, int b) => a + b;

    public void CreateAndExec(SandboxFunc f, params object[] args) => SandboxFuncTools.InvokeSandboxFunc(f, args);

    private static List<(SandboxFunc, object[])> execs = new List<(SandboxFunc, object[])>();

    public void CreateAndExecLater(SandboxFunc f, params object[] args) => execs.Add((f, args));

    internal static void exec()
    {
        foreach ((SandboxFunc, object[]) valueTuple in execs)
        {
            SandboxFuncTools.InvokeSandboxFunc(valueTuple.Item1, valueTuple.Item2);
        }
    }
}