# Nexbox
A Simple C# Sandboxing Library Supporting Multiple Languages

## How to use

1) Select your target framework
    
    + `Nexbox.48.dll` for net48
    + `Nexbox.60.dll` for net6.0
2) Import the reference
3) Create a new `IInterpreter`
    
    + **NOTE:** You **MUST** know what language the code is in!

```cs
// Our code to execute (this does not have to be a constant)
const string CODE = "local sum = 10 + 20 \r\n" +
                    "print(sum)";

// This code is in lua because of the local declaration
IInterpreter interpreter = new LuaInterpreter();
/*
 * Essentially what initializes the Sandboxing environment
 * The first and only parameter is where to direct print functions
*/
interpreter.StartSandbox(Console.WriteLine);
// Execute the code. This can be ran multiple times.
interpreter.RunScript(CODE);
// Call Stop when you're done with the interpreter
interpreter.Stop();
```

### Creating Global Values

You can create global values by using the `IInterpreter.CreateGlobal(string name, object global)` method

```cs
// Typically, you would use JavaScript's Math.PI; however, for the example, we will make our own
const string CODE = "let sum = pi + 20 \r\n" +
                    "print(sum)";

// This code is in JavaScript because of the let declaration
IInterpreter interpreter = new JavaScriptInterpreter();
interpreter.StartSandbox(Console.WriteLine);
// Create our pi global
interpreter.CreateGlobal("pi", Math.PI);
interpreter.RunScript(CODE);
interpreter.Stop();
```

### Creating Reference Types

In some scenarios, you may want to share classes with the interpreter. **PLEASE MAKE SURE THESE CLASSES ARE SAFE TO SHARE!**

In this example, we create our own data object, then pass it to the interpreter to use.

```cs
const string LUA_CODE = "local person = PersonData() \r\n" +
                        "person.Name = \"Joe\" \r\n" +
                        "person.Age = 42 \r\n" +
                        "print(person.Name..\" is \"..tostring(person.Age)..\" years old!\")";
const string JS_CODE = "let person = new PersonData() \r\n" +
                       "person.Name = \"Amy\" \r\n" +
                       "person.Age = 25 \r\n" +
                       "print(person.Name + \" is \" + person.Age + \" years old!\")";
// Parameter Constructors can be utilized in JavaScript
// When running a Script under the same interpreter, be sure to change variable names!
const string JS_CODE2 = "let person2 = new PersonData(\"Michael\", 17) \r\n" +
                        "print(person2.Name + \" is \" + person2.Age + \" years old!\")";

IInterpreter lua = new LuaInterpreter();
IInterpreter js = new JavaScriptInterpreter();
lua.StartSandbox(Console.WriteLine);
js.StartSandbox(Console.WriteLine);
lua.ForwardType("PersonData", typeof(PersonData));
js.ForwardType("PersonData", typeof(PersonData));
lua.RunScript(LUA_CODE);
js.RunScript(JS_CODE);
js.RunScript(JS_CODE2);
lua.Stop();
js.Stop();

public class PersonData
{
    public string Name;
    public int Age;

    public PersonData(){}
    public PersonData(string Name, int Age)
    {
        this.Name = Name;
        this.Age = Age;
    }
}
```

**NOTE**: All types passed into `IInterpreter.ForwardType(string, Type)` will make the type need to be created.

For JavaScript, do

```js
let obj = new Type()
```

and for Lua, do

```lua
local obj = Type()
```

There **MUST** be a parameter-less constructor, otherwise an exception will be thrown when Forwarding the Type in Lua.
