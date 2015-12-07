﻿using Llvm.NET.Values;

namespace Llvm.NET.Instructions
{
    public class CallInstruction
        : Instruction
        , IAttributeSetContainer
    {
        public AttributeSet Attributes
        {
            get 
            {
                if( TargetFunction == null )
                    return null;

                return new AttributeSet( TargetFunction, NativeMethods.GetCallSiteAttributeSet( ValueHandle ) );
            }

            set
            {
                NativeMethods.SetCallSiteAttributeSet( ValueHandle, value.NativeAttributeSet );
            }
        }

        public Function TargetFunction
        {
            get
            {
                if( Operands.Count < 1 )
                    return null;

                // last Operand is the target function
                return Operands[ Operands.Count - 1 ] as Function;
            }
        }

        public bool IsTailCall 
        {
            get
            {
                return NativeMethods.IsTailCall( ValueHandle );
            }

            set
            {
                NativeMethods.SetTailCall( ValueHandle, value );
            }
        }

        internal CallInstruction( LLVMValueRef valueRef )
            : this( valueRef, false )
        {
        }

        internal CallInstruction( LLVMValueRef valueRef, bool preValidated )
            : base( preValidated ? valueRef : ValidateConversion( valueRef, NativeMethods.IsACallInst ) )
        {
        }
    }
}
