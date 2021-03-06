﻿// -----------------------------------------------------------------------
// <copyright file="ConditionalExpression.cs" company="Ubiquity.NET Contributors">
// Copyright (c) Ubiquity.NET Contributors. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

using Ubiquity.ArgValidators;

namespace Kaleidoscope.Grammar.AST
{
    public class ConditionalExpression
        : IExpression
    {
        public ConditionalExpression( SourceSpan location
                                    , IExpression condition
                                    , IExpression thenExpression
                                    , IExpression elseExpression
                                    , LocalVariableDeclaration resultVar
                                    )
        {
            Location = location;
            Condition = condition;
            ThenExpression = thenExpression;
            ElseExpression = elseExpression;
            ResultVariable = resultVar;
        }

        public SourceSpan Location { get; }

        public IExpression Condition { get; }

        public IExpression ThenExpression { get; }

        public IExpression ElseExpression { get; }

        // compiler generated result variable supports building conditional
        // expressions without the need for SSA form by using mutable variables
        // The result is assigned a value from both sides of the branch. In
        // pure SSA form this isn't needed as a PHI node would be used instead.
        public LocalVariableDeclaration ResultVariable { get; }

        public TResult? Accept<TResult>( IAstVisitor<TResult> visitor )
            where TResult : class
        {
            return visitor.ValidateNotNull( nameof( visitor ) ).Visit( this );
        }

        public IEnumerable<IAstNode> Children
        {
            get
            {
                yield return Condition;
                yield return ThenExpression;
                yield return ElseExpression;
            }
        }

        public override string ToString( )
        {
            return $"Conditional({Condition}, {ThenExpression}, {ElseExpression})";
        }
    }
}
