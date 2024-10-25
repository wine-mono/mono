#!/bin/sh

do_build ()
{
	./autogen.sh || exit 1

	make all || exit 1

	test ! -e mcs/class/lib/monolite-* || echo Monolite should not be required in CI
	test ! -e mcs/class/lib/monolite-* || exit 1

	make test || exit 1

	make -C runtime xunit-test || exit 1

	make -j1 TEST_BUNDLE_PATH=$PWD/test-bundle test-bundle || exit 1

	make -j1 -C mcs/class package-monolite-latest-all-platforms || exit 1

	make -C mono/unit-tests check || exit 1

	make -C mono/eglib check || exit 1

	make -C mono/tests/fullaot-mixed check || exit 1

	make -j1 -C mcs/class/System.Web.Extensions run-standalone-test || exit 1

	make -C mcs/tools/linker check || exit 1

	touch success-marker
}

rm -f success-marker

do_build | ts '[%H:%M:%S]'

test -f success-marker

