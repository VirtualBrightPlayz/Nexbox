#ifndef MACROLIB_H
#define MACROLIB_H

#define PUBLIC __attribute__((used, retain))

#ifdef __cplusplus

#define API_METHOD_RET_5_DECLARE(call, type, ret, name, type1, name1, type2, name2, type3, name3, type4, name4, type5, name5)
#define API_METHOD_RET_6_DECLARE(call, type, ret, name, type1, name1, type2, name2, type3, name3, type4, name4, type5, name5, type6, name6)
#define API_OBJECT_METHOD_RET_5_DECLARE(call, type, ret, name, type1, name1, type2, name2, type3, name3, type4, name4, type5, name5)
#define API_OBJECT_METHOD_RET_6_DECLARE(call, type, ret, name, type1, name1, type2, name2, type3, name3, type4, name4, type5, name5, type6, name6)
#define API_METHOD_RET_5(call, type, ret, name, type1, name1, type2, name2, type3, name3, type4, name4, type5, name5)
#define API_METHOD_RET_6(call, type, ret, name, type1, name1, type2, name2, type3, name3, type4, name4, type5, name5, type6, name6)
#define API_OBJECT_METHOD_RET_5(call, type, ret, name, type1, name1, type2, name2, type3, name3, type4, name4, type5, name5)
#define API_OBJECT_METHOD_RET_6(call, type, ret, name, type1, name1, type2, name2, type3, name3, type4, name4, type5, name5, type6, name6)

#define API_OBJECT_FWD_DECLARE(name) struct name;

