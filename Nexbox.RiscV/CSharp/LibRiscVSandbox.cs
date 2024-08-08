using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LibRiscV
{
    public unsafe class LibRiscVSandbox : IDisposable
    {
        public const ulong MAX_INSTRUCTIONS = 5_000_000_000UL;

        private LibRiscVNative.RISCVMachine *machine;
        private Func<string> stdin;
        private Action<string> stdout;
        private Action<int, string, long> stderr;
        private Func<string, ulong, ulong> UserSyscall;
        private Queue<byte> stdinCache = new Queue<byte>();
        private GCHandle handle;
        private bool stopped;
        private static LibRiscVNative.riscv_error_func_t g_stderr;
        private static LibRiscVNative.riscv_stdin_func_t g_stdin;
        private static LibRiscVNative.riscv_stdout_func_t g_stdout;
        private static LibRiscVNative.riscv_syscall_handler_t g_user;

        static LibRiscVSandbox()
        {
            g_stderr = ErrCallback;
            g_stdin = StdInCallback;
            g_stdout = StdOutCallback;
            g_user = Syscall_UserFunc;
        }

        private static long StdInCallback(IntPtr opaque, byte *msg, uint size)
        {
            GCHandle handle = (GCHandle)opaque;
            LibRiscVSandbox sandbox = handle.Target as LibRiscVSandbox;
            char[] str = sandbox.stdin?.Invoke().ToCharArray() ?? Array.Empty<char>();
            for (int i = 0; i < str.Length; i++)
            {
                sandbox.stdinCache.Enqueue((byte)str[i]);
            }
            byte[] data = new byte[size];
            int read = 0;
            for (int i = 0; i < size; i++)
            {
                if (sandbox.stdinCache.Count > 0)
                    data[i] = sandbox.stdinCache.Dequeue();
                else
                {
                    read = i;
                    break;
                }
            }
            if (read == 0)
            {
                return 0;
            }
            else
            {
                Marshal.Copy(data, 0, (IntPtr)msg, (int)size);
                return read;
            }
        }

        private static void StdOutCallback(IntPtr opaque, byte *msg, uint size)
        {
            GCHandle handle = (GCHandle)opaque;
            LibRiscVSandbox sandbox = handle.Target as LibRiscVSandbox;
            string str = Marshal.PtrToStringAnsi((IntPtr)msg, (int)size);
            sandbox.stdout?.Invoke(str);
        }

        private static void ErrCallback(IntPtr opaque, int type, byte *msg, long data)
        {
            GCHandle handle = (GCHandle)opaque;
            LibRiscVSandbox sandbox = handle.Target as LibRiscVSandbox;
            string str = Marshal.PtrToStringAnsi((IntPtr)msg);
            sandbox.stderr?.Invoke(type, str, data);
        }

        private static void Syscall_Exit(LibRiscVNative.RISCVMachine *machine)
        {
            LibRiscVNative.RISCVRegisters *regs = LibRiscVNative.libriscv_get_registers(machine);
            GCHandle handle = (GCHandle)LibRiscVNative.libriscv_opaque(machine);
            LibRiscVSandbox sandbox = handle.Target as LibRiscVSandbox;
            const int REG_A0 = 10;
            sandbox.stdout?.Invoke($"Exit called! Status={regs->r[REG_A0]}");
            LibRiscVNative.libriscv_stop(machine);
        }

        private static void Syscall_Read(LibRiscVNative.RISCVMachine *machine)
        {
            LibRiscVNative.RISCVRegisters *regs = LibRiscVNative.libriscv_get_registers(machine);
            GCHandle handle = (GCHandle)LibRiscVNative.libriscv_opaque(machine);
            LibRiscVSandbox sandbox = handle.Target as LibRiscVSandbox;
            const int REG_A0 = 10;
            const int REG_A1 = 11;
            const int REG_A2 = 12;
            ulong fd = regs->r[REG_A0];
            ulong buf = regs->r[REG_A1];
            ulong count = regs->r[REG_A2];
            // sandbox.stdout?.Invoke($"Read called! fd={regs->r[REG_A0]} buf={regs->r[REG_A1]} count={regs->r[REG_A2]}");
            if (fd == 0)
            {
                string line;
                do
                {
                    line = Console.ReadLine();
                } while (string.IsNullOrEmpty(line));
                line += '\n';
                byte[] data = line.Select(x => (byte)x).ToArray();
                sandbox.MemSet(buf, data);
                regs->r[REG_A0] = (ulong)data.Length;
            }
            else
            {
                regs->r[REG_A0] = unchecked((ulong)-1);
                // LibRiscVNative.libriscv_stop(machine);
            }
        }

        private static void Syscall_Write(LibRiscVNative.RISCVMachine *machine)
        {
            LibRiscVNative.RISCVRegisters *regs = LibRiscVNative.libriscv_get_registers(machine);
            GCHandle handle = (GCHandle)LibRiscVNative.libriscv_opaque(machine);
            LibRiscVSandbox sandbox = handle.Target as LibRiscVSandbox;
            const int REG_A0 = 10;
            const int REG_A1 = 11;
            const int REG_A2 = 12;
            ulong fd = regs->r[REG_A0];
            ulong buf = regs->r[REG_A1];
            ulong count = regs->r[REG_A2];
            // sandbox.stdout?.Invoke($"Write called! fd={regs->r[REG_A0]} buf={regs->r[REG_A1]} count={regs->r[REG_A2]}");
            if (fd == 1 || fd == 2)
            {
                byte[] mem = sandbox.MemGet(buf, (uint)count);
                string line = string.Join("", mem.Select(x => (char)x));
                sandbox.stdout?.Invoke(line);
                regs->r[REG_A0] = (ulong)line.Length;
            }
            else
            {
                regs->r[REG_A0] = unchecked((ulong)-1);
                // LibRiscVNative.libriscv_stop(machine);
            }
        }

        private static void Syscall_UserFunc(LibRiscVNative.RISCVMachine *machine)
        {
            ulong *ctPtr = LibRiscVNative.libriscv_max_counter_pointer(machine);
            ulong prevCtPtr = *ctPtr;
            LibRiscVNative.RISCVRegisters *regs = LibRiscVNative.libriscv_get_registers(machine);
            LibRiscVNative.RISCVRegisters prevRegs = *regs;
            GCHandle handle = (GCHandle)LibRiscVNative.libriscv_opaque(machine);
            LibRiscVSandbox sandbox = handle.Target as LibRiscVSandbox;
            const int REG_A0 = 10;
            const int REG_A1 = 11;
            ulong a0 = sandbox.UserSyscall?.Invoke(sandbox.MemString(regs->r[REG_A0]), regs->r[REG_A1]) ?? 0;
            *regs = prevRegs;
            *ctPtr = prevCtPtr;
            regs->r[REG_A0] = a0;
        }

        public LibRiscVSandbox(byte[] elf, Func<string> stdi = null, Action<string> stdo = null, Action<int, string, long> stde = null, Func<string, ulong, ulong> user = null, params string[] args)
        {
            stdin = stdi;
            stdout = stdo;
            stderr = stde;
            UserSyscall = user;
            handle = GCHandle.Alloc(this);
            IntPtr[] arr = new IntPtr[args?.Length ?? 0];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = Marshal.StringToHGlobalAnsi(args[i]);
            }
            fixed (void *dataArr = arr)
            {
                LibRiscVNative.RISCVOptions options = new LibRiscVNative.RISCVOptions();
                LibRiscVNative.libriscv_set_defaults(ref options);
                options.max_memory = 1UL << 30; // 1 GiB(?)
                options.argc = (uint)arr.Length;
                options.argv = (byte **)dataArr;
                options.stdin = g_stdin;
                options.stdout = g_stdout;
                options.error = g_stderr;
                options.opaque = (void *)(IntPtr)handle;

                fixed (void *data = elf)
                {
                    machine = LibRiscVNative.libriscv_new(data, (uint)elf.LongLength, ref options);
                }
            }
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] != IntPtr.Zero)
                    Marshal.FreeHGlobal(arr[i]);
            }
            if (machine == null)
            {
                stde?.Invoke(0, "Machine is null", 0);
                handle.Free();
                return;
            }
            // LibRiscVNative.libriscv_set_syscall_handler(63, Syscall_Read);
            // LibRiscVNative.libriscv_set_syscall_handler(64, Syscall_Write);
            // LibRiscVNative.libriscv_set_syscall_handler(93, Syscall_Exit);
            // LibRiscVNative.libriscv_set_syscall_handler(94, Syscall_Exit);
            LibRiscVNative.libriscv_set_syscall_handler(510, g_user);
        }

        public bool Run(out long ret)
        {
            ret = -1;
            if (machine == null)
                return false;
            stopped = false;
            LibRiscVNative.libriscv_run(machine, MAX_INSTRUCTIONS);
            if (LibRiscVNative.libriscv_instruction_counter(machine) >= MAX_INSTRUCTIONS)
            {
                ret = 0;
                return false;
            }
            ret = LibRiscVNative.libriscv_return_value(machine);
            return true;
        }

        public void Stop()
        {
            if (machine == null)
                return;
            stopped = true;
            LibRiscVNative.libriscv_stop(machine);
            throw new Exception();
        }

        public string MemString(ulong src)
        {
            if (machine == null || stopped)
                return default;
            uint len = 0;
            IntPtr ptr = (IntPtr)LibRiscVNative.libriscv_memstring(machine, src, 1000, ref len);
            if (ptr == IntPtr.Zero)
                return null;
            return Marshal.PtrToStringAnsi(ptr, (int)len);
        }

        public float MemFloat(ulong src)
        {
            if (machine == null || stopped)
                return default;
            IntPtr ptr = (IntPtr)LibRiscVNative.libriscv_memview(machine, src, sizeof(float));
            float fl = default;
            Unsafe.Copy(ref fl, ptr.ToPointer());
            return fl;
        }

        public IntPtr MemObject(ulong src, uint size)
        {
            if (machine == null || stopped)
                return IntPtr.Zero;
            IntPtr ptr = (IntPtr)LibRiscVNative.libriscv_memview(machine, src, size);
            return ptr;
        }

        public ulong StackPushString(string str)
        {
            if (machine == null || stopped)
                return 0;
            IntPtr ptr = Marshal.StringToHGlobalAnsi(str);
            LibRiscVNative.RISCVRegisters *regs = LibRiscVNative.libriscv_get_registers(machine);
            ulong vAddr = LibRiscVNative.libriscv_stack_push(machine, regs, ptr.ToPointer(), (uint)str.Length + 1); // add one to copy null byte
            Marshal.FreeHGlobal(ptr);
            return vAddr;
        }

        public void MemSetString(ulong addr, string str)
        {
            if (machine == null || stopped)
                return;
            IntPtr ptr = Marshal.StringToHGlobalAnsi(str);
            LibRiscVNative.libriscv_copy_to_guest(machine, addr, ptr.ToPointer(), (uint)str.Length + 1); // add one to copy null byte
            Marshal.FreeHGlobal(ptr);
        }

        public void MemSet(ulong addr, byte[] data)
        {
            if (machine == null || stopped)
                return;
            fixed (void *ptr = data)
            {
                LibRiscVNative.libriscv_copy_to_guest(machine, addr, ptr, (uint)data.Length);
            }
        }

        public byte[] MemGet(ulong src, uint size)
        {
            if (machine == null || stopped)
                return default;
            byte[] data = new byte[size];
            fixed (void *ptr = data)
            {
                LibRiscVNative.libriscv_copy_from_guest(machine, ptr, src, size);
            }
            return data;
        }

        public void MemSetObject(ulong addr, object obj)
        {
            if (machine == null || stopped)
                return;
            IntPtr ptr = (IntPtr)LibRiscVNative.libriscv_memview(machine, addr, (uint)Marshal.SizeOf(obj));
            Marshal.StructureToPtr(obj, ptr, false);
        }

        public ulong Malloc(ulong size)
        {
            if (Call("malloc", out long ret, size))
            {
                return unchecked((ulong)ret);
            }
            return 0;
        }

        public void Free(ulong addr)
        {
            Call("free", out long _, addr);
        }

        private void Convert(LibRiscVNative.RISCVRegisters *regs, int offset, object obj, ref int flOffset)
        {
            switch (obj)
            {
                case string str:
                    IntPtr ptr = Marshal.StringToHGlobalAnsi(str);
                    ulong vAddr = LibRiscVNative.libriscv_stack_push(machine, regs, ptr.ToPointer(), (uint)str.Length + 1); // add one to copy null byte
                    LibRiscVNative.LIBRISCV_ARG_REGISTER_SET(regs, offset, vAddr);
                    Marshal.FreeHGlobal(ptr);
                    break;
                case int i:
                    LibRiscVNative.LIBRISCV_ARG_REGISTER_SET(regs, offset, unchecked((ulong)i));
                    break;
                case uint ui:
                    LibRiscVNative.LIBRISCV_ARG_REGISTER_SET(regs, offset, unchecked(ui));
                    break;
                case long l:
                    LibRiscVNative.LIBRISCV_ARG_REGISTER_SET(regs, offset, unchecked((ulong)l));
                    break;
                case ulong ul:
                    LibRiscVNative.LIBRISCV_ARG_REGISTER_SET(regs, offset, unchecked(ul));
                    break;
                case float fl:
                    regs->fr32[(10 + flOffset) * 2 + 1] = fl;
                    flOffset += 1;
                    LibRiscVNative.LIBRISCV_ARG_REGISTER_SET(regs, offset, 0);
                    break;
                case double dl:
                    // TODO: this does not work
                    regs->fr64[10 + flOffset] = dl;
                    flOffset += 1;
                    LibRiscVNative.LIBRISCV_ARG_REGISTER_SET(regs, offset, 0);
                    break;
                default:
                    LibRiscVNative.LIBRISCV_ARG_REGISTER_SET(regs, offset, 0);
                    break;
            }
        }

        public bool Call(string funcName, out long ret, params object[] args)
        {
            ret = 0;
            if (machine == null || stopped)
                return false;
            ulong vaddr = LibRiscVNative.libriscv_address_of(machine, funcName);
            return CallPtr(vaddr, out ret, args);
        }

        public bool CallPtr(ulong vaddr, out long ret, params object[] args)
        {
            ret = 0;
            if (machine == null || stopped)
                return false;
            if (args.Length > 8)
                return false;
            // check for valid address
            if (vaddr == 0)
                return false;
            ulong *ctPtr = LibRiscVNative.libriscv_max_counter_pointer(machine);
            ulong prevCtPtr = *ctPtr;
            LibRiscVNative.RISCVRegisters *regs = LibRiscVNative.libriscv_get_registers(machine);
            LibRiscVNative.RISCVRegisters prevRegs = *regs;
            if (LibRiscVNative.libriscv_setup_vmcall(machine, vaddr) == 0)
            {
                for (int i = 0, j = 0; i < args.Length; i++)
                {
                    Convert(regs, i, args[i], ref j);
                }
                LibRiscVNative.libriscv_run(machine, MAX_INSTRUCTIONS);
                if (machine == null || stopped)
                    return false;
                ret = LibRiscVNative.libriscv_return_value(machine);
                // Unsafe.Copy(regs, ref prevRegs);
                // *regs = prevRegs;
                // *ctPtr = prevCtPtr;
                return true;
            }
            else
            {
                // unable to jump to function at vaddr, restore last state
                *regs = prevRegs;
                *ctPtr = prevCtPtr;
                return false;
            }
        }

        public void Dispose()
        {
            if (machine == null)
                return;
            LibRiscVNative.libriscv_delete(machine);
            machine = null;
            handle.Free();
        }
    }
}