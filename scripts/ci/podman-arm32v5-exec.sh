#!/bin/sh

SRCDIR=$(realpath "$(dirname "$(realpath "$0")")/../..")

podman image build -f ${SRCDIR}/scripts/ci/linux-arm32v5.docker -t mono-linux-arm32v5 || exit 1

mkdir -p "${SRCDIR}/.podman-home" || exit 1

exec podman run -u root -t --init -a stdin -a stdout -a stderr --volume "${SRCDIR}:/var/hostdir:Z" --volume "${SRCDIR}/.podman-home:/root:Z" -w /var/hostdir --read-only mono-linux-arm32v5 "$@"

