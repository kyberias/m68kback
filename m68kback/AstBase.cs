using System;
using System.Collections.Generic;

namespace m68kback
{
    public abstract class AstBase
    {
        public abstract object Visit(IVisitor visitor);
    }

    public abstract class Expression : AstBase
    {
        public TypeDeclaration Type { get; set; }
    }

    public class AllocaExpression : Expression
    {
        public int Alignment { get; set; }
        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class CallExpression : Expression
    {
        public string FunctionName { get; set; }
        public List<Expression> Parameters { get; set; }
        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class ArithmeticExpression : Expression
    {
        public bool NoUnsignedWrap { get; set; }
        public bool NoSignedWrap { get; set; }
        public Token Operator { get; set; }
        public Expression Operand1 { get; set; }
        public Expression Operand2 { get; set; }
        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class IntegerConstant : Expression
    {
        public int Constant { get; set; }
        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class GetElementPtr : Expression
    {
        public TypeDeclaration PtrType { get; set; }
        public string PtrVar { get; set; }
        public List<Expression> Indices { get; set; } = new List<Expression>();
        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class VariableReference : Expression
    {
        public string Variable { get; set; }
        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public abstract class Statement : AstBase
    {
        public string Label { get; set; }
    }

    public class ExpressionStatement : Statement
    {
        public Expression Expression { get; set; }
        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class TypeDeclaration : AstBase
    {
        public Token Type { get; set; }
        public bool IsPointer { get; set; }
        public bool IsArray { get; set; }
        public int ArrayX { get; set; }
        public int PointerDepth { get; set; }

        public int ElementWidth
        {
            get
            {
                switch (this.Type)
                {
                    case Token.I32:
                        return 4;
                    case Token.I8:
                        return 1;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class RetStatement : Statement
    {
        public TypeDeclaration Type { get; set; }
        //public string Value { get; set; }
        public Expression Value { get; set; }
        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class IcmpExpression : Expression
    {
        public Token Condition { get; set; }
        public string Var { get; set; }
        public Expression Value { get; set; }
        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class LabelBrStatement : Statement
    {
        public string TargetLabel { get; set; }
        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class ConditionalBrStatement : Statement
    {
        public TypeDeclaration Type { get; set; }
        public string Identifier { get; set; }
        public string Label1 { get; set; }
        public string Label2 { get; set; }
        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class LoadExpression : Expression
    {
        public Expression Value { get; set; }
        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class PhiValue
    {
        public Expression Expr { get; set; }
        public string Label { get; set; }
    }

    public class PhiExpression : Expression
    {
        public IList<PhiValue> Values { get; set; } = new List<PhiValue>();
        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class StoreStatement : Statement
    {
        public TypeDeclaration ExprType { get; set; }
        public Expression Value { get; set; }
        public TypeDeclaration Type { get; set; }
        public string Variable { get; set; }
        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class VariableAssignmentStatement : Statement
    {
        public string Variable { get; set; }
        public Expression Expr { get; set; }
        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class FunctionParameter
    {
        public TypeDeclaration Type { get; set; }
        public string Name { get; set; }
    }

    public class FunctionDefinition : AstBase
    {
        public string Name { get; set; }
        public TypeDeclaration ReturnType { get; set; }
        public List<Statement> Statements { get; set; } = new List<Statement>();
        public List<FunctionParameter> Parameters { get; set; } = new List<FunctionParameter>();

        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class Declaration : AstBase
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class Program : AstBase
    {
        public List<FunctionDefinition> Functions { get; set; } = new List<FunctionDefinition>();
        public List<Declaration> Declarations { get; set; } = new List<Declaration>();
        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

}