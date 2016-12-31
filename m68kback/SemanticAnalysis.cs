using System.Collections.Generic;

namespace m68kback
{
    public class SemanticAnalysis : IVisitor
    {
        public object Visit(Program el)
        {
            foreach (var t in el.TypeDefinitions)
            {
                t.Visit(this);
            }

            foreach (var t in el.TypeDefinitions)
            {
                if (t is UserTypeDefinition)
                {
                    var utd = t as UserTypeDefinition;
                    foreach (var member in utd.Members)
                    {
                        var dt = member as DefinedTypeReference;
                        if (dt != null)
                        {
                            if (dt.Type == null)
                            {
                                dt.Type = typeDefinitions[dt.Name];
                            }
                        }
                    }
                }
            }

            foreach (var f in el.Functions)
            {
                f.Visit(this);
            }

            foreach (var mt in fieldsMissingType)
            {
                var ttd = mt.Type as DefinedTypeReference;
                if (ttd != null)
                {
                    //ttd.Type = declarations[ttd.Name].;
                }
            }

            return null;
        }

        public object Visit(FunctionDefinition el)
        {
            foreach (var stmt in el.Statements)
            {
                stmt.Visit(this);
            }
            return null;
        }

        List<StructField> fieldsMissingType = new List<StructField>();

        public object Visit(StructExpression expr)
        {
            foreach (var v in expr.Values)
            {
                if (v.Type == null)
                {
                    fieldsMissingType.Add(v);
                }
            }
            return null;
        }

        public object Visit(AllocaExpression allocaExpression)
        {
            return null;
        }

        public object Visit(CastExpression expression)
        {
            return null;
        }

        public object Visit(CallExpression callExpression)
        {
            return null;
        }

        public object Visit(ArithmeticExpression arithmeticExpression)
        {
            return null;
        }

        public object Visit(SelectExpression expr)
        {
            return null;
        }

        public object Visit(IntegerConstant integerConstant)
        {
            return null;
        }

        public object Visit(BooleanConstant constant)
        {
            return null;
        }

        public object Visit(GetElementPtr getElementPtr)
        {
            return null;
        }

        public object Visit(VariableReference variableReference)
        {
            return null;
        }

        public object Visit(ExpressionStatement expressionStatement)
        {
            return null;
        }

        public object Visit(TypeReference typeReference)
        {
            return null;
        }

        public object Visit(RetStatement retStatement)
        {
            return null;
        }

        public object Visit(SwitchStatement statement)
        {
            return null;
        }

        public object Visit(IcmpExpression icmpExpression)
        {
            return null;
        }

        public object Visit(LabelBrStatement labelBrStatement)
        {
            return null;
        }

        public object Visit(ConditionalBrStatement conditionalBrStatement)
        {
            return null;
        }

        public object Visit(LoadExpression loadExpression)
        {
            return loadExpression.Value.Visit(this);
        }

        public object Visit(StoreStatement storeStatement)
        {
            return null;
        }

        //Dictionary<string,Register> varRegs = new Dictionary<string, Register>();

        public object Visit(VariableAssignmentStatement variableAssignmentStatement)
        {
            return variableAssignmentStatement.Expr.Visit(this);

            /*if (variableAssignmentStatement.Expr.Type is PointerReference)
            {
                varRegs[variableAssignmentStatement.Variable] = NewAddress
            }

            return null;*/
        }

        Dictionary<string,Declaration> declarations = new Dictionary<string, Declaration>();

        public object Visit(Declaration declaration)
        {
            declarations[declaration.Name] = declaration;

            return null;
        }

        public object Visit(PhiExpression loadExpression)
        {
            return null;
        }

        Dictionary<string, TypeDefinition> typeDefinitions = new Dictionary<string, TypeDefinition>();

        public object Visit(TypeDefinition typeDef)
        {
            typeDefinitions[typeDef.Name] = typeDef;
            return null;
        }
    }
}