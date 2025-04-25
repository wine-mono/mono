#!/bin/sh

SRCDIR=$(realpath "$(dirname "$(realpath "$0")")/../..")

podman image build -f ${SRCDIR}/scripts/ci/linux-ppc64le.docker -t mono-linux-ppc64le || exit 1

mkdir -p "${SRCDIR}/.podman-home" || exit 1

exec podman run --platform linux/ppc64le -u root -t --init -a stdin -a stdout -a stderr --volume "${SRCDIR}:/var/hostdir:Z" --volume "${SRCDIR}/.podman-home:/root:Z" -w /var/hostdir --read-only mono-linux-ppc64le "$@"

