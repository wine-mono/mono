#!/bin/sh

set -e

./autogen.sh

make -j$(nproc) all | ts '[%H:%M:%S]'

make test | ts '[%H:%M:%S]'

make TEST_BUNDLE_PATH=$PWD/test-bundle test-bundle | ts '[%H:%M:%S]'
