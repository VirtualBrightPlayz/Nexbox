def gen_api_method_declare(m, n, pfx):
    s = ["call", "type", "ret", "name"]
    arg = []
    for i in range(n):
        s.append(f"type{i}")
        s.append(f"name{i}")
        arg.append(f"type{i} name{i}")
    return f"#define {m}_{n}_DECLARE({", ".join(s)}) {pfx}inline ret name({", ".join(arg)});"

def gen_api_method(m, n, r, body):
    s = ["call", "type", "ret", "name"]
    arg = []
    for i in range(n):
        s.append(f"type{i}")
        s.append(f"name{i}")
        arg.append(f"type{i} name{i}")
        body.append(f"args.args[{i}] = (void*)&name{i};")
    return f"""#define {m}_{n}({", ".join(s)}) \\
inline ret type::name({", ".join(arg)}) {{ \\
    UserArgStruct args; \\
    {" \\\n    ".join(body)} \\
    {r}call(#type "_" #name, &args); \\
}}"""


print("""#ifndef MACROLIB_H
#define MACROLIB_H

#define PUBLIC __attribute__((used, retain))

#ifdef __cplusplus

#define API_OBJECT_FWD_DECLARE(name) struct name;

#define API_OBJECT_DECLARE(name) typedef void* name##_ptr; struct name { \\
    name##_ptr addr; \\
    name(void* ptr) { \\
        this->addr = (name##_ptr)ptr; \\
    }
#define API_OBJECT_DECLARE_END() };

#define API_OBJECT_BEGIN(name)
#define API_OBJECT_END()
""")
for i in range(8):
    print(gen_api_method_declare("API_METHOD", i, "static "))
print("")
for i in range(8):
    print(gen_api_method_declare("API_METHOD_RET", i, "static "))
print("")
for i in range(8):
    print(gen_api_method_declare("API_OBJECT_METHOD", i, ""))
print("")
for i in range(8):
    print(gen_api_method_declare("API_OBJECT_METHOD_RET", i, ""))
print("")
for i in range(8):
    print(gen_api_method("API_METHOD", i, "", []))
print("")
for i in range(8):
    print(gen_api_method("API_METHOD_RET", i, "return (ret)", []))
print("")
for i in range(8):
    print(gen_api_method("API_OBJECT_METHOD", i, "", ["args.target = (void*)this->addr;"]))
print("")
for i in range(8):
    print(gen_api_method("API_OBJECT_METHOD_RET", i, "return (ret)", ["args.target = (void*)this->addr;"]))
print("""
#endif

#endif""")
