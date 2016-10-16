using System;
using System.Collections.Generic;
//using System.Data;
using System.Linq;

namespace m68kback
{
    public class Parser
    {
        private List<TokenElement> lexicalElements;
        private int cursor;

        public Parser(IEnumerable<TokenElement> lexicalElements)
        {
            this.lexicalElements = lexicalElements.ToList();

            internalTypes[Token.I1] = new InternalTypeDefinition { Name = "i1", Type = Token.I1 };
            internalTypes[Token.I8] = new InternalTypeDefinition {Name = "i8", Type = Token.I8};
            internalTypes[Token.I32] = new InternalTypeDefinition { Name = "i32", Type = Token.I32 };
            internalTypes[Token.Void] = new InternalTypeDefinition { Name = "void", Type = Token.Void };

            cursor = 0;
        }

        public void ParseTarget()
        {
            AcceptElement(Token.Target);
            AcceptElement(Token.Datalayout, Token.Triple);
            AcceptElement(Token.Assign);
            AcceptElement(Token.StringLiteral);
        }

        public void ParseComdat()
        {
            AcceptElement(Token.Dollar);
            AcceptElement(Token.StringLiteral);
            AcceptElement(Token.Assign);
            AcceptElement(Token.Comdat);
            AcceptElement(Token.Any);
        }

        void ParseAttributes()
        {
            AcceptElement(Token.Attributes);
            AcceptElement(Token.Hash);
            AcceptElement(Token.IntegerLiteral);
            AcceptElement(Token.Assign);
            AcceptElement(Token.CurlyBraceOpen);
            while (PeekElement().Type != Token.CurlyBraceClose)
            {
                AcceptElement(PeekElement().Type);
            }
            AcceptElement(Token.CurlyBraceClose);
        }

        void ParseMetadata()
        {
            AcceptElement(Token.Exclamation);
            AcceptElement(Token.IntegerLiteral, Token.Symbol);
            AcceptElement(Token.Assign);
            AcceptElement(Token.Exclamation);
            while (PeekElement().Type != Token.CurlyBraceClose)
            {
                AcceptElement(PeekElement().Type);
            }
            AcceptElement(Token.CurlyBraceClose);
        }

        public TypeDefinition ParseTypeDefinition()
        {
            var typeDef = new UserTypeDefinition();
            var id = AcceptElement(Token.LocalIdentifier);

            typeDef.Name = id.Data;

            AcceptElement(Token.Assign);
            AcceptElement(Token.Type);
            AcceptElement(Token.CurlyBraceOpen);

            while (PeekElement().Type != Token.CurlyBraceClose)
            {
                var t = ParseType();
                typeDef.Members.Add(t);
                if (PeekElement().Type != Token.CurlyBraceClose)
                {
                    AcceptElement(Token.Comma);
                }
            }

            AcceptElement(Token.CurlyBraceClose);

            types[typeDef.Name] = typeDef;

            return typeDef;
        }

        public Program ParseProgram()
        {
            var program = new Program();

            while (PeekElement() != null)
            {
                switch (PeekElement().Type)
                {
                    case Token.LocalIdentifier:
                        ParseTypeDefinition();
                        break;
                    case Token.Exclamation:
                        ParseMetadata();
                        break;
                    case Token.Attributes:
                        ParseAttributes();
                        break;
                    case Token.Target:
                        ParseTarget();
                        break;
                    case Token.Dollar:
                        ParseComdat();
                        break;
                    case Token.GlobalIdentifier:
                    case Token.Declare:
                        program.Declarations.Add(ParseDeclaration());
                        break;
                    case Token.Define:
                        program.Functions.Add(ParseFunctionDefinition());
                        break;
                    default:
                        throw new NotImplementedException(PeekElement().Type.ToString());
                }
            }
            return program;
        }

