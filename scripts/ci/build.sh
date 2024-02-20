#!/bin/sh

set -e

./autogen.sh

make -j$(nproc)
