#!/bin/sh

if test ! -e test-bundle/mono-libgdiplus.so; then
	wget -nv -O libgdiplus.zip "https://gitlab.winehq.org/wine-mono/libgdiplus/-/jobs/artifacts/main/download?job=linux-build" || exit 1
	unzip -q libgdiplus.zip || exit 1
	cp usr/local/lib/libgdiplus.so test-bundle/mono-libgdiplus.so || exit 1
fi

if test x$GITLAB_CI = xtrue; then
	cat >$HOME/xorg.conf << EOF
Section "Device"
  Identifier "dummy"
  Driver "dummy"
  VideoRam 32768
EndSection
EOF

	echo 'exec /usr/bin/fvwm -f config -c "Style * MwmDecor" -c "Style * UsePPosition" 2>/dev/null' >$HOME/.xinitrc
	export DISPLAY=:0
	startx -- -config $HOME/xorg.conf $DISPLAY &
fi

rm -rf test-results

mkdir test-results

TEST_RESULTS_DIR=$PWD/test-results

touch "$TEST_RESULTS_DIR"/test-failures.txt

run_test ()
{
	SUITE="$1"
	shift 1

	echo Running \"$SUITE\" tests...
	test-bundle/mono-test.sh "$@" > "$TEST_RESULTS_DIR"/"$SUITE".log 2>&1 && echo "Succeeded." && return 0

	if test x$RETRIES = x -o x$RETRIEX = x0; then
		echo $SUITE >> "$TEST_RESULTS_DIR"/test-failures.txt; echo "Failed."

		if test -e testResults.xml; then
			grep 'result="Fail"' testResults.xml|sed -e 's/^.*name="//g'|sed -e 's/".*$//g'|tee -a "$TEST_RESULTS_DIR"/test-failures.txt

			mv testResults.xml "$TEST_RESULTS_DIR"/"$SUITE".xml
		fi
	else
		if test -e testResults.xml; then
			grep 'result="Fail"' testResults.xml|sed -e 's/^.*name="//g'|sed -e 's/".*$//g'

			mv testResults.xml "$TEST_RESULTS_DIR"/"$SUITE".xml
		fi

		echo Retrying "(${RETRIES})"
		RETRIES=$(expr ${RETRIES} - 1) run_test "$SUITE" "$@"
	fi
}

run_test mini --mini

run_test mini-aot --mini --aot="mcpu=native"

run_test runtime --runtime

RETRIES=9 run_test System.Runtime.Caching --nunit net_4_x/tests/net_4_x_System.Runtime.Caching_test.dll

for name in corlib System System.Xml Mono.Security System.Security System.Data Mono.Posix System.Web System.Web.Services System.Runtime.Serialization.Formatters.Soap System.Runtime.Remoting Cscompmgd Commons.Xml.Relaxng System.ServiceProcess I18N.CJK I18N.West I18N.MidEast I18N.Rare I18N.Other System.DirectoryServices Microsoft.Build.Engine Microsoft.Build.Framework Microsoft.Build.Tasks Microsoft.Build.Utilities Mono.C5 Mono.Options Mono.Tasklets System.Configuration System.Transactions System.Web.Extensions System.Core System.Drawing System.Windows.Forms System.Windows.Forms.DataVisualization System.Xml.Linq System.Data.DataSetExtensions System.Web.Abstractions System.Web.Routing System.Runtime.Serialization System.IdentityModel System.ServiceModel System.ServiceModel.Web System.ComponentModel.DataAnnotations Mono.CodeContracts System.Data.Services System.Web.DynamicData Mono.CSharp WindowsBase System.Numerics System.Runtime.DurableInstancing System.ServiceModel.Discovery System.Xaml System.Net.Http System.Net.Http.WebRequest System.Json System.Threading.Tasks.Dataflow Mono.Debugger.Soft Microsoft.Build System.IO.Compression Mono.Data.Sqlite System.Data.OracleClient Mono.Messaging; do
	run_test $name --nunit net_4_x/tests/net_4_x_${name}_test.dll
done

touch "$TEST_RESULTS_DIR"/expected-failures.txt

diff -q "$TEST_RESULTS_DIR"/test-failures.txt "$TEST_RESULTS_DIR"/expected-failures.txt >/dev/null || exit 127

