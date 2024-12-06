using Nexbox;
using Nexbox.Interpreters;
using Nexbox.Tests;

const string JSON_STRING = "\'{\"username\": \"joe\"}'";

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
i.ForwardType("SomeEnum", typeof(SomeEnum));
i.ForwardType("StaticExample", typeof(StaticExample));
i.ForwardType("RandomStruct", typeof(RandomStruct));
if (isLua)
{
    i.RunScript("print(tools().AddNumbers(one, 10))");
    i.RunScript("local data1 = ObjectData() \r\n" +
                "local data2 = ObjectData() \r\n" +
                "data1.a = 5 \r\n" +
                "data2.a = 7 \r\n" +
                "print(\"data1 is \"..tostring(data1.a)..\" and data2 is \"..tostring(data2.a)) \r\n" +
                "print(SomeEnum.A) \r\n" +
                "print(StaticExample.f)", Console.WriteLine);
    i.RunScript("local j = " + JSON_STRING + " " + " \r\n" +
                "local table = json.parse(j) \r\n" +
                "print(table[\"username\"])", Console.WriteLine);
    i.RunScript("local table = {[\"username\"] = json.null()} \r\n" +
                "print(json.serialize(table))", Console.WriteLine);
    i.RunScript("tools().CreateAndExec(SandboxFunc().SetAction(function(a, b, c) print('Hello '..tostring(a)..', '..tostring(b)..', and '..tostring(c)..'!') end), 'one', 'two', 'three')", Console.WriteLine);
    i.RunScript("local data1 = ObjectData() \r\n" +
                "print(tostring(data1.values['a'])..' '..tostring(data1.values['b'])..' '..tostring(data1.values['c']))", Console.WriteLine);
    i.RunScript("tools().CreateAndExec(SandboxFunc().SetAction(function(x, y) print('Some Event Happened!') end), 'a')", Console.WriteLine);
    i.RunScript("tools().CreateAndExecLater(SandboxFunc().SetAction(function() print('Bad!') end))", Console.WriteLine);
    i.RunScript("local data3 = ObjectData(nilctor, 3)\r\nprint(\"b is: \"..tostring(data3.a)..\" and c is \"..tostring(data3.c))", Console.WriteLine);
    i.RunScript("local data4 = ObjectData(8, nilctor)\r\nprint(\"b is: \"..tostring(data4.a)..\" and c is \"..tostring(data4.c))", Console.WriteLine);
    i.RunScript("local data5 = ObjectData(nil, 3)\r\nprint(\"b is: \"..tostring(data5.a)..\" and c is \"..tostring(data5.c))", Console.WriteLine);
    i.RunScript("local data6 = ObjectData(8, nil)\r\nprint(\"b is: \"..tostring(data6.a)..\" and c is \"..tostring(data6.c))", Console.WriteLine);
    i.RunScript("tools().CreateAndExec(function() print(\"pretty function!\") end)", Console.WriteLine);
    i.RunScript("local rc = RandomStruct()\r\n" +
                "rc.a = \"Hello\"\r\n" +
                "rc.b = \"World!\"\r\n" +
                "print(rc.a..\" \"..rc.b)", Console.WriteLine);
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
    i.RunScript("print(SomeEnum.A)");
    i.RunScript("print(StaticExample.f)");
    i.RunScript("let j = " + JSON_STRING + " " + " \r\n" +
                "let table = JSON.parse(j) \r\n" +
                "print(table[\"username\"])", Console.WriteLine);
    i.RunScript("new tools().CreateAndExec(new SandboxFunc(engine).SetAction((a, b, c) => { print('Hello ' + a + ', ' + b + ', and ' + c + '!') }), 'one', 'two', 'three')", Console.WriteLine);
    i.RunScript("print(data1.values['a'] + ' ' + data1.values['b'] + ' ' + data1.values['c'])");
    i.RunScript("new tools().CreateAndExec(new SandboxFunc(engine).SetAction((x, y) => { print('Some Event Happened!') }), 'a')", Console.WriteLine);
    i.RunScript("new tools().CreateAndExecLater(new SandboxFunc(engine).SetAction(() => print('Bad!')))", Console.WriteLine);
    i.RunScript("let rc = new RandomStruct()\r\n" +
                "rc.a = \"Hello\"\r\n" +
                "rc.b = \"World!\"\r\n" +
                "print(rc.a + \" \" + rc.b)", Console.WriteLine);
}
i.Stop();
// TestClass.exec();