/**
 * \file
 * exception support for ARM64
 *
 * Copyright 2013 Xamarin Inc
 *
 * Based on exceptions-arm.c:
 *
 * Authors:
 *   Dietmar Maurer (dietmar@ximian.com)
 *   Paolo Molaro (lupus@ximian.com)
 *
 * (C) 2001 Ximian, Inc.
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

#include "mini.h"
#include "mini-runtime.h"
#include "aot-runtime.h"

#include <mono/arch/arm64/arm64-codegen.h>
#include <mono/metadata/abi-details.h>
#include <mono/utils/mono-state.h>
#include "mono/utils/mono-tls-inline.h"

#ifdef TARGET_WIN32
static void (*restore_stack) (void);
static MonoW32ExceptionHandler fpe_handler;
static MonoW32ExceptionHandler ill_handler;
static MonoW32ExceptionHandler segv_handler;

LPTOP_LEVEL_EXCEPTION_FILTER mono_old_win_toplevel_exception_filter;
void *mono_win_vectored_exception_handle;

#define W32_SEH_HANDLE_EX(_ex) \
	if (_ex##_handler) _ex##_handler(er->ExceptionCode, &info, ctx)

static LONG CALLBACK seh_unhandled_exception_filter(EXCEPTION_POINTERS* ep)
{
#ifndef MONO_CROSS_COMPILE
	if (mono_old_win_toplevel_exception_filter) {
		return (*mono_old_win_toplevel_exception_filter)(ep);
	}
#endif

	if (mono_dump_start ())
	{
		mono_handle_native_crash (mono_get_signame (SIGSEGV), NULL, NULL);
	}

	return EXCEPTION_CONTINUE_SEARCH;
}

#if 0 //HAVE_API_SUPPORT_WIN32_RESET_STKOFLW TODO
static gpointer
get_win32_restore_stack (void)
{
	static guint8 *start = NULL;
	guint8 *code;

	if (start)
		return start;

	const int size = 128;

	/* restore_stack (void) */
	start = code = mono_global_codeman_reserve (size);

	amd64_push_reg (code, AMD64_RBP);
	amd64_mov_reg_reg (code, AMD64_RBP, AMD64_RSP, 8);

	/* push 32 bytes of stack space for Win64 calling convention */
	amd64_alu_reg_imm (code, X86_SUB, AMD64_RSP, 32);

	/* restore guard page */
	amd64_mov_reg_imm (code, AMD64_R11, _resetstkoflw);
	amd64_call_reg (code, AMD64_R11);

	/* get jit_tls with context to restore */
	amd64_mov_reg_imm (code, AMD64_R11, mono_tls_get_jit_tls_extern);
	amd64_call_reg (code, AMD64_R11);

	/* move jit_tls from return reg to arg reg */
	amd64_mov_reg_reg (code, AMD64_ARG_REG1, AMD64_RAX, 8);

	/* retrieve pointer to saved context */
	amd64_alu_reg_imm (code, X86_ADD, AMD64_ARG_REG1, MONO_STRUCT_OFFSET (MonoJitTlsData, stack_restore_ctx));

	/* this call does not return */
	amd64_mov_reg_imm (code, AMD64_R11, mono_restore_context);
	amd64_call_reg (code, AMD64_R11);

	g_assertf ((code - start) <= size, "%d %d", (int)(code - start), size);

	mono_arch_flush_icache (start, code - start);
	MONO_PROFILER_RAISE (jit_code_buffer, (start, code - start, MONO_PROFILER_CODE_BUFFER_EXCEPTION_HANDLING, NULL));

	return start;
}
#else
static gpointer
get_win32_restore_stack (void)
{
	// _resetstkoflw unsupported on none desktop Windows platforms.
	return NULL;
}
#endif /* HAVE_API_SUPPORT_WIN32_RESET_STKOFLW */

/*
 * Unhandled Exception Filter
 * Top-level per-process exception handler.
 */
static LONG CALLBACK seh_vectored_exception_handler(EXCEPTION_POINTERS* ep)
{
	EXCEPTION_RECORD* er;
	CONTEXT* ctx;
	LONG res;
	MonoJitTlsData *jit_tls = mono_tls_get_jit_tls ();
	MonoDomain* domain = mono_domain_get ();
	MonoWindowsSigHandlerInfo info = { TRUE, ep };

	/* If the thread is not managed by the runtime return early */
	if (!jit_tls)
		return EXCEPTION_CONTINUE_SEARCH;

	res = EXCEPTION_CONTINUE_EXECUTION;

	er = ep->ExceptionRecord;
	ctx = ep->ContextRecord;

	switch (er->ExceptionCode) {
	case EXCEPTION_STACK_OVERFLOW:
		if (!mono_aot_only && restore_stack) {
			if (mono_arch_handle_exception (ctx, domain->stack_overflow_ex)) {
				/* need to restore stack protection once stack is unwound
				 * restore_stack will restore stack protection and then
				 * resume control to the saved stack_restore_ctx */
				mono_sigctx_to_monoctx (ctx, &jit_tls->stack_restore_ctx);
				ctx->Pc = (guint64)restore_stack;
			}
		} else {
			info.handled = FALSE;
		}
		break;
	case EXCEPTION_ACCESS_VIOLATION:
		W32_SEH_HANDLE_EX(segv);
		break;
	case EXCEPTION_ILLEGAL_INSTRUCTION:
		W32_SEH_HANDLE_EX(ill);
		break;
	case EXCEPTION_INT_DIVIDE_BY_ZERO:
	case EXCEPTION_INT_OVERFLOW:
	case EXCEPTION_FLT_DIVIDE_BY_ZERO:
	case EXCEPTION_FLT_OVERFLOW:
	case EXCEPTION_FLT_UNDERFLOW:
	case EXCEPTION_FLT_INEXACT_RESULT:
		W32_SEH_HANDLE_EX(fpe);
		break;
	default:
		info.handled = FALSE;
		break;
	}

	if (!info.handled) {
		/* Don't copy context back if we chained exception
		* as the handler may have modfied the EXCEPTION_POINTERS
		* directly. We don't pass sigcontext to chained handlers.
		* Return continue search so the UnhandledExceptionFilter
		* can correctly chain the exception.
		*/
		res = EXCEPTION_CONTINUE_SEARCH;
	}

	return res;
}

void win32_seh_init()
{
	if (!mono_aot_only)
		restore_stack = (void (*) (void))get_win32_restore_stack ();

	mono_old_win_toplevel_exception_filter = SetUnhandledExceptionFilter(seh_unhandled_exception_filter);
	mono_win_vectored_exception_handle = AddVectoredExceptionHandler (1, seh_vectored_exception_handler);
}

void win32_seh_cleanup()
{
	guint32 ret = 0;

	if (mono_old_win_toplevel_exception_filter) SetUnhandledExceptionFilter(mono_old_win_toplevel_exception_filter);

	ret = RemoveVectoredExceptionHandler (mono_win_vectored_exception_handle);
	g_assert (ret);
}

void win32_seh_set_handler(int type, MonoW32ExceptionHandler handler)
{
	switch (type) {
	case SIGFPE:
		fpe_handler = handler;
		break;
	case SIGILL:
		ill_handler = handler;
		break;
	case SIGSEGV:
		segv_handler = handler;
		break;
	default:
		break;
	}
}

#endif /* TARGET_WIN32 */

#ifndef DISABLE_JIT

