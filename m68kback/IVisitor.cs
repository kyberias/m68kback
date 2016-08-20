namespace m68kback
{
    public interface IVisitor
    {
        void Visit(Program el);
        void Visit(FunctionDefinition el);
        void Visit(AllocaExpression allocaExpression);
        void Visit(CallExpression callExpression);
        void Visit(ArithmeticExpression arithmeticExpression);
        void Visit(IntegerConstant integerConstant);
        void Visit(GetElementPtr getElementPtr);
        void Visit(VariableReference variableReference);
        void Visit(ExpressionStatement expressionStatement);
        void Visit(TypeDeclaration typeDeclaration);
        void Visit(RetStatement retStatement);
        void Visit(IcmpExpression icmpExpression);
        void Visit(LabelBrStatement labelBrStatement);
        void Visit(ConditionalBrStatement conditionalBrStatement);
        void Visit(LoadExpression loadExpression);
        void Visit(StoreStatement storeStatement);
        void Visit(VariableAssignmentStatement variableAssignmentStatement);
        void Visit(Declaration declaration);
    }
}
