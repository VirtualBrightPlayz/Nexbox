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
i.CreateGlobal("one", 1);
i.ForwardType("tools", typeof(TestClass));
i.ForwardType("ObjectData", typeof(ObjectData));
if (isLua)
{
    i.RunScript("print(tools().AddNumbers(one, 10))");
    i.RunScript("local data1 = ObjectData() \r\n" +
                "local data2 = ObjectData() \r\n" +
                "data1.a = 5 \r\n" +
                "data2.a = 7 \r\n" +
                "print(\"data1 is \"..tostring(data1.a)..\" and data2 is \"..tostring(data2.a))", Console.WriteLine);
}
else
{
    i.RunScript("print(new tools().AddNumbers(one, 10))");
    i.RunScript("let data1 = new ObjectData() \r\n" +
                "let data2 = new ObjectData() \r\n" +
                "data1.a = 5 \r\n" +
                "data2.a = 7 \r\n" +
                "print(\"data1 is \" + data1.a + \" and data2 is \" + data2.a)");
    i.RunScript("let data3 = new ObjectData(5) \r\n" +
                "let data4 = new ObjectData(7) \r\n" +
                "print(\"data3 is \" + data3.a + \" and data4 is \" + data4.a)", Console.WriteLine);
}
i.Stop();