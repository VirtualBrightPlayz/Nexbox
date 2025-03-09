using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Nexbox;

namespace LibRiscV
{
    public class LibRiscVInterpreter : IInterpreter
    {
        public const string HEADER_START = @"// This file is auto-generated

#ifndef APICALL_H
#define APICALL_H

#include ""macrolib.h""
#include ""syscall.h""

#define PUBLIC __attribute__((used, retain))

typedef struct {
    void* target;
    void* args[8];
} UserArgStruct;
";

        public const string HEADER_STRUCT_DEF = @"
typedef struct {0} {{
{1}
}} {0};
";
        public const string HEADER_STRUCT_DEF_FWD = "typedef struct {0} {0};\n";
        public const string HEADER_STRUCT_PROP = "    {0} {1};";
        public const string HEADER_CLASS_DEF_FWD_DECLARE = "API_OBJECT_FWD_DECLARE({0})\n";
        public const string HEADER_CLASS_DEF_FWD = "API_OBJECT_DECLARE({0})\n";
        public const string HEADER_CLASS_DEF_FWD_STATIC_FUNC = "API_METHOD_{0}_DECLARE({4}, {1}, {2}, {3})\n";
        public const string HEADER_CLASS_DEF_FWD_FUNC = "API_OBJECT_METHOD_{0}_DECLARE({4}, {1}, {2}, {3})\n";
        public const string HEADER_CLASS_DEF = "API_OBJECT_BEGIN({0})\n";
        public const string HEADER_CLASS_DEF_STATIC_FUNC = "API_METHOD_{0}({4}, {1}, {2}, {3})\n";
        public const string HEADER_CLASS_DEF_FUNC = "API_OBJECT_METHOD_{0}({4}, {1}, {2}, {3})\n";
        public const string HEADER_CLASS_DEF_END = "API_OBJECT_END()\n\n";
        public const string HEADER_CLASS_DEF_FWD_END = "API_OBJECT_DECLARE_END()\n\n";

        public const string HEADER_FUNC_RET = @"
static inline {0} {1}({2}) {{
    UserArgStruct args;
{3}
    {4}pusercall(""{1}"", &args);
}}
";
        public const string HEADER_FUNC_ARG_SIG = "{0} arg{1}";
        public const string HEADER_FUNC_TARGET = "    args.target = (void*)arg{0};";
        public const string HEADER_FUNC_ARG_PTR = "    args.args[{0}] = &arg{0};";
        public const string HEADER_FUNC_ARG_NOPTR = "    args.args[{0}] = arg{0};";

        public const string HEADER_END = @"
#endif // APICALL_H
";

        private Func<string> stdin;
        private Action<string> stdout;
        private Action<Exception> stderr;
        private LibRiscVSandbox sandbox;
        private Dictionary<string, object> globalObjs;
        private Dictionary<string, Type> modules;
        private Dictionary<string, Func<LibRiscVInterpreter, ulong, ulong>> funcs;
        private Dictionary<string, Delegate> delegates;
        private Dictionary<string, MethodDataInfo> methods;
        private bool stopped = false;
        private Dictionary<ulong, object> targets;
        private ulong targetIdCounter;
        private Queue<KeyValuePair<ulong, ulong>> valueMemory;

        private struct MethodDataInfo
        {
            public MethodBase method;
            public Type[] parameters;

            public MethodDataInfo(MethodBase m)
            {
                method = m;
                parameters = m.GetParameters().Select(x => x.ParameterType).ToArray();
            }
        }

        internal class LibRiscVEngine : IScriptEngine
        {
            internal LibRiscVInterpreter e;
            internal LibRiscVEngine(LibRiscVInterpreter e) => this.e = e;
        }

        #region APIs

        public bool IsStopped => stopped;

        public void StartSandbox(Action<object> print)
        {
            if (stopped && sandbox == null)
                return;
            stdout = print;
            sandbox = null;
            globalObjs = new Dictionary<string, object>();
            modules = new Dictionary<string, Type>();
            funcs = new Dictionary<string, Func<LibRiscVInterpreter, ulong, ulong>>();
            delegates = new Dictionary<string, Delegate>();
            methods = new Dictionary<string, MethodDataInfo>();
            targets = new Dictionary<ulong, object>();
            targetIdCounter = 1_000_000_000; // start at 1000000
            valueMemory = new Queue<KeyValuePair<ulong, ulong>>();
            CreateGlobal("engine", new LibRiscVEngine(this));
            ForwardType("SandboxFunc", typeof(SandboxFunc));
            ForwardType("LibRiscVEngine", typeof(LibRiscVEngine));
        }

