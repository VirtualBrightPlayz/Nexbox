namespace Nexbox.Tests;

public class TestClass
{
    public int AddNumbers(int a, int b) => a + b;

    public void CreateAndExec(object f, params object[] args) => SandboxFuncTools.InvokeSandboxFunc(SandboxFuncTools.TryConvert(f), args);

    private static List<(SandboxFunc, object[])> execs = new List<(SandboxFunc, object[])>();

    [Obsolete("Use CreateAndExec")]
    public void CreateAndExecLater(SandboxFunc f, params object[] args) => execs.Add((f, args));

    internal static void exec()
    {
        foreach ((SandboxFunc, object[]) valueTuple in execs)
        {
            SandboxFuncTools.InvokeSandboxFunc(valueTuple.Item1, valueTuple.Item2);
        }
    }

    public static void SetTick(SandboxFunc f) => execs.Add((f, Array.Empty<object>()));

    internal static void tick(long delta)
    {
        foreach ((SandboxFunc, object[]) valueTuple in execs)
        {
            SandboxFuncTools.InvokeSandboxFunc(valueTuple.Item1, delta);
        }
    }


    public static int Color
    {
        get => (int)Console.ForegroundColor;
        set => Console.ForegroundColor = (ConsoleColor)value;
    }
    public static void Write(string value) => Console.Write(value);
    public static void Clear() => Console.Clear();
    public static void Beep() => Console.Beep();
    public static void ResetColor() => Console.ResetColor();
    public static void SetPosition(int left, int top) => Console.SetCursorPosition(left, top);
}