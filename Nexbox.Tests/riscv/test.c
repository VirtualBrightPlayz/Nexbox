#include "macrolib.h"
#include "api.h"
#include <stdio.h>
#include <stdarg.h>

API_OBJECT_BEGIN(thing)
API_OBJECT_METHOD_RET_V(thing, void*, test2, 1, h)
API_METHOD_RET_V(thing_ptr, thing_new_1, 0)
API_OBJECT_END()

void test() {
    tools_Clear();
    tools_set_Color(12);
    tools_Write("hi\n");
    tools_ResetColor();
}

void timer(long delta) {
    printf("%ld\n", delta);
}

int main() {
    test();
    SandboxFunc func = SandboxFunc_new_1(get_engine());
    SandboxFunc_SetAction(func, (Object)timer);
    tools_SetTick(func);
    return 0;
}