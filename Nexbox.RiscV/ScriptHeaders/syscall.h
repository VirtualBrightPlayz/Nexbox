#ifndef SYSCALL_H
#define SYSCALL_H

#define USER_SYSCALL 510

static inline long syscall(long n, long arg0, long arg1) {
    register long a0 asm("a0") = arg0;
    register long a1 asm("a1") = arg1;
    register long syscall_id asm("a7") = n;
    __asm__ volatile("scall" : "+r"(a0) : "r"(a1), "r"(syscall_id));
    return a0;
}

static inline void* psyscall(long n, const void *arg0, const void *arg1) {
    asm("" ::: "memory");
    return (void*)syscall(n, (long long)arg0, (long long)arg1);
}

static inline long usercall(const char *func, long arg) {
    return (long)syscall(USER_SYSCALL, (long)func, (long)arg);
}

static inline void* pusercall(const char *func, const void *arg) {
    return (void *)psyscall(USER_SYSCALL, (const void *)func, (const void *)arg);
}

#endif