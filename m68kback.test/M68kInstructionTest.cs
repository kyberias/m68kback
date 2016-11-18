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

        [Test]
        public void LeaDef()
        {
            var cmp = new M68kInstruction
            {
                Opcode = M68kOpcode.Lea,
                Variable = "foobar",
                AddressingMode1 = M68kAddressingMode.Absolute,
                AddressingMode2 = M68kAddressingMode.Register,
                Register2 =  new Register { Type = RegType.Address, Number = 2 }
            };

            CollectionAssert.AreEqual(new[] { "A2" }, cmp.Def(RegType.Address));
        }

        [Test]
        public void LeaUse()
        {
            var cmp = new M68kInstruction
            {
                Opcode = M68kOpcode.Lea,
                Variable = "foobar",
                AddressingMode1 = M68kAddressingMode.Absolute,
                AddressingMode2 = M68kAddressingMode.Register,
                Register2 = new Register { Type = RegType.Address, Number = 2 }
            };

            CollectionAssert.IsEmpty(cmp.Use(RegType.Address));
            CollectionAssert.IsEmpty(cmp.Use(RegType.Data));
        }

        [Test]
        public void AddressStoreDef()
        {
            var i = new M68kInstruction
            {
                Opcode = M68kOpcode.Move,
                AddressingMode1 = M68kAddressingMode.Register,
                Register1 = new Register { Type = RegType.Data, Number = 1 },
                AddressingMode2 = M68kAddressingMode.Address,
                Register2 = new Register { Type = RegType.Address, Number = 2 }
            };

            CollectionAssert.IsEmpty(i.Def(RegType.Address));
            CollectionAssert.IsEmpty(i.Def(RegType.Data));
        }

        [Test]
        public void AddressStoreUse()
        {
            var i = new M68kInstruction
            {
                Opcode = M68kOpcode.Move,
                AddressingMode1 = M68kAddressingMode.Register,
                Register1 = new Register { Type = RegType.Data, Number = 1 },
                AddressingMode2 = M68kAddressingMode.Address,
                Register2 = new Register { Type = RegType.Address, Number = 2 }
            };

            CollectionAssert.AreEquivalent(new[] { "D1", }, i.Use(RegType.Data));
            CollectionAssert.AreEquivalent(new[] { "A2", }, i.Use(RegType.Address));
        }

        [Test]
        public void AddressLoadUse()
        {
            var i = new M68kInstruction
            {
                Opcode = M68kOpcode.Move,
                AddressingMode1 = M68kAddressingMode.Address,
                Register1 = new Register { Type = RegType.Address, Number = 2 },
                AddressingMode2 = M68kAddressingMode.Register,
                Register2 = new Register { Type = RegType.Data, Number = 1 },
            };

            CollectionAssert.IsEmpty(i.Use(RegType.Data));
            CollectionAssert.AreEquivalent(new[] { "A2", }, i.Use(RegType.Address));
        }

        [Test]
        public void AddressLoadDef()
        {
            var i = new M68kInstruction
            {
                Opcode = M68kOpcode.Move,
                AddressingMode1 = M68kAddressingMode.Address,
                Register1 = new Register { Type = RegType.Address, Number = 2 },
                AddressingMode2 = M68kAddressingMode.Register,
                Register2 = new Register { Type = RegType.Data, Number = 1 },
            };

            CollectionAssert.IsEmpty(i.Def(RegType.Address));
            CollectionAssert.AreEquivalent(new[] { "D1", }, i.Def(RegType.Data));
        }

        [Test]
        public void LsrDef()
        {
            var cmp = new M68kInstruction(M68kOpcode.Lsr, new Register { Type = RegType.Data, Number = 1 }, new Register { Type = RegType.Data, Number = 2 });

            CollectionAssert.AreEqual(new[] { "D2" }, cmp.Def(RegType.Data));
        }

        [Test]
        public void LsrUse()
        {
            var cmp = new M68kInstruction(M68kOpcode.Lsr, new Register { Type = RegType.Data, Number = 1 }, new Register { Type = RegType.Data, Number = 2 });

            CollectionAssert.AreEquivalent(new[] { "D1", "D2" }, cmp.Use(RegType.Data));
        }

        [Test]
        public void TstUse()
        {
            var tst = new M68kInstruction
            {
                Opcode = M68kOpcode.Tst,
                Register1 = new Register { Type = RegType.Data, Number = 1}
            };

            CollectionAssert.AreEquivalent(new[] { "D1" }, tst.Use(RegType.Data));
        }

        [Test]
        public void TstDef()
        {
            var tst = new M68kInstruction
            {
                Opcode = M68kOpcode.Tst,
                Register1 = new Register { Type = RegType.Data, Number = 1 }
            };

            CollectionAssert.IsEmpty(tst.Def(RegType.Data));
        }

    }
}
