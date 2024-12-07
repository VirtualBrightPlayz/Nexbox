# Compiling LibRiscV

- In order to compile you must use LLVM/Clang, not MSVC

```cmd
mkdir build
cd build
cmake -T ClangCL ..
cmake --build .
```

# Compiling Guest/Nexbox Programs

Compiling a RISC-V program requires Linux or Windows Subsystem for Linux ([WSL Install Guide](https://learn.microsoft.com/en-us/windows/wsl/install)).

Use the command (or similar) to compile a single file using GCC
```bash

```

[Further documentation from LibRiscV](https://github.com/libriscv/libriscv/blob/master/docs/INTEGRATION.md#compiling-a-risc-v-program)