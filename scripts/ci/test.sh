#!/bin/sh

set -ex

test-bundle/mono-test.sh --mini

test-bundle/mono-test.sh --mini --aot="mcpu=native"

