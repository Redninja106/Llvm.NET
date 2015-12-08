﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Llvm.NET.Values
{
    /// <summary>Single attribute for functions, function returns and function parameters</summary>
    /// <remarks>
    /// This is the equivalent to the underlying llvm::Attribute class. The name was changed to 
    /// AttributeValue in .NET to prevent confusion with the <see cref="Attribute"/> class
    /// that is used throughout .NET libraries. As with the underlying LLVM type, this is an 
    /// immutable value type. 
    /// </remarks>
    [SuppressMessage( "Microsoft.Design"
                    , "CA1049:TypesThatOwnNativeResourcesShouldBeDisposable"
                    , Justification = "The native pointer isn't owned by this structure, it is owned by the LLVM context"
                    )
    ]
    public struct AttributeValue
    {
        /// <summary>Creates a simple boolean attribute</summary>
        /// <param name="kind">Kind of attribute</param>
        public AttributeValue( AttributeKind kind )
            : this( Context.CurrentContext, kind, 0ul )
        {
            if( kind.RequiresIntValue() )
                throw new ArgumentException( $"Attribute {kind} requires a value", nameof( kind ) );
        }

        /// <summary>Creates a simple boolean attribute</summary>
        /// <param name="ctx">Context for creating the attribute</param>
        /// <param name="kind">Kind of attribute</param>
        public AttributeValue( Context ctx, AttributeKind kind )
            : this( ctx, kind, 0ul )
        {
            if( kind.RequiresIntValue() )
                throw new ArgumentException( $"Attribute {kind} requires a value", nameof( kind ) );
        }

        /// <summary>Creates an attribute with an integer value parameter</summary>
        /// <param name="kind">The kind of attribute</param>
        /// <param name="value">Value for the attribute</param>
        /// <remarks>
        /// <para>Not all attributes support a value and those that do don't all support
        /// a full 64bit value. The following table provides the kinds of attributes
        /// accepting a value and the allowed size of the values.</para>
        /// <list type="table">
        /// <listheader><term><see cref="AttributeKind"/></term><term>Bit Length</term></listheader>
        /// <item><term><see cref="AttributeKind.Alignment"/></term><term>32</term></item>
        /// <item><term><see cref="AttributeKind.StackAlignment"/></term><term>32</term></item>
        /// <item><term><see cref="AttributeKind.Dereferenceable"/></term><term>64</term></item>
        /// <item><term><see cref="AttributeKind.DereferenceableOrNull"/></term><term>64</term></item>
        /// </list>
        /// </remarks>
        public AttributeValue( AttributeKind kind, UInt64 value )
            : this( Context.CurrentContext, kind, value )
        {
        }

        /// <summary>Creates an attribute with an integer value parameter</summary>
        /// <param name="ctx">Context used for interning attributes</param>
        /// <param name="kind">The kind of attribute</param>
        /// <param name="value">Value for the attribute</param>
        /// <remarks>
        /// <para>Not all attributes support a value and those that do don't all support
        /// a full 64bit value. The following table provides the kinds of attributes
        /// accepting a value and the allowed size of the values.</para>
        /// <list type="table">
        /// <listheader><term><see cref="AttributeKind"/></term><term>Bit Length</term></listheader>
        /// <item><term><see cref="AttributeKind.Alignment"/></term><term>32</term></item>
        /// <item><term><see cref="AttributeKind.StackAlignment"/></term><term>32</term></item>
        /// <item><term><see cref="AttributeKind.Dereferenceable"/></term><term>64</term></item>
        /// <item><term><see cref="AttributeKind.DereferenceableOrNull"/></term><term>64</term></item>
        /// </list>
        /// </remarks>
        public AttributeValue( Context ctx, AttributeKind kind, UInt64 value )
            : this( NativeMethods.CreateAttribute( ctx.VerifyAsArg( nameof( ctx ) ).ContextHandle, ( LLVMAttrKind )kind, value ) )
        {
        }

        /// <summary>Adds a valueless named attribute</summary>
        /// <param name="name">Attribute name</param>
        public AttributeValue( string name )
            : this( Context.CurrentContext, name, string.Empty )
        {
        }

        /// <summary>Adds a valueless named attribute</summary>
        /// <param name="ctx">Context to use for interning attributes</param>
        /// <param name="name">Attribute name</param>
        public AttributeValue( Context ctx, string name )
            : this( ctx, name, string.Empty )
        {
        }

        /// <summary>Adds a Target specific named attribute with value</summary>
        /// <param name="name">Name of the attribute</param>
        /// <param name="value">Value of the attribute</param>
        public AttributeValue( string name, string value )
            : this( Context.CurrentContext, name, value )
        {
        }

        /// <summary>Adds a Target specific named attribute with value</summary>
        /// <param name="ctx">Context to use for interning attributes</param>
        /// <param name="name">Name of the attribute</param>
        /// <param name="value">Value of the attribute</param>
        public AttributeValue( Context ctx, string name, string value )
        {
            ctx.VerifyAsArg( nameof( ctx ) );
            if( string.IsNullOrWhiteSpace( name ) )
                throw new ArgumentException( "AttributeValue name cannot be null, Empty or all whitespace" );

            NativeAttribute = NativeMethods.CreateTargetDependentAttribute( ctx.ContextHandle, name, value );
        }

        internal AttributeValue( UIntPtr nativeValue )
        {
            NativeAttribute = nativeValue;
        }

        #region IEquatable
        public override int GetHashCode( ) => NativeAttribute.GetHashCode( );

        public override bool Equals( object obj )
        {
            if( obj is AttributeValue )
                return Equals( ( LLVMMetadataRef )obj );

            if( obj is UIntPtr )
                return NativeAttribute.Equals( obj );

            return base.Equals( obj );
        }

        public bool Equals( AttributeValue other ) => NativeAttribute == other.NativeAttribute;
        
        public static bool operator ==( AttributeValue lhs, AttributeValue rhs ) => lhs.Equals( rhs );
        public static bool operator !=( AttributeValue lhs, AttributeValue rhs ) => !lhs.Equals( rhs );
        #endregion

        /// <summary>Kind of the attribute, or null for target specif named attributes</summary>
        public AttributeKind? Kind => IsString ? (AttributeKind?)null : ( AttributeKind )NativeMethods.GetAttributeKind( NativeAttribute );

        /// <summary>Name of a named attribute or null for other kinds of attributes</summary>
        public string Name
        {
            get
            {
                if( !IsString )
                    return null;

                var llvmString = NativeMethods.GetAttributeName( NativeAttribute );
                return Marshal.PtrToStringAnsi( llvmString );
            }
        }

        /// <summary>StringValue for named attributes with values</summary>
        public string StringValue
        {
            get
            {
                if( !IsString )
                    return null;

                var llvmString = NativeMethods.GetAttributeStringValue( NativeAttribute );
                return Marshal.PtrToStringAnsi( llvmString );
            }
        }

        /// <summary>Integer value of the attribute or null if the attribute doesn't have a value</summary>
        public UInt64? IntegerValue => IsInt ? NativeMethods.GetAttributeValue( NativeAttribute ) : (UInt64?)null;

        /// <summary>Flag to indicate if this attribute is a target specific string value</summary>
        public bool IsString => NativeMethods.IsStringAttribute( NativeAttribute );

        /// <summary>Flag to indicate if this attribute has an integer attribute</summary>
        public bool IsInt => NativeMethods.IsIntAttribute( NativeAttribute );

        /// <summary>Flag to indicate if this attribute is a simple enumeration value</summary>
        public bool IsEnum => NativeMethods.IsEnumAttribute( NativeAttribute );

        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources" )]
        internal readonly UIntPtr NativeAttribute;

        /// <summary>Implicitly cast an <see cref="AttributeKind"/> to an <see cref="AttributeValue"/></summary>
        /// <param name="kind">Kind of attribute to create</param>
        [SuppressMessage( "Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Justification = "Available via constructor, this is for convenience" )]
        public static implicit operator AttributeValue( AttributeKind kind ) => new AttributeValue( kind );

        /// <summary>Implicitly cast a string to an named <see cref="AttributeValue"/></summary>
        /// <param name="kind">Attribute name</param>
        [SuppressMessage( "Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Justification = "Available via constructor, this is for convenience" )]
        public static implicit operator AttributeValue( string kind ) => new AttributeValue( kind );

    }
}
