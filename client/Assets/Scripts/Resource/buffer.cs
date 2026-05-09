
namespace game.resource
{
    public class Buffer
    {
        public byte[] data;
        public int size = 0;

        public Buffer() { }

        public Buffer(int _size)
        {
            if(_size <= 0)
            {
                return;
            }

            this.data = new byte[_size];
            this.size = _size;
        }

        public static implicit operator byte[](Buffer _buffer)
        {
            return _buffer.data;
        }

        public static implicit operator Buffer(byte[] _data)
        {
            return new()
            {
                data = _data,
                size = _data.Length
            };
        }

        public class Encoding
        {
            public System.Text.Encoding encoding;
            public int byteOrderMarks;

            public Encoding(System.Text.Encoding encoding, int byteOrderMarks)
            {
                this.encoding = encoding;
                this.byteOrderMarks = byteOrderMarks;
            }
        }

        public Buffer.Encoding GetEncoding()
        {
            return this.GetEncoding(System.Text.Encoding.GetEncoding(1252));
        }

        public Buffer.Encoding GetEncoding(System.Text.Encoding defaultEncoding)
        {
            if (this.data == null || this.size <= 0)
            {
                return new Encoding(defaultEncoding ?? System.Text.Encoding.GetEncoding(1252), 0);
            }

            if (this.size >= 4)
            {
                if (this.data[0] == 0xff && this.data[1] == 0xfe && this.data[2] == 0 && this.data[3] == 0) return new Encoding(System.Text.Encoding.UTF32, 4); //UTF-32LE
                if (this.data[0] == 0 && this.data[1] == 0 && this.data[2] == 0xfe && this.data[3] == 0xff) return new Encoding(new System.Text.UTF32Encoding(true, true), 4);  //UTF-32BE
            }

            if (this.size >= 3)
            {
                if (this.data[0] == 0x2b && this.data[1] == 0x2f && this.data[2] == 0x76) return new Encoding(System.Text.Encoding.UTF7, 3);
                if (this.data[0] == 0xef && this.data[1] == 0xbb && this.data[2] == 0xbf) return new Encoding(System.Text.Encoding.UTF8, 3);
            }

            if (this.size >= 2)
            {
                if (this.data[0] == 0xff && this.data[1] == 0xfe) return new Encoding(System.Text.Encoding.Unicode, 2); //UTF-16LE
                if (this.data[0] == 0xfe && this.data[1] == 0xff) return new Encoding(System.Text.Encoding.BigEndianUnicode, 2); //UTF-16BE
            }

            return new Encoding(defaultEncoding ?? System.Text.Encoding.GetEncoding(1252), 0);
        }

        public string GetString()
        {
            return this.GetString(System.Text.Encoding.GetEncoding(1252));
        }

        public string GetString(System.Text.Encoding defaultEncoding)
        {
            if (this.data == null || this.size <= 0)
            {
                return string.Empty;
            }

            Buffer.Encoding encoding = this.GetEncoding(defaultEncoding);
            return encoding.encoding.GetString(this.data, encoding.byteOrderMarks, this.size - encoding.byteOrderMarks);
        }
    }
}

