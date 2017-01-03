using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace m68kback
{
    class ObjectFileWriter : OutputFileWriter
    {

        const ushort HUNK_HEADER = 0x03F3;
        const ushort HUNK_CODE = 0x03E9;
        const ushort HUNK_END = 0x03F2;
        const ushort HUNK_EXT = 0x03EF;

        const byte EXT_DEF = 1;
        const byte EXT_REF32 = 129;

        public void WriteFile(string fileName, CodeGenerator codegenerator)
        {
            using (var stream = File.Open(fileName, FileMode.Create))
            {
                Write(stream, HUNK_END);

                using (var memStream = new MemoryStream())
                {
                    foreach (var func in codegenerator.Functions)
                    {
                        foreach(var i in func.Value.Instructions)
                        {
                            EncodeInstruction(i, memStream);
                        }
                    }

                    var len = ((uint)memStream.Length - 1) / 4 + 1;
                    Write(stream, len);

                    var padding = Enumerable.Range(0, (int)len * 4 - (int)memStream.Length).Select(r => (byte)0).ToArray();

                    memStream.WriteTo(stream);
                    if(padding.Length > 0)
                    {
                        stream.Write(padding, 0, padding.Length);
                    }
                }
            }
        }

        // http://info.sonicretro.org/SCHG:68000_ASM-to-Hex_Code_Reference

        void EncodeInstruction(M68kInstruction inst, Stream stream)
        {
            switch(inst.Opcode)
            {
                case M68kOpcode.Rts:
                    Write(stream, 0x4E75);
                    break;
                case M68kOpcode.Jsr:
                    Write(stream, 0x4EB9);
                    Write(stream, (uint)0);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        void Write(Stream s, ushort data)
        {
            var bytes = BitConverter.GetBytes(data);
            s.Write(bytes, 0, bytes.Length);
        }

        void Write(Stream s, uint data)
        {
            var bytes = BitConverter.GetBytes(data);
            s.Write(bytes, 0, bytes.Length);
        }
    }
}