        public void CreateGlobal(string name, object global)
        {
            if (sandbox == null)
            {
                globalObjs.Add($"get_{name}", global);
                Type type = global.GetType();
                if (type == typeof(void))
                    return;
                if (type == typeof(string))
                    return;
                if (type == typeof(ulong))
                    return;
                if (type == typeof(long))
                    return;
                if (type == typeof(uint))
                    return;
                if (type == typeof(int))
                    return;
                if (type == typeof(float))
                    return;
                if (type == typeof(bool))
                    return;
                /*foreach (var info in type.GetMethods(BindingFlags.Instance | BindingFlags.Public))
                {
                    string n = $"g_{name}_{info.Name}";
                    if (info.DeclaringType == type && !methods.ContainsKey(n))
                        methods.Add(n, info);
                }*/
                /*
                foreach (var info in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
                {
                    string n = $"g_{name}_new_{info.GetParameters().Length}";
                    if (info.DeclaringType == type && !methods.ContainsKey(n))
                        methods.Add(n, info);
                }
                */
            }
        }

        public void ForwardType(string module, Type type)
        {
            if (sandbox == null)
            {
                modules.Add(module, type);
                foreach (var info in type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public))
                {
                    string n = $"{module}_{info.Name}";
                    if (info.DeclaringType == type && !methods.ContainsKey(n))
                        methods.Add(n, new MethodDataInfo(info));
                }
                foreach (var info in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
                {
                    string n = $"{module}_new_{info.GetParameters().Length}";
                    if (info.DeclaringType == type && !methods.ContainsKey(n))
                        methods.Add(n, new MethodDataInfo(info));
                }
            }
        }

        public void RunScript(string script, Action<Exception> OnException)
        {
            RunScript(Convert.FromBase64String(script), OnException, "script");
        }

        public void RunScript(byte[] script, Action<Exception> OnException, params string[] args)
        {
            if (sandbox == null)
            {
                stderr = OnException;
                RunElf(script, args);
            }
        }

        public void Stop()
        {
            stopped = true;
            var box = sandbox;
            sandbox = null;
            box?.Stop();
            box?.Dispose();
        }

        public IScriptEngine GetEngine()
        {
            return new LibRiscVEngine(this);
        }

        public void CallFunction(object func, object args)
        {
            Jump((ulong)func, (object[])args);
        }

        #endregion

        #region Custom APIs

        private unsafe struct UserArgStruct
        {
            public ulong target;
            public fixed ulong args[8];
        }

