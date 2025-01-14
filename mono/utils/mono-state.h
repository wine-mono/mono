/**
 * \file
 * Support for cooperative creation of unmanaged state dumps
 *
 * Author:
 *   Alexander Kyte (alkyte@microsoft.com)
 *
 * (C) 2018 Microsoft, Inc.
 *
 */
#ifndef __MONO_UTILS_NATIVE_STATE__
#define __MONO_UTILS_NATIVE_STATE__

// Dump context functions (enter/leave)

gboolean
mono_dump_start (void);
gboolean
mono_dump_complete (void);

#endif // MONO_UTILS_NATIVE_STATE