        FunctionDefinition ParseFunctionDefinition()
        {
            var func = new FunctionDefinition();
            AcceptElement(Token.Define);

            func.ReturnType = ParseType();

            var name = AcceptElement(Token.GlobalIdentifier);
            func.Name = name.Data;

            AcceptElement(Token.ParenOpen);

            if (PeekElement().Type != Token.ParenClose)
            {
                while (true)
                {
                    var par = new FunctionParameter();
                    par.Type = ParseType();

                    AcceptElementIfNext(Token.NoCapture);
                    AcceptElementIfNext(Token.ReadOnly);

                    par.Name = AcceptElement(Token.LocalIdentifier).Data;
                    func.Parameters.Add(par);

                    if (PeekElement().Type == Token.ParenClose)
                    {
                        break;
                    }

                    AcceptElement(Token.Comma);
                }
            }

            AcceptElement(Token.ParenClose);

            if (PeekElement().Type == Token.Hash)
            {
                AcceptElement(Token.Hash);
                AcceptElement(Token.IntegerLiteral);
            }

            AcceptElement(Token.CurlyBraceOpen);

            while (PeekElement().Type != Token.CurlyBraceClose)
            {
                func.Statements.Add(ParseStatement());
            }

            AcceptElement(Token.CurlyBraceClose);

            return func;
        }

        Dictionary<Token, TypeDefinition> internalTypes = new Dictionary<Token, TypeDefinition>();
        Dictionary<string, TypeDefinition> types = new Dictionary<string, TypeDefinition>();

        TypeDeclaration ParseType()
        {
            TypeDeclaration decl;// = new TypeDeclaration();
            var el = PeekElement();
            switch (el.Type)
            {
                case Token.I64:
                case Token.I32:
                case Token.I8:
                case Token.I1:
                case Token.Void:
                    decl = new DefinedTypeDeclaration(internalTypes[el.Type]);
                    AcceptElement(el.Type);
                    break;
                case Token.BracketOpen:
                    {
                        AcceptElement(Token.BracketOpen);
                        var arr = new ArrayDeclaration();
                        arr.ArrayX = int.Parse(AcceptElement(Token.IntegerLiteral).Data);
                        AcceptElement(Token.Symbol);
                        arr.BaseType = 
                            new DefinedTypeDeclaration(internalTypes[AcceptElement(Token.I32, Token.I8).Type]);
                        AcceptElement(Token.BracketClose);
                        decl = arr;
                    }
                    break;
                case Token.LocalIdentifier:
                    {
                        var id = AcceptElement(Token.LocalIdentifier);
                        decl = new DefinedTypeDeclaration(types[id.Data]);
                    }
                    break;
                default:
                    throw new Exception();
            }

            while (PeekElement().Type == Token.Asterisk)
            {
                AcceptElement(Token.Asterisk);
                var ptr = new PointerDeclaration();
                ptr.BaseType = decl;
                decl = ptr;
            }

            if (PeekElement().Type == Token.ParenOpen)
            {
                AcceptElement(Token.ParenOpen);
                // Parameter list
                while (true)
                {
                    if (PeekElement().Type == Token.Ellipsis)
                    {
                        AcceptElement(Token.Ellipsis);
                        break;
                    }

                    ParseType();
                    if (PeekElement().Type == Token.ParenClose)
                    {
                        break;
                    }
                    AcceptElement(Token.Comma);
                }

                AcceptElement(Token.ParenClose);
            }

            AcceptElementIfNext(Token.Asterisk);

            return decl;
        }

        StoreStatement ParseStoreInstruction()
        {
            var expr = new StoreStatement();
            AcceptElement(Token.Store);

            expr.ExprType = ParseType();
            expr.Value = ParseExpression();
            AcceptElement(Token.Comma);

            expr.Type = ParseType();
            expr.Variable = AcceptElement(Token.LocalIdentifier).Data;

            if (PeekElement().Type == Token.Comma)
            {
                AcceptElement(Token.Comma);
                AcceptElement(Token.Align);
                AcceptElement(Token.IntegerLiteral);
            }

            if (PeekElement().Type == Token.Comma)
            {
                AcceptElement(Token.Comma);
                AcceptElement(Token.Exclamation);
                AcceptElement(Token.Symbol);
                AcceptElement(Token.Exclamation);
                AcceptElement(Token.IntegerLiteral);
            }

            return expr;
        }

        LoadExpression ParseLoadExpression()
        {
            var expr = new LoadExpression();
            AcceptElement(Token.Load);

            expr.Type = ParseType();
            AcceptElement(Token.Comma);
            ParseType();

            expr.Value = ParseExpression();

            if (PeekElement().Type == Token.Comma)
            {
                AcceptElement(Token.Comma);
                AcceptElement(Token.Align);
                AcceptElement(Token.IntegerLiteral);
            }

            if (PeekElement().Type == Token.Comma)
            {
                AcceptElement(Token.Comma);
                AcceptElement(Token.Exclamation);
                AcceptElement(Token.Symbol);
                AcceptElement(Token.Exclamation);
                AcceptElement(Token.IntegerLiteral);
            }

            return expr;
        }

