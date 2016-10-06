using System.Collections.Generic;
using System.Linq;

namespace m68kback
{
    public class CfgNode
    {
        public M68kInstruction Instruction { get; set; }
        public List<CfgNode> Succ { get; } = new List<CfgNode>();
        public List<CfgNode> Pred { get; } = new List<CfgNode>();

        public HashSet<string> Out { get; set; } = new HashSet<string>();
        public HashSet<string> In { get; set; } = new HashSet<string>();

        public List<string> Def { get; } = new List<string>();
    }

    public class LivenessAnalysis
    {
        public LivenessAnalysis(IList<M68kInstruction> code, RegType regType = RegType.Data)
        {
            CfgNode prev = null;

            var labels = new Dictionary<string,int>();
            //var currLabels = new List<string>();
            var defs = new List<string>();

            /*for (int i = 0; i < code.Count; i++)
            {
                if (code[i].Opcode == M68kOpcode.Label)
                {
                    currLabels.Add(code[i].Label);
                }
                else if (currLabels.Count > 0)
                {
                    foreach (var label in currLabels)
                    {
                        labels[label] = i;
                    }
                    currLabels.Clear();
                }
            }*/

            var nodes = new List<CfgNode>();

            //M68kInstruction prevLab = null;

            for (int i = 0; i < code.Count; i++)
            {
                /*if (code[i].Opcode == M68kOpcode.Label)
                {
                    prevLab = code[i];
                    nodes.Add(null);
                    continue;
                }*/

                var node = new CfgNode();
                nodes.Add(node);
                node.Instruction = code[i];

                // TODO: For address registers, this is not enough!!
                /*if (code[i].Register2 != null && code[i].Register2.Type == regType)
                {
                    var def = code[i].Register2.ToString();
                    if (!defs.Contains(def))
                    {
                        defs.Add(def);
                    }
                    node.Def.Add(def);
                }*/

                var idefs = code[i].Def(regType).ToList();
                foreach (var def in idefs)
                {
                    if (!defs.Contains(def))
                    {
                        defs.Add(def);
                    }
                    node.Def.Add(def);
                }

                if (prev != null && 
                    (!prev.Instruction.IsBranch() || 
                    (prev.Instruction.IsBranch() && prev.Instruction.TargetLabel == node.Instruction.Label)
                    || prev.Instruction.IsConditionalBranch()))
                {
                    prev.Succ.Add(node);
                    node.Pred.Add(prev);
                }

                if (code[i].Opcode != M68kOpcode.Rts)
                {
                    prev = node;
                }
                else
                {
                    prev = null;
                }
            }

            for (int i = 0; i < code.Count; i++)
            {
                if (nodes[i].Instruction.IsBranch())
                {
                    foreach (var node in nodes.Where(n => n.Instruction.Opcode == M68kOpcode.Label))
                    {
                        if (node.Instruction.Label == nodes[i].Instruction.TargetLabel.Replace("%",""))
                        {
                            node.Pred.Add(nodes[i]);
                            nodes[i].Succ.Add(node);
                        }
                    }
                }
            }

            IterativeDataFlowAnalysis(nodes, regType);

            /*var n = nodes.First(e => e != null);
            foreach (var i in Enumerable.Range(2, 6))
            {
                n.In.Add("D" + i);
            }*/

            _nodes = nodes;
        }

        private IList<CfgNode> _nodes;
        public IList<CfgNode> Nodes => _nodes;

        void IterativeDataFlowAnalysis(IList<CfgNode> nodes, RegType regType = RegType.Data)
        {
            bool changes;
            do
            {
                changes = false;
                for (int i = nodes.Count - 1; i >= 0; i--)
                {
                    var n = nodes[i];

                    if (n == null)
                    {
                        // label?
                        continue;
                    }

                    var newout = new HashSet<string>(n.Out);

                    foreach (var s in n.Succ)
                    {
                        foreach (var sn in s.In)
                        {
                            newout.Add(sn);
                        }
                    }

                    var use = n.Instruction.Use(regType).ToList();
                    var def = n.Def;

                    var newin = new HashSet<string>(use.Union(newout.Where(o => !def.Contains(o))));

                    if (!newin.SetEquals(n.In))
                    {
                        changes = true;
                        n.In = newin;
                    }

                    if (!newout.SetEquals(n.Out))
                    {
                        changes = true;
                        n.Out = newout;
                    }
                }
            } while (changes);
        }
    }
}