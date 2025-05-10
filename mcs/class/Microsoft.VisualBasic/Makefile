thisdir = vbruntime/Microsoft.VisualBasic
include ../../build/rules.make

LIBRARY = Microsoft.VisualBasic.dll
LIB_IS_VB = yes

LIB_REFS = System System.Core System.Data System.Web System.Xml System.Windows.Forms System.Drawing

BUILT_FILES = strings2.resources
KEYFILE = ../msfinal.pub
LIB_VBC_FLAGS = -define:NET_VER=4.5 -vbruntime- -define:_MYTYPE="Empty" -define:DONTSIGN=True \
	-optionstrict+ -optioninfer+ -delaysign+ \
	-imports:System,System.Collections,System.Data,System.Diagnostics,System.Collections.Generic,System.Threading.Tasks

strings2.txt: strings.txt strings-only2.txt
	cat strings.txt > strings2.txt
	cat strings-only2.txt >> strings2.txt

RESOURCES_DEFS = strings,strings.txt strings2,strings2.txt

include ../../build/library.make

EXTRA_DISTFILES =	\
	strings.txt				\
	strings-only2.txt			\
	ChangeLog				\
