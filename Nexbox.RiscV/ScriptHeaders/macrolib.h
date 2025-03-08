#ifndef MACROLIB_H
#define MACROLIB_H

#define PUBLIC __attribute__((used, retain))

#ifdef __cplusplus

#define API_OBJECT_FWD_DECLARE(name) struct name;

#define API_OBJECT_DECLARE(name) typedef void* name##_ptr; struct name { \
    name##_ptr addr; \
    name(void* ptr) { \
        this->addr = (name##_ptr)ptr; \
    }
#define API_OBJECT_DECLARE_END() };

#define API_OBJECT_BEGIN(name)
#define API_OBJECT_END()

#define API_METHOD_0_DECLARE(call, type, ret, name) static inline ret name();
#define API_METHOD_1_DECLARE(call, type, ret, name, type0, name0) static inline ret name(type0 name0);
#define API_METHOD_2_DECLARE(call, type, ret, name, type0, name0, type1, name1) static inline ret name(type0 name0, type1 name1);
#define API_METHOD_3_DECLARE(call, type, ret, name, type0, name0, type1, name1, type2, name2) static inline ret name(type0 name0, type1 name1, type2 name2);
#define API_METHOD_4_DECLARE(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3) static inline ret name(type0 name0, type1 name1, type2 name2, type3 name3);
#define API_METHOD_5_DECLARE(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3, type4, name4) static inline ret name(type0 name0, type1 name1, type2 name2, type3 name3, type4 name4);
#define API_METHOD_6_DECLARE(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3, type4, name4, type5, name5) static inline ret name(type0 name0, type1 name1, type2 name2, type3 name3, type4 name4, type5 name5);
#define API_METHOD_7_DECLARE(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3, type4, name4, type5, name5, type6, name6) static inline ret name(type0 name0, type1 name1, type2 name2, type3 name3, type4 name4, type5 name5, type6 name6);

#define API_METHOD_RET_0_DECLARE(call, type, ret, name) static inline ret name();
#define API_METHOD_RET_1_DECLARE(call, type, ret, name, type0, name0) static inline ret name(type0 name0);
#define API_METHOD_RET_2_DECLARE(call, type, ret, name, type0, name0, type1, name1) static inline ret name(type0 name0, type1 name1);
#define API_METHOD_RET_3_DECLARE(call, type, ret, name, type0, name0, type1, name1, type2, name2) static inline ret name(type0 name0, type1 name1, type2 name2);
#define API_METHOD_RET_4_DECLARE(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3) static inline ret name(type0 name0, type1 name1, type2 name2, type3 name3);
#define API_METHOD_RET_5_DECLARE(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3, type4, name4) static inline ret name(type0 name0, type1 name1, type2 name2, type3 name3, type4 name4);
#define API_METHOD_RET_6_DECLARE(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3, type4, name4, type5, name5) static inline ret name(type0 name0, type1 name1, type2 name2, type3 name3, type4 name4, type5 name5);
#define API_METHOD_RET_7_DECLARE(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3, type4, name4, type5, name5, type6, name6) static inline ret name(type0 name0, type1 name1, type2 name2, type3 name3, type4 name4, type5 name5, type6 name6);

#define API_OBJECT_METHOD_0_DECLARE(call, type, ret, name) inline ret name();
#define API_OBJECT_METHOD_1_DECLARE(call, type, ret, name, type0, name0) inline ret name(type0 name0);
#define API_OBJECT_METHOD_2_DECLARE(call, type, ret, name, type0, name0, type1, name1) inline ret name(type0 name0, type1 name1);
#define API_OBJECT_METHOD_3_DECLARE(call, type, ret, name, type0, name0, type1, name1, type2, name2) inline ret name(type0 name0, type1 name1, type2 name2);
#define API_OBJECT_METHOD_4_DECLARE(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3) inline ret name(type0 name0, type1 name1, type2 name2, type3 name3);
#define API_OBJECT_METHOD_5_DECLARE(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3, type4, name4) inline ret name(type0 name0, type1 name1, type2 name2, type3 name3, type4 name4);
#define API_OBJECT_METHOD_6_DECLARE(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3, type4, name4, type5, name5) inline ret name(type0 name0, type1 name1, type2 name2, type3 name3, type4 name4, type5 name5);
#define API_OBJECT_METHOD_7_DECLARE(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3, type4, name4, type5, name5, type6, name6) inline ret name(type0 name0, type1 name1, type2 name2, type3 name3, type4 name4, type5 name5, type6 name6);

#define API_OBJECT_METHOD_RET_0_DECLARE(call, type, ret, name) inline ret name();
#define API_OBJECT_METHOD_RET_1_DECLARE(call, type, ret, name, type0, name0) inline ret name(type0 name0);
#define API_OBJECT_METHOD_RET_2_DECLARE(call, type, ret, name, type0, name0, type1, name1) inline ret name(type0 name0, type1 name1);
#define API_OBJECT_METHOD_RET_3_DECLARE(call, type, ret, name, type0, name0, type1, name1, type2, name2) inline ret name(type0 name0, type1 name1, type2 name2);
#define API_OBJECT_METHOD_RET_4_DECLARE(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3) inline ret name(type0 name0, type1 name1, type2 name2, type3 name3);
#define API_OBJECT_METHOD_RET_5_DECLARE(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3, type4, name4) inline ret name(type0 name0, type1 name1, type2 name2, type3 name3, type4 name4);
#define API_OBJECT_METHOD_RET_6_DECLARE(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3, type4, name4, type5, name5) inline ret name(type0 name0, type1 name1, type2 name2, type3 name3, type4 name4, type5 name5);
#define API_OBJECT_METHOD_RET_7_DECLARE(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3, type4, name4, type5, name5, type6, name6) inline ret name(type0 name0, type1 name1, type2 name2, type3 name3, type4 name4, type5 name5, type6 name6);

#define API_METHOD_0(call, type, ret, name) \
inline ret type::name() { \
    UserArgStruct args; \
     \
    call(#type "_" #name, &args); \
}
#define API_METHOD_1(call, type, ret, name, type0, name0) \
inline ret type::name(type0 name0) { \
    UserArgStruct args; \
    args.args[0] = (void*)&name0; \
    call(#type "_" #name, &args); \
}
#define API_METHOD_2(call, type, ret, name, type0, name0, type1, name1) \
inline ret type::name(type0 name0, type1 name1) { \
    UserArgStruct args; \
    args.args[0] = (void*)&name0; \
    args.args[1] = (void*)&name1; \
    call(#type "_" #name, &args); \
}
#define API_METHOD_3(call, type, ret, name, type0, name0, type1, name1, type2, name2) \
inline ret type::name(type0 name0, type1 name1, type2 name2) { \
    UserArgStruct args; \
    args.args[0] = (void*)&name0; \
    args.args[1] = (void*)&name1; \
    args.args[2] = (void*)&name2; \
    call(#type "_" #name, &args); \
}
#define API_METHOD_4(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3) \
inline ret type::name(type0 name0, type1 name1, type2 name2, type3 name3) { \
    UserArgStruct args; \
    args.args[0] = (void*)&name0; \
    args.args[1] = (void*)&name1; \
    args.args[2] = (void*)&name2; \
    args.args[3] = (void*)&name3; \
    call(#type "_" #name, &args); \
}
#define API_METHOD_5(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3, type4, name4) \
inline ret type::name(type0 name0, type1 name1, type2 name2, type3 name3, type4 name4) { \
    UserArgStruct args; \
    args.args[0] = (void*)&name0; \
    args.args[1] = (void*)&name1; \
    args.args[2] = (void*)&name2; \
    args.args[3] = (void*)&name3; \
    args.args[4] = (void*)&name4; \
    call(#type "_" #name, &args); \
}
#define API_METHOD_6(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3, type4, name4, type5, name5) \
inline ret type::name(type0 name0, type1 name1, type2 name2, type3 name3, type4 name4, type5 name5) { \
    UserArgStruct args; \
    args.args[0] = (void*)&name0; \
    args.args[1] = (void*)&name1; \
    args.args[2] = (void*)&name2; \
    args.args[3] = (void*)&name3; \
    args.args[4] = (void*)&name4; \
    args.args[5] = (void*)&name5; \
    call(#type "_" #name, &args); \
}
#define API_METHOD_7(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3, type4, name4, type5, name5, type6, name6) \
inline ret type::name(type0 name0, type1 name1, type2 name2, type3 name3, type4 name4, type5 name5, type6 name6) { \
    UserArgStruct args; \
    args.args[0] = (void*)&name0; \
    args.args[1] = (void*)&name1; \
    args.args[2] = (void*)&name2; \
    args.args[3] = (void*)&name3; \
    args.args[4] = (void*)&name4; \
    args.args[5] = (void*)&name5; \
    args.args[6] = (void*)&name6; \
    call(#type "_" #name, &args); \
}

#define API_METHOD_RET_0(call, type, ret, name) \
inline ret type::name() { \
    UserArgStruct args; \
     \
    return (ret)call(#type "_" #name, &args); \
}
#define API_METHOD_RET_1(call, type, ret, name, type0, name0) \
inline ret type::name(type0 name0) { \
    UserArgStruct args; \
    args.args[0] = (void*)&name0; \
    return (ret)call(#type "_" #name, &args); \
}
#define API_METHOD_RET_2(call, type, ret, name, type0, name0, type1, name1) \
inline ret type::name(type0 name0, type1 name1) { \
    UserArgStruct args; \
    args.args[0] = (void*)&name0; \
    args.args[1] = (void*)&name1; \
    return (ret)call(#type "_" #name, &args); \
}
#define API_METHOD_RET_3(call, type, ret, name, type0, name0, type1, name1, type2, name2) \
inline ret type::name(type0 name0, type1 name1, type2 name2) { \
    UserArgStruct args; \
    args.args[0] = (void*)&name0; \
    args.args[1] = (void*)&name1; \
    args.args[2] = (void*)&name2; \
    return (ret)call(#type "_" #name, &args); \
}
#define API_METHOD_RET_4(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3) \
inline ret type::name(type0 name0, type1 name1, type2 name2, type3 name3) { \
    UserArgStruct args; \
    args.args[0] = (void*)&name0; \
    args.args[1] = (void*)&name1; \
    args.args[2] = (void*)&name2; \
    args.args[3] = (void*)&name3; \
    return (ret)call(#type "_" #name, &args); \
}
#define API_METHOD_RET_5(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3, type4, name4) \
inline ret type::name(type0 name0, type1 name1, type2 name2, type3 name3, type4 name4) { \
    UserArgStruct args; \
    args.args[0] = (void*)&name0; \
    args.args[1] = (void*)&name1; \
    args.args[2] = (void*)&name2; \
    args.args[3] = (void*)&name3; \
    args.args[4] = (void*)&name4; \
    return (ret)call(#type "_" #name, &args); \
}
#define API_METHOD_RET_6(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3, type4, name4, type5, name5) \
inline ret type::name(type0 name0, type1 name1, type2 name2, type3 name3, type4 name4, type5 name5) { \
    UserArgStruct args; \
    args.args[0] = (void*)&name0; \
    args.args[1] = (void*)&name1; \
    args.args[2] = (void*)&name2; \
    args.args[3] = (void*)&name3; \
    args.args[4] = (void*)&name4; \
    args.args[5] = (void*)&name5; \
    return (ret)call(#type "_" #name, &args); \
}
#define API_METHOD_RET_7(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3, type4, name4, type5, name5, type6, name6) \
inline ret type::name(type0 name0, type1 name1, type2 name2, type3 name3, type4 name4, type5 name5, type6 name6) { \
    UserArgStruct args; \
    args.args[0] = (void*)&name0; \
    args.args[1] = (void*)&name1; \
    args.args[2] = (void*)&name2; \
    args.args[3] = (void*)&name3; \
    args.args[4] = (void*)&name4; \
    args.args[5] = (void*)&name5; \
    args.args[6] = (void*)&name6; \
    return (ret)call(#type "_" #name, &args); \
}

#define API_OBJECT_METHOD_0(call, type, ret, name) \
inline ret type::name() { \
    UserArgStruct args; \
    args.target = (void*)this->addr; \
    call(#type "_" #name, &args); \
}
#define API_OBJECT_METHOD_1(call, type, ret, name, type0, name0) \
inline ret type::name(type0 name0) { \
    UserArgStruct args; \
    args.target = (void*)this->addr; \
    args.args[0] = (void*)&name0; \
    call(#type "_" #name, &args); \
}
#define API_OBJECT_METHOD_2(call, type, ret, name, type0, name0, type1, name1) \
inline ret type::name(type0 name0, type1 name1) { \
    UserArgStruct args; \
    args.target = (void*)this->addr; \
    args.args[0] = (void*)&name0; \
    args.args[1] = (void*)&name1; \
    call(#type "_" #name, &args); \
}
#define API_OBJECT_METHOD_3(call, type, ret, name, type0, name0, type1, name1, type2, name2) \
inline ret type::name(type0 name0, type1 name1, type2 name2) { \
    UserArgStruct args; \
    args.target = (void*)this->addr; \
    args.args[0] = (void*)&name0; \
    args.args[1] = (void*)&name1; \
    args.args[2] = (void*)&name2; \
    call(#type "_" #name, &args); \
}
#define API_OBJECT_METHOD_4(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3) \
inline ret type::name(type0 name0, type1 name1, type2 name2, type3 name3) { \
    UserArgStruct args; \
    args.target = (void*)this->addr; \
    args.args[0] = (void*)&name0; \
    args.args[1] = (void*)&name1; \
    args.args[2] = (void*)&name2; \
    args.args[3] = (void*)&name3; \
    call(#type "_" #name, &args); \
}
#define API_OBJECT_METHOD_5(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3, type4, name4) \
inline ret type::name(type0 name0, type1 name1, type2 name2, type3 name3, type4 name4) { \
    UserArgStruct args; \
    args.target = (void*)this->addr; \
    args.args[0] = (void*)&name0; \
    args.args[1] = (void*)&name1; \
    args.args[2] = (void*)&name2; \
    args.args[3] = (void*)&name3; \
    args.args[4] = (void*)&name4; \
    call(#type "_" #name, &args); \
}
#define API_OBJECT_METHOD_6(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3, type4, name4, type5, name5) \
inline ret type::name(type0 name0, type1 name1, type2 name2, type3 name3, type4 name4, type5 name5) { \
    UserArgStruct args; \
    args.target = (void*)this->addr; \
    args.args[0] = (void*)&name0; \
    args.args[1] = (void*)&name1; \
    args.args[2] = (void*)&name2; \
    args.args[3] = (void*)&name3; \
    args.args[4] = (void*)&name4; \
    args.args[5] = (void*)&name5; \
    call(#type "_" #name, &args); \
}
#define API_OBJECT_METHOD_7(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3, type4, name4, type5, name5, type6, name6) \
inline ret type::name(type0 name0, type1 name1, type2 name2, type3 name3, type4 name4, type5 name5, type6 name6) { \
    UserArgStruct args; \
    args.target = (void*)this->addr; \
    args.args[0] = (void*)&name0; \
    args.args[1] = (void*)&name1; \
    args.args[2] = (void*)&name2; \
    args.args[3] = (void*)&name3; \
    args.args[4] = (void*)&name4; \
    args.args[5] = (void*)&name5; \
    args.args[6] = (void*)&name6; \
    call(#type "_" #name, &args); \
}

#define API_OBJECT_METHOD_RET_0(call, type, ret, name) \
inline ret type::name() { \
    UserArgStruct args; \
    args.target = (void*)this->addr; \
    return (ret)call(#type "_" #name, &args); \
}
#define API_OBJECT_METHOD_RET_1(call, type, ret, name, type0, name0) \
inline ret type::name(type0 name0) { \
    UserArgStruct args; \
    args.target = (void*)this->addr; \
    args.args[0] = (void*)&name0; \
    return (ret)call(#type "_" #name, &args); \
}
#define API_OBJECT_METHOD_RET_2(call, type, ret, name, type0, name0, type1, name1) \
inline ret type::name(type0 name0, type1 name1) { \
    UserArgStruct args; \
    args.target = (void*)this->addr; \
    args.args[0] = (void*)&name0; \
    args.args[1] = (void*)&name1; \
    return (ret)call(#type "_" #name, &args); \
}
#define API_OBJECT_METHOD_RET_3(call, type, ret, name, type0, name0, type1, name1, type2, name2) \
inline ret type::name(type0 name0, type1 name1, type2 name2) { \
    UserArgStruct args; \
    args.target = (void*)this->addr; \
    args.args[0] = (void*)&name0; \
    args.args[1] = (void*)&name1; \
    args.args[2] = (void*)&name2; \
    return (ret)call(#type "_" #name, &args); \
}
#define API_OBJECT_METHOD_RET_4(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3) \
inline ret type::name(type0 name0, type1 name1, type2 name2, type3 name3) { \
    UserArgStruct args; \
    args.target = (void*)this->addr; \
    args.args[0] = (void*)&name0; \
    args.args[1] = (void*)&name1; \
    args.args[2] = (void*)&name2; \
    args.args[3] = (void*)&name3; \
    return (ret)call(#type "_" #name, &args); \
}
#define API_OBJECT_METHOD_RET_5(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3, type4, name4) \
inline ret type::name(type0 name0, type1 name1, type2 name2, type3 name3, type4 name4) { \
    UserArgStruct args; \
    args.target = (void*)this->addr; \
    args.args[0] = (void*)&name0; \
    args.args[1] = (void*)&name1; \
    args.args[2] = (void*)&name2; \
    args.args[3] = (void*)&name3; \
    args.args[4] = (void*)&name4; \
    return (ret)call(#type "_" #name, &args); \
}
#define API_OBJECT_METHOD_RET_6(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3, type4, name4, type5, name5) \
inline ret type::name(type0 name0, type1 name1, type2 name2, type3 name3, type4 name4, type5 name5) { \
    UserArgStruct args; \
    args.target = (void*)this->addr; \
    args.args[0] = (void*)&name0; \
    args.args[1] = (void*)&name1; \
    args.args[2] = (void*)&name2; \
    args.args[3] = (void*)&name3; \
    args.args[4] = (void*)&name4; \
    args.args[5] = (void*)&name5; \
    return (ret)call(#type "_" #name, &args); \
}
#define API_OBJECT_METHOD_RET_7(call, type, ret, name, type0, name0, type1, name1, type2, name2, type3, name3, type4, name4, type5, name5, type6, name6) \
inline ret type::name(type0 name0, type1 name1, type2 name2, type3 name3, type4 name4, type5 name5, type6 name6) { \
    UserArgStruct args; \
    args.target = (void*)this->addr; \
    args.args[0] = (void*)&name0; \
    args.args[1] = (void*)&name1; \
    args.args[2] = (void*)&name2; \
    args.args[3] = (void*)&name3; \
    args.args[4] = (void*)&name4; \
    args.args[5] = (void*)&name5; \
    args.args[6] = (void*)&name6; \
    return (ret)call(#type "_" #name, &args); \
}

#endif

#endif
