/*
 * Tencent is pleased to support the open source community by making InjectFix available.
 * Copyright (C) 2019 Tencent.  All rights reserved.
 * InjectFix is licensed under the MIT License, except for the third-party components listed in the file 'LICENSE' which may be subject to their corresponding license terms. 
 * This file is subject to the terms and conditions defined in file 'LICENSE', which is part of this source code package.
 */

namespace IFix.Core
{
    public enum Code
    {
        Conv_U1,
        Endfilter,
        Ldarga,
        And,
        Ldind_I8,
        Stelem_Any,
        Ldind_U4,
        Stind_I1,
        Cpblk,
        Box,
        Rethrow,
        Clt,
        Stind_R4,
        Localloc,
        Ldloca,
        Ldc_I4,
        Stind_I2,
        Ckfinite,
        Ldfld,
        Ldelem_I2,
        No,
        Conv_Ovf_I_Un,
        Conv_Ovf_U2,
        Unbox_Any,
        Blt_Un,
        Cpobj,
        Newarr,
        Conv_Ovf_I4,
        CallExtern,
        Ldind_I1,
        Conv_Ovf_I1_Un,
        Add_Ovf,
        Stelem_Ref,
        Bge,
        Stsfld,
        Conv_R4,
        Ceq,
        Ldind_I2,
        Conv_Ovf_U4,
        Break,
        Conv_R_Un,
        Neg,
        Conv_I8,
        Unaligned,
        Ldind_R8,
        Bgt,
        Conv_U2,
        Brfalse,
        Ldc_I8,
        Refanyval,
        Stobj,
        Or,
        Isinst,
        Shr,
        Pop,
        Ldc_R8,
        Mul_Ovf,
        Switch,
        Sub_Ovf,
        Ldelem_I1,
        Rem,
        Stelem_I1,
        Stelem_R4,
        Throw,
        //Calli,
        Ret,
        Conv_I4,
        Ldind_I,
        Callvirt,
        Leave,
        Stfld,
        Conv_Ovf_U_Un,
        Ble,
        Callvirtvirt,
        Stind_I4,
        Jmp,
        Readonly,
        Sizeof,
        Conv_Ovf_I2,
        Ldind_Ref,
        Cgt,
        Conv_Ovf_I4_Un,
        Mul,
        Stelem_I8,
        Mul_Ovf_Un,
        Stelem_I4,
        Rem_Un,
        Conv_I2,
        Bgt_Un,

        //Pseudo instruction
        StackSpace,
        Conv_Ovf_U8_Un,
        Conv_Ovf_U,
        Ldelem_I4,
        Dup,
        Starg,
        Bge_Un,
        Not,
        Volatile,
        Add_Ovf_Un,
        Ldind_U2,
        Ldc_R4,
        Conv_U4,
        Bne_Un,
        Conv_Ovf_I1,
        Stind_Ref,
        Brtrue,
        Call,
        Ldsfld,
        Ldelem_I8,
        Ldelem_Any,
        Stind_I8,
        Xor,
        Nop,
        Ldnull,
        Stelem_R8,
        Endfinally,
        Conv_Ovf_I,
        Ldtype, // custom
        Sub,
        Tail,
        Constrained,
        Cgt_Un,
        Ldind_R4,
        Sub_Ovf_Un,
        Ldvirtftn2,
        Ldsflda,
        Ldvirtftn,
        Stind_I,
        Ldelem_U2,
        Conv_Ovf_U1,
        Ldelema,
        Conv_I,
        Clt_Un,
        Ldtoken,
        Unbox,
        Conv_Ovf_I2_Un,
        Ldelem_I,
        Arglist,
        Initblk,
        Div,
        Conv_R8,
        Add,
        Conv_Ovf_I8_Un,
        Conv_Ovf_U2_Un,
        Ldlen,
        Shr_Un,
        Beq,
        Ldind_I4,
        Conv_Ovf_U4_Un,
        Mkrefany,
        Stelem_I2,
        Ldobj,
        Ldloc,
        Initobj,
        Stelem_I,
        Ldelem_R8,
        Refanytype,
        Br,
        Ldelem_Ref,
        Stloc,
        Conv_Ovf_U1_Un,
        Ldelem_U1,
        Ldind_U1,
        Ldflda,
        Stind_R8,
        Ble_Un,
        Newanon,
        Blt,
        Div_Un,
        Ldstr,
        Ldftn,
        Ldarg,
        Shl,
        Castclass,
        Conv_U8,
        Newobj,
        Ldelem_R4,
        Conv_Ovf_U8,
        Ldelem_U4,
        Conv_I1,
        Conv_Ovf_I8,
        Conv_U,
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct Instruction
    {
        /// <summary>
        /// 指令MAGIC
        /// </summary>
        public const ulong INSTRUCTION_FORMAT_MAGIC = 4423565712617255911;

        /// <summary>
        /// 当前指令
        /// </summary>
        public Code Code;

        /// <summary>
        /// 操作数
        /// </summary>
        public int Operand;
    }

    public enum ExceptionHandlerType
    {
        Catch = 0,
        Filter = 1,
        Finally = 2,
        Fault = 4
    }

    public sealed class ExceptionHandler
    {
        public System.Type CatchType;
        public int CatchTypeId;
        public int HandlerEnd;
        public int HandlerStart;
        public ExceptionHandlerType HandlerType;
        public int TryEnd;
        public int TryStart;
    }
}