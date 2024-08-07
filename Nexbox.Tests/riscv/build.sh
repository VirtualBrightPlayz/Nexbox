#!/bin/sh
riscv64-linux-gnu-gcc-12 -march=rv64g -mabi=lp64d -static -O2 -ffreestanding test.c -o test.elf
base64 test.elf > test.dat