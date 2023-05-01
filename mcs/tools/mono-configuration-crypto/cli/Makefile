thisdir = tools/mono-configuration-crypto/cli
SUBDIRS = 
include ../../../build/rules.make

LOCAL_MCS_FLAGS =
LIB_REFS = WineMono.Security System.Security System.Configuration System Mono.Configuration.Crypto

PROGRAM = mono-configuration-crypto.exe
PROGRAM_INSTALL_DIR = $(mono_libdir)/mono/mono-configuration-crypto/$(FRAMEWORK_VERSION)

include ../../../build/executable.make