#define API_OBJECT_DECLARE(name) typedef void* name##_ptr; struct name { \
    name##_ptr addr; \
    name(void* ptr) { \
        this->addr = (name##_ptr)ptr; \
    }
#define API_OBJECT_DECLARE_END() };
#define API_METHOD_RET_0_DECLARE(call, type, ret, name) static inline ret name();
#define API_METHOD_RET_1_DECLARE(call, type, ret, name, type1, name1) static inline ret name(type1 name1);
#define API_METHOD_RET_2_DECLARE(call, type, ret, name, type1, name1, type2, name2) static inline ret name(type1 name1, type2 name2);
#define API_METHOD_RET_3_DECLARE(call, type, ret, name, type1, name1, type2, name2, type3, name3) static inline ret name(type1 name1, type2 name2, type3 name3);
#define API_METHOD_RET_4_DECLARE(call, type, ret, name, type1, name1, type2, name2, type3, name3, type4, name4) static inline ret name(type1 name1, type2 name2, type3 name3, type4 name4);

#define API_OBJECT_METHOD_RET_0_DECLARE(call, type, ret, name) inline ret name();
#define API_OBJECT_METHOD_RET_1_DECLARE(call, type, ret, name, type1, name1) inline ret name(type1 name1);
#define API_OBJECT_METHOD_RET_2_DECLARE(call, type, ret, name, type1, name1, type2, name2) inline ret name(type1 name1, type2 name2);
#define API_OBJECT_METHOD_RET_3_DECLARE(call, type, ret, name, type1, name1, type2, name2, type3, name3) inline ret name(type1 name1, type2 name2, type3 name3);
#define API_OBJECT_METHOD_RET_4_DECLARE(call, type, ret, name, type1, name1, type2, name2, type3, name3, type4, name4) inline ret name(type1 name1, type2 name2, type3 name3, type4 name4);


#define API_OBJECT_BEGIN(name)
#define API_OBJECT_END()

#define API_METHOD_RET_0(call, type, ret, name) \
static inline ret type::name() { \
    UserArgStruct args; \
    return (ret)call(#type "_" #name, &args); \
}
#define API_METHOD_RET_1(call, type, ret, name, type1, name1) \
static inline ret type::name(type1 name1) { \
    UserArgStruct args; \
    args.args[0] = (void*)&name1; \
    return (ret)call(#type "_" #name, &args); \
}
#define API_METHOD_RET_2(call, type, ret, name, type1, name1, type2, name2) \
static inline ret type::name(type1 name1, type2 name2) { \
    UserArgStruct args; \
    args.args[0] = (void*)&name1; \
    args.args[1] = (void*)&name2; \
    return (ret)call(#type "_" #name, &args); \
}
#define API_METHOD_RET_3(call, type, ret, name, type1, name1, type2, name2, type3, name3) \
static inline ret type::name(type1 name1, type2 name2, type3 name3) { \
    UserArgStruct args; \
    args.args[0] = (void*)&name1; \
    args.args[1] = (void*)&name2; \
    args.args[2] = (void*)&name3; \
    return (ret)call(#type "_" #name, &args); \
}
#define API_METHOD_RET_4(call, type, ret, name, type1, name1, type2, name2, type3, name3, type4, name4) \
static inline ret type::name(type1 name1, type2 name2, type3 name3, type4 name4) { \
    UserArgStruct args; \
    args.args[0] = (void*)&name1; \
    args.args[1] = (void*)&name2; \
    args.args[2] = (void*)&name3; \
    args.args[3] = (void*)&name4; \
    return (ret)call(#type "_" #name, &args); \
}

#define API_OBJECT_METHOD_RET_0(call, type, ret, name) \
inline ret type::name() { \
    UserArgStruct args; \
    args.target = (void*)this->addr; \
    return (ret)call(#type "_" #name, &args); \
}
#define API_OBJECT_METHOD_RET_1(call, type, ret, name, type1, name1) \
inline ret type::name(type1 name1) { \
    UserArgStruct args; \
    args.target = (void*)this->addr; \
    args.args[0] = (void*)&name1; \
    return (ret)call(#type "_" #name, &args); \
}
#define API_OBJECT_METHOD_RET_2(call, type, ret, name, type1, name1, type2, name2) \
inline ret type::name(type1 name1, type2 name2) { \
    UserArgStruct args; \
    args.target = (void*)this->addr; \
    args.args[0] = (void*)&name1; \
    args.args[1] = (void*)&name2; \
    return (ret)call(#type "_" #name, &args); \
}
#define API_OBJECT_METHOD_RET_3(call, type, ret, name, type1, name1, type2, name2, type3, name3) \
inline ret type::name(type1 name1, type2 name2, type3 name3) { \
    UserArgStruct args; \
    args.target = (void*)this->addr; \
    args.args[0] = (void*)&name1; \
    args.args[1] = (void*)&name2; \
    args.args[2] = (void*)&name3; \
    return (ret)call(#type "_" #name, &args); \
}
#define API_OBJECT_METHOD_RET_4(call, type, ret, name, type1, name1, type2, name2, type3, name3, type4, name4) \
inline ret type::name(type1 name1, type2 name2, type3 name3, type4 name4) { \
    UserArgStruct args; \
    args.target = (void*)this->addr; \
    args.args[0] = (void*)&name1; \
    args.args[1] = (void*)&name2; \
    args.args[2] = (void*)&name3; \
    args.args[3] = (void*)&name4; \
    return (ret)call(#type "_" #name, &args); \
}

#else

#error "Not supported yet"

#define API_OBJECT_DECLARE(name) typedef void* name;

#define API_OBJECT_BEGIN(name) typedef void* name;
#define API_OBJECT_END()

#define API_METHOD_RET_0(type, ret, name) \
static inline ret type##_##name() { \
    UserArgStruct args; \
    return (ret)pusercall(#type "_" #name, &args); \
}
#define API_METHOD_RET_1(type, ret, name, type1, name1) \
static inline ret type##_##name(type1 name1) { \
    UserArgStruct args; \
    args.args[0] = (void*)name1; \
    return (ret)pusercall(#type "_" #name, &args); \
}
#define API_METHOD_RET_2(type, ret, name, type1, name1, type2, name2) \
static inline ret type##_##name(type1 name1, type2 name2) { \
    UserArgStruct args; \
    args.args[0] = (void*)name1; \
    args.args[1] = (void*)name2; \
    return (ret)pusercall(#type "_" #name, &args); \
}
#define API_METHOD_RET_3(type, ret, name, type1, name1, type2, name2, type3, name3) static ret type##_##name(type1, type2, type3);
#define API_METHOD_RET_4(type, ret, name, type1, name1, type2, name2, type3, name3, type4, name4) static ret type##_##name(type1, type2, type3, type4);

#define API_OBJECT_METHOD_RET_0(type, ret, name) \
static inline ret type##_##name(type target) { \
    UserArgStruct args; \
    args.target = (void*)target; \
    return (ret)pusercall(#type "_" #name, &args); \
}
#define API_OBJECT_METHOD_RET_1(type, ret, name, type1, name1) \
static inline ret type##_##name(type target, type1 name1) { \
    UserArgStruct args; \
    args.target = (void*)target; \
    args.args[0] = (void*)name1; \
    return (ret)pusercall(#type "_" #name, &args); \
}
#define API_OBJECT_METHOD_RET_2(type, ret, name, type1, name1, type2, name2) \
static inline ret type##_##name(type target, type1 name1, type2 name2) { \
    UserArgStruct args; \
    args.target = (void*)target; \
    args.args[0] = (void*)name1; \
    args.args[1] = (void*)name2; \
    return (ret)pusercall(#type "_" #name, &args); \
}
#define API_OBJECT_METHOD_RET_3(type, ret, name, type1, name1, type2, name2, type3, name3) static ret type##_##name(type, type1, type2, type3);
#define API_OBJECT_METHOD_RET_4(type, ret, name, type1, name1, type2, name2, type3, name3, type4, name4) static ret type##_##name(type, type1, type2, type3, type4);

#endif

#endif