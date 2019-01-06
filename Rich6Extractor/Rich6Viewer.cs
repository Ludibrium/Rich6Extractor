using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.IO.MemoryMappedFiles;

namespace Rich6Extractor
{
    public class Rich6Viewer
    {
        private string path;
        MemoryMappedFile m;
        MemoryMappedViewAccessor mv;
        public List<string> fileName;
        public List<int> fileSize;
        public List<long> fileOffset;
        public Rich6Viewer(string path)
        {
            this.path = path;
            try
            {
                m = MemoryMappedFile.CreateFromFile(path);
                mv = m.CreateViewAccessor();
            }
            catch (Exception e) { throw e; }
            fileSize = new List<int>();
            fileName = new List<string>();
            fileOffset = new List<long>();
        }

        public void ReadAll()
        {
            int stride = 0x108;
            var encoding = Encoding.GetEncoding(936);
            for (int begin = 0x9; begin < 0x8a200; begin += stride)
            {
                int len = 0;
                while (mv.ReadByte(begin + len) != 0) len++;
                byte[] second = new byte[len + 2];
                mv.ReadArray<byte>(begin, second, 0, len + 2);
                fileName.Add(encoding.GetString(second));
                var third = new byte[8];
                mv.ReadArray<byte>(begin + 0x100, third, 0, 4);
                //Buffer.BlockCopy(data, begin + 0x100, third, 0, 4);
                fileOffset.Add(BitConverter.ToInt64(third, 0));
                third = new byte[4];
                mv.ReadArray(begin + 0x104, third, 0, 4);
               // Buffer.BlockCopy(data, begin + 0x104, third, 0, 4);
                fileSize.Add(BitConverter.ToInt32(third, 0));
            }
        }

        public byte[] ReadNode(int index)
        {
            long offset = fileOffset[index];
            int len = fileSize[index];
            var buffer = new byte[len];
            mv.ReadArray(offset, buffer, 0, len);
            //Buffer.BlockCopy(data, Convert.ToInt32(offset), buffer, 0, len);
            return buffer;
        }

        public string InputNode(string p, int index)
        {
            var result = "";
            try
            {
                var fileinfo = new FileInfo(p);
                if (fileinfo.Length > fileSize[index])
                {
                    throw new ArgumentException("文件太大。大小为" + fileinfo.Length + ", " +
                        "最大大小为" + fileSize[index]);
                }
                var input = File.ReadAllBytes(p);
                long offset = fileOffset[index];
                mv.WriteArray(offset, input, 0, input.Length);
                //Buffer.BlockCopy(input, 0, data, Convert.ToInt32(offset), Convert.ToInt32(fileinfo.Length));
                var size = BitConverter.GetBytes(fileSize[index]);
                mv.Write(0x9 + 0x108 * (index - 1) + 0x104, Convert.ToInt32(fileinfo.Length));
                //Buffer.BlockCopy(size, 0, data, 0x9 + 0x108 * (index - 1) + 0x104, 4);
                result = "Success";        
            }
            catch (Exception e)
            {
                result = e.Message;
            }
            return result;
        }

        public void SaveFile()
        {
            
        }
    }
}
