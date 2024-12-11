# Compiling LibRiscV/riscv_capi

- In order to compile you must use LLVM/Clang, not MSVC
- Make sure to install LLVM/Clang from the Visual Studio Installer

```bash
mkdir build
cd build
cmake -T ClangCL ..
cmake --build .
```

# Compiling Guest/Nexbox Programs

Compiling a RISC-V program requires [MSYS2](https://www.msys2.org/), (Ubuntu) Linux or [Ubuntu WSL](https://learn.microsoft.com/en-us/windows/wsl/install).

## Non-Ubuntu Linux

[Further documentation from LibRiscV](https://github.com/libriscv/libriscv/blob/master/docs/INTEGRATION.md#compiling-a-risc-v-program)

## Ubuntu Linux or Ubuntu WSL

Install the riscv64 GCC compiler. The command below *should* work. If the package isn't found, try replacing the `12` with `13` or `14`

```bash
sudo apt install gcc-12-riscv64-linux-gnu g++-12-riscv64-linux-gnu
```

Use this command (or similar) to compile a single file using riscv64 GCC

```bash
riscv64-linux-gnu-gcc-12 -march=rv64g -mabi=lp64d -fPIC -static -O2 -fpermissive main.cpp -o script.elf
```

## MSYS2 (Untested)

When installing MSYS2, make note of the installation path as you will need it later.

In the `MSYS2 MINGW64` terminal, run this to install the riscv64 GCC compiler. Use right click->Paste to paste, not Control + V

```bash
pacman -Sy mingw-w64-x86_64-riscv64-unknown-elf-gcc
```

Next edit your environment variable `PATH` to contain the `mingw64\bin` subfolder of you MSYS2 installation. An example might be `C:\msys64\mingw64\bin`

Now you can run `riscv64-unknown-elf-gcc` to compile your program.