        AllocaExpression ParseAllocaExpression()
        {
            var expr = new AllocaExpression();
            AcceptElement(Token.Alloca);

            expr.Type = ParseType();
            AcceptElement(Token.Comma);
            AcceptElement(Token.Align);
            expr.Alignment = int.Parse(AcceptElement(Token.IntegerLiteral).Data);

            return expr;
        }

        class BitCast
        {
            public string Name { get; set; }
        }

        BitCast ParseBitcast()
        {
            AcceptElement(Token.Bitcast);
            AcceptElement(Token.ParenOpen);
            ParseType();

            var bc = new BitCast();

            bc.Name = AcceptElement(Token.LocalIdentifier, Token.GlobalIdentifier).Data;
            AcceptElement(Token.To);

            ParseType();

            AcceptElement(Token.ParenClose);
            return bc;
        }

        // <result> = [tail | musttail | notail ] 
        //            call [fast-math flags] [cconv] [ret attrs] 
        //                 <ty>|<fnty> <fnptrval>(<function args>) [fn attrs] [operand bundles]
        //[TestCase("define i32 @main() #0 { %call5 = call i32 (i8*, ...) @printf(i8* getelementptr inbounds ([13 x i8], [13 x i8]* @\"\\01??_C@_0N@GIINEEDM@hello?5world?6?$AA@\", i32 0, i32 0))\n }")]
        //[TestCase("define i32 @main() #0 { %call1 = call i32 bitcast (i32 (...)* @atoi to i32 (i8*)*)(i8* %3) }")]

        CallExpression ParseCallExpression()
        {
            var expr = new CallExpression();
            expr.Parameters = new List<Expression>();

            AcceptElementIfNext(Token.Tail);
            AcceptElement(Token.Call);

            expr.Type = ParseType();

            if (PeekElement().Type == Token.Bitcast)
            {
                expr.FunctionName = ParseBitcast().Name;
            }
            else
            {
                expr.FunctionName = AcceptElement(Token.GlobalIdentifier).Data;
            }

            AcceptElement(Token.ParenOpen);

            if (PeekElement().Type != Token.ParenClose)
            {
                while (true)
                {
                    ParseType();

                    expr.Parameters.Add(ParseExpression());

                    if (PeekElement().Type == Token.Comma)
                    {
                        AcceptElement(Token.Comma);
                    }

                    if (PeekElement().Type == Token.ParenClose)
                    {
                        break;
                    }
                }
            }
            AcceptElement(Token.ParenClose);

            if (AcceptElementIfNext(Token.Hash))
            {
                AcceptElement(Token.IntegerLiteral);
            }
            return expr;
        }

        GetElementPtr ParseGetElementPtr()
        {
            var ptr = new GetElementPtr();
            AcceptElement(Token.GetElementPtr);
            AcceptElementIfNext(Token.Inbounds);

            var parens = AcceptElementIfNext(Token.ParenOpen);

            ptr.PtrType = ParseType();
            AcceptElement(Token.Comma);
            ptr.Type = ParseType();
            ptr.PtrVar = AcceptElement(Token.LocalIdentifier, Token.GlobalIdentifier).Data;

            AcceptElement(Token.Comma);
            while (true)
            {
                AcceptElement(Token.I32);
                ptr.Indices.Add(ParseExpression());

                if (PeekElement().Type == Token.Comma)
                {
                    AcceptElement(Token.Comma);
                }
                else
                {
                    if (parens)
                    {
                        AcceptElement(Token.ParenClose);
                    }
                    break;
                }
            }
            return ptr;
        }

        ArithmeticExpression ParseArithmeticExpression()
        {
            var expr = new ArithmeticExpression();
            expr.Operator = AcceptElement(Token.Mul, Token.Add, Token.Sub, Token.Srem, Token.Xor, Token.Zext).Type;

            if (AcceptElementIfNext(Token.Nuw))
            {
                expr.NoUnsignedWrap = true;
            }
            if (AcceptElementIfNext(Token.Nsw))
            {
                expr.NoSignedWrap = true;
            }

            expr.Type = ParseType();
            expr.Operand1 = ParseExpression();

            if (PeekElement().Type == Token.Comma)
            {
                AcceptElement(Token.Comma);
                expr.Operand2 = ParseExpression();
            }
            else
            {
                AcceptElement(Token.To);
                expr.To = ParseType();
            }
            return expr;
        }

