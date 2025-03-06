using System;
using System.Runtime.InteropServices;

namespace LibRiscV
{
    public unsafe static class LibRiscVNative
    {
        public const string DLL_NAME = "riscv_capi";
        public const CallingConvention DLL_DECL = CallingConvention.Cdecl;

        [StructLayout(LayoutKind.Sequential)]
        public struct RISCVOptions
        {
            public UInt64 max_memory;
            public UInt32 stack_size;
            public int strict_sandbox;  /* No file or socket permissions */
            public uint argc;        /* Program arguments */
            public byte **argv;
            public riscv_error_func_t error; /* Error callback */
            public riscv_stdout_func_t stdout; /* Stdout callback */
            public riscv_stdin_func_t stdin; /* Stdout callback */
            public void* opaque;             /* User-provided pointer */
        }

        public struct RISCVMachine
        { }

        [StructLayout(LayoutKind.Explicit)]
        public struct RISCVFloat
        {
            [FieldOffset(0)] public fixed float f32[2];
            [FieldOffset(0)] public double f64;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 32)]
        public struct RISCVRegisters
        {
            [FieldOffset(0)] public UInt64 pc;
            [FieldOffset(sizeof(UInt64))] public fixed UInt64 r[32];
            [FieldOffset(sizeof(UInt64) * 33)] public UInt32 fcsr;
            [FieldOffset(sizeof(UInt64) * 33 + sizeof(UInt32))] public fixed float fr32[64];
            [FieldOffset(sizeof(UInt64) * 33 + sizeof(UInt32))] public fixed double fr64[32];
        }

        public delegate void riscv_error_func_t(IntPtr opaque, int type, byte *msg, long data);
        public delegate void riscv_stdout_func_t(IntPtr opaque, byte *msg, uint size);
        public delegate long riscv_stdin_func_t(IntPtr opaque, byte *msg, uint size);

        [DllImport(DLL_NAME, CallingConvention = DLL_DECL)]
        public static extern void libriscv_set_defaults(ref RISCVOptions options);

        [DllImport(DLL_NAME, CallingConvention = DLL_DECL)]
        public static extern RISCVMachine* libriscv_new(void* elf_prog, uint elf_size, ref RISCVOptions o);

        [DllImport(DLL_NAME, CallingConvention = DLL_DECL)]
        public static extern int libriscv_delete(RISCVMachine* m);

        [DllImport(DLL_NAME, CallingConvention = DLL_DECL)]
        public static extern int libriscv_run(RISCVMachine* m, UInt64 instruction_limit);

        [DllImport(DLL_NAME, CallingConvention = DLL_DECL)]
        public static extern string libriscv_strerror(int return_value);

        [DllImport(DLL_NAME, CallingConvention = DLL_DECL)]
        public static extern Int64 libriscv_return_value(RISCVMachine* m);

        [DllImport(DLL_NAME, CallingConvention = DLL_DECL)]
        public static extern UInt64 libriscv_address_of(RISCVMachine* m, string name);

        [DllImport(DLL_NAME, CallingConvention = DLL_DECL)]
        public static extern IntPtr libriscv_opaque(RISCVMachine* m);

        [DllImport(DLL_NAME, CallingConvention = DLL_DECL)]
        public static extern RISCVRegisters* libriscv_get_registers(RISCVMachine* m);

        [DllImport(DLL_NAME, CallingConvention = DLL_DECL)]
        public static extern int libriscv_jump(RISCVMachine* m, UInt64 address);

        [DllImport(DLL_NAME, CallingConvention = DLL_DECL)]
        public static extern int libriscv_copy_to_guest(RISCVMachine* m, UInt64 dst, void* src, uint len);

        [DllImport(DLL_NAME, CallingConvention = DLL_DECL)]
        public static extern int libriscv_copy_from_guest(RISCVMachine* m, void* dst, UInt64 src, uint len);

        [DllImport(DLL_NAME, CallingConvention = DLL_DECL)]
        public static extern void *libriscv_memstring(RISCVMachine* m, UInt64 src, uint maxlen, ref uint length);

        [DllImport(DLL_NAME, CallingConvention = DLL_DECL)]
        public static extern byte *libriscv_memview(RISCVMachine* m, UInt64 src, uint length);

        [DllImport(DLL_NAME, CallingConvention = DLL_DECL)]
        public static extern void libriscv_stop(RISCVMachine* m);

        [DllImport(DLL_NAME, CallingConvention = DLL_DECL)]
        public static extern UInt64 libriscv_instruction_counter(RISCVMachine* m);

        [DllImport(DLL_NAME, CallingConvention = DLL_DECL)]
        public static extern UInt64* libriscv_max_counter_pointer(RISCVMachine* m);

        public delegate void riscv_syscall_handler_t(RISCVMachine* m);

        [DllImport(DLL_NAME, CallingConvention = DLL_DECL)]
        public static extern int libriscv_set_syscall_handler(uint num, riscv_syscall_handler_t sys_handler);

        [DllImport(DLL_NAME, CallingConvention = DLL_DECL)]
        public static extern void libriscv_trigger_exception(RISCVMachine* m, uint exception, UInt64 data);

        [DllImport(DLL_NAME, CallingConvention = DLL_DECL)]
        public static extern int libriscv_setup_vmcall(RISCVMachine* m, UInt64 address);

        public static UInt64 LIBRISCV_ARG_REGISTER_GET(RISCVRegisters* regs, int n)
        {
            return regs->r[10 + n];
        }

        public static void LIBRISCV_ARG_REGISTER_SET(RISCVRegisters* regs, int n, UInt64 val)
        {
            regs->r[10 + n] = val;
        }

        [DllImport(DLL_NAME, CallingConvention = DLL_DECL)]
        public static extern UInt64 libriscv_stack_push(RISCVMachine* m, RISCVRegisters* regs, void* data, uint len);

        public static UInt64 libriscv_stack_push2(RISCVMachine* m, RISCVRegisters* regs, void* data, uint len)
        {
            unchecked
            {
                regs->r[2] -= len;
                regs->r[2] &= ~0xFUL;
            }
            libriscv_copy_to_guest(m, regs->r[2], data, len);
            return regs->r[2];
        }
    }
}