gpointer
mono_arch_get_restore_context (MonoTrampInfo **info, gboolean aot)
{
	guint8 *start, *code;
	MonoJumpInfo *ji = NULL;
	GSList *unwind_ops = NULL;
	int i, ctx_reg, size;
	guint8 *labels [16];

	size = 256;
	code = start = mono_global_codeman_reserve (size);

	MINI_BEGIN_CODEGEN ();

	arm_movx (code, ARMREG_IP0, ARMREG_R0);
	ctx_reg = ARMREG_IP0;

	/* Restore fregs */
	arm_ldrx (code, ARMREG_IP1, ctx_reg, MONO_STRUCT_OFFSET (MonoContext, has_fregs));
	labels [0] = code;
	arm_cbzx (code, ARMREG_IP1, 0);
	for (i = 0; i < 32; ++i)
		arm_ldrfpx (code, i, ctx_reg, MONO_STRUCT_OFFSET (MonoContext, fregs) + (i * sizeof (MonoContextSimdReg)));
	mono_arm_patch (labels [0], code, MONO_R_ARM64_CBZ);
	/* Restore gregs */
	// FIXME: Restore less registers
	// FIXME: fp should be restored later
	code = mono_arm_emit_load_regarray (code, 0xffffffff & ~(1 << ctx_reg) & ~(1 << ARMREG_SP), ctx_reg, MONO_STRUCT_OFFSET (MonoContext, regs));
	/* ip0/ip1 doesn't need to be restored */
	/* ip1 = pc */
	arm_ldrx (code, ARMREG_IP1, ctx_reg, MONO_STRUCT_OFFSET (MonoContext, pc));
	/* ip0 = sp */
	arm_ldrx (code, ARMREG_IP0, ctx_reg, MONO_STRUCT_OFFSET (MonoContext, regs) + (ARMREG_SP * 8));
	/* Restore sp, ctx is no longer valid */
	arm_movspx (code, ARMREG_SP, ARMREG_IP0); 
	/* Branch to pc */
	code = mono_arm_emit_brx (code, ARMREG_IP1);
	/* Not reached */
	arm_brk (code, 0);

	g_assert ((code - start) < size);

	MINI_END_CODEGEN (start, code - start, MONO_PROFILER_CODE_BUFFER_EXCEPTION_HANDLING, NULL);

	if (info)
		*info = mono_tramp_info_create ("restore_context", start, code - start, ji, unwind_ops);

	return MINI_ADDR_TO_FTNPTR (start);
}

gpointer
mono_arch_get_call_filter (MonoTrampInfo **info, gboolean aot)
{
	guint8 *code;
	guint8* start;
	int i, size, offset, gregs_offset, fregs_offset, ctx_offset, num_fregs, frame_size;
	MonoJumpInfo *ji = NULL;
	GSList *unwind_ops = NULL;
	guint8 *labels [16];

	size = 512;
	start = code = mono_global_codeman_reserve (size);

	/* Compute stack frame size and offsets */
	offset = 0;
	/* frame block */
	offset += 2 * 8;
	/* gregs */
	gregs_offset = offset;
	offset += 32 * 8;
	/* fregs */
	num_fregs = 8;
	fregs_offset = offset;
	offset += num_fregs * 8;
	ctx_offset = offset;
	offset += 8;
	frame_size = ALIGN_TO (offset, MONO_ARCH_FRAME_ALIGNMENT);

	/*
	 * We are being called from C code, ctx is in r0, the address to call is in r1.
	 * We need to save state, restore ctx, make the call, then restore the previous state,
	 * returning the value returned by the call.
	 */

	MINI_BEGIN_CODEGEN ();

	/* Setup a frame */
	arm_stpx_pre (code, ARMREG_FP, ARMREG_LR, ARMREG_SP, -frame_size);
	arm_movspx (code, ARMREG_FP, ARMREG_SP);

	/* Save ctx */
	arm_strx (code, ARMREG_R0, ARMREG_FP, ctx_offset);
	/* Save gregs */
	code = mono_arm_emit_store_regarray (code, MONO_ARCH_CALLEE_SAVED_REGS | (1 << ARMREG_FP), ARMREG_FP, gregs_offset);
	/* Save fregs */
	for (i = 0; i < num_fregs; ++i)
		arm_strfpx (code, ARMREG_D8 + i, ARMREG_FP, fregs_offset + (i * 8));

	/* Load regs from ctx */
	code = mono_arm_emit_load_regarray (code, MONO_ARCH_CALLEE_SAVED_REGS, ARMREG_R0, MONO_STRUCT_OFFSET (MonoContext, regs));
	/* Load fregs */
	arm_ldrx (code, ARMREG_IP0, ARMREG_R0, MONO_STRUCT_OFFSET (MonoContext, has_fregs));
	labels [0] = code;
	arm_cbzx (code, ARMREG_IP0, 0);
	for (i = 0; i < num_fregs; ++i)
		arm_ldrfpx (code, ARMREG_D8 + i, ARMREG_R0, MONO_STRUCT_OFFSET (MonoContext, fregs) + ((i + 8) * sizeof (MonoContextSimdReg)));
	mono_arm_patch (labels [0], code, MONO_R_ARM64_CBZ);
	/* Load fp */
	arm_ldrx (code, ARMREG_FP, ARMREG_R0, MONO_STRUCT_OFFSET (MonoContext, regs) + (ARMREG_FP * 8));

	/* Make the call */
	code = mono_arm_emit_blrx (code, ARMREG_R1);
	/* For filters, the result is in R0 */

	/* Restore fp */
	arm_ldrx (code, ARMREG_FP, ARMREG_SP, gregs_offset + (ARMREG_FP * 8));
	/* Load ctx */
	arm_ldrx (code, ARMREG_IP0, ARMREG_FP, ctx_offset);
	/* Save registers back to ctx */
	/* This isn't strictly necessary since we don't allocate variables used in eh clauses to registers */
	code = mono_arm_emit_store_regarray (code, MONO_ARCH_CALLEE_SAVED_REGS, ARMREG_IP0, MONO_STRUCT_OFFSET (MonoContext, regs));

	/* Restore regs */
	code = mono_arm_emit_load_regarray (code, MONO_ARCH_CALLEE_SAVED_REGS, ARMREG_FP, gregs_offset);
	/* Restore fregs */
	for (i = 0; i < num_fregs; ++i)
		arm_ldrfpx (code, ARMREG_D8 + i, ARMREG_FP, fregs_offset + (i * 8));
	/* Destroy frame */
	code = mono_arm_emit_destroy_frame (code, frame_size, (1 << ARMREG_IP0));
	arm_retx (code, ARMREG_LR);

	g_assert ((code - start) < size);

	MINI_END_CODEGEN (start, code - start, MONO_PROFILER_CODE_BUFFER_EXCEPTION_HANDLING, NULL);

	if (info)
		*info = mono_tramp_info_create ("call_filter", start, code - start, ji, unwind_ops);

	return MINI_ADDR_TO_FTNPTR (start);
}

