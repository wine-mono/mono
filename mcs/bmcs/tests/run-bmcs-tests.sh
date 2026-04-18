#!/bin/sh
set -eu

HERE=$(cd "$(dirname "$0")" && pwd)
RESULTS="${BMCS_RESULTS_DIR:-$HERE/test-results}"

PROJECT_ROOT=$(cd "$HERE/../../../.." && pwd)
DEFAULT_ARTIFACTS="$PROJECT_ROOT/build/artifacts"

BMCS_EXE="${BMCS_EXE:-$DEFAULT_ARTIFACTS/bmcs.exe}"
BMCS_MONO="${BMCS_MONO:-mono}"
BMCS_MONO_PATH="${BMCS_MONO_PATH:-$DEFAULT_ARTIFACTS}"

if [ ! -f "$BMCS_EXE" ]; then
    echo "bmcs.exe not found at $BMCS_EXE" >&2
    exit 1
fi

mkdir -p "$RESULTS"

pass=0
fail=0
failures=""

for vb in "$HERE"/*.vb; do
    name=$(basename "$vb" .vb)
    exe="$RESULTS/test-$name.exe"

    expected=$(sed -n "s/^' EXPECT: \\?//p" "$vb")
    pp_args=$(sed -n "s/^' PREPROCESS: //p" "$vb")
    bmcs_args=$(sed -n "s/^' BMCS-ARGS: //p" "$vb")
    expect_fail=0
    if grep -q "^' EXPECT-COMPILE-FAIL$" "$vb"; then
        expect_fail=1
    fi
    expect_error=$(sed -n "s/^' EXPECT-COMPILE-ERROR: //p" "$vb")
    compile_contains=$(sed -n "s/^' EXPECT-COMPILE-CONTAINS: //p" "$vb")
    compile_not_contains=$(sed -n "s/^' EXPECT-COMPILE-NOT-CONTAINS: //p" "$vb")
    expect_compile_log=0
    if [ -n "$compile_contains" ] || [ -n "$compile_not_contains" ]; then
        expect_compile_log=1
    fi

    compile_log="$RESULTS/test-$name.compile.log"
    run_log="$RESULTS/test-$name.run.log"

    bmcs_pp_args=""
    for arg in $pp_args; do
        case "$arg" in
            -D) ;;
            -D*) bmcs_pp_args="$bmcs_pp_args -d:${arg#-D}" ;;
            *) bmcs_pp_args="$bmcs_pp_args -d:$arg" ;;
        esac
    done

    compile_ok=0
    # shellcheck disable=SC2086
    MONO_PATH="$BMCS_MONO_PATH${MONO_PATH:+:$MONO_PATH}" \
        "$BMCS_MONO" "$BMCS_EXE" \
        $bmcs_pp_args $bmcs_args "$vb" -out:"$exe" > "$compile_log" 2>&1 && compile_ok=1

    if [ -n "$compile_contains" ]; then
        missing_contains=0
        while IFS= read -r needle; do
            [ -n "$needle" ] || continue
            if ! grep -F -q "$needle" "$compile_log"; then
                missing_contains=1
                missing_needle="$needle"
                break
            fi
        done <<EOF
$compile_contains
EOF

        if [ $missing_contains -ne 0 ]; then
            echo "FAIL  $name (missing compile log text)"
            echo "  expected compile log to contain: $missing_needle"
            echo "  see $compile_log"
            fail=$((fail+1))
            failures="$failures $name"
            continue
        fi
    fi

    if [ -n "$compile_not_contains" ]; then
        found_forbidden=0
        while IFS= read -r needle; do
            [ -n "$needle" ] || continue
            if grep -F -q "$needle" "$compile_log"; then
                found_forbidden=1
                forbidden_needle="$needle"
                break
            fi
        done <<EOF
$compile_not_contains
EOF

        if [ $found_forbidden -ne 0 ]; then
            echo "FAIL  $name (unexpected compile log text)"
            echo "  compile log unexpectedly contained: $forbidden_needle"
            echo "  see $compile_log"
            fail=$((fail+1))
            failures="$failures $name"
            continue
        fi
    fi

    if [ -n "$expect_error" ]; then
        if [ $compile_ok -eq 1 ]; then
            echo "FAIL  $name (expected error $expect_error but compile succeeded)"
            fail=$((fail+1))
            failures="$failures $name"
            continue
        fi
        if ! grep -q "$expect_error" "$compile_log"; then
            echo "FAIL  $name (expected error $expect_error not found in compile output)"
            echo "  see $compile_log"
            fail=$((fail+1))
            failures="$failures $name"
            continue
        fi
        echo "PASS  $name"
        pass=$((pass+1))
        continue
    fi

    if [ $expect_fail -eq 1 ] || [ $expect_compile_log -eq 1 ]; then
        if [ $compile_ok -eq 1 ]; then
            echo "FAIL  $name (expected compile failure but compile succeeded)"
            fail=$((fail+1))
            failures="$failures $name"
            continue
        fi
        echo "PASS  $name"
        pass=$((pass+1))
        continue
    fi

    if [ $compile_ok -eq 0 ]; then
        echo "FAIL  $name (compile)"
        echo "  see $compile_log"
        fail=$((fail+1))
        failures="$failures $name"
        continue
    fi

    MONO_PATH="$BMCS_MONO_PATH${MONO_PATH:+:$MONO_PATH}" \
        "$BMCS_MONO" "$exe" > "$run_log" 2>&1 || {
            echo "FAIL  $name (runtime)"
            echo "  see $run_log"
            fail=$((fail+1))
            failures="$failures $name"
            continue
        }

    if [ -n "$expected" ]; then
        actual=$(cat "$run_log")
        if [ "$actual" != "$expected" ]; then
            echo "FAIL  $name (output mismatch)"
            echo "  expected: $expected"
            echo "  actual:   $actual"
            fail=$((fail+1))
            failures="$failures $name"
            continue
        fi
    fi

    echo "PASS  $name"
    pass=$((pass+1))
done

echo
echo "$pass passed, $fail failed"
if [ $fail -gt 0 ]; then
    echo "failures:$failures"
    exit 1
fi
