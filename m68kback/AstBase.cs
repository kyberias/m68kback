using System;
using System.Collections.Generic;
using System.Linq;

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
        public TypeDeclaration To { get; set; }
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

    public class BooleanConstant : Expression
    {
        public bool Constant { get; set; }
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

    public abstract class TypeDeclaration : AstBase
    {
        public abstract int Width { get; }

        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class DefinedTypeDeclaration : TypeDeclaration
    {
        public DefinedTypeDeclaration(TypeDefinition typeDef)
        {
            Type = typeDef;
        }
        public TypeDefinition Type { get; set; }
        public override int Width => Type.Width;
    }

    public abstract class IndirectTypeDeclaration : TypeDeclaration
    {
        public TypeDeclaration BaseType { get; set; }
    }

    public class PointerDeclaration : IndirectTypeDeclaration
    {
        public override int Width => 4;
    }

    public class ArrayDeclaration : IndirectTypeDeclaration
    {
        public override int Width => BaseType.Width * ArrayX;
        public int ArrayX { get; set; }
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

    public abstract class TypeDefinition : AstBase
    {
        public string Name { get; set; }

        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }

        public abstract int Width { get; }
    }

    public class InternalTypeDefinition : TypeDefinition
    {
        public Token Type { get; set; }

        public override int Width
        {
            get
            {
                switch (Type)
                {
                    case Token.I32:
                        return 4;
                    case Token.I8:
                        return 1;
                    case Token.Void:
                        return 0;
                    default:
                        throw new NotSupportedException();
                }
            }
        }
    }

    public class UserTypeDefinition : TypeDefinition
    {
        public List<TypeDeclaration> Members { get; } = new List<TypeDeclaration>();

        public override int Width
        {
            get { return Members.Sum(m => m.Width); }
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