#!/bin/sh
riscv64-linux-gnu-gcc-12 -march=rv64g -mabi=lp64d -fPIC -static -O2 test.cpp -o test.elf
base64 test.elf > test.dat