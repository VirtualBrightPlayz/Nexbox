#include "api.h"

void test() {
    tools_Clear();
    tools_SetPosition(10, 0);
    tools_set_Color(12);
    tools_Write("hi\n");
    tools_ResetColor();
    tools_Beep();
}

int main() {
    SandboxFunc func = SandboxFunc_new_1(engine());
    SandboxFunc_SetAction(func, (Object)test);
    TestClass cl = tools_new_0();
    tools_CreateAndExec(cl, func, 0);
    return 0;
}