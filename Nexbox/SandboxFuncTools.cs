using System.Reflection;

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
}