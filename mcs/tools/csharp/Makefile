thisdir = tools/csharp
SUBDIRS = 
mono_sourcelibs_DIR  = $(DESTDIR)$(mono_libdir)/mono-source-libs

include ../../build/rules.make

// 3021: CLS attribute not needed since assembly is not CLS compliant
NOWARNS = -nowarn:3021
LOCAL_MCS_FLAGS = -unsafe -main:Mono.Driver -define:ON_DOTNET $(NOWARNS)
LIB_REFS = System System.Core System.Xml

BUILT_SOURCES = $(PROFILE)-parser.cs

$(PROFILE)-parser.cs: $(topdir)/mcs/cs-parser.jay $(topdir)/jay/skeleton.cs
	$(topdir)/jay/jay -c -o $(PROFILE)-jay-tmp.out $< < $(topdir)/jay/skeleton.cs && mv $(PROFILE)-jay-tmp.out $@

PROGRAM = csharp.exe

DISTFILES = repl.txt

CLEAN_FILES = csharp.exe *.mdb

include ../../build/executable.make

install-local: install-source

install-source:
	-$(MKINSTALLDIRS) $(mono_sourcelibs_DIR)
	$(INSTALL) -m 644 getline.cs $(mono_sourcelibs_DIR)

