using NUnit.Framework;

namespace m68kback.test
{
    [TestFixture]
    class M68kInstructionTest
    {
        [Test]
        public void UseWhenLoadBySp()
        {
            var i = new M68kInstruction
            {
                Opcode = M68kOpcode.Move,
                FinalRegister1 = M68kRegister.SP,
                AddressingMode1 = M68kAddressingMode.AddressWithOffset,
                Offset = 0,
                Register2 = new Register {Type = RegType.Address, Number = 15},
                AddressingMode2 = M68kAddressingMode.Register
            };

            CollectionAssert.AreEqual(new[] {"A15"}, i.Def(RegType.Address));
        }

        [Test]
        public void CmpUse()
        {
            var cmp = new M68kInstruction(M68kOpcode.Cmp, new Register { Type = RegType.Data, Number = 1}, new Register { Type = RegType.Data, Number = 2 });

            CollectionAssert.AreEquivalent(new[] {"D1", "D2"}, cmp.Use(RegType.Data));
        }

        [Test]
        public void CmpDef()
        {
            var cmp = new M68kInstruction(M68kOpcode.Cmp, new Register { Type = RegType.Data, Number = 1 }, new Register { Type = RegType.Data, Number = 2 });

            CollectionAssert.IsEmpty(cmp.Def(RegType.Data));
        }

        [Test]
        public void DefWhenAddressRegsUsed()
        {
            var i = new M68kInstruction
            {
                Opcode = M68kOpcode.RegDef,
                DefsUses = new[] {"A1", "D1" }
            };

            CollectionAssert.AreEqual(new[] { "A1" }, i.Def(RegType.Address));
        }
    }
}
