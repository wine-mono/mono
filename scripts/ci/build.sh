#!/bin/sh

set -e

./autogen.sh

make -j$(nproc) all

make test

make TEST_BUNDLE_PATH=$PWD/test-bundle test-bundle
