thisdir = vbruntime/Microsoft.VisualBasic
include ../../build/rules.make

LIBRARY = Microsoft.VisualBasic.dll
LIB_IS_VB = yes

LIB_REFS = System System.Core System.Data System.Web System.Xml System.Windows.Forms System.Drawing
TEST_LIB_REFS = System System.Windows.Forms

KEYFILE = ../msfinal.pub
LIB_VBC_FLAGS = -define:NET_VER=4.5 -vbruntime- -define:_MYTYPE="Empty" -define:DONTSIGN=True \
	-optionstrict+ -optioninfer+ -delaysign+ \
	-imports:System,System.Collections,System.Data,System.Diagnostics,System.Collections.Generic,System.Threading.Tasks

RESOURCE_DEFS = strings,strings.txt

include ../../build/library.make

EXTRA_DISTFILES =	\
	strings.txt				\
	ChangeLog				\
