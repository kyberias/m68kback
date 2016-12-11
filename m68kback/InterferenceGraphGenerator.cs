using System;
using System.Collections.Generic;
using System.Linq;

namespace m68kback
{
    public class InterferenceGraphGenerator
    {
        static bool IsRegType(RegType regType, string regName)
        {
            return (regType == RegType.Data && regName[0] == 'D') || (regType == RegType.Address && regName[0] == 'A') || (regType == RegType.ConditionCode && regName.StartsWith("CCR"));
        }

        public static InterferenceGraph MakeGraph(IList<CfgNode> nodes, RegType regType, IList<string> preColored)
        {
            var graph = new HashSet<Tuple<string, string>>();
            var g = new InterferenceGraph { Graph = graph };

            int added = 0;
            foreach (var p1 in preColored)
            {
                foreach (var p2 in preColored)
                {
                    if (p1 != p2 && !g.IsEdgeBetween(p1, p2) && !g.IsEdgeBetween(p2, p1))
                    {
                        g.Graph.Add(new Tuple<string, string>(p1, p2));
                        added++;
                    }
                }
                g.Nodes.Add(p1);
            }

            foreach (var node in nodes.Where(n => n != null))
            {
                foreach (var def in node.Def.Where(dn => IsRegType(regType, dn)))
                {
                    foreach (var o in node.Out.Where(dn => IsRegType(regType, dn)))
                    {
                        if (o != def && !graph.Contains(new Tuple<string, string>(def,o)) && !graph.Contains(new Tuple<string, string>(o, def)))
                        {
                            graph.Add(new Tuple<string, string>(def, o));
                        }
                        g.Nodes.Add(o);
                        g.Nodes.Add(def);
                    }
                }

                var inst = node.Instruction;

                if (inst.Register1 != null && inst.Register2 != null 
                    && inst.Register1.Type == regType && inst.Register2.Type == regType
                    && inst.AddressingMode1 == M68kAddressingMode.Register && inst.AddressingMode2 == M68kAddressingMode.Register)
                {
                    var a = node.Instruction.Register1.ToString();
                    var c = node.Instruction.Register2.ToString();

                    if (node.Instruction.Opcode == M68kOpcode.Move)
                    {
                        //g.Moves.Add(a);
                        //g.Moves.Add(c);
                        g.Moves.Add(node.Instruction);
                    }

                    foreach (var b in node.Out.Where(dn => IsRegType(regType, dn)))
                    {
                        if (b != c && !graph.Contains(new Tuple<string, string>(a,b)) && !graph.Contains(new Tuple<string, string>(b,a)))
                        {
                            graph.Add(new Tuple<string, string>(a, b));
                        }
                        g.Nodes.Add(b);
                    }
                    g.Nodes.Add(c);
                }
            }

            return g;
        }
    }
}