        private unsafe ulong UserSyscall(ulong func, ulong vaddr)
        {
            if (sandbox != null)
            {
                byte* funcSpanBytes = sandbox.MemStringCopy(func, out uint funcSpanLen);
                char* funcSpanChars = stackalloc char[(int)funcSpanLen];
                Encoding.UTF8.GetChars(funcSpanBytes, (int)funcSpanLen, funcSpanChars, (int)funcSpanLen);
                Marshal.FreeHGlobal((IntPtr)funcSpanBytes);
                Span<char> funcSpan = new Span<char>(funcSpanChars, (int)funcSpanLen);
                foreach (var kvp in funcs)
                {
                    if (funcSpan.SequenceEqual(kvp.Key.AsSpan()))
                    {
                        return kvp.Value.Invoke(this, vaddr);
                    }
                }
                foreach (var gkvp in globalObjs)
                {
                    if (funcSpan.SequenceEqual(gkvp.Key.AsSpan()))
                    {
                        object ret = gkvp.Value;
                        if (stopped)
                            return 0;
                        if (ret == null)
                            return 0;
                        else if (ret is string str)
                            return MemAllocString(str);
                        else if (ret.GetType().IsValueType)
                            return MemAllocObject(ret);
                        else
                        {
                            foreach (var kvp in targets)
                            {
                                if (ReferenceEquals(kvp.Value, ret))
                                {
                                    return kvp.Key;
                                }
                            }
                            targets.Add(targetIdCounter, ret);
                            targetIdCounter++;
                            return targetIdCounter - 1;
                        }
                    }
                }
                foreach (var mkvp in methods)
                {
                    if (funcSpan.SequenceEqual(mkvp.Key.AsSpan()))
                    {
                        MethodBase method = mkvp.Value.method;
                        Type[] mArgs = mkvp.Value.parameters;
                        UserArgStruct args = MemGetObject<UserArgStruct>(vaddr);
                        ulong[] ptrArr = new ulong[mArgs.Length];
                        for (int i = 0; i < mArgs.Length; i++)
                        {
                            if (args.args[i] == 0)
                                ptrArr[i] = 0;
                            else
                                ptrArr[i] = MemGetPtr(args.args[i]);
                        }
                        object[] argArr = new object[mArgs.Length];
                        for (int i = 0; i < mArgs.Length; i++)
                        {
                            // TODO: add support for arrays
                            if (mArgs[i] == typeof(string))
                            {
                                argArr[i] = MemGetString(ptrArr[i]);
                            }
                            else if (!mArgs[i].IsValueType)
                            {
                                if (ptrArr[i] == 0)
                                {
                                    argArr[i] = null;
                                }
                                else if (targets.TryGetValue(ptrArr[i], out var targ))
                                {
                                    argArr[i] = targ;
                                }
                                else
                                {
                                    argArr[i] = ptrArr[i];
                                }
                            }
                            else
                            {
                                object arg = MemGetObjectFromType(args.args[i], mArgs[i]);
                                argArr[i] = arg;
                            }
                        }
                        object target = null;
                        if (args.target != 0)
                        {
                            targets.TryGetValue(args.target, out target);
                        }
                        object ret = null;
                        // try
                        {
                            if (method is ConstructorInfo ctor)
                                ret = ctor.Invoke(argArr);
                            else
                                ret = method.Invoke(target, argArr);
                        }
                        // catch (TargetException)
                        // {
                        //     stdout?.Invoke(method.Name);
                        //     throw;
                        // }
                        if (stopped)
                            return 0;
                        if (ret == null)
                            return 0;
                        else if (ret is string str)
                            return MemAllocString(str);
                        else if (ret is float fl)
                            return sandbox.StackPushFloat(fl);
                        else if (ret.GetType().IsValueType)
                            return MemAllocObject(ret);
                        else
                        {
                            foreach (var kvp in targets)
                            {
                                if (ReferenceEquals(kvp.Value, ret))
                                {
                                    return kvp.Key;
                                }
                            }
                            targets.Add(targetIdCounter, ret);
                            targetIdCounter++;
                            return targetIdCounter - 1;
                        }
                    }
                }
                throw new Exception($"User System Call not found: {funcSpan.ToString()}");
            }
            return 0;
        }

        public void ForwardFunction(string name, Delegate info)
        {
            if (sandbox == null)
                delegates.Add(name, info);
        }

        public void ForwardFunctionRaw(string name, Func<LibRiscVInterpreter, ulong, ulong> func)
        {
            if (sandbox == null)
                funcs.Add(name, func);
        }

        public string MemGetString(ulong vaddr)
        {
            if (sandbox == null)
                return default;
            return sandbox.MemString(vaddr);
        }

        public T MemGetObject<T>(ulong vaddr) where T : unmanaged
        {
            if (sandbox == null)
                return default;
            IntPtr ptr = sandbox.MemObject(vaddr, (uint)Marshal.SizeOf<T>());
            return Marshal.PtrToStructure<T>(ptr);
        }

        public IntPtr MemGetObjectPtr(ulong vaddr, uint size)
        {
            if (sandbox == null)
                return IntPtr.Zero;
            IntPtr ptr = sandbox.MemObject(vaddr, size);
            return ptr;
        }

        public unsafe ulong MemGetPtr(ulong vaddr)
        {
            IntPtr ptr = sandbox.MemObject(vaddr, sizeof(ulong));
            ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(ptr.ToPointer(), sizeof(ulong));
            return BitConverter.ToUInt64(span.ToArray(), 0);
        }

