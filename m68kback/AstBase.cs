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
        public TypeReference Type { get; set; }
    }

    public class StructField
    {
        public TypeReference Type { get; set; }
        public Expression Value { get; set; }
        public bool InitializeToZero { get; set; }
    }

    public class StructExpression : Expression
    {
        public IList<StructField> Values { get; set; }
        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class ArrayExpression : Expression
    {
        public IList<Expression> Values { get; set; }
        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class AllocaExpression : Expression
    {
        public int Alignment { get; set; }
        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class CastExpression : Expression
    {
        public Token CastType { get; set; }
        public Expression Value { get; set; }
        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class CallExpression : Expression
    {
        public class CallParameter
        {
            public TypeReference Type { get; set; }
            public Expression Expression { get; set; }
            public bool ByVal { get; set; }
            public int? Align { get; set; }
        }

        public string FunctionName { get; set; }
        public string VariableName { get; set; }
        public List<CallParameter> Parameters { get; set; } = new List<CallParameter>();
        public bool ZeroExtended { get; set; }
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
        public TypeReference To { get; set; }
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
        public TypeReference PtrType { get; set; }
        //public string PtrVar { get; set; }
        public Expression PtrVal { get; set; }
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

    public class SelectExpression : Expression
    {
        public Expression Expr { get; set; }
        public Expression TrueExpression { get; set; }
        public Expression FalseExpression { get; set; }
        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public abstract class Statement : AstBase
    {
        public string Label { get; set; }
    }

    public class Unreachable : Statement
    {
        public override object Visit(IVisitor visitor)
        {
            return null;
        }
    }

    public class ExpressionStatement : Statement
    {
        public Expression Expression { get; set; }
        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public abstract class TypeReference : AstBase
    {
        public abstract int Width { get; }

        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class DefinedTypeReference : TypeReference
    {
        public DefinedTypeReference(string name)
        {
            Name = name;
        }

        public DefinedTypeReference(TypeDefinition typeDef)
        {
            Type = typeDef;
        }
        public string Name { get; set; }
        public TypeDefinition Type { get; set; }
        public override int Width => Type.Width;
    }

    public class FunctionTypeReference : TypeReference
    {
        public override int Width
        {
            get
            {
                throw new Exception("Function type doesn't have width");
            }
        }

        public TypeReference ReturnValue { get; set; }

        public List<TypeReference> Parameters { get; set; }  = new List<TypeReference>();
    }

    public abstract class IndirectTypeReference : TypeReference
    {
        public TypeReference BaseType { get; set; }
    }

    public class PointerReference : IndirectTypeReference
    {
        public override int Width => 4;
    }

    public class ArrayReference : IndirectTypeReference
    {
        public override int Width => BaseType.Width * ArrayX;
        public int ArrayX { get; set; }
    }

    public class RetStatement : Statement
    {
        public TypeReference Type { get; set; }
        //public string Value { get; set; }
        public Expression Value { get; set; }
        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class SwitchCase
    {
        public Expression Value { get; set; }
        public string Label { get; set; }
    }

    public class SwitchStatement : Statement
    {
        public TypeReference Type { get; set; }
        public Expression Value { get; set; }
        public string DefaultLabel { get; set; }

        public IList<SwitchCase> Switches { get; set; } = new List<SwitchCase>();

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
        public TypeReference Type { get; set; }
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
        public TypeReference ExprType { get; set; }
        public Expression Value { get; set; }
        public TypeReference Type { get; set; }
        public Expression PtrExpr { get; set; }

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

    public class OpaqueTypeDefinition : TypeDefinition
    {
        public override int Width
        {
            get
            {
                throw new Exception("Opaque type doesn't have width");
            }
        }
    }

    /// <summary>
    /// The type that defines the signature of the function
    /// </summary>
/*    public class FunctionTypeDefinition : TypeDefinition
    {
        public override int Width
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public TypeDefinition ReturnType { get; set; }
        public List<TypeDefinition> Parameters { get; set; }
    }*/

    public class InternalTypeDefinition : TypeDefinition
    {
        public Token Type { get; set; }

        public override int Width
        {
            get
            {
                switch (Type)
                {
                    case Token.I64:
                        return 8;
                    case Token.I32:
                        return 4;
                    case Token.I16:
                        return 2;
                    case Token.I8:
                        return 1;
                    case Token.I1:
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
        public List<TypeReference> Members { get; } = new List<TypeReference>();

        public override int Width
        {
            get { return Members.Sum(m => m.Width); }
        }
    }

    public class FunctionParameter
    {
        public TypeReference Type { get; set; }
        public string Name { get; set; }
    }

    public interface INamed
    {
        string Name { get; set; }
    }

    public class FunctionDefinition : AstBase, INamed
    {
        public string Name { get; set; }
        public TypeReference ReturnType { get; set; }
        public List<Statement> Statements { get; set; } = new List<Statement>();
        public List<FunctionParameter> Parameters { get; set; } = new List<FunctionParameter>();
        public bool VariableNumParameters { get; set; }
        public bool Internal { get; set; }

        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    /// <summary>
    /// For example, function forware declaration
    /// </summary>
    public class Declaration : AstBase, INamed
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public Expression Expr { get; set; }
        public TypeReference Type { get; set; }
        public bool InitializeToZero { get; set; }
        public bool Declare { get; set; }
        public bool Global { get; set; }
        public bool Constant { get; set; }
        public bool External { get; set; }
        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class Program : AstBase
    {
        public List<FunctionDefinition> Functions { get; set; } = new List<FunctionDefinition>();
        public List<Declaration> Declarations { get; set; } = new List<Declaration>();
        public List<TypeDefinition> TypeDefinitions { get; set; } = new List<TypeDefinition>();

        public override object Visit(IVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }

}