static gpointer 
get_throw_trampoline (int size, gboolean corlib, gboolean rethrow, gboolean llvm, gboolean resume_unwind, const char *tramp_name, MonoTrampInfo **info, gboolean aot, gboolean preserve_ips)
{
	guint8 *start, *code;
	MonoJumpInfo *ji = NULL;
	GSList *unwind_ops = NULL;
	int i, offset, gregs_offset, fregs_offset, frame_size, num_fregs;

	code = start = mono_global_codeman_reserve (size);

	/* We are being called by JITted code, the exception object/type token is in R0 */

	/* Compute stack frame size and offsets */
	offset = 0;
	/* frame block */
	offset += 2 * 8;
	/* gregs */
	gregs_offset = offset;
	offset += 32 * 8;
	/* fregs */
	num_fregs = 8;
	fregs_offset = offset;
	offset += num_fregs * 8;
	frame_size = ALIGN_TO (offset, MONO_ARCH_FRAME_ALIGNMENT);

	MINI_BEGIN_CODEGEN ();

	mono_add_unwind_op_def_cfa (unwind_ops, code, start, ARMREG_SP, 0);

	/* Setup a frame */
	arm_stpx_pre (code, ARMREG_FP, ARMREG_LR, ARMREG_SP, -frame_size);
	mono_add_unwind_op_def_cfa_offset (unwind_ops, code, start, frame_size);
	mono_add_unwind_op_offset (unwind_ops, code, start, ARMREG_FP, -frame_size);
	mono_add_unwind_op_offset (unwind_ops, code, start, ARMREG_LR, -frame_size + 8);
	arm_movspx (code, ARMREG_FP, ARMREG_SP);
	mono_add_unwind_op_def_cfa_reg (unwind_ops, code, start, ARMREG_FP);

	/* Save gregs */
	code = mono_arm_emit_store_regarray (code, 0xffffffff, ARMREG_FP, gregs_offset);
	if (corlib && !llvm)
		/* The real LR is in R1 */
		arm_strx (code, ARMREG_R1, ARMREG_FP, gregs_offset + (ARMREG_LR * 8));
	/* Save fp/sp */
	arm_ldrx (code, ARMREG_IP0, ARMREG_FP, 0);
	arm_strx (code, ARMREG_IP0, ARMREG_FP, gregs_offset + (ARMREG_FP * 8));
	arm_addx_imm (code, ARMREG_IP0, ARMREG_FP, frame_size);
	arm_strx (code, ARMREG_IP0, ARMREG_FP, gregs_offset + (ARMREG_SP * 8));	
	/* Save fregs */
	for (i = 0; i < num_fregs; ++i)
		arm_strfpx (code, ARMREG_D8 + i, ARMREG_FP, fregs_offset + (i * 8));

	/* Call the C trampoline function */
	/* Arg1 =  exception object/type token */
	arm_movx (code, ARMREG_R0, ARMREG_R0);
	/* Arg2 = caller ip */
	if (corlib) {
		if (llvm)
			arm_ldrx (code, ARMREG_R1, ARMREG_FP, gregs_offset + (ARMREG_LR * 8));
		else
			arm_movx (code, ARMREG_R1, ARMREG_R1);
	} else {
		arm_ldrx (code, ARMREG_R1, ARMREG_FP, 8);
	}
	/* Arg 3 = gregs */
	arm_addx_imm (code, ARMREG_R2, ARMREG_FP, gregs_offset);
	/* Arg 4 = fregs */
	arm_addx_imm (code, ARMREG_R3, ARMREG_FP, fregs_offset);
	/* Arg 5 = corlib */
	arm_movzx (code, ARMREG_R4, corlib ? 1 : 0, 0);
	/* Arg 6 = rethrow */
	arm_movzx (code, ARMREG_R5, rethrow ? 1 : 0, 0);
	if (!resume_unwind) {
		/* Arg 7 = preserve_ips */
		arm_movzx (code, ARMREG_R6, preserve_ips ? 1 : 0, 0);
	}

	/* Call the function */
	if (aot) {
		MonoJitICallId icall_id;

		if (resume_unwind)
			icall_id = MONO_JIT_ICALL_mono_arm_resume_unwind;
		else
			icall_id = MONO_JIT_ICALL_mono_arm_throw_exception;

		code = mono_arm_emit_aotconst (&ji, code, start, ARMREG_LR, MONO_PATCH_INFO_JIT_ICALL_ADDR, GUINT_TO_POINTER (icall_id));
	} else {
		gpointer icall_func;

		if (resume_unwind)
			icall_func = (gpointer)mono_arm_resume_unwind;
		else
			icall_func = (gpointer)mono_arm_throw_exception;

		code = mono_arm_emit_imm64 (code, ARMREG_LR, (guint64)icall_func);
	}
	code = mono_arm_emit_blrx (code, ARMREG_LR);
	/* This shouldn't return */
	arm_brk (code, 0x0);

	g_assert ((code - start) < size);

	MINI_END_CODEGEN (start, code - start, MONO_PROFILER_CODE_BUFFER_EXCEPTION_HANDLING, NULL);

	if (info)
		*info = mono_tramp_info_create (tramp_name, start, code - start, ji, unwind_ops);

	return MINI_ADDR_TO_FTNPTR (start);
}

gpointer 
mono_arch_get_throw_exception (MonoTrampInfo **info, gboolean aot)
{
	return get_throw_trampoline (256, FALSE, FALSE, FALSE, FALSE, "throw_exception", info, aot, FALSE);
}

gpointer
mono_arch_get_rethrow_exception (MonoTrampInfo **info, gboolean aot)
{
	return get_throw_trampoline (256, FALSE, TRUE, FALSE, FALSE, "rethrow_exception", info, aot, FALSE);
}

gpointer
mono_arch_get_rethrow_preserve_exception (MonoTrampInfo **info, gboolean aot)
{
	return get_throw_trampoline (256, FALSE, TRUE, FALSE, FALSE, "rethrow_preserve_exception", info, aot, TRUE);
}

gpointer 
mono_arch_get_throw_corlib_exception (MonoTrampInfo **info, gboolean aot)
{
	return get_throw_trampoline (256, TRUE, FALSE, FALSE, FALSE, "throw_corlib_exception", info, aot, FALSE);
}

GSList*
mono_arm_get_exception_trampolines (gboolean aot)
{
	MonoTrampInfo *info;
	GSList *tramps = NULL;

	// FIXME Macroize.

	/* LLVM uses the normal trampolines, but with a different name */
	get_throw_trampoline (256, TRUE, FALSE, FALSE, FALSE, "llvm_throw_corlib_exception_trampoline", &info, aot, FALSE);
	info->jit_icall_info = &mono_get_jit_icall_info ()->mono_llvm_throw_corlib_exception_trampoline;
	tramps = g_slist_prepend (tramps, info);

	get_throw_trampoline (256, TRUE, FALSE, TRUE, FALSE, "llvm_throw_corlib_exception_abs_trampoline", &info, aot, FALSE);
	info->jit_icall_info = &mono_get_jit_icall_info ()->mono_llvm_throw_corlib_exception_abs_trampoline;
	tramps = g_slist_prepend (tramps, info);

	get_throw_trampoline (256, FALSE, FALSE, FALSE, TRUE, "llvm_resume_unwind_trampoline", &info, aot, FALSE);
	info->jit_icall_info = &mono_get_jit_icall_info ()->mono_llvm_resume_unwind_trampoline;
	tramps = g_slist_prepend (tramps, info);

	return tramps;
}

#else

GSList*
mono_arm_get_exception_trampolines (gboolean aot)
{
	g_assert_not_reached ();
	return NULL;
}

#endif /* DISABLE_JIT */

void
mono_arch_exceptions_init (void)
{
	gpointer tramp;
	GSList *tramps, *l;

	if (mono_aot_only) {
		tramp = mono_aot_get_trampoline ("llvm_throw_corlib_exception_trampoline");
		mono_register_jit_icall_info (&mono_get_jit_icall_info ()->mono_llvm_throw_corlib_exception_trampoline, tramp, "llvm_throw_corlib_exception_trampoline", NULL, TRUE, NULL);

		tramp = mono_aot_get_trampoline ("llvm_throw_corlib_exception_abs_trampoline");
		mono_register_jit_icall_info (&mono_get_jit_icall_info ()->mono_llvm_throw_corlib_exception_abs_trampoline, tramp, "llvm_throw_corlib_exception_abs_trampoline", NULL, TRUE, NULL);

		tramp = mono_aot_get_trampoline ("llvm_resume_unwind_trampoline");
		mono_register_jit_icall_info (&mono_get_jit_icall_info ()->mono_llvm_resume_unwind_trampoline, tramp, "llvm_resume_unwind_trampoline", NULL, TRUE, NULL);
	} else {
		tramps = mono_arm_get_exception_trampolines (FALSE);
		for (l = tramps; l; l = l->next) {
			MonoTrampInfo *info = (MonoTrampInfo*)l->data;
			mono_register_jit_icall_info (info->jit_icall_info, info->code, g_strdup (info->name), NULL, TRUE, NULL);
			mono_tramp_info_register (info, NULL);
		}
		g_slist_free (tramps);
	}
}

