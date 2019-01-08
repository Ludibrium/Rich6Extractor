using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
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

        public int begin;
        public int stride = 0x108;
        int sizeLen;
        int fileNum;

        public Rich6Viewer(string path,int RichVersion)
        {
            this.path = path;
            switch (RichVersion) {
                case 6:
                    sizeLen = 4;
                    begin = 9;
                    fileNum = 2143;
                    break;
                case 7:
                    sizeLen = 4;
                    begin = 8;
                    fileNum = 3671;
                    break;
                default:
                    throw new ArgumentException("No Support Rich Version.");
            }

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
            var encoding = Encoding.GetEncoding(936);
            for (int temp_begin = begin; temp_begin < 0x108*fileNum; temp_begin += stride)
            {
                int len = 0;
                while (mv.ReadByte(temp_begin + len) != 0) len++;
                byte[] second = new byte[len + 2];
                mv.ReadArray<byte>(temp_begin, second, 0, len + 2);
                fileName.Add(encoding.GetString(second));
                var third = new byte[8];
                mv.ReadArray<byte>(temp_begin + stride - sizeLen - 0x4, third, 0, 4);
                //Buffer.BlockCopy(data, begin + 0x100, third, 0, 4);
                fileOffset.Add(BitConverter.ToInt64(third, 0));
                third = new byte[4];
                mv.ReadArray(temp_begin + stride - sizeLen, third, 0, 4);
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
                mv.Write(begin + stride * (index - 1) + stride - sizeLen, Convert.ToInt32(fileinfo.Length));
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