        public unsafe object MemGetObjectFromType(ulong vaddr, Type type)
        {
            if (sandbox == null)
                return default;
            if (type == typeof(bool))
                return (MemGetPtr(vaddr) & 1) != 0;
            if (type == typeof(ulong))
                return MemGetPtr(vaddr);
            if (type == typeof(long))
                return unchecked((long)MemGetPtr(vaddr));
            if (type == typeof(uint))
                return unchecked((uint)MemGetPtr(vaddr));
            if (type == typeof(int))
                return unchecked((int)MemGetPtr(vaddr));
            if (type == typeof(float))
            {
                Span<ulong> sp = stackalloc ulong[1];
                sp[0] = MemGetPtr(vaddr);
                return MemoryMarshal.Cast<ulong, float>(sp)[0];
            }
            uint size = (uint)Marshal.SizeOf(type);
            IntPtr ptr = sandbox.MemObject(vaddr, size);
            return Marshal.PtrToStructure(ptr, type);
        }

        public ulong MemAllocObject(object obj)
        {
            FreeMappedValues();
            if (sandbox == null)
                return 0;
            ulong objAddr = sandbox.MemMap((ulong)Marshal.SizeOf(obj));
            valueMemory.Enqueue(new KeyValuePair<ulong, ulong>(objAddr, (ulong)Marshal.SizeOf(obj)));
            if (objAddr == 0)
                return objAddr;
            sandbox.MemSetObject(objAddr, obj);
            return objAddr;
        }

        public ulong MemAllocObject<T>(T obj) where T : unmanaged
        {
            FreeMappedValues();
            if (sandbox == null)
                return 0;
            ulong objAddr = sandbox.MemMap((ulong)Marshal.SizeOf<T>());
            valueMemory.Enqueue(new KeyValuePair<ulong, ulong>(objAddr, (ulong)Marshal.SizeOf<T>()));
            if (objAddr == 0)
                return objAddr;
            sandbox.MemSetObject(objAddr, obj);
            return objAddr;
        }

        public ulong MemAllocString(string str)
        {
            FreeMappedValues();
            if (sandbox == null)
                return 0;
            ulong strAddr = sandbox.MemMap((ulong)(str.Length + 1));
            valueMemory.Enqueue(new KeyValuePair<ulong, ulong>(strAddr, (ulong)(str.Length + 1)));
            if (strAddr == 0)
                return strAddr;
            sandbox.MemSetString(strAddr, str);
            return strAddr;
        }

        public void FreeMappedValues()
        {
            if (sandbox == null)
                return;
            if (valueMemory.Count > 16)
            {
                while (valueMemory.Count > 0)
                {
                    var kvp = valueMemory.Dequeue();
                    sandbox.MemUnmap(kvp.Key, kvp.Value);
                }
            }
        }

        public ulong Jump(ulong addr, params object[] args)
        {
            if (sandbox == null)
                return 0;
            if (sandbox.CallPtr(addr, out long ret, args))
            {
                return unchecked((ulong)ret);
            }
            return 0;
        }

        public void SetStdIn(Func<string> func)
        {
            stdin = func;
        }

        private void RunElf(byte[] elf, params string[] args)
        {
            if (sandbox == null)
            {
                sandbox = new LibRiscVSandbox(elf, stdin, stdout, (type, msg, data) => stderr?.Invoke(new Exception($"Error: type={type} msg={msg} data={data}")), UserSyscall, args);
                long ret = 0;
                while (ret == 0)
                {
                    if (sandbox.Run(out ret))
                        break;
                }
                if (ret != 0)
                {
                    Stop();
                }
            }
        }

        #endregion

        #region Headers

        private Queue<Type> headerExportedTypes = new Queue<Type>();

