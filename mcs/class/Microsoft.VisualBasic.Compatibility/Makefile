thisdir = class/Microsoft.VisualBasic.Compatibility
include ../../build/rules.make

LIBRARY = Microsoft.VisualBasic.Compatibility.dll
LIB_IS_VB = yes

LIB_REFS = System System.Drawing System.Windows.Forms Microsoft.VisualBasic

KEYFILE = ../msfinal.pub
LIB_VBC_FLAGS = -define:NET_VER=4.5 -vbruntime- -define:_MYTYPE="Empty" -define:DONTSIGN=True \
	-optionstrict+ -optioninfer+ -delaysign+

include ../../build/library.make

GENERATED_CLASSES = \
	ButtonArray \
	CheckBoxArray \
	ComboBoxArray \
	GroupBoxArray \
	LabelArray \
	PictureBoxArray \
	RadioButtonArray \
	TextBoxArray \
	ToolStripMenuItemArray

genvb.exe: genvb.cs
	$(CSCOMPILE) $< -r:$(topdir)/class/lib/$(PROFILE)/mscorlib.dll

%.generated.vb: %.vb.cpp genvb.exe Microsoft.VisualBasic.Compatibility.VB6/ControlArrayCommon.vb.h
	MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) genvb.exe $< $@

gen-sources: $(GENERATED_CLASSES:%=Microsoft.VisualBasic.Compatibility.VB6/%.generated.vb)

EXTRA_DISTFILES =	\
	$(GENERATED_CLASSES:%=Microsoft.VisualBasic.Compatibility.VB6/%.vb.cpp) \
	Microsoft.VisualBasic.Compatibility.VB6/ControlArrayCommon.vb.h \
	genvb.cs
