namespace m68kback
{
    public interface IVisitor
    {
        object Visit(Program el);
        object Visit(FunctionDefinition el);
        object Visit(AllocaExpression allocaExpression);
        object Visit(CallExpression callExpression);
        object Visit(ArithmeticExpression arithmeticExpression);
        object Visit(IntegerConstant integerConstant);
        object Visit(GetElementPtr getElementPtr);
        object Visit(VariableReference variableReference);
        object Visit(ExpressionStatement expressionStatement);
        object Visit(TypeDeclaration typeDeclaration);
        object Visit(RetStatement retStatement);
        object Visit(IcmpExpression icmpExpression);
        object Visit(LabelBrStatement labelBrStatement);
        object Visit(ConditionalBrStatement conditionalBrStatement);
        object Visit(LoadExpression loadExpression);
        object Visit(StoreStatement storeStatement);
        object Visit(VariableAssignmentStatement variableAssignmentStatement);
        object Visit(Declaration declaration);
        object Visit(PhiExpression loadExpression);
        object Visit(TypeDefinition typeDef);
    }
}