// Implies defined(TARGET_WIN32)
#ifdef MONO_ARCH_HAVE_UNWIND_TABLE

// Dynamic function table used when registering unwind info for OS unwind support.
static GList *g_dynamic_function_table_begin;
static GList *g_dynamic_function_table_end;

// SRW lock (lightweight read/writer lock) protecting dynamic function table.
static SRWLOCK g_dynamic_function_table_lock = SRWLOCK_INIT;

static GList *
fast_find_range_in_table_no_lock_ex (gsize begin_range, gsize end_range, gboolean *continue_search)
{
	GList *found_entry = NULL;

	// Fast path, look at boundaries.
	if (g_dynamic_function_table_begin != NULL) {
		DynamicFunctionTableEntry *first_entry = (DynamicFunctionTableEntry*)g_dynamic_function_table_begin->data;
		DynamicFunctionTableEntry *last_entry = (g_dynamic_function_table_end != NULL ) ? (DynamicFunctionTableEntry*)g_dynamic_function_table_end->data : first_entry;

		// Sorted in descending order based on begin_range, check first item, that is the entry with highest range.
		if (first_entry != NULL && first_entry->begin_range <= begin_range && first_entry->end_range >= end_range) {
				// Entry belongs to first entry in list.
				found_entry = g_dynamic_function_table_begin;
				*continue_search = FALSE;
		} else {
			if (first_entry != NULL && first_entry->begin_range >= begin_range) {
				if (last_entry != NULL && last_entry->begin_range <= begin_range) {
					// Entry has a range that could exist in table, continue search.
					*continue_search = TRUE;
				}
			}
		}
	}

	return found_entry;
}

static GList *
find_range_in_table_no_lock_ex (const gpointer code_block, gsize block_size)
{
	GList *found_entry = NULL;
	gboolean continue_search = FALSE;

	gsize begin_range = (gsize)code_block;
	gsize end_range = begin_range + block_size;

	// Fast path, check table boundaries.
	found_entry = fast_find_range_in_table_no_lock_ex (begin_range, end_range, &continue_search);
	if (found_entry || continue_search == FALSE)
		return found_entry;

	// Scan table for an entry including range.
	for (GList *node = g_dynamic_function_table_begin; node; node = node->next) {
		DynamicFunctionTableEntry *current_entry = (DynamicFunctionTableEntry *)node->data;
		g_assert_checked (current_entry != NULL);

		// Do we have a match?
		if (current_entry->begin_range == begin_range && current_entry->end_range == end_range) {
			found_entry = node;
			break;
		}
	}

	return found_entry;
}

static DynamicFunctionTableEntry *
find_range_in_table_no_lock (const gpointer code_block, gsize block_size)
{
	GList *found_entry = find_range_in_table_no_lock_ex (code_block, block_size);
	return (found_entry != NULL) ? (DynamicFunctionTableEntry *)found_entry->data : NULL;
}

#define ENABLE_CHECKED_BUILD_UNWINDINFO
#ifdef ENABLE_CHECKED_BUILD_UNWINDINFO
static void
validate_table_no_lock (void)
{
	// Validation method checking that table is sorted as expected and don't include overlapped regions.
	// Method will assert on failure to explicitly indicate what check failed.
	if (g_dynamic_function_table_begin != NULL) {
		g_assert_checked (g_dynamic_function_table_end != NULL);

		DynamicFunctionTableEntry *previous_entry = NULL;
		DynamicFunctionTableEntry *current_entry = NULL;
		for (GList *node = g_dynamic_function_table_begin; node; node = node->next) {
			current_entry = (DynamicFunctionTableEntry *)node->data;

			g_assert_checked (current_entry != NULL);
			g_assert_checked (current_entry->end_range > current_entry->begin_range);

			if (previous_entry != NULL) {
				// List should be sorted in descending order on begin_range.
				g_assert_checked (previous_entry->begin_range > current_entry->begin_range);

				// Check for overlapped regions.
				g_assert_checked (previous_entry->begin_range >= current_entry->end_range);
			}

			previous_entry = current_entry;
		}
	}
}

#else

static void
validate_table_no_lock (void)
{
}
#endif /* ENABLE_CHECKED_BUILD_UNWINDINFO */

static DynamicFunctionTableEntry *
mono_arch_unwindinfo_insert_range_in_table (const gpointer code_block, gsize block_size)
{
	DynamicFunctionTableEntry *new_entry = NULL;

	gsize begin_range = (gsize)code_block;
	gsize end_range = begin_range + block_size;

	AcquireSRWLockExclusive (&g_dynamic_function_table_lock);
	new_entry = find_range_in_table_no_lock (code_block, block_size);
	if (new_entry == NULL && block_size != 0) {
		// Allocate new entry.
		new_entry = g_new0 (DynamicFunctionTableEntry, 1);
		if (new_entry != NULL) {

			// Pre-allocate RUNTIME_FUNCTION array, assume average method size of
			// MONO_UNWIND_INFO_RT_FUNC_SIZE bytes.
			InitializeSRWLock (&new_entry->lock);
			new_entry->handle = NULL;
			new_entry->begin_range = begin_range;
			new_entry->end_range = end_range;
			new_entry->rt_funcs_max_count = (block_size / MONO_UNWIND_INFO_RT_FUNC_SIZE) + 1;
			new_entry->rt_funcs_current_count = 0;
			new_entry->rt_funcs = g_new0 (RUNTIME_FUNCTION, new_entry->rt_funcs_max_count);

			if (new_entry->rt_funcs != NULL) {
				// Check insert on boundaries. List is sorted descending on begin_range.
				if (g_dynamic_function_table_begin == NULL) {
					g_dynamic_function_table_begin = g_list_append (g_dynamic_function_table_begin, new_entry);
					g_dynamic_function_table_end = g_dynamic_function_table_begin;
				} else if (((DynamicFunctionTableEntry *)(g_dynamic_function_table_begin->data))->begin_range < begin_range) {
					// Insert at the head.
					g_dynamic_function_table_begin = g_list_prepend (g_dynamic_function_table_begin, new_entry);
				} else if (((DynamicFunctionTableEntry *)(g_dynamic_function_table_end->data))->begin_range > begin_range) {
					// Insert at tail.
					g_list_append (g_dynamic_function_table_end, new_entry);
					g_dynamic_function_table_end = g_dynamic_function_table_end->next;
				} else {
					//Search and insert at correct position.
					for (GList *node = g_dynamic_function_table_begin; node; node = node->next) {
						DynamicFunctionTableEntry * current_entry = (DynamicFunctionTableEntry *)node->data;
						g_assert_checked (current_entry != NULL);

						if (current_entry->begin_range < new_entry->begin_range) {
							g_dynamic_function_table_begin = g_list_insert_before (g_dynamic_function_table_begin, node, new_entry);
							break;
						}
					}
				}

				// Allocate new growable handle table for entry.
				g_assert_checked (new_entry->handle == NULL);
				DWORD result = RtlAddGrowableFunctionTable (&new_entry->handle,
									new_entry->rt_funcs, new_entry->rt_funcs_current_count,
									new_entry->rt_funcs_max_count, new_entry->begin_range, new_entry->end_range);
				g_assert (!result);

				// Only included in checked builds. Validates the structure of table after insert.
				validate_table_no_lock ();

			} else {
				g_free (new_entry);
				new_entry = NULL;
			}
		}
	}
	ReleaseSRWLockExclusive (&g_dynamic_function_table_lock);

	return new_entry;
}

