#ifndef SYSCALL_H
#define SYSCALL_H

#define USER_SYSCALL 510

static inline long syscall(long n, long arg0, long arg1) {
    register long a0 asm("a0") = arg0;
    register long a1 asm("a1") = arg1;
    register long syscall_id asm("a7") = n;
    __asm__ volatile("ecall" : "+r"(a0) : "r"(a1), "r"(syscall_id));
    return a0;
}

static inline float fsyscall(long n, long arg0, long arg1) {
    register float fa0 asm("fa0") = 0.0f;
    register long a0 asm("a0") = arg0;
    register long a1 asm("a1") = arg1;
    register long syscall_id asm("a7") = n;
    __asm__ volatile("ecall" : "+f"(fa0), "+r"(a0) : "r"(a1), "r"(syscall_id));
    return fa0;
}

static inline void* psyscall(long n, const void *arg0, const void *arg1) {
    asm("" ::: "memory");
    return (void*)syscall(n, (long)arg0, (long)arg1);
}

static inline long usercall(const char *func, const void *arg) {
    asm("" ::: "memory");
    return (long)syscall(USER_SYSCALL, (long)func, (long)arg);
}

static inline float fpusercall(const char *func, const void *arg) {
    asm("" ::: "memory");
    return fsyscall(USER_SYSCALL, (long)func, (long)arg);
}

static inline void* pusercall(const char *func, const void *arg) {
    return (void *)psyscall(USER_SYSCALL, (const void *)func, (const void *)arg);
}

#endif
