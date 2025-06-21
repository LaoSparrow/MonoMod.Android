// clang -O3 -dynamiclib helper_macos_arm64.c -o helper_macos_arm64.dylib

#include <stddef.h>
#include <string.h>
#include <pthread.h>

void mmch_jit_memcpy(void *dst, const void *src, size_t n)
{
    pthread_jit_write_protect_np(0);
    memcpy(dst, src, n);
    pthread_jit_write_protect_np(1);
}

// the following fn ptr's calling conventions are technically incorrect since 
// they are c++ instance methods, but the abi is the same on mac aarch64

void mmch_precompile_icorejitcompiler21_compilemethod(unsigned (*p)(void *, void *, void *, unsigned, unsigned char **, unsigned long *))
{
    p(NULL, NULL, NULL, 0, NULL, NULL);
}

void mmch_precompile_icorejitinfo70_allocmem(void (*p)(void *, void *))
{
    p(NULL, NULL);
}
