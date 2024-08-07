#include "api.h"
#include <stdio.h>

int test() {
    printf("test\n");
    return 0;
}

int main() {
    SandboxFunc func = SandboxFunc_new_1(engine());
    SandboxFunc_SetAction(func, (Object)test);
    TestClass cl = tools_new_0();
    Object obj[0];
    tools_CreateAndExec(cl, func, 0);
    return 0;
}