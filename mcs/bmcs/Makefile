thisdir := bmcs
SUBDIRS := tests
include ../build/rules.make

#uncomment to enable some debug stuff
#DEBUG_FLAGS=/define:MCS_DEBUG

BUILT_SOURCES = mb-parser.cs

EXTRA_DISTFILES = mb-parser.jay

LOCAL_MCS_FLAGS = -d:NET_1_1 -d:ONLY_1_1 -debug -r:Microsoft.VisualBasic.dll -nowarn:219 -nowarn:162

CLEAN_FILES = y.output

PROGRAM = $(topdir)/class/lib/$(PROFILE)/bmcs.exe

mb-parser.cs: mb-parser.jay $(topdir)/jay/skeleton.cs
	$(topdir)/jay/jay -ctv < $(topdir)/jay/skeleton.cs $< > jay-tmp.out && mv jay-tmp.out $@

include ../build/executable.make

diffus:
	diff --exclude='.svn' --exclude='*-parser.*' --exclude='*-tokenizer.cs' -ruN ../gmcs ../bmcs
