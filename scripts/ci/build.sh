#!/bin/sh

do_build ()
{
	./autogen.sh || exit 1

	make -j$(nproc) all || exit 1

	make test || exit 1

	make -C runtime xunit-test || exit 1

	make TEST_BUNDLE_PATH=$PWD/test-bundle test-bundle || exit 1

	make -C mono/unit-tests check || exit 1

	make -C mono/eglib check || exit 1

	make -C mono/tests/fullaot-mixed check || exit 1

	make -C mcs/class/System.Web.Extensions run-standalone-test || exit 1

	make -C mcs/tools/linker check || exit 1

	touch success-marker
}

rm -f success-marker

do_build | ts '[%H:%M:%S]'

test -f success-marker

