﻿using System;
using System.Collections.Generic;
using System.Data;
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
            cursor = 0;
        }

        public Program ParseProgram()
        {
            var program = new Program();

            while (PeekElement() != null)
            {
                switch (PeekElement().Type)
                {
                    case Token.Target:
                        break;
                    case Token.Dollar:
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

        TypeDeclaration ParseType()
        {
            var decl = new TypeDeclaration();
            var el = PeekElement();
            switch (el.Type)
            {
                case Token.I32:
                case Token.I8:
                case Token.I1:
                case Token.Void:
                    decl.Type = el.Type;
                    AcceptElement(el.Type);
                    break;
                case Token.BracketOpen:
                    {
                        AcceptElement(Token.BracketOpen);
                        decl.IsArray = true;
                        decl.ArrayX = int.Parse(AcceptElement(Token.IntegerLiteral).Data);
                        AcceptElement(Token.Symbol);
                        decl.Type = AcceptElement(Token.I32, Token.I8).Type;
                        AcceptElement(Token.BracketClose);
                    }
                    break;
                default:
                    throw new SyntaxErrorException();
            }

            while (PeekElement().Type == Token.Asterisk)
            {
                AcceptElement(Token.Asterisk);
                decl.IsPointer = true;
                decl.PointerDepth++;
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
            return expr;
        }

        GetElementPtr ParseGetElementPtr()
        {
            var ptr = new GetElementPtr();
            AcceptElement(Token.GetElementPtr);
            AcceptElementIfNext(Token.Inbounds);

            var parents = AcceptElementIfNext(Token.ParenOpen);

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
                    if (parents)
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
            expr.Operator = AcceptElement(Token.Mul, Token.Add, Token.Sub, Token.Srem).Type;

            if (PeekElement().Type == Token.Nsw)
            {
                expr.Wrap = AcceptElement(Token.Nsw).Type;
            }

            expr.Type = ParseType();
            expr.Operand1 = ParseExpression();
            AcceptElement(Token.Comma);
            expr.Operand2 = ParseExpression();
            return expr;
        }

        Expression ParseExpression()
        {
            switch (PeekElement().Type)
            {
                case Token.Alloca:
                    return ParseAllocaExpression();
                case Token.Call:
                    return ParseCallExpression();
                case Token.Icmp:
                    return ParseIcmpExpression();
                case Token.Mul:
                case Token.Add:
                case Token.Sub:
                case Token.Srem:
                    return ParseArithmeticExpression();
                case Token.GetElementPtr:
                    return ParseGetElementPtr();
                case Token.Load:
                    return ParseLoadExpression();
            }

            //var type = ParseType();
            if (PeekElement().Type == Token.IntegerLiteral || PeekElement().Type == Token.Minus)
            {
                bool minus = AcceptElementIfNext(Token.Minus);

                return new IntegerConstant
                {
                    Constant = int.Parse(AcceptElement(Token.IntegerLiteral).Data) * (minus ? -1 : 1),
                };
            }
            else
            {
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

            if (stmt.Type.Type != Token.Void)
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
            switch (PeekElement().Type)
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
                case Token.Call:
                    stmt = new ExpressionStatement
                    {
                        Expression = ParseExpression()
                    };
                    break;
                default:
                    throw new NotImplementedException();
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
                AcceptElement(Token.Comdat);
                AcceptElement(Token.Comma);
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
                        while (AcceptElementIfNext(Token.Comma))
                        {
                            if (AcceptElementIfNext(Token.Ellipsis))
                            {
                                break;
                            }
                            ParseType();
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
                    cursor++;
                    //return new TokenElement(type, el.Data);
                }
            }
        }
    }
}