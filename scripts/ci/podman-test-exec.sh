#!/bin/sh

SRCDIR=$(realpath "$(dirname "$(realpath "$0")")/../..")

podman image build -f ${SRCDIR}/scripts/ci/linux-test.docker -t mono-linux-test || exit 1

mkdir -p "${SRCDIR}/.podman-home" || exit 1
mkdir -p "${SRCDIR}/.podman-log" || exit 1

exec podman run -u root -t --init -a stdin -a stdout -a stderr --volume "${SRCDIR}:/var/hostdir:Z" --volume "${SRCDIR}/.podman-home:/root:Z" --volume "${SRCDIR}/.podman-log:/var/log:Z" -w /var/hostdir --read-only mono-linux-test "$@"

