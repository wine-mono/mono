#ifndef __MONO_JIT_ICALLS_H__
#define __MONO_JIT_ICALLS_H__

#include <math.h>

#include "mini.h"

/*
 * ARM EABI code generation in this backend uses Mono's soft-float managed
 * ABI for helper calls: floating-point helper arguments/results are passed in
 * core registers.  armhf C code uses the VFP procedure-call standard by
 * default.  Annotate only JIT helper entry points that expose float/double so
 * those C functions match the calls this backend emits, without changing
 * ordinary runtime C calls.
 */
#if defined(__arm__) && defined(__GNUC__) && defined(__ARM_PCS_VFP)
#define MONO_JIT_ICALL_FP_ABI __attribute__((pcs("aapcs")))
#else
#define MONO_JIT_ICALL_FP_ABI
#endif

void* mono_ldftn (MonoMethod *method) MONO_INTERNAL;

void* mono_ldvirtfn (MonoObject *obj, MonoMethod *method) MONO_INTERNAL;

void* mono_ldvirtfn_gshared (MonoObject *obj, MonoMethod *method) MONO_INTERNAL;

void mono_helper_stelem_ref_check (MonoArray *array, MonoObject *val) MONO_INTERNAL;

gint64 mono_llmult (gint64 a, gint64 b) MONO_INTERNAL;

guint64 mono_llmult_ovf_un (guint64 a, guint64 b) MONO_INTERNAL;

guint64 mono_llmult_ovf (gint64 a, gint64 b) MONO_INTERNAL;

gint32 mono_idiv (gint32 a, gint32 b) MONO_INTERNAL;

guint32 mono_idiv_un (guint32 a, guint32 b) MONO_INTERNAL;

gint32 mono_irem (gint32 a, gint32 b) MONO_INTERNAL;

guint32 mono_irem_un (guint32 a, guint32 b) MONO_INTERNAL;

gint32 mono_imul (gint32 a, gint32 b) MONO_INTERNAL;

gint32 mono_imul_ovf (gint32 a, gint32 b) MONO_INTERNAL;

gint32 mono_imul_ovf_un (guint32 a, guint32 b) MONO_INTERNAL;

double MONO_JIT_ICALL_FP_ABI mono_fdiv (double a, double b) MONO_INTERNAL;

gint64 mono_lldiv (gint64 a, gint64 b) MONO_INTERNAL;

gint64 mono_llrem (gint64 a, gint64 b) MONO_INTERNAL;

guint64 mono_lldiv_un (guint64 a, guint64 b) MONO_INTERNAL;

guint64 mono_llrem_un (guint64 a, guint64 b) MONO_INTERNAL;

guint64 mono_lshl (guint64 a, gint32 shamt) MONO_INTERNAL;

guint64 mono_lshr_un (guint64 a, gint32 shamt) MONO_INTERNAL;

gint64 mono_lshr (gint64 a, gint32 shamt) MONO_INTERNAL;

MonoArray *mono_array_new_va (MonoMethod *cm, ...) MONO_INTERNAL;

MonoArray *mono_array_new_1 (MonoMethod *cm, guint32 length) MONO_INTERNAL;

MonoArray *mono_array_new_2 (MonoMethod *cm, guint32 length1, guint32 length2) MONO_INTERNAL;

MonoArray *mono_array_new_3 (MonoMethod *cm, guint32 length1, guint32 length2, guint32 length3) MONO_INTERNAL;

gpointer mono_class_static_field_address (MonoDomain *domain, MonoClassField *field) MONO_INTERNAL;

gpointer mono_ldtoken_wrapper (MonoImage *image, int token, MonoGenericContext *context) MONO_INTERNAL;

gpointer mono_ldtoken_wrapper_generic_shared (MonoImage *image, int token, MonoMethod *method) MONO_INTERNAL;

guint64 MONO_JIT_ICALL_FP_ABI mono_fconv_u8 (double v) MONO_INTERNAL;

gint64 MONO_JIT_ICALL_FP_ABI mono_fconv_i8 (double v) MONO_INTERNAL;

guint32 MONO_JIT_ICALL_FP_ABI mono_fconv_u4 (double v) MONO_INTERNAL;

gint64 MONO_JIT_ICALL_FP_ABI mono_fconv_ovf_i8 (double v) MONO_INTERNAL;

guint64 MONO_JIT_ICALL_FP_ABI mono_fconv_ovf_u8 (double v) MONO_INTERNAL;

double MONO_JIT_ICALL_FP_ABI mono_lconv_to_r8 (gint64 a) MONO_INTERNAL;

double MONO_JIT_ICALL_FP_ABI mono_conv_to_r8 (gint32 a) MONO_INTERNAL;

double MONO_JIT_ICALL_FP_ABI mono_conv_to_r4 (gint32 a) MONO_INTERNAL;

#ifdef MONO_ARCH_SOFT_FLOAT
double MONO_JIT_ICALL_FP_ABI mono_lconv_to_r4 (gint64 a) MONO_INTERNAL;
#else
float MONO_JIT_ICALL_FP_ABI mono_lconv_to_r4 (gint64 a) MONO_INTERNAL;
#endif