        PhiExpression ParsePhi()
        {
            var phi = new PhiExpression();
            AcceptElement(Token.Phi);
            phi.Type = ParseType();


            while (true)
            {
                AcceptElement(Token.BracketOpen);

                var expr = ParseExpression();
                AcceptElement(Token.Comma);
                var label = AcceptElement(Token.LocalIdentifier);

                phi.Values.Add( new PhiValue { Expr = expr, Label = label.Data});

                AcceptElement(Token.BracketClose);

                if (!AcceptElementIfNext(Token.Comma))
                {
                    break;
                }
            }
            return phi;
        }

        Expression ParseExpression()
        {
            switch (PeekElement().Type)
            {
                case Token.Alloca:
                    return ParseAllocaExpression();
                case Token.Tail:
                case Token.Call:
                    return ParseCallExpression();
                case Token.Icmp:
                    return ParseIcmpExpression();
                case Token.Mul:
                case Token.Add:
                case Token.Sub:
                case Token.Xor:
                case Token.Srem:
                case Token.Zext:
                    return ParseArithmeticExpression();
                case Token.GetElementPtr:
                    return ParseGetElementPtr();
                case Token.Load:
                    return ParseLoadExpression();
                case Token.Phi:
                    return ParsePhi();
            }

            //var type = ParseType();
            var peek = PeekElement().Type;

            switch (peek)
            {
                case Token.IntegerLiteral:
                case Token.Minus:
                    {
                        bool minus = AcceptElementIfNext(Token.Minus);

                        return new IntegerConstant
                        {
                            Constant = int.Parse(AcceptElement(Token.IntegerLiteral).Data) * (minus ? -1 : 1),
                        };
                    }
                case Token.True:
                case Token.False:
                    AcceptElement(peek);
                    return new BooleanConstant()
                    {
                        Constant = peek == Token.True
                    };
                default:
                    return new VariableReference
                    {
                        Variable = AcceptElement(Token.LocalIdentifier).Data
                    };

            }
        }

        VariableAssignmentStatement ParseAssignmentStatement()
        {
            var stmt = new VariableAssignmentStatement();
            stmt.Variable = AcceptElement(Token.LocalIdentifier).Data;
            AcceptElement(Token.Assign);
            stmt.Expr = ParseExpression();
            return stmt;
        }

        RetStatement ParseRetStatement()
        {
            var stmt = new RetStatement();
            AcceptElement(Token.Ret);
            stmt.Type = ParseType();

            var type = stmt.Type as DefinedTypeDeclaration;
            var intDef = type?.Type as InternalTypeDefinition;

            if (intDef == null || intDef.Type != Token.Void)
            {
                stmt.Value = ParseExpression();
            }
            /*switch (stmt.Type.Type)
            {
                case Token.I32:
                    stmt.Value = AcceptElement(Token.IntegerLiteral).Data;
                    break;
                default:
                    throw new NotImplementedException();
            }*/
            return stmt;
        }

        IcmpExpression ParseIcmpExpression()
        {
            var stmt = new IcmpExpression();
            AcceptElement(Token.Icmp);
            stmt.Condition = AcceptElement(Token.Slt, Token.Sgt, Token.Eq, Token.Ne).Type;
            stmt.Type = ParseType();

            stmt.Var = AcceptElement(Token.GlobalIdentifier, Token.LocalIdentifier).Data;
            AcceptElement(Token.Comma);
            stmt.Value = ParseExpression();
            return stmt;
        }

        Statement ParseBrInstruction()
        {
            AcceptElement(Token.Br);
            if (AcceptElementIfNext(Token.Label))
            {
                return new LabelBrStatement
                {
                    TargetLabel = AcceptElement(Token.LocalIdentifier).Data
                };
            }
            else
            {
                var stmt = new ConditionalBrStatement();
                stmt.Type = ParseType();
                stmt.Identifier = AcceptElement(Token.LocalIdentifier).Data;
                AcceptElement(Token.Comma);
                AcceptElement(Token.Label);
                stmt.Label1 = AcceptElement(Token.LocalIdentifier).Data;
                AcceptElement(Token.Comma);
                AcceptElement(Token.Label);
                stmt.Label2 = AcceptElement(Token.LocalIdentifier).Data;
                return stmt;
            }
        }