        public string ExportHeader()
        {
            headerExportedTypes.Clear();
            StringBuilder sb = new StringBuilder();
            sb.Append(HEADER_START);

            StringBuilder body = new StringBuilder();
            foreach (var kvp in globalObjs)
            {
                body.Append(ExportHeaderGlobal(kvp.Key, kvp.Value.GetType()));
            }
            foreach (var kvp in delegates)
            {
                ExportHeaderFunction(kvp.Key, kvp.Value.Method, 1);
                // body.Append(ExportHeaderFunction(kvp.Key, kvp.Value.Method, 1));
            }
            foreach (var kvp in methods)
            {
                ExportHeaderFunction(kvp.Key, kvp.Value.method);
                // body.Append(ExportHeaderFunction(kvp.Key, kvp.Value));
            }

            List<Type> exported = new List<Type>();
            Dictionary<Type, string> defs = new Dictionary<Type, string>();
            Dictionary<Type, List<Type>> deps = new Dictionary<Type, List<Type>>();
            Dictionary<Type, int> depsCount = new Dictionary<Type, int>();
            while (headerExportedTypes.Count > 0)
            {
                Type t = headerExportedTypes.Dequeue();
                if (exported.Contains(t))
                    continue;
                Type nt = Nullable.GetUnderlyingType(t);
                if (nt != null)
                    headerExportedTypes.Enqueue(nt);
                else if (t.IsArray)
                    headerExportedTypes.Enqueue(t.GetElementType());
                else
                {
                    exported.Add(t);
                    deps.Add(t, new List<Type>());
                    defs.Add(t, ExportHeaderStruct(t, deps[t], true));
                    depsCount.Add(t, GetAllDepends(t).Count);
                }
            }
            List<Type> forwarded = new List<Type>();
            StringBuilder fwddecls = new StringBuilder();
            StringBuilder decls = new StringBuilder();
            StringBuilder impls = new StringBuilder();
            foreach (var kvp in depsCount.OrderBy(x => x.Value))
            {
                if (!forwarded.Contains(kvp.Key) && !kvp.Key.IsArray)
                {
                    forwarded.Add(kvp.Key);
                    if (kvp.Key == typeof(string))
                        continue;
                    if (kvp.Key.IsValueType)
                        fwddecls.Append(string.Format(HEADER_STRUCT_DEF_FWD, GetCType(kvp.Key, false)));
                    else
                    {
                        fwddecls.Append(string.Format(HEADER_CLASS_DEF_FWD_DECLARE, GetCType(kvp.Key, false)));
                        decls.Append(ExportHeaderStruct(kvp.Key, new List<Type>(), false));
                    }
                    impls.Append(ExportHeaderStruct(kvp.Key, new List<Type>(), true));
                }
            }
            sb.Append(fwddecls.ToString());
            sb.Append(decls.ToString());
            sb.Append(impls.ToString());
            sb.Append(body.ToString());

            sb.Append(HEADER_END);
            return sb.ToString();
        }

        public Dictionary<Type, int> GetAllDepends(Type type)
        {
            Dictionary<Type, int> deps = new Dictionary<Type, int>();
            Queue<Type> toBeChecked = new Queue<Type>();
            toBeChecked.Enqueue(type);
            while (toBeChecked.Count > 0)
            {
                Type checkType = toBeChecked.Dequeue();
                if (deps.ContainsKey(checkType))
                    continue;
                List<Type> types = GetDirectDepends(checkType);
                deps.Add(checkType, types.Count);
                foreach (var t in types)
                    toBeChecked.Enqueue(t);
            }
            return deps;
        }

        public List<Type> GetDirectDepends(Type type)
        {
            List<Type> deps = new List<Type>();
            foreach (var method in methods)
            {
                if (method.Value.method.DeclaringType == type)
                {
                    if (method.Value.method is MethodInfo info)
                    {
                        ParameterInfo[] margs = method.Value.method.GetParameters();
                        for (int i = 0; i < margs.Length; i++)
                        {
                            if (!IsBaseCType(margs[i].ParameterType))
                                deps.Add(margs[i].ParameterType);
                        }
                        if (!IsBaseCType(info.ReturnType))
                            deps.Add(info.ReturnType);
                    }
                    else if (method.Value.method is ConstructorInfo ctorInfo && !ctorInfo.IsStatic)
                    {
                        ParameterInfo[] margs = method.Value.method.GetParameters();
                        for (int i = 0; i < margs.Length; i++)
                        {
                            if (!IsBaseCType(margs[i].ParameterType))
                                deps.Add(margs[i].ParameterType);
                        }
                        if (!IsBaseCType(ctorInfo.DeclaringType))
                            deps.Add(ctorInfo.DeclaringType);
                    }
                }
            }
            return deps;
        }

