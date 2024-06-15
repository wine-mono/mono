#!/bin/sh

do_build ()
{
	./autogen.sh || exit 1

	make -j$(nproc) all || exit 1

	make test || exit 1

	make -C runtime xunit-test || exit 1

	make TEST_BUNDLE_PATH=$PWD/test-bundle test-bundle || exit 1
}

do_build | ts '[%H:%M:%S]'
