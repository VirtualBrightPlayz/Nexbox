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
#define API_METHOD_RET_V(ret, name, num_args, ...) \
static inline ret name(...) { \
    UserArgStruct args; \
    va_list valist; \
    int num = num_args; \
    va_start(valist, num); \
    for (int i = 0; i < num; i++) { \
        args.args[i] = va_arg(valist, void*); \
    } \
    va_end(valist); \
    return (ret)pusercall(#name, &args); \
}
#define API_OBJECT_METHOD_RET_V(type, ret, name, num_args, ...) \
inline ret name(...) { \
    UserArgStruct args; \
    args.target = (void*)this->addr; \
    va_list valist; \
    int num = num_args; \
    va_start(valist, num); \
    for (int i = 0; i < num; i++) { \
        args.args[i] = va_arg(valist, void*); \
    } \
    va_end(valist); \
    return (ret)pusercall(#name, &args); \
}


#define API_METHOD_RET_0(type, ret, name) \
static inline ret name() { \
    return (ret)type##_##name(); \
}

#define API_OBJECT_METHOD_RET_0(type, ret, name) \
inline ret name() { \
    return (ret)type##_##name((void*)this->addr); \
}
#define API_OBJECT_METHOD_RET_1(type, ret, name, type1, name1) \
inline ret name(type1 name1) { \
    return (ret)type##_##name((void*)this->addr, name1); \
}
#define API_OBJECT_METHOD_RET_2(type, ret, name, type1, name1, type2, name2) \
inline ret name(type1 name1, type2 name2) { \
    return (ret)type##_##name((void*)this->addr, name1, name2); \
}
#define API_OBJECT_METHOD_RET_3(type, ret, name, type1, name1, type2, name2, type3, name3) \
inline ret name(type1 name1, type2 name2, type3 name3) { \
    return (ret)type##_##name((void*)this->addr, name1, name2, name3); \
}
#define API_OBJECT_METHOD_RET_4(type, ret, name, type1, name1, type2, name2, type3, name3, type4, name4) \
inline ret name(type1 name1, type2 name2, type3 name3, type4 name4) { \
    return (ret)type##_##name((void*)this->addr, name1, name2, name3, name4); \
}

#else

#define API_OBJECT_BEGIN(name) typedef void* name##_ptr; typedef name##_ptr name;
#define API_OBJECT_END()
#define API_METHOD_RET_V(ret, name, num_args, ...) \
static inline ret name##_static(...) { \
    UserArgStruct args; \
    va_list valist; \
    int num = num_args; \
    va_start(valist, num); \
    for (int i = 0; i < num; i++) { \
        args.args[i] = va_arg(valist, void*); \
    } \
    va_end(valist); \
    return (ret)pusercall(#name, &args); \
}
#define API_OBJECT_METHOD_RET_V(type, ret, name, num_args, ...) \
static inline ret type##_##name(type argt, ...) { \
    UserArgStruct args; \
    args.target = argt; \
    va_list valist; \
    int num = num_args; \
    va_start(valist, num); \
    for (int i = 0; i < num; i++) { \
        args.args[i] = va_arg(valist, void*); \
    } \
    va_end(valist); \
    return (ret)pusercall(#name, &args); \
}

#endif

#endif