# Compiling LibRiscV/riscv_capi

## Windows

- In order to compile you must use LLVM/Clang, not MSVC
- Make sure to install LLVM/Clang from the Visual Studio Installer

```bash
mkdir build
cd build
cmake -T ClangCL -DCMAKE_BUILD_TYPE=Release ..
cmake --build . --config Release
```

## Linux

```bash
mkdir build
cd build
cmake -DCMAKE_BUILD_TYPE=Release ..
cmake --build . --config Release
```

# Compiling Guest/Nexbox Programs

## C/C++

Due to the nature of guest programs not having a real Standard C Library, the library *must* be packaged with the guest program. This isn't great as some standard C Libraries are not Licensed for propietary use when distributed. In other words, find a standard library which does not have restrictive licensing, such as MSVC (Microsoft) or glibc (GNU).

Note the library must use Linux RISC-V System Calls.

tl;dr
Use Musl, which has a more permissive [license](https://git.musl-libc.org/cgit/musl/tree/COPYRIGHT).

### Musl

Compiling a RISC-V program with Musl libc requires Linux, which can be easily "emulated" via [WSL](https://learn.microsoft.com/en-us/windows/wsl/install) when using Windows.

#### Toolchain

According to [Musl's Official website](https://musl.libc.org/), there is a [community wiki](https://wiki.musl-libc.org/), which has an [unofficial, pre-compiled toolchain](https://musl.cc/) for download. Unless you are using a RISC-V CPU, use the `riscv64-linux-musl-cross.tgz` file on the website.

#### Compile

Inside the `bin` folder, you will find `riscv64-linux-musl-gcc`, which is our C compiler.

In order to compile, you must statically link a program. Example command:

```bash
riscv64-linux-musl-gcc -march=rv64g -mabi=lp64d -fPIC -static -O2 -fpermissive test.cpp -o test.elf
```

### GNU C Library

Compiling a RISC-V program with glibc requires [MSYS2](https://www.msys2.org/), (Ubuntu) Linux or [Ubuntu WSL](https://learn.microsoft.com/en-us/windows/wsl/install).

#### Non-Ubuntu Linux

[Further documentation from LibRiscV](https://github.com/libriscv/libriscv/blob/master/docs/INTEGRATION.md#compiling-a-risc-v-program)

#### Ubuntu Linux or Ubuntu WSL

Install the riscv64 GCC compiler. The command below *should* work. If the package isn't found, try replacing the `12` with `13` or `14`

```bash
sudo apt install gcc-12-riscv64-linux-gnu g++-12-riscv64-linux-gnu
```

Use this command (or similar) to compile a single file using riscv64 GCC

```bash
riscv64-linux-gnu-gcc-12 -march=rv64g -mabi=lp64d -fPIC -static -O2 -fpermissive main.cpp -o script.elf
```

#### MSYS2 (Untested)

When installing MSYS2, make note of the installation path as you will need it later.

In the `MSYS2 MINGW64` terminal, run this to install the riscv64 GCC compiler. Use right click->Paste to paste, not Control + V

```bash
pacman -Sy mingw-w64-x86_64-riscv64-unknown-elf-gcc
```

Next edit your environment variable `PATH` to contain the `mingw64\bin` subfolder of you MSYS2 installation. An example might be `C:\msys64\mingw64\bin`

Now you can run `riscv64-unknown-elf-gcc` to compile your program.
