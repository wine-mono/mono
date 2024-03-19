#!/bin/sh

rm -rf test-results

mkdir test-results

TEST_RESULTS_DIR=$PWD/test-results

touch "$TEST_RESULTS_DIR"/test-failures.txt

run_test ()
{
	SUITE="$1"
	shift 1

	echo Running \"$SUITE\" tests...
	test-bundle/mono-test.sh "$@" > "$TEST_RESULTS_DIR"/"$SUITE".log 2>&1 && echo "Succeeded." || (echo $SUITE >> "$TEST_RESULTS_DIR"/test-failures.txt; echo "Failed.")

	if test -e testResults.xml; then
		grep 'result="Fail"' testResults.xml|sed -e 's/^.*name="//g'|sed -e 's/".*$//g'|tee -a "$TEST_RESULTS_DIR"/test-failures.txt

		mv testResults.xml "$TEST_RESULTS_DIR"/"$SUITE".xml
	fi
}

run_test mini --mini

run_test mini-aot --mini --aot="mcpu=native"

run_test runtime --runtime

touch "$TEST_RESULTS_DIR"/expected-failures.txt

diff -q "$TEST_RESULTS_DIR"/test-failures.txt "$TEST_RESULTS_DIR"/expected-failures.txt >/dev/null || exit 127