double MONO_JIT_ICALL_FP_ABI mono_conv_to_r8_un (guint32 a) MONO_INTERNAL;

double MONO_JIT_ICALL_FP_ABI mono_lconv_to_r8_un (guint64 a) MONO_INTERNAL;

gpointer mono_helper_compile_generic_method (MonoObject *obj, MonoMethod *method, gpointer *this_arg) MONO_INTERNAL;

MonoString *mono_helper_ldstr (MonoImage *image, guint32 idx) MONO_INTERNAL;

MonoString *mono_helper_ldstr_mscorlib (guint32 idx) MONO_INTERNAL;

MonoObject *mono_helper_newobj_mscorlib (guint32 idx) MONO_INTERNAL;

double MONO_JIT_ICALL_FP_ABI mono_fsub (double a, double b) MONO_INTERNAL;

double MONO_JIT_ICALL_FP_ABI mono_fadd (double a, double b) MONO_INTERNAL;

double MONO_JIT_ICALL_FP_ABI mono_fmul (double a, double b) MONO_INTERNAL;

double MONO_JIT_ICALL_FP_ABI mono_fneg (double a) MONO_INTERNAL;

double MONO_JIT_ICALL_FP_ABI mono_fconv_r4 (double a) MONO_INTERNAL;

gint8 MONO_JIT_ICALL_FP_ABI mono_fconv_i1 (double a) MONO_INTERNAL;

gint16 MONO_JIT_ICALL_FP_ABI mono_fconv_i2 (double a) MONO_INTERNAL;

gint32 MONO_JIT_ICALL_FP_ABI mono_fconv_i4 (double a) MONO_INTERNAL;

guint8 MONO_JIT_ICALL_FP_ABI mono_fconv_u1 (double a) MONO_INTERNAL;

guint16 MONO_JIT_ICALL_FP_ABI mono_fconv_u2 (double a) MONO_INTERNAL;

gboolean MONO_JIT_ICALL_FP_ABI mono_fcmp_eq (double a, double b) MONO_INTERNAL;

gboolean MONO_JIT_ICALL_FP_ABI mono_fcmp_ge (double a, double b) MONO_INTERNAL;

gboolean MONO_JIT_ICALL_FP_ABI mono_fcmp_gt (double a, double b) MONO_INTERNAL;

gboolean MONO_JIT_ICALL_FP_ABI mono_fcmp_le (double a, double b) MONO_INTERNAL;

gboolean MONO_JIT_ICALL_FP_ABI mono_fcmp_lt (double a, double b) MONO_INTERNAL;

gboolean MONO_JIT_ICALL_FP_ABI mono_fcmp_ne_un (double a, double b) MONO_INTERNAL;

gboolean MONO_JIT_ICALL_FP_ABI mono_fcmp_ge_un (double a, double b) MONO_INTERNAL;

gboolean MONO_JIT_ICALL_FP_ABI mono_fcmp_gt_un (double a, double b) MONO_INTERNAL;

gboolean MONO_JIT_ICALL_FP_ABI mono_fcmp_le_un (double a, double b) MONO_INTERNAL;

gboolean MONO_JIT_ICALL_FP_ABI mono_fcmp_lt_un (double a, double b) MONO_INTERNAL;

gboolean MONO_JIT_ICALL_FP_ABI mono_fceq (double a, double b) MONO_INTERNAL;

gboolean MONO_JIT_ICALL_FP_ABI mono_fcgt (double a, double b) MONO_INTERNAL;

gboolean MONO_JIT_ICALL_FP_ABI mono_fcgt_un (double a, double b) MONO_INTERNAL;

gboolean MONO_JIT_ICALL_FP_ABI mono_fclt (double a, double b) MONO_INTERNAL;

gboolean MONO_JIT_ICALL_FP_ABI mono_fclt_un (double a, double b) MONO_INTERNAL;

gboolean MONO_JIT_ICALL_FP_ABI mono_isfinite (double a) MONO_INTERNAL;

double   MONO_JIT_ICALL_FP_ABI mono_fload_r4 (float *ptr) MONO_INTERNAL;

void     MONO_JIT_ICALL_FP_ABI mono_fstore_r4 (double val, float *ptr) MONO_INTERNAL;

guint32  MONO_JIT_ICALL_FP_ABI mono_fload_r4_arg (double val) MONO_INTERNAL;

#if defined(__arm__) && defined(__ARM_PCS_VFP)
double MONO_JIT_ICALL_FP_ABI mono_frem (double a, double b) MONO_INTERNAL;
#endif

void     mono_break (void) MONO_INTERNAL;

MonoException *mono_create_corlib_exception_0 (guint32 token) MONO_INTERNAL;

MonoException *mono_create_corlib_exception_1 (guint32 token, MonoString *arg) MONO_INTERNAL;

MonoException *mono_create_corlib_exception_2 (guint32 token, MonoString *arg1, MonoString *arg2) MONO_INTERNAL;

MonoObject* mono_object_castclass (MonoObject *obj, MonoClass *klass) MONO_INTERNAL;

gpointer mono_get_native_calli_wrapper (MonoImage *image, MonoMethodSignature *sig, gpointer func) MONO_INTERNAL;

#endif /* __MONO_JIT_ICALLS_H__ */

