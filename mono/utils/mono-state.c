/**
 * \file
 * Support for verbose unmanaged crash dumps
 *
 * Author:
 *   Alexander Kyte (alkyte@microsoft.com)
 *
 * (C) 2018 Microsoft, Inc.
 *
 */
#include <config.h>
#include <glib.h>
#include <mono/utils/mono-state.h>
#include <mono/utils/atomic.h>

static volatile int32_t dump_status;

gboolean
mono_dump_start (void)
{
	return (mono_atomic_xchg_i32(&dump_status, 1) == 0);  // return true if we started the dump
}

gboolean
mono_dump_complete (void)
{
	return (mono_atomic_xchg_i32(&dump_status, 0) == 1);  // return true if we completed the dump
}
