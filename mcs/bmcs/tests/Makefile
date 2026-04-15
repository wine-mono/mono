thisdir = bmcs/tests
SUBDIRS =
include ../../build/rules.make

DISTFILES = run-bmcs-tests.sh $(wildcard *.vb)

BMCS_EXE ?= $(topdir)/../../build/artifacts/bmcs.exe
BMCS_MONO ?= mono
BMCS_MONO_PATH ?= $(topdir)/../../build/artifacts
BMCS_RESULTS_DIR ?= $(CURDIR)/test-results

all-local install-local uninstall-local test-local:
	@:

run-test-local:
	BMCS_EXE="$(BMCS_EXE)" BMCS_MONO="$(BMCS_MONO)" BMCS_MONO_PATH="$(BMCS_MONO_PATH)" \
		BMCS_RESULTS_DIR="$(BMCS_RESULTS_DIR)" \
		./run-bmcs-tests.sh

run-test-ondotnet-local:
	@:

clean-local:
	-rm -rf test-results

dist-local: dist-default
