#include "api.h"
#include <stdio.h>
#include <stdarg.h>
#include <stdlib.h>
#include <stdbool.h>
#include <sys/types.h>
#include <unistd.h>
#include <pthread.h>

void do_work() {
    while (true) {
        printf("test\n");
        // char buffer[BUFSIZ];
        // fgets(buffer, BUFSIZ, stdin);
        // printf(buffer);
        fflush(stdout);
        LibRiscVEngine_Yield(get_engine());
    }
}

PUBLIC void dummy() {
}

int main() {
    // int pipefd[2];
    // if (pipe(pipefd) != 0) {
    //     printf("failed to make pipe\n");
    //     return 1;
    // }
    SandboxFunc func = SandboxFunc_new_1(get_engine());
    SandboxFunc_SetAction(func, (void*)&dummy);
    tools_SetTick(func);
    pthread_t thread;
    pthread_create(&thread, NULL, &do_work, NULL);
    LibRiscVEngine_Exit(get_engine());
    // pthread_join(thread, NULL);
    printf("bad\n");
    return 0;
}