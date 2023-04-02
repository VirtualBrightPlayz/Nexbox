using Nexbox;
using Nexbox.Interpreters;
using Nexbox.Tests;

IInterpreter i;
Console.WriteLine("Which language would you like to test? (js/lua)");
string r = Console.ReadLine() ?? String.Empty;
bool isLua = false;
if (r.ToLower().Contains("lua"))
{
    isLua = true;
    i = new LuaInterpreter();
}
else
    i = new JavaScriptInterpreter();
i.StartSandbox(Console.WriteLine);
i.ForwardType("tools", typeof(TestClass));
i.ForwardType("ObjectData", typeof(ObjectData));
if(isLua)
    i.RunScript("print(\"hi\") \r\n" +
                "local sum = tools.AddNumbers(1, 5) \r\n" +
                "local o = tools.GetObjectData(sum) \r\n" +
                "print(o.a) \r\n" +
                "print(o.getB())");
else
    i.RunScript("const t = new tools() \r\n" +
                "print(\"hi\") \r\n" +
                "let sum = t.AddNumbers(1, 5) \r\n" +
                "let o = t.GetObjectData(sum) \r\n" +
                "print(o.a) \r\n" +
                "print(o.getB()) \r\n" +
                "print(o.b)");
i.Stop();