        public string ExportHeaderClass(Type type, List<Type> deps, bool impl)
        {
            StringBuilder sb2 = new StringBuilder();
            string typename = GetCType(type, false);
            sb2.Append(string.Format(impl ? HEADER_CLASS_DEF : HEADER_CLASS_DEF_FWD, typename));
            foreach (var method in methods)
            {
                if (method.Value.method.DeclaringType == type)
                {
                    if (method.Value.method is MethodInfo info)
                    {
                        ParameterInfo[] margs = method.Value.method.GetParameters();
                        string[] nameAndArgs = new string[margs.Length + 1];
                        nameAndArgs[0] = method.Value.method.Name;
                        for (int i = 0; i < margs.Length; i++)
                        {
                            if (!IsBaseCType(margs[i].ParameterType))
                                deps.Add(margs[i].ParameterType);
                            string fmt = "{0}, _{1}";
                            // if (margs[i].ParameterType.IsValueType)
                            //     fmt = "{0}*, _{1}";
                            nameAndArgs[i+1] = string.Format(fmt, GetCType(margs[i].ParameterType, false), margs[i].Name);
                        }
                        if (!IsBaseCType(info.ReturnType))
                            deps.Add(info.ReturnType);
                        string ret = GetCType(info.ReturnType, false);
                        if (info.ReturnType.IsValueType && info.ReturnType != typeof(void) && info.ReturnType != typeof(float))
                            ret += "*";
                        string funcFmt = info.IsStatic ? HEADER_CLASS_DEF_FWD_STATIC_FUNC : HEADER_CLASS_DEF_FWD_FUNC;
                        if (impl)
                            funcFmt = info.IsStatic ? HEADER_CLASS_DEF_STATIC_FUNC : HEADER_CLASS_DEF_FUNC;
                        string pfx = string.Empty;
                        if (info.ReturnType != typeof(void))
                            pfx += "RET_";
                        string call = "pusercall";
                        if (info.ReturnType == typeof(float))
                            call = "fpusercall";
                        else if (IsBaseCValueType(info.ReturnType))
                            call = "usercall";
                        sb2.Append(string.Format(funcFmt, pfx + margs.Length, typename, ret, string.Join(", ", nameAndArgs), call));
                    }
                    else if (method.Value.method is ConstructorInfo ctorInfo && !ctorInfo.IsStatic)
                    {
                        ParameterInfo[] margs = method.Value.method.GetParameters();
                        string[] nameAndArgs = new string[margs.Length + 1];
                        nameAndArgs[0] = "new_" + margs.Length;
                        for (int i = 0; i < margs.Length; i++)
                        {
                            if (!IsBaseCType(margs[i].ParameterType))
                                deps.Add(margs[i].ParameterType);
                            string fmt = "{0}, _{1}";
                            // if (margs[i].ParameterType.IsValueType)
                                // fmt = "{0}*, _{1}";
                            nameAndArgs[i+1] = string.Format(fmt, GetCType(margs[i].ParameterType, false), margs[i].Name);
                        }
                        if (!IsBaseCType(ctorInfo.DeclaringType))
                            deps.Add(ctorInfo.DeclaringType);
                        string ret = GetCType(ctorInfo.DeclaringType, false);
                        if (ctorInfo.DeclaringType.IsValueType)
                            ret += "*";
                        string pfx = "RET_";
                        sb2.Append(string.Format(impl ? HEADER_CLASS_DEF_STATIC_FUNC : HEADER_CLASS_DEF_FWD_STATIC_FUNC, pfx + margs.Length, typename, ret, string.Join(", ", nameAndArgs), "pusercall"));
                    }
                }
            }
            sb2.Append(impl ? HEADER_CLASS_DEF_END : HEADER_CLASS_DEF_FWD_END);
            return sb2.ToString();
        }

        public string ExportHeaderStruct(Type type, List<Type> deps, bool impl)
        {
            if (!type.IsValueType)
            {
                return ExportHeaderClass(type, deps, impl);
            }
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
            string[] code = new string[fields.Length];
            for (int i = 0; i < fields.Length; i++)
            {
                if (!IsBaseCType(fields[i].FieldType))
                    deps.Add(fields[i].FieldType);
                code[i] = string.Format(HEADER_STRUCT_PROP, GetCType(fields[i].FieldType, false), fields[i].Name);
            }
            if (!IsBaseCType(type))
                deps.Add(type);
            return string.Format(HEADER_STRUCT_DEF, GetCType(type, false), string.Join("\n", code));
        }