        Statement ParseStatement()
        {
            string label = null;
            if (PeekElement().Type == Token.Symbol)
            {
                label = AcceptElement(Token.Symbol).Data;
                AcceptElement(Token.Colon);
            }

            Statement stmt;
            var next = PeekElement().Type;
            switch (next)
            {
                case Token.LocalIdentifier:
                    stmt = ParseAssignmentStatement();
                    break;
                case Token.Ret:
                    stmt = ParseRetStatement();
                    break;
                case Token.Store:
                    stmt = ParseStoreInstruction();
                    break;
                case Token.Br:
                    stmt = ParseBrInstruction();
                    break;
                case Token.Tail:
                case Token.Call:
                    stmt = new ExpressionStatement
                    {
                        Expression = ParseExpression()
                    };
                    break;
                default:
                    throw new NotImplementedException(next.ToString());
            }

            stmt.Label = label;
            return stmt;
        }

        Declaration ParseDeclaration()
        {
            var decl = new Declaration();

            if (PeekElement().Type == Token.GlobalIdentifier)
            {
                decl.Name = AcceptElement(Token.GlobalIdentifier).Data;

                AcceptElement(Token.Assign);
                while (PeekElement().Type == Token.Symbol)
                {
                    AcceptElement(Token.Symbol);
                }

                if (PeekElement().Type == Token.Constant)
                {
                    AcceptElement(Token.Constant);
                }

                if (PeekElement().Type == Token.BracketOpen)
                {
                    AcceptElement(Token.BracketOpen);
                    while (PeekElement().Type != Token.BracketClose)
                        AcceptElement(PeekElement().Type);
                    AcceptElement(Token.BracketClose);
                }

                decl.Value = AcceptElement(Token.StringLiteral).Data;
                AcceptElement(Token.Comma);

                if(PeekElement().Type == Token.Comdat)
                {
                    AcceptElement(Token.Comdat);
                    AcceptElement(Token.Comma);
                }

                AcceptElement(Token.Align);
                AcceptElement(Token.IntegerLiteral);
            }
            else if (PeekElement().Type == Token.Declare)
            {
                // declare i32 @printf(i8*, ...) #1

                AcceptElement(Token.Declare);
                ParseType();

                decl.Name = AcceptElement(Token.GlobalIdentifier).Data;
                AcceptElement(Token.ParenOpen);

                if (!AcceptElementIfNext(Token.Ellipsis))
                {
                    if (PeekElement().Type != Token.ParenClose)
                    {
                        ParseType();
                        AcceptElementIfNext(Token.NoCapture);
                        AcceptElementIfNext(Token.ReadOnly);

                        while (AcceptElementIfNext(Token.Comma))
                        {
                            if (AcceptElementIfNext(Token.Ellipsis))
                            {
                                break;
                            }
                            ParseType();
                            AcceptElementIfNext(Token.NoCapture);
                            AcceptElementIfNext(Token.ReadOnly);
                        }
                    }
                }

                AcceptElement(Token.ParenClose);

                if (AcceptElementIfNext(Token.Hash))
                {
                    AcceptElement(Token.IntegerLiteral);
                }
            }
            return decl;
        }

        TokenElement PeekElement()
        {
            if (lexicalElements.Count > cursor)
            {
                return lexicalElements[cursor];
            }
            return null;
        }

        bool AcceptElementIfNext(Token type)
        {
            var el = PeekElement();
            if (el.Type == type)
            {
                AcceptElement(type);
                return true;
            }
            return false;
        }

        TokenElement AcceptElement(params Token[] types)
        {
            while (true)
            {
                var el = PeekElement();

                if (el == null)
                {
                    /*Errors.Add(new ParserError
                    {
                        Type = ParserErrorType.UnexpectedEndOfFile,
                        Column = el.Column,
                        Line = el.Line
                    });*/
                    return null;
                }

                if (types.Contains(el.Type))
                {
//                    Console.WriteLine($"Accept {el.Type} {el.Data}");
                    cursor++;
                    return el;
                }

                throw new Exception("Expected " + types[0] + ". Unexpected token " + el.Type + " " + (el.Data ?? ""));

                //                if (Errors.Count == 0 || Errors.Last().Type != ParserErrorType.UnexpectedToken)
                {
                    /*Errors.Add(new ParserError
                    {
                        Type = ParserErrorType.UnexpectedToken,
                        Column = el.Column,
                        Line = el.Line
                    });*/
                    //cursor++;
                    //return new TokenElement(type, el.Data);
                }
            }
        }
    }
}
