#include "api.h"
#include <stdio.h>
#include <stdarg.h>

void test() {
    tools_Clear();
    tools_set_Color(12);
    tools_Write("hi\n");
    tools_ResetColor();
}

void timer(long delta) {
    ObjectData t = ObjectData((void*)0);
    t.set_a(0);
    printf("%ld\n", delta);
}

int main() {
    test();
    SandboxFunc func = SandboxFunc_new_1(get_engine());
    SandboxFunc_SetAction(func, (Object)timer);
    tools_SetTick(func);
    return 0;
}