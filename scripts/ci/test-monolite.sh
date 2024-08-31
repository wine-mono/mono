#!/bin/sh

set -e

./autogen.sh

MONO_CORLIB_VERSION=$(grep '^MONO_CORLIB_VERSION=' configure.ac |sed -e 's/MONO_CORLIB_VERSION=//')

mkdir -p mcs/class/lib/monolite-linux

(cd mcs/class/lib && tar xf ../monolite-linux-${MONO_CORLIB_VERSION}-latest.tar.gz)

(cd mcs/class/lib && mv -f monolite-linux-${MONO_CORLIB_VERSION}-latest monolite-linux/${MONO_CORLIB_VERSION})

mkdir -p mcs/build/deps
touch mcs/build/deps/use-monolite

make -j$(nproc) -C external/bdwgc
make -j$(nproc) -C mono
make -j$(nproc) -C runtime etc/mono/config
make -j$(nproc) -C mcs/tools/gensources PROFILE=build
make -j$(nproc) -C mcs/build common/Consts.cs PROFILE=build
make -j$(nproc) -C mcs/jay
make -j$(nproc) -C mcs/class/PEAPI PROFILE=build
make -j$(nproc) -C mcs/ilasm PROFILE=build
make -j$(nproc) -C mcs/class/Mono.Cecil PROFILE=build
make -j$(nproc) -C mcs/tools/cil-stringreplacer PROFILE=build
make -j$(nproc) -C mcs/class/corlib PROFILE=build

