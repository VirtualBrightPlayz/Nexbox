cmake_minimum_required(VERSION 3.9.4)
project(riscv)

add_subdirectory(libriscv/lib libriscv)

add_library(riscv_capi SHARED libriscv.cpp libriscv.h)
target_include_directories(riscv_capi PUBLIC .)
target_link_libraries(riscv_capi PRIVATE riscv)