static void
mono_arch_unwindinfo_create (gpointer* monoui)
{
	PUNWIND_INFO newunwindinfo;
	*monoui = newunwindinfo = g_new0 (UNWIND_INFO, 1);
}

#define DEBUG_UWOP_ENABLED 0
#define DEBUG_UWOP if (DEBUG_UWOP_ENABLED) g_print

static void
push_unwind_op1 (PUNWIND_INFO unwindinfo, guint8 op)
{
	unwindinfo->code_count++;
	g_assert (unwindinfo->code_count <= MONO_MAX_UNWIND_CODES);
	unwindinfo->unwind_codes[MONO_MAX_UNWIND_CODES - unwindinfo->code_count] = op;
	unwindinfo->xdata.CodeWords = (unwindinfo->code_count + 3) >> 2;
	DEBUG_UWOP ("UNWIND OP: %x\n", op);
}

static void
push_unwind_op2 (PUNWIND_INFO unwindinfo, guint16 op)
{
	unwindinfo->code_count+=2;
	g_assert (unwindinfo->code_count <= MONO_MAX_UNWIND_CODES);
	unwindinfo->unwind_codes[MONO_MAX_UNWIND_CODES - unwindinfo->code_count] = op >> 8;
	unwindinfo->unwind_codes[MONO_MAX_UNWIND_CODES - unwindinfo->code_count + 1] = op;
	unwindinfo->xdata.CodeWords = (unwindinfo->code_count + 3) >> 2;
	DEBUG_UWOP ("UNWIND OP: %x\n", op);
}

static void
initialize_unwind_info_internal_ex (GSList *unwind_ops, PUNWIND_INFO unwindinfo)
{
	if (unwindinfo != NULL)
		push_unwind_op1 (unwindinfo, UWOP_END);

	if (unwind_ops != NULL && unwindinfo != NULL) {
		MonoUnwindOp *unwind_op_data;
		gint32 prev_cfa_ofs = 0;
		guint32 prev_when = 0;
		guint16 prev_regp = 0;
		guint32 prev_regp_when = 0;

		for (GSList *l = unwind_ops; l; l = l->next) {
			unwind_op_data = (MonoUnwindOp *)l->data;

			if (unwind_op_data->op == DW_CFA_def_cfa) {
				DEBUG_UWOP ("DW_CFA_def_cfa %s %x %x\n", mono_arch_regname(unwind_op_data->reg), unwind_op_data->val, unwind_op_data->when);
				g_assert (unwind_op_data->reg == ARMREG_SP);
				g_assert (unwind_op_data->val == 0);
				g_assert (unwind_op_data->when == 0);
				continue;
			}

			g_assert (unwind_op_data->when >= prev_when + 4);

			while (unwind_op_data->when > prev_when + 4)
			{
				push_unwind_op1 (unwindinfo, UWOP_NOP);
				prev_when += 4;
			}

			switch (unwind_op_data->op) {
				case DW_CFA_def_cfa_offset:
				{
					gint32 alloc_amount = unwind_op_data->val - prev_cfa_ofs;
					DEBUG_UWOP ("DW_CFA_def_cfa_offset %x %x\n", unwind_op_data->val, unwind_op_data->when);
					g_assert (unwind_op_data->when == prev_when + 4);

					if (l->next && l->next->next) {
						MonoUnwindOp* next_op = (MonoUnwindOp *)l->next->data;
						MonoUnwindOp* next2_op = (MonoUnwindOp *)l->next->next->data;

						if (next_op->when == unwind_op_data->when &&
							next2_op->when == unwind_op_data->when)
						{
							g_assert (next_op->op == DW_CFA_offset);
							DEBUG_UWOP ("DW_CFA_offset %s %x %x\n", mono_arch_regname(next_op->reg), next_op->val, next_op->when);
							g_assert (next_op->reg == ARMREG_FP);
							g_assert (next2_op->op == DW_CFA_offset);
							DEBUG_UWOP ("DW_CFA_offset %s %x %x\n", mono_arch_regname(next2_op->reg), next2_op->val, next2_op->when);
							g_assert (next2_op->reg == ARMREG_LR);

							gint32 fp_ofs = prev_cfa_ofs + next_op->val;
							gint32 lr_ofs = prev_cfa_ofs + next2_op->val;

							g_assert (fp_ofs == -alloc_amount);
							g_assert (lr_ofs == fp_ofs + 8);

							// UWOP_SAVE_FPLR_X adds an extra 8 bytes to offset
							gint32 adjusted_offset = alloc_amount - 8;

							g_assert ((adjusted_offset & 0xfffffe03) == 0);

							push_unwind_op1 (unwindinfo, UWOP_SAVE_FPLR_X | (adjusted_offset >> 3));

							prev_cfa_ofs = unwind_op_data->val;
							prev_when = next_op->when;

							l = l->next->next;
							break;
						}
					}

					if ((alloc_amount & 0xfffffe0f) == 0)
						push_unwind_op1 (unwindinfo, UWOP_ALLOC_S | (alloc_amount >> 4));
					else if ((alloc_amount & 0xffff800f) == 0)
						push_unwind_op2 (unwindinfo, UWOP_ALLOC_M | (alloc_amount >> 4));
					else
						g_assert_not_reached (); // TODO: UWOP_ALLOC_L
					prev_when = unwind_op_data->when;
					prev_cfa_ofs = unwind_op_data->val;
					break;
				}
				case DW_CFA_offset:
				{
					DEBUG_UWOP ("DW_CFA_offset %s %x %x\n", mono_arch_regname(unwind_op_data->reg), unwind_op_data->val, unwind_op_data->when);
					g_assert (unwind_op_data->when == prev_when + 4);

					if (unwind_op_data->reg == ARMREG_LR)
					{
						g_assert (l->next);
						MonoUnwindOp* next_op = (MonoUnwindOp *)l->next->data;
						g_assert (next_op->op == DW_CFA_offset);
						DEBUG_UWOP ("DW_CFA_offset %s %x %x\n", mono_arch_regname(next_op->reg), next_op->val, next_op->when);
						g_assert (next_op->reg == ARMREG_FP);
						g_assert (next_op->when == prev_when + 4);

						gint32 lr_ofs = prev_cfa_ofs + unwind_op_data->val;
						gint32 fp_ofs = prev_cfa_ofs + next_op->val;

						g_assert (lr_ofs == fp_ofs + 8);
						g_assert ((fp_ofs & 0xfffffe07) == 0);

						push_unwind_op1 (unwindinfo, UWOP_SAVE_FPLR | (fp_ofs >> 3));

						prev_when = next_op->when;
						l = l->next;
						break;
					}

					if (l->next)
					{
						MonoUnwindOp* next_op = (MonoUnwindOp *)l->next->data;

						if (next_op->when == unwind_op_data->when) {
							g_assert (next_op->op == DW_CFA_offset);
							DEBUG_UWOP ("DW_CFA_offset %s %x %x\n", mono_arch_regname(next_op->reg), next_op->val, next_op->when);
							g_assert (next_op->reg == unwind_op_data->reg + 1);
							g_assert (next_op->val == unwind_op_data->val + 8);

							gint32 reg_ofs = prev_cfa_ofs + unwind_op_data->val;
							g_assert ((reg_ofs & 0xfffffe07) == 0);
							gint32 adjusted_reg = unwind_op_data->reg - 19;
							g_assert ((adjusted_reg & 0xfffffff0) == 0);

							if (prev_regp == unwind_op_data->reg - 2 && prev_regp_when == unwind_op_data->when - 4)
								push_unwind_op1 (unwindinfo, UWOP_SAVE_NEXT);
							else
								push_unwind_op2 (unwindinfo, UWOP_SAVE_REGP | (adjusted_reg << 6) | (reg_ofs >> 3));

							prev_when = unwind_op_data->when;
							prev_regp = unwind_op_data->reg;
							prev_regp_when = unwind_op_data->when;
							l = l->next;
							break;
						}
					}

					gint32 reg_ofs = prev_cfa_ofs + unwind_op_data->val;
					g_assert ((reg_ofs & 0xfffffe07) == 0);
					gint32 adjusted_reg = unwind_op_data->reg - 19;
					g_assert ((adjusted_reg & 0xfffffff0) == 0);

					push_unwind_op2 (unwindinfo, UWOP_SAVE_REG | (adjusted_reg << 6) | (reg_ofs >> 3));

					prev_when = unwind_op_data->when;
					break;
				}
				case DW_CFA_def_cfa_register:
					DEBUG_UWOP ("DW_CFA_def_cfa_register %s %x\n", mono_arch_regname(unwind_op_data->reg), unwind_op_data->when);
					g_assert (unwind_op_data->when == prev_when + 4);
					g_assert (unwind_op_data->reg == ARMREG_FP);

					prev_when = unwind_op_data->when;

					push_unwind_op1 (unwindinfo, UWOP_SET_FP);
					break;
				default:
					DEBUG_UWOP ("%x\n", unwind_op_data->op);
					g_assert_not_reached ();
					break;
			}
		}
	}
}

