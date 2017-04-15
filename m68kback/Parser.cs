using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            internalTypes[Token.I16] = new InternalTypeDefinition { Name = "i16", Type = Token.I16 };
            internalTypes[Token.I32] = new InternalTypeDefinition { Name = "i32", Type = Token.I32 };
            internalTypes[Token.I64] = new InternalTypeDefinition { Name = "i64", Type = Token.I64 };
            internalTypes[Token.Void] = new InternalTypeDefinition { Name = "void", Type = Token.Void };

            cursor = 0;
        }

        public void ParseTarget()
        {
            if (!AcceptElementIfNext(Token.Symbol))
            {
                AcceptElement(Token.Target);
                AcceptElement(Token.Datalayout, Token.Triple);
            }
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

            if (AcceptElementIfNext(Token.Opaque))
            {
                var td = new OpaqueTypeDefinition();
                td.Name = id.Data;
                return td;
            }

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
                        program.TypeDefinitions.Add(ParseTypeDefinition());
                        break;
                    case Token.Exclamation:
                        ParseMetadata();
                        break;
                    case Token.Attributes:
                        ParseAttributes();
                        break;
                    case Token.Symbol:
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
            func.Internal = AcceptElementIfNext(Token.Internal);
            func.ReturnType = ParseType();

            var name = AcceptElement(Token.GlobalIdentifier);
            func.Name = name.Data;

            AcceptElement(Token.ParenOpen);

            if (PeekElement().Type != Token.ParenClose)
            {
                while (true)
                {
                    if (AcceptElementIfNext(Token.Ellipsis))
                    {
                        func.VariableNumParameters = true;
                        break;
                    }

                    var par = new FunctionParameter
                    {
                        Type = ParseType()
                    };

                    AcceptElementIfNext(Token.NoAlias);
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

            AcceptElementIfNext(Token.LocalUnnamedAddr);

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

        TypeReference ParseType()
        {
            TypeReference decl;// = new TypeReference();
            var el = PeekElement();
            switch (el.Type)
            {
                case Token.I64:
                case Token.I32:
                case Token.I16:
                case Token.I8:
                case Token.I1:
                case Token.Void:
                    decl = new DefinedTypeReference(internalTypes[el.Type]);
                    AcceptElement(el.Type);
                    break;
                case Token.BracketOpen:
                    {
                        AcceptElement(Token.BracketOpen);
                        var arr = new ArrayReference();
                        arr.ArrayX = int.Parse(AcceptElement(Token.IntegerLiteral).Data);
                        AcceptElement(Token.Symbol);
                        arr.BaseType = ParseType();
                            //new DefinedTypeReference(internalTypes[AcceptElement(Token.I32, Token.I16, Token.I8).Type]);
                        AcceptElement(Token.BracketClose);
                        decl = arr;
                    }
                    break;
                case Token.LocalIdentifier:
                    {
                        var id = AcceptElement(Token.LocalIdentifier);
                        if (types.ContainsKey(id.Data))
                        {
                            decl = new DefinedTypeReference(types[id.Data]);
                        }
                        else
                        {
                            decl = new DefinedTypeReference(id.Data);
                        }
                    }
                    break;
                default:
                    throw new Exception();
            }

            while (PeekElement().Type == Token.Asterisk)
            {
                AcceptElement(Token.Asterisk);
                var ptr = new PointerReference();
                ptr.BaseType = decl;
                decl = ptr;
            }

            if (PeekElement().Type == Token.ParenOpen)
            {
                var ftype = new FunctionTypeReference
                {
                    ReturnValue = decl
                };
                decl = ftype;
                AcceptElement(Token.ParenOpen);
                // Parameter list
                while (true)
                {
                    if (PeekElement().Type == Token.Ellipsis)
                    {
                        AcceptElement(Token.Ellipsis);
                        break;
                    }

                    ftype.Parameters.Add(ParseType());
                    if (PeekElement().Type == Token.ParenClose)
                    {
                        break;
                    }
                    AcceptElement(Token.Comma);
                }

                AcceptElement(Token.ParenClose);
            }

            while (AcceptElementIfNext(Token.Asterisk))
            {
                var ptype = new PointerReference();
                ptype.BaseType = decl;
                decl = ptype;
            }

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

            expr.PtrExpr = ParseExpression();

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

        CastExpression ParseCastExpression()
        {
            var expr = new CastExpression();
            expr.CastType = AcceptElement(Token.Bitcast, Token.Trunc, Token.Sext, Token.Inttoptr, Token.PtrToInt).Type;

            var paren = false;
            if (expr.CastType == Token.Bitcast)
            {
                paren = AcceptElementIfNext(Token.ParenOpen);
            }

            expr.Type = ParseType();
            expr.Value = ParseExpression();
            AcceptElement(Token.To);
            expr.Type = ParseType();

            if (paren)
            {
                AcceptElement(Token.ParenClose);
            }

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

        CallExpression ParseCallExpression()
        {
            var expr = new CallExpression();
            expr.Parameters = new List<Expression>();

            AcceptElementIfNext(Token.Tail);
            AcceptElement(Token.Call);

            expr.ZeroExtended = AcceptElementIfNext(Token.ZeroExt);

            expr.Type = ParseType();

            if (PeekElement().Type == Token.Bitcast)
            {
                expr.FunctionName = ParseBitcast().Name;
            }
            else
            {
                if (PeekElement().Type == Token.GlobalIdentifier)
                {
                    expr.FunctionName = AcceptElement(Token.GlobalIdentifier).Data;
                }
                else
                {
                    expr.VariableName = AcceptElement(Token.LocalIdentifier).Data;
                }
            }

            AcceptElement(Token.ParenOpen);

            if (PeekElement().Type != Token.ParenClose)
            {
                while (true)
                {
                    ParseType();

                    AcceptElementIfNext(Token.Nonnull);

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
            //ptr.PtrVar = AcceptElement(Token.LocalIdentifier, Token.GlobalIdentifier).Data;
            ptr.PtrVal = ParseExpression();

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
            expr.Operator = AcceptElement(Token.Mul, Token.Sdiv, Token.Add, Token.Sub, Token.Srem, Token.Xor, Token.Zext, Token.And, Token.Or,
                Token.Ashr, Token.Lshr).Type;

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

        ArrayExpression ParseArrayExpression()
        {
            var expr = new ArrayExpression()
            {
                Values = new List<Expression>()
            };

            AcceptElement(Token.BracketOpen);

            if (PeekElement().Type != Token.BracketClose)
            {
                while (true)
                {
                    ParseType();
                    var val = ParseExpression();
                    expr.Values.Add(val);

                    if (!AcceptElementIfNext(Token.Comma))
                    {
                        break;
                    }
                    //AcceptElement(Token.Comma);
                }
            }

            AcceptElement(Token.BracketClose);
            return expr;
        }

        StructExpression ParseStructExpression()
        {
            var expr = new StructExpression
            {
                Values = new List<StructField>()
            };

            AcceptElement(Token.CurlyBraceOpen);

            while (PeekElement().Type != Token.CurlyBraceClose)
            {
                var val = new StructField();

                val.Type = ParseType();

                if (AcceptElementIfNext(Token.Null))
                {
                }
                else if (AcceptElementIfNext(Token.ZeroInitializer))
                {
                    val.InitializeToZero = true;
                }
                else
                {
                    val.Value = ParseExpression();
                }

                expr.Values.Add(val);

                AcceptElementIfNext(Token.Comma);
            }

            AcceptElement(Token.CurlyBraceClose);

            return expr;
        }

        SelectExpression ParseSelect()
        {
            var expr = new SelectExpression();
            AcceptElement(Token.Select);
            expr.Type = ParseType();
            expr.Expr = ParseExpression();
            AcceptElement(Token.Comma);
            var ttype = ParseType();
            expr.TrueExpression = ParseExpression();
            expr.TrueExpression.Type = ttype;
            AcceptElement(Token.Comma);
            var ftype = ParseType();
            expr.FalseExpression = ParseExpression();
            expr.FalseExpression.Type = ftype;
            return expr;
        }

        Expression ParseExpression()
        {
            switch (PeekElement().Type)
            {
                case Token.BracketOpen:
                    return ParseArrayExpression();
                case Token.PtrToInt:
                    return ParseCastExpression();
                case Token.Select:
                    return ParseSelect();
                case Token.CurlyBraceOpen:
                    return ParseStructExpression();
                case Token.Alloca:
                    return ParseAllocaExpression();
                case Token.Bitcast:
                case Token.Trunc:
                case Token.Sext:
                case Token.Inttoptr:
                    return ParseCastExpression();
                case Token.Tail:
                case Token.Call:
                    return ParseCallExpression();
                case Token.Icmp:
                    return ParseIcmpExpression();
                case Token.Mul:
                case Token.Sdiv:
                case Token.Add:
                case Token.Sub:
                case Token.Xor:
                case Token.Srem:
                case Token.Zext:
                case Token.And:
                case Token.Or:
                case Token.Ashr:
                case Token.Lshr:
                    return ParseArithmeticExpression();
                case Token.GetElementPtr:
                    return ParseGetElementPtr();
                case Token.Load:
                    return ParseLoadExpression();
                case Token.Phi:
                    return ParsePhi();
            }

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
                case Token.LocalIdentifier:
                    return new VariableReference
                    {
                        Variable = AcceptElement(Token.LocalIdentifier).Data
                    };
                case Token.GlobalIdentifier:
                    return new VariableReference
                    {
                        Variable = AcceptElement(Token.GlobalIdentifier).Data
                    };
                case Token.Null:
                    AcceptElement(Token.Null);
                    return new IntegerConstant
                    {
                        Constant = 0
                    };
                    //return null;
                default:
                    throw new NotSupportedException();
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

            var type = stmt.Type as DefinedTypeReference;
            var intDef = type?.Type as InternalTypeDefinition;

            if (intDef == null || intDef.Type != Token.Void)
            {
                if(!AcceptElementIfNext(Token.Undef))
                { 
                    stmt.Value = ParseExpression();
                }
            }
            return stmt;
        }

        IcmpExpression ParseIcmpExpression()
        {
            var stmt = new IcmpExpression();
            AcceptElement(Token.Icmp);
            stmt.Condition = AcceptElement(Token.Slt, Token.Sgt, Token.Sge, Token.Eq, Token.Ne, Token.Ult).Type;
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
                case Token.Unreachable:
                    AcceptElement(Token.Unreachable);
                    stmt = new Unreachable();
                    break;
                case Token.LocalIdentifier:
                    stmt = ParseAssignmentStatement();
                    break;
                case Token.Switch:
                    stmt = ParseSwitchStatement();
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

        private SwitchStatement ParseSwitchStatement()
        {
            AcceptElement(Token.Switch);
            var stmt = new SwitchStatement();

            stmt.Type = ParseType();
            stmt.Value = ParseExpression();

            AcceptElement(Token.Comma);
            AcceptElement(Token.Label);

            stmt.DefaultLabel = AcceptElement(Token.LocalIdentifier).Data;
            AcceptElement(Token.BracketOpen);

            while (PeekElement().Type != Token.BracketClose)
            {
                var sc = new SwitchCase();

                ParseType();

                sc.Value = ParseExpression();
                AcceptElement(Token.Comma);
                AcceptElement(Token.Label);
                sc.Label = AcceptElement(Token.LocalIdentifier).Data;

                stmt.Switches.Add(sc);
            }

            AcceptElement(Token.BracketClose);

            return stmt;
        }

        Declaration ParseDeclaration()
        {
            var decl = new Declaration();

            if (PeekElement().Type == Token.GlobalIdentifier)
            {
                decl.Name = AcceptElement(Token.GlobalIdentifier).Data;

                AcceptElement(Token.Assign);

                AcceptElementIfNext(Token.Common);
                AcceptElementIfNext(Token.Private);
                bool ext = AcceptElementIfNext(Token.External);
                decl.External = ext;
                AcceptElementIfNext(Token.LocalUnnamedAddr);

                if (PeekElement().Type == Token.Global)
                {
                    AcceptElement(Token.Global);
                    decl.Global = true;
                    decl.Type = ParseType();
                    Debug.Assert(decl.Type != null);

                    if (!ext)
                    {
                        if (AcceptElementIfNext(Token.ZeroInitializer))
                        {
                            decl.InitializeToZero = true;
                        }
                        else
                        {
                            if (PeekElement().Type != Token.Null)
                            {
                                decl.Expr = ParseExpression();
                            }
                            else
                            {
                                AcceptElement(Token.Null);
                            }
                        }
                    }
                }
                else
                {
                    while (PeekElement().Type == Token.Symbol)
                    {
                        AcceptElement(Token.Symbol);
                    }

                    decl.Constant = AcceptElementIfNext(Token.Constant);

                    if (PeekElement().Type == Token.BracketOpen)
                    {
                        AcceptElement(Token.BracketOpen);
                        var arr = new ArrayReference();

                        arr.ArrayX = int.Parse(AcceptElement(Token.IntegerLiteral).Data);
                        AcceptElement(Token.Symbol);
                        arr.BaseType = ParseType();

                        /*while (PeekElement().Type != Token.BracketClose)
                        {

                            AcceptElement(PeekElement().Type);
                        }*/
                        AcceptElement(Token.BracketClose);
                        decl.Type = arr;
                    }

                    if (PeekElement().Type == Token.StringLiteral)
                    {
                        decl.Value = AcceptElement(Token.StringLiteral).Data;
                    }
                    else
                    {
                        decl.Expr = ParseExpression();
                    }
                }

                if (AcceptElementIfNext(Token.Comma))
                {

                    if (PeekElement().Type == Token.Comdat)
                    {
                        AcceptElement(Token.Comdat);
                        AcceptElement(Token.Comma);
                    }

                    AcceptElement(Token.Align);
                    AcceptElement(Token.IntegerLiteral);
                }
            }
            else if (PeekElement().Type == Token.Declare)
            {
                decl.Declare = true;
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
                        AcceptElementIfNext(Token.WriteOnly);
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
                AcceptElementIfNext(Token.LocalUnnamedAddr);

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
            }
        }
    }
}
