using System.IO;

namespace FubuCsProjFile.MSBuild
{
    public class ByteOrderMark
    {
        private static readonly ByteOrderMark[] table = new[]
        {
            new ByteOrderMark("UTF-8", new byte[] {0xEF, 0xBB, 0xBF}),
            new ByteOrderMark("UTF-32BE", new byte[] {0x00, 0x00, 0xFE, 0xFF}),
            new ByteOrderMark("UTF-32LE", new byte[] {0xFF, 0xFE, 0x00, 0x00}),
            new ByteOrderMark("UTF-16BE", new byte[] {0xFE, 0xFF}),
            new ByteOrderMark("UTF-16LE", new byte[] {0xFF, 0xFE}),
            new ByteOrderMark("UTF-7", new byte[] {0x2B, 0x2F, 0x76, 0x38}),
            new ByteOrderMark("UTF-7", new byte[] {0x2B, 0x2F, 0x76, 0x39}),
            new ByteOrderMark("UTF-7", new byte[] {0x2B, 0x2F, 0x76, 0x2B}),
            new ByteOrderMark("UTF-7", new byte[] {0x2B, 0x2F, 0x76, 0x2F}),
            new ByteOrderMark("UTF-1", new byte[] {0xF7, 0x64, 0x4C}),
            new ByteOrderMark("UTF-EBCDIC", new byte[] {0xDD, 0x73, 0x66, 0x73}),
            new ByteOrderMark("SCSU", new byte[] {0x0E, 0xFE, 0xFF}),
            new ByteOrderMark("BOCU-1", new byte[] {0xFB, 0xEE, 0x28}),
            new ByteOrderMark("GB18030", new byte[] {0x84, 0x31, 0x95, 0x33}),
        };

        private ByteOrderMark(string name, byte[] bytes)
        {
            Bytes = bytes;
            Name = name;
        }

        public string Name { get; private set; }

        public byte[] Bytes { get; private set; }

        public int Length
        {
            get { return Bytes.Length; }
        }

        public static ByteOrderMark GetByName(string name)
        {
            for (int i = 0; i < table.Length; i++)
            {
                if (table[i].Name == name)
                    return table[i];
            }

            return null;
        }

        public static bool TryParse(byte[] buffer, int available, out ByteOrderMark bom)
        {
            if (buffer.Length >= 2)
            {
                for (int i = 0; i < table.Length; i++)
                {
                    bool matched = true;

                    if (available < table[i].Bytes.Length)
                        continue;

                    for (int j = 0; j < table[i].Bytes.Length; j++)
                    {
                        if (buffer[j] != table[i].Bytes[j])
                        {
                            matched = false;
                            break;
                        }
                    }

                    if (matched)
                    {
                        bom = table[i];
                        return true;
                    }
                }
            }

            bom = null;

            return false;
        }

        public static bool TryParse(Stream stream, out ByteOrderMark bom)
        {
            var buffer = new byte[4];
            int nread;

            if ((nread = stream.Read(buffer, 0, buffer.Length)) < 2)
            {
                bom = null;

                return false;
            }

            return TryParse(buffer, nread, out bom);
        }
    }
}