static PUNWIND_INFO
initialize_unwind_info_internal (GSList *unwind_ops)
{
	PUNWIND_INFO unwindinfo;

	mono_arch_unwindinfo_create ((gpointer*)&unwindinfo);
	initialize_unwind_info_internal_ex (unwind_ops, unwindinfo);

	return unwindinfo;
}

guint
mono_arch_unwindinfo_init_method_unwind_info (gpointer cfg)
{
	MonoCompile * current_cfg = (MonoCompile *)cfg;
	g_assert (current_cfg->arch.unwindinfo == NULL);
	current_cfg->arch.unwindinfo = initialize_unwind_info_internal (current_cfg->unwind_ops);
	return mono_arch_unwindinfo_get_size ((PUNWIND_INFO)(current_cfg->arch.unwindinfo));
}

static void
mono_arch_unwindinfo_free_unwind_info (PUNWIND_INFO unwind_info)
{
	g_free (unwind_info);
}

static GList *
find_pc_in_table_no_lock_ex (const gpointer pc)
{
	GList *found_entry = NULL;
	gboolean continue_search = FALSE;

	gsize begin_range = (gsize)pc;
	gsize end_range = begin_range;

	// Fast path, check table boundaries.
	found_entry = fast_find_range_in_table_no_lock_ex (begin_range, begin_range, &continue_search);
	if (found_entry || continue_search == FALSE)
		return found_entry;

	// Scan table for a entry including range.
	for (GList *node = g_dynamic_function_table_begin; node; node = node->next) {
		DynamicFunctionTableEntry *current_entry = (DynamicFunctionTableEntry *)node->data;
		g_assert_checked (current_entry != NULL);

		// Do we have a match?
		if (current_entry->begin_range <= begin_range && current_entry->end_range >= end_range) {
			found_entry = node;
			break;
		}
	}

	return found_entry;
}

static DynamicFunctionTableEntry *
find_pc_in_table_no_lock (const gpointer pc)
{
	GList *found_entry = find_pc_in_table_no_lock_ex (pc);
	return (found_entry != NULL) ? (DynamicFunctionTableEntry *)found_entry->data : NULL;
}

#ifdef ENABLE_CHECKED_BUILD_UNWINDINFO
static void
validate_rt_funcs_in_table_no_lock (DynamicFunctionTableEntry *entry)
{
	// Validation method checking that runtime function table is sorted as expected and don't include overlapped regions.
	// Method will assert on failure to explicitly indicate what check failed.
	g_assert_checked (entry != NULL);
	g_assert_checked (entry->rt_funcs_max_count >= entry->rt_funcs_current_count);
	g_assert_checked (entry->rt_funcs != NULL);

	PRUNTIME_FUNCTION current_rt_func = NULL;
	PRUNTIME_FUNCTION previous_rt_func = NULL;
	for (int i = 0; i < entry->rt_funcs_current_count; ++i) {
		current_rt_func = &(entry->rt_funcs [i]);

		g_assert_checked (current_rt_func->BeginAddress < current_rt_func->UnwindData);

		if (previous_rt_func != NULL) {
			// List should be sorted in ascending order based on BeginAddress.
			g_assert_checked (previous_rt_func->BeginAddress < current_rt_func->BeginAddress);

			// Check for overlapped regions.
			g_assert_checked (previous_rt_func->UnwindData <= current_rt_func->BeginAddress);
		}

		previous_rt_func = current_rt_func;
	}
}

#else

static void
validate_rt_funcs_in_table_no_lock (DynamicFunctionTableEntry *entry)
{
}
#endif /* ENABLE_CHECKED_BUILD_UNWINDINFO */

PRUNTIME_FUNCTION
mono_arch_unwindinfo_insert_rt_func_in_table (const gpointer code, gsize code_size)
{
	PRUNTIME_FUNCTION new_rt_func = NULL;

	gsize begin_range = (gsize)code;
	gsize end_range = begin_range + code_size;

	AcquireSRWLockShared (&g_dynamic_function_table_lock);

	DynamicFunctionTableEntry *found_entry = find_pc_in_table_no_lock (code);

	if (found_entry != NULL) {

		AcquireSRWLockExclusive (&found_entry->lock);

		g_assert_checked (found_entry->begin_range <= begin_range);
		g_assert_checked (found_entry->end_range >= begin_range && found_entry->end_range >= end_range);
		g_assert_checked (found_entry->rt_funcs != NULL);
		g_assert_checked ((guchar*)code - found_entry->begin_range >= 0);

		gsize code_offset = (gsize)code - found_entry->begin_range;
		gsize entry_count = found_entry->rt_funcs_current_count;
		gsize max_entry_count = found_entry->rt_funcs_max_count;
		PRUNTIME_FUNCTION current_rt_funcs = found_entry->rt_funcs;

		RUNTIME_FUNCTION new_rt_func_data;
		new_rt_func_data.BeginAddress = code_offset;

		gsize aligned_unwind_data = ALIGN_TO(end_range, UNWIND_INFO_ALIGN);
		new_rt_func_data.UnwindData = aligned_unwind_data - found_entry->begin_range;

		PRUNTIME_FUNCTION new_rt_funcs = NULL;

		// List needs to be sorted in ascending order based on BeginAddress (Windows requirement if list
		// going to be directly reused in OS func tables. Check if we can append to end of existing table without realloc.
		if (entry_count == 0 || ((entry_count < max_entry_count) && (current_rt_funcs [entry_count - 1].BeginAddress) < code_offset)) {
			new_rt_func = &(current_rt_funcs [entry_count]);
			*new_rt_func = new_rt_func_data;
			entry_count++;
		} else {
			// No easy way out, need to realloc, grow to double size (or current max, if to small).
			max_entry_count = entry_count * 2 > max_entry_count ? entry_count * 2 : max_entry_count;
			new_rt_funcs = g_new0 (RUNTIME_FUNCTION, max_entry_count);

			if (new_rt_funcs != NULL) {
				gsize from_index = 0;
				gsize to_index = 0;

				// Copy from old table into new table. Make sure new rt func gets inserted
				// into correct location based on sort order.
				for (; from_index < entry_count; ++from_index) {
					if (new_rt_func == NULL && current_rt_funcs [from_index].BeginAddress > new_rt_func_data.BeginAddress) {
						new_rt_func = &(new_rt_funcs [to_index++]);
						*new_rt_func = new_rt_func_data;
					}

					if (current_rt_funcs [from_index].UnwindData != 0)
						new_rt_funcs [to_index++] = current_rt_funcs [from_index];
				}

				// If we didn't insert by now, put it last in the list.
				if (new_rt_func == NULL) {
					new_rt_func = &(new_rt_funcs [to_index]);
					*new_rt_func = new_rt_func_data;
				}
			}

			entry_count++;
		}

		// Update the stats for current entry.
		found_entry->rt_funcs_current_count = entry_count;
		found_entry->rt_funcs_max_count = max_entry_count;

		if (new_rt_funcs == NULL) {
			// No new table just report increase in use.
			g_assert_checked (found_entry->handle != NULL);
			RtlGrowFunctionTable (found_entry->handle, found_entry->rt_funcs_current_count);
		} else {
			// New table, delete old table and rt funcs, and register a new one.
			RtlDeleteGrowableFunctionTable (found_entry->handle);
			found_entry->handle = NULL;
			g_free (found_entry->rt_funcs);
			found_entry->rt_funcs = new_rt_funcs;
			DWORD result = RtlAddGrowableFunctionTable (&found_entry->handle,
								found_entry->rt_funcs, found_entry->rt_funcs_current_count,
								found_entry->rt_funcs_max_count, found_entry->begin_range, found_entry->end_range);
			g_assert (!result);
		}

		// Only included in checked builds. Validates the structure of table after insert.
		validate_rt_funcs_in_table_no_lock (found_entry);

		ReleaseSRWLockExclusive (&found_entry->lock);
	}

	ReleaseSRWLockShared (&g_dynamic_function_table_lock);

	return new_rt_func;
}

