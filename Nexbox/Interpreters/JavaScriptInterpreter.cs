using System.Collections;
using System.Reflection;
using System.Text;
using Jint;
using Jint.Runtime.Interop;
using Nexbox.Internals;

namespace Nexbox.Interpreters;

public class JavaScriptInterpreter : IInterpreter
{
    internal bool stop;
    internal Engine engine;
    [ThreadStatic]
    internal static Engine activeEngine;
    internal Dictionary<Type, string> modules = new Dictionary<Type, string>();
    internal Dictionary<Type, string> globals = new Dictionary<Type, string>();
    internal Type[] builtinTypes = new Type[]
    {
        typeof(void),
        typeof(bool),
        typeof(int),
        typeof(long),
        typeof(float),
        typeof(double),
        typeof(string),
        typeof(IList),
        typeof(IDictionary),
        typeof(SimpleJSON.JSONNode),
        typeof(SandboxFunc),
    };

    public void StartSandbox(Action<object> print)
    {
        if (stop || engine != null)
            return;
        modules.Clear();
        globals.Clear();
        engine = new Engine(options =>
        {
            options.LimitMemory(4_000_000);
        });
        engine.SetValue("print", new Action<object>(print));
        CreateGlobal("engine", new JsEngine(this));
        ForwardType("SandboxFunc", typeof(SandboxFunc));
    }

    public void CreateGlobal(string name, object global)
    {
        if (stop)
            return;
        engine.SetValue(name, global);
        if (!globals.ContainsKey(global.GetType()))
            globals.Add(global.GetType(), name);
    }

    public void ForwardType(string module, Type type)
    {
        if (stop)
            return;
        engine.SetValue(module, TypeReference.CreateTypeReference(engine, type));
        if (!modules.ContainsKey(type))
            modules.Add(type, module);
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

    private string ExportTypeName(Type type)
    {
        if (type == null)
            return string.Empty;
        string array = type.HasElementType ? "[]" : string.Empty;
        if (type.HasElementType)
            type = type.GetElementType();
        if (!type.IsPrimitive && modules.TryGetValue(type, out string name))
            return name + array;
        if (!type.IsPrimitive && globals.TryGetValue(type, out string tname))
            return "Type_" + tname + array;
        if (type.IsGenericType)
            return type.Name.Replace('.', '_').Split('`')?[0] + array;
        if (type == typeof(object) || builtinTypes.Any(x => x.IsAssignableFrom(type)))
            return type.Name.Split('`')?[0] + array;
        return type.FullName.Replace('.', '_') + array;
    }

    private void ExportType(string name, Type type, StringBuilder sb, List<Type> extraTypes)
    {
        Type refType = type;
        if (refType.IsPrimitive)
            return;
        sb.AppendLine();
        sb.AppendLine($"declare class {name} {{");
        MemberInfo[] members = refType.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
        foreach (var member in members)
        {
            // JInt excludes this normally
            if (member.Name == nameof(GetType))
                continue;
            // don't include inherited members
            if (member.DeclaringType != refType)
                continue;
            switch (member)
            {
                case FieldInfo field:
                    extraTypes?.Add(field.FieldType);
                    sb.AppendLine($"public {field.Name}: {ExportTypeName(field.FieldType)};");
                    break;
                case PropertyInfo property:
                    extraTypes?.Add(property.PropertyType);
                    if (property.CanRead)
                        sb.AppendLine($"public get {property.Name}(): {ExportTypeName(property.PropertyType)};");
                    if (property.CanWrite)
                        sb.AppendLine($"public set {property.Name}(value: {ExportTypeName(property.PropertyType)});");
                    break;
                case MethodInfo method:
                    if (method.IsSpecialName)
                        break;
                    ParameterInfo[] parameters = method.GetParameters();
                    string[] types = new string[parameters.Length];
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        extraTypes?.Add(parameters[i].ParameterType);
                        if (parameters[i].GetCustomAttribute<ParamArrayAttribute>() != null)
                            types[i] = $"...{parameters[i].Name}: {ExportTypeName(parameters[i].ParameterType)}";
                        else
                            types[i] = $"{parameters[i].Name}: {ExportTypeName(parameters[i].ParameterType)}";
                    }
                    extraTypes?.Add(method.ReturnType);
                    string args = string.Join(", ", types);
                    if (method.IsStatic)
                        sb.AppendLine($"public static {method.Name}({args}): {ExportTypeName(method.ReturnType)};");
                    else
                        sb.AppendLine($"public {method.Name}({args}): {ExportTypeName(method.ReturnType)};");
                    break;
            }
        }
        sb.AppendLine($"}} // {name}");
    }

    public string ExportApiToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("type Void = void;");
        sb.AppendLine("type Int32 = Number;");
        sb.AppendLine("type Int64 = Number;");
        sb.AppendLine("type Single = Number;");
        sb.AppendLine("type Double = Number;");
        sb.AppendLine("type List = Array<Object>;");
        sb.AppendLine("type Dictionary = Object;");
        sb.AppendLine("type JSONNode = Object;");
        sb.AppendLine("type SimpleJSON_JSONNode = JSONNode;");
        sb.AppendLine("type SandboxFunc = Function;");
        sb.AppendLine("declare function print(...args: any): Void;");
        List<Type> extraTypes = new List<Type>();
        foreach (var mod in modules)
        {
            if (mod.Key == typeof(SandboxFunc))
                continue;
            ExportType(mod.Value, mod.Key, sb, extraTypes);
        }
        foreach (var mod in globals)
        {
            ExportType(ExportTypeName(mod.Key), mod.Key, sb, extraTypes);
            sb.AppendLine($"declare let {mod.Value}: {ExportTypeName(mod.Key)};");
        }
        foreach (var type in extraTypes.Select(x => x.HasElementType ? x.GetElementType() : x).Distinct())
        {
            if (modules.ContainsKey(type))
                continue;
            if (type.HasElementType)
            {
                continue;
            }
            if (type.IsGenericType)
            {
                continue;
            }
            if (type == typeof(object) || builtinTypes.Any(x => x.IsAssignableFrom(type)) || builtinTypes.Any(x => x.FullName == type.FullName))
                continue;
            if (type.IsPublic)
                ExportType(type.FullName.Replace('.', '_'), type, sb, null);
        }
        return sb.ToString();
    }
}