        public string ExportHeaderGlobal(string name, Type type)
        {
            string ret = GetCType(type);
            // return string.Format(HEADER_FUNC_RET, ret, name, "", "", "return ");
            return string.Format(HEADER_FUNC_RET, ret, name, "", "", $"return ({ret})");
        }

        public string ExportHeaderFunction(string name, MethodBase info, int skip = 0)
        {
            ParameterInfo[] margs = info.GetParameters().Skip(skip).ToArray();
            int start = 0;
            int len = margs.Length;
            if (!info.IsStatic && skip == 0 && !info.IsConstructor)
            {
                start++;
                len++;
            }
            string[] args = new string[len];
            string[] code = new string[len];
            for (int i = start; i < len; i++)
            {
                args[i] = string.Format(HEADER_FUNC_ARG_SIG, GetCType(margs[i-start].ParameterType), i-start);
                code[i] = string.Format(margs[i-start].ParameterType.IsValueType ? HEADER_FUNC_ARG_PTR : HEADER_FUNC_ARG_NOPTR, i-start);
            }
            if (!info.IsStatic && skip == 0 && !info.IsConstructor)
            {
                args[0] = string.Format(HEADER_FUNC_ARG_SIG, GetCType(info.DeclaringType), "t");
                code[0] = string.Format(HEADER_FUNC_TARGET, "t");
            }
            Type retType = typeof(void);
            if (info.IsConstructor)
                retType = info.DeclaringType;
            else if (info is MethodInfo info2)
                retType = info2.ReturnType;
            string ret = GetCType(retType);
            if (IsBaseCValueType(retType))
                ret += "*";
            return string.Format(HEADER_FUNC_RET, ret, name, string.Join(", ", args), string.Join("\n", code), ret == "void" ? "" : $"return ({ret})");
        }

        public string GetCType(Type type, bool add = true)
        {
            if (type == typeof(void))
                return "void";
            if (type == typeof(string))
                return "const char*";
            if (type == typeof(ulong))
                return "unsigned long long";
            if (type == typeof(long))
                return "long long";
            if (type == typeof(uint))
                return "unsigned int";
            if (type == typeof(int))
                return "int";
            if (type == typeof(float))
                return "float";
            if (type == typeof(bool))
                return "bool";
            headerExportedTypes.Enqueue(type);
            Type nt = Nullable.GetUnderlyingType(type);
            if (nt != null)
                return GetCType(nt);
            if (type.IsArray)
                return GetCType(type.GetElementType()) + "*";
            foreach (var mod in modules)
            {
                if (mod.Value == type)
                {
                    return mod.Key.Replace("`", "_").Replace("&", "").Replace("[]", "*");
                }
            }
            return type.Name.Replace("`", "_").Replace("&", "").Replace("[]", "*");
        }

        public bool IsBaseCType(Type type)
        {
            if (type == typeof(void))
                return true;
            if (type == typeof(string))
                return true;
            if (type == typeof(ulong))
                return true;
            if (type == typeof(long))
                return true;
            if (type == typeof(uint))
                return true;
            if (type == typeof(int))
                return true;
            if (type == typeof(float))
                return true;
            if (type == typeof(bool))
                return true;
            return false;
        }

        public bool IsBaseCValueType(Type type)
        {
            if (type == typeof(string))
                return true;
            if (type == typeof(ulong))
                return true;
            if (type == typeof(long))
                return true;
            if (type == typeof(uint))
                return true;
            if (type == typeof(int))
                return true;
            if (type == typeof(float))
                return true;
            if (type == typeof(bool))
                return true;
            return false;
        }

        public bool IsCValueType(Type type)
        {
            if (IsBaseCType(type))
                return true;
            /*
            if (type == typeof(void))
                return false;
            if (type == typeof(string))
                return false;
            if (type == typeof(ulong))
                return false;
            if (type == typeof(long))
                return false;
            if (type == typeof(uint))
                return false;
            if (type == typeof(int))
                return false;
            if (type == typeof(float))
                return false; // note.
            if (type == typeof(bool))
                return false;
            */
            return !type.IsValueType;
        }

        #endregion
    }
}