void
mono_arch_unwindinfo_install_method_unwind_info (PUNWIND_INFO *monoui, gpointer code, guint code_size)
{
	PUNWIND_INFO unwindinfo, targetinfo;
	guchar codecount;
	guint64 targetlocation;
	if (!*monoui)
		return;

	unwindinfo = *monoui;
	targetlocation = (guint64)&(((guchar*)code)[code_size]);
	targetinfo = (PUNWIND_INFO) ALIGN_TO(targetlocation, UNWIND_INFO_ALIGN);

	targetinfo->xdata = unwindinfo->xdata;

	g_assert ((code_size & 3) == 0);
	g_assert (code_size < (1 << 20)); // Need multiple records in this case
	targetinfo->xdata.FunctionLength = code_size >> 2;

	codecount = unwindinfo->code_count;
	if (codecount) {
		memcpy (&targetinfo->unwind_codes [0], &unwindinfo->unwind_codes [MONO_MAX_UNWIND_CODES - codecount],
			sizeof (UNWIND_CODE) * codecount);
	}

	mono_arch_unwindinfo_free_unwind_info (unwindinfo);
	*monoui = 0;

	// Register unwind info in table.
	mono_arch_unwindinfo_insert_rt_func_in_table (code, code_size);
}

void
mono_arch_unwindinfo_install_tramp_unwind_info (GSList *unwind_ops, gpointer code, guint code_size)
{
	PUNWIND_INFO unwindinfo = initialize_unwind_info_internal (unwind_ops);
	if (unwindinfo != NULL) {
		mono_arch_unwindinfo_install_method_unwind_info (&unwindinfo, code, code_size);
	}
}

void
mono_arch_code_chunk_new (void *chunk, int size)
{
	mono_arch_unwindinfo_insert_range_in_table (chunk, size);
}

static void
remove_range_in_table_no_lock (GList *entry)
{
	if (entry != NULL) {
		if (entry == g_dynamic_function_table_end)
			g_dynamic_function_table_end = entry->prev;

		g_dynamic_function_table_begin = g_list_remove_link (g_dynamic_function_table_begin, entry);
		DynamicFunctionTableEntry *removed_entry = (DynamicFunctionTableEntry *)entry->data;

		g_assert_checked (removed_entry != NULL);
		g_assert_checked (removed_entry->rt_funcs != NULL);

		// Remove function table from OS.
		if (removed_entry->handle != NULL) {
			RtlDeleteGrowableFunctionTable (removed_entry->handle);
		}

		g_free (removed_entry->rt_funcs);
		g_free (removed_entry);

		g_list_free_1 (entry);
	}

	// Only included in checked builds. Validates the structure of table after remove.
	validate_table_no_lock ();
}

void
mono_arch_unwindinfo_remove_pc_range_in_table (const gpointer code)
{
	AcquireSRWLockExclusive (&g_dynamic_function_table_lock);

	GList *found_entry = find_pc_in_table_no_lock_ex (code);

	g_assert_checked (found_entry != NULL || ((DynamicFunctionTableEntry *)found_entry->data)->begin_range == (gsize)code);
	remove_range_in_table_no_lock (found_entry);

	ReleaseSRWLockExclusive (&g_dynamic_function_table_lock);
}

void
mono_arch_code_chunk_destroy (void *chunk)
{
	mono_arch_unwindinfo_remove_pc_range_in_table (chunk);
}

#endif /* MONO_ARCH_HAVE_UNWIND_TABLE */

/*
 * mono_arm_throw_exception:
 *
 *   This function is called by the exception trampolines.
 * FP_REGS points to the 8 callee saved fp regs.
 */
void
mono_arm_throw_exception (gpointer arg, host_mgreg_t pc, host_mgreg_t *int_regs, gdouble *fp_regs, gboolean corlib, gboolean rethrow, gboolean preserve_ips)
{
	ERROR_DECL (error);
	MonoContext ctx;
	MonoObject *exc = NULL;
	guint32 ex_token_index, ex_token;

	if (!corlib)
		exc = (MonoObject*)arg;
	else {
		ex_token_index = (guint64)arg;
		ex_token = MONO_TOKEN_TYPE_DEF | ex_token_index;
		exc = (MonoObject*)mono_exception_from_token (mono_defaults.corlib, ex_token);
	}

	/* Adjust pc so it points into the call instruction */
	pc -= 4;

	/* Initialize a ctx based on the arguments */
	memset (&ctx, 0, sizeof (MonoContext));
	memcpy (&(ctx.regs [0]), int_regs, sizeof (host_mgreg_t) * 32);
	for (int i = 0; i < 8; i++)
		*((gdouble*)&ctx.fregs [ARMREG_D8 + i]) = fp_regs [i];
	ctx.has_fregs = 1;
	ctx.pc = pc;

	if (mono_object_isinst_checked (exc, mono_defaults.exception_class, error)) {
		MonoException *mono_ex = (MonoException*)exc;
		if (!rethrow && !mono_ex->caught_in_unmanaged) {
			mono_ex->stack_trace = NULL;
			mono_ex->trace_ips = NULL;
		} else if (preserve_ips) {
			mono_ex->caught_in_unmanaged = TRUE;
		}
	}
	mono_error_assert_ok (error);

	mono_handle_exception (&ctx, exc);

	mono_restore_context (&ctx);
}

void
mono_arm_resume_unwind (gpointer arg, host_mgreg_t pc, host_mgreg_t *int_regs, gdouble *fp_regs, gboolean corlib, gboolean rethrow)
{
	MonoContext ctx;

	/* Adjust pc so it points into the call instruction */
	pc -= 4;

	/* Initialize a ctx based on the arguments */
	memset (&ctx, 0, sizeof (MonoContext));
	memcpy (&(ctx.regs [0]), int_regs, sizeof (host_mgreg_t) * 32);
	for (int i = 0; i < 8; i++)
		*((gdouble*)&ctx.fregs [ARMREG_D8 + i]) = fp_regs [i];
	ctx.has_fregs = 1;
	ctx.pc = pc;

	mono_resume_unwind (&ctx);
}

