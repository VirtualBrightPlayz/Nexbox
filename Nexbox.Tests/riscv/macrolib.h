#ifndef MACROLIB_H
#define MACROLIB_H

#define PUBLIC __attribute__((used, retain))

#ifdef __cplusplus

#define API_OBJECT_BEGIN(name) typedef void* name##_ptr; struct name { \
    name##_ptr addr; \
    name(void* ptr) { \
        this->addr = (name##_ptr)ptr; \
    } \
    operator void*() { return this->addr; }
#define API_OBJECT_END() };

#define API_METHOD_RET_0(type, ret, name) \
static inline ret name() { \
    UserArgStruct args; \
    return (ret)pusercall(#type "_" #name, &args); \
}
#define API_METHOD_RET_1(type, ret, name, type1, name1) \
static inline ret name(type1 name1) { \
    UserArgStruct args; \
    args.args[0] = (void*)name1; \
    return (ret)pusercall(#type "_" #name, &args); \
}
#define API_METHOD_RET_2(type, ret, name, type1, name1, type2, name2) \
static inline ret name(type1 name1, type2 name2) { \
    UserArgStruct args; \
    args.args[0] = (void*)name1; \
    args.args[1] = (void*)name2; \
    return (ret)pusercall(#type "_" #name, &args); \
}
#define API_METHOD_RET_3(type, ret, name, type1, name1, type2, name2, type3, name3) \
static inline ret name(type1 name1, type2 name2, type3 name3) { \
    UserArgStruct args; \
    args.args[0] = (void*)name1; \
    args.args[1] = (void*)name2; \
    args.args[2] = (void*)name3; \
    return (ret)pusercall(#type "_" #name, &args); \
}
#define API_METHOD_RET_4(type, ret, name, type1, name1, type2, name2, type3, name3, type4, name4) \
static inline ret name(type1 name1, type2 name2, type3 name3, type4 name4) { \
    UserArgStruct args; \
    args.args[0] = (void*)name1; \
    args.args[1] = (void*)name2; \
    args.args[2] = (void*)name3; \
    args.args[3] = (void*)name4; \
    return (ret)pusercall(#type "_" #name, &args); \
}

#define API_OBJECT_METHOD_RET_0(type, ret, name) \
inline ret name() { \
    UserArgStruct args; \
    args.target = (void*)this->addr; \
    return (ret)pusercall(#type "_" #name, &args); \
}
#define API_OBJECT_METHOD_RET_1(type, ret, name, type1, name1) \
inline ret name(type1 name1) { \
    UserArgStruct args; \
    args.target = (void*)this->addr; \
    args.args[0] = (void*)name1; \
    return (ret)pusercall(#type "_" #name, &args); \
}
#define API_OBJECT_METHOD_RET_2(type, ret, name, type1, name1, type2, name2) \
inline ret name(type1 name1, type2 name2) { \
    UserArgStruct args; \
    args.target = (void*)this->addr; \
    args.args[0] = (void*)name1; \
    args.args[1] = (void*)name2; \
    return (ret)pusercall(#type "_" #name, &args); \
}
#define API_OBJECT_METHOD_RET_3(type, ret, name, type1, name1, type2, name2, type3, name3) \
inline ret name(type1 name1, type2 name2, type3 name3) { \
    UserArgStruct args; \
    args.target = (void*)this->addr; \
    args.args[0] = (void*)name1; \
    args.args[1] = (void*)name2; \
    args.args[2] = (void*)name3; \
    return (ret)pusercall(#type "_" #name, &args); \
}
#define API_OBJECT_METHOD_RET_4(type, ret, name, type1, name1, type2, name2, type3, name3, type4, name4) \
inline ret name(type1 name1, type2 name2, type3 name3, type4 name4) { \
    UserArgStruct args; \
    args.target = (void*)this->addr; \
    args.args[0] = (void*)name1; \
    args.args[1] = (void*)name2; \
    args.args[2] = (void*)name3; \
    args.args[3] = (void*)name4; \
    return (ret)pusercall(#type "_" #name, &args); \
}

#else

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