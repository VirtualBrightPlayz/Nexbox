#include "api.h"
#include <stdio.h>
#include <stdarg.h>

void test() {
    tools::Clear();
    tools::set_Color(12);
    tools::Write("hi\n");
    tools::ResetColor();
}

void timer(long delta) {
    printf("%ld\n", delta);
}

int main() {
    test();
    SandboxFunc func = SandboxFunc::new_1(get_engine());
    func.SetAction((void*)timer);
    tools::SetTick(func);
    return 0;
}