/* 
 * mono_arch_unwind_frame:
 *
 * See exceptions-amd64.c for docs;
 */
gboolean
mono_arch_unwind_frame (MonoDomain *domain, MonoJitTlsData *jit_tls, 
							 MonoJitInfo *ji, MonoContext *ctx, 
							 MonoContext *new_ctx, MonoLMF **lmf,
							 host_mgreg_t **save_locations,
							 StackFrameInfo *frame)
{
	memset (frame, 0, sizeof (StackFrameInfo));
	frame->ji = ji;

	*new_ctx = *ctx;

	if (ji != NULL) {
		host_mgreg_t regs [MONO_MAX_IREGS + 8 + 1];
		guint8 *cfa;
		guint32 unwind_info_len;
		guint8 *unwind_info;

		if (ji->is_trampoline)
			frame->type = FRAME_TYPE_TRAMPOLINE;
		else
			frame->type = FRAME_TYPE_MANAGED;

		unwind_info = mono_jinfo_get_unwind_info (ji, &unwind_info_len);

		memcpy (regs, &new_ctx->regs, sizeof (host_mgreg_t) * 32);
		/* v8..v15 are callee saved */
		for (int i = 0; i < 8; i++)
			(regs + MONO_MAX_IREGS) [i] = *((host_mgreg_t*)&new_ctx->fregs [8 + i]);

		gpointer ip = MINI_FTNPTR_TO_ADDR (MONO_CONTEXT_GET_IP (ctx));
		gboolean success = mono_unwind_frame (unwind_info, unwind_info_len, (guint8*)ji->code_start,
						   (guint8*)ji->code_start + ji->code_size,
						   (guint8*)ip, NULL, regs, MONO_MAX_IREGS + 8,
						   save_locations, MONO_MAX_IREGS, (guint8**)&cfa);

		if (!success)
			return FALSE;

		memcpy (&new_ctx->regs, regs, sizeof (host_mgreg_t) * 32);
		for (int i = 0; i < 8; i++)
			*((host_mgreg_t*)&new_ctx->fregs [8 + i]) = (regs + MONO_MAX_IREGS) [i];

		new_ctx->pc = regs [ARMREG_LR];
		new_ctx->regs [ARMREG_SP] = (host_mgreg_t)(gsize)cfa;

		if (*lmf && (*lmf)->gregs [MONO_ARCH_LMF_REG_SP] && (MONO_CONTEXT_GET_SP (ctx) >= (gpointer)(*lmf)->gregs [MONO_ARCH_LMF_REG_SP])) {
			/* remove any unused lmf */
			*lmf = (MonoLMF*)(((gsize)(*lmf)->previous_lmf) & ~3);
		}

		/* we substract 1, so that the IP points into the call instruction */
		new_ctx->pc--;

		return TRUE;
	} else if (*lmf) {
		g_assert ((((guint64)(*lmf)->previous_lmf) & 2) == 0);

		frame->type = FRAME_TYPE_MANAGED_TO_NATIVE;

		ji = mini_jit_info_table_find (domain, (gpointer)(*lmf)->pc, NULL);
		if (!ji)
			return FALSE;

		g_assert (MONO_ARCH_LMF_REGS == ((0x3ff << 19) | (1 << ARMREG_FP) | (1 << ARMREG_SP)));
		memcpy (&new_ctx->regs [ARMREG_R19], &(*lmf)->gregs [0], sizeof (host_mgreg_t) * 10);
		new_ctx->regs [ARMREG_FP] = (*lmf)->gregs [MONO_ARCH_LMF_REG_FP];
		new_ctx->regs [ARMREG_SP] = (*lmf)->gregs [MONO_ARCH_LMF_REG_SP];
		new_ctx->pc = (*lmf)->pc;

		/* we substract 1, so that the IP points into the call instruction */
		new_ctx->pc--;

		*lmf = (MonoLMF*)(((gsize)(*lmf)->previous_lmf) & ~3);

		return TRUE;
	}

	return FALSE;
}

/*
 * handle_exception:
 *
 *   Called by resuming from a signal handler.
 */
static void
handle_signal_exception (gpointer obj)
{
	MonoJitTlsData *jit_tls = mono_tls_get_jit_tls ();
	MonoContext ctx;

	memcpy (&ctx, &jit_tls->ex_ctx, sizeof (MonoContext));

	mono_handle_exception (&ctx, (MonoObject*)obj);

	mono_restore_context (&ctx);
}

/*
 * This is the function called from the signal handler
 */
gboolean
mono_arch_handle_exception (void *ctx, gpointer obj)
{
#if defined(MONO_CROSS_COMPILE)
	g_assert_not_reached ();
#elif defined(MONO_ARCH_USE_SIGACTION)
	MonoContext mctx;
	MonoJitTlsData *jit_tls;
	void *sigctx = ctx;

	/*
	 * Resume into the normal stack and handle the exception there.
	 */
	jit_tls = mono_tls_get_jit_tls ();

	/* Pass the ctx parameter in TLS */
	mono_sigctx_to_monoctx (sigctx, &jit_tls->ex_ctx);

	/* The others in registers */
	mctx = jit_tls->ex_ctx;
	mctx.regs [0] = (host_mgreg_t) obj;

	gpointer addr = (gpointer)handle_signal_exception;
	mctx.pc = (host_mgreg_t) addr;

	host_mgreg_t sp = mctx.regs [ARMREG_SP] - MONO_ARCH_REDZONE_SIZE;
	mctx.regs [ARMREG_SP] = sp;

	mono_monoctx_to_sigctx (&mctx, sigctx);
#else
	MonoContext mctx;

	mono_sigctx_to_monoctx (ctx, &mctx);

	mono_handle_exception (&mctx, obj);

	mono_monoctx_to_sigctx (&mctx, ctx);
#endif

	return TRUE;
}

gpointer
mono_arch_ip_from_context (void *sigctx)
{
#ifdef MONO_CROSS_COMPILE
	g_assert_not_reached ();
	return NULL;
#elif defined(HOST_WIN32)
	return (gpointer)((CONTEXT*)sigctx)->Pc;
#else
	return (gpointer)UCONTEXT_REG_PC (sigctx);
#endif
}

void
mono_arch_setup_async_callback (MonoContext *ctx, void (*async_cb)(void *fun), gpointer user_data)
{
	host_mgreg_t sp = (host_mgreg_t)MONO_CONTEXT_GET_SP (ctx);

	// FIXME:
	g_assert (!user_data);

	/* Allocate a stack frame */
	sp -= 32;
	MONO_CONTEXT_SET_SP (ctx, sp);

	mono_arch_setup_resume_sighandler_ctx (ctx, (gpointer)async_cb);
}

/*
 * mono_arch_setup_resume_sighandler_ctx:
 *
 *   Setup CTX so execution continues at FUNC.
 */
void
mono_arch_setup_resume_sighandler_ctx (MonoContext *ctx, gpointer func)
{
	MONO_CONTEXT_SET_IP (ctx,func);
}

void
mono_arch_undo_ip_adjustment (MonoContext *ctx)
{
	gpointer pc = (gpointer)ctx->pc;
	pc = (gpointer)((guint64)MINI_FTNPTR_TO_ADDR (pc) + 1);
	ctx->pc = (host_mgreg_t)MINI_ADDR_TO_FTNPTR (pc);
}

void
mono_arch_do_ip_adjustment (MonoContext *ctx)
{
	gpointer pc = (gpointer)ctx->pc;
	pc = (gpointer)((guint64)MINI_FTNPTR_TO_ADDR (pc) - 1);
	ctx->pc = (host_mgreg_t)MINI_ADDR_TO_FTNPTR (pc);
}
