#include "api.h"
#include <stdio.h>

void test() {
    tools_Clear();
    tools_SetPosition(10, 0);
    tools_set_Color(12);
    tools_Write("hi\n");
    tools_ResetColor();
    tools_Beep();
}

void timer(long delta) {
    printf("%ld\n", delta);
}

int main() {
    SandboxFunc func = SandboxFunc_new_1(get_engine());
    SandboxFunc_SetAction(func, (Object)timer);
    tools_SetTick(func);
    return 0;
}