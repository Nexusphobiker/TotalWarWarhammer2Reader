using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TotalWarPackReader
{
    public class PACK
    {
        public string path;
        public long EndOfDirectory = 0;
        public PackFile[] FileList;

        public PACK(string FilePath)
        {
            path = FilePath;
            Stream fileStream = File.OpenRead(path);
            byte[] intBuff = new byte[4];
            fileStream.Read(intBuff, 0, intBuff.Length);

            //Check magic number
            if (BitConverter.ToInt32(intBuff, 0) != 893929040)
            {
                Console.WriteLine("Magic number check failed");
                return;
            }

            //Skip because UNK
            fileStream.Read(intBuff, 0, intBuff.Length);

            bool isPartOf = false;
            Array.Clear(intBuff, 0, intBuff.Length);
            fileStream.Read(intBuff, 0, intBuff.Length);
            if (BitConverter.ToInt32(intBuff, 0) == 1)
            {
                isPartOf = true;
                Console.WriteLine("This file is part of another file");
            }
            else if (BitConverter.ToInt32(intBuff, 0) > 1)
            {
                Console.WriteLine("Exception while reading header...");
                return;
            }

            //If the file is part of another the first entry in the file directory will be a string with this length
            Array.Clear(intBuff, 0, intBuff.Length);
            fileStream.Read(intBuff, 0, intBuff.Length);
            int PartOfNameLength = BitConverter.ToInt32(intBuff, 0);
            Console.WriteLine("PartOfNameLength:" + PartOfNameLength);

            //Number of files in this .pack file
            Array.Clear(intBuff, 0, intBuff.Length);
            fileStream.Read(intBuff, 0, intBuff.Length);
            int FileNum = BitConverter.ToInt32(intBuff, 0);
            Console.WriteLine("FileNum:" + FileNum);

            //Size of FileDirectory
            Array.Clear(intBuff, 0, intBuff.Length);
            fileStream.Read(intBuff, 0, intBuff.Length);
            int FileDirectorySize = BitConverter.ToInt32(intBuff, 0);
            Console.WriteLine("FileDirectorySize:" + FileDirectorySize);

            //Skip because UNK
            fileStream.Read(intBuff, 0, intBuff.Length);

            if (isPartOf)
            {
                byte[] partOfName = new byte[PartOfNameLength];
                fileStream.Read(partOfName, 0, partOfName.Length);
                Console.WriteLine("Part of " + System.Text.Encoding.Default.GetString(partOfName));
            }

            //Console.WriteLine("Done reading header. Press any key to continue");
            //Console.ReadKey();

            //read directory
            FileList = new PackFile[FileNum];
            int i = 0;
            while (i < FileNum)
            {
                Array.Clear(intBuff, 0, intBuff.Length);
                fileStream.Read(intBuff, 0, intBuff.Length);
                fileStream.ReadByte();
                string temp = "";
                char tempChar = (char)fileStream.ReadByte();
                while (tempChar != 0)
                {
                    temp = temp + tempChar;
                    tempChar = (char)fileStream.ReadByte();
                }
                Console.WriteLine("Name:" + temp + " Length:" + BitConverter.ToInt32(intBuff, 0));
                FileList[i] = new PackFile(temp, BitConverter.ToInt32(intBuff, 0));
                i++;
            }

            //Saving end of directory
            EndOfDirectory = fileStream.Position;
            Console.WriteLine("Done loading");
            //Releasing stream reader
            fileStream.Flush();
        }

        public long CalculateFilePosition(int i)
        {
            int a = 0;
            long ret = 0;
            while (a != i)
            {
                ret = ret + FileList[a].length;
                a++;
            }
            return (ret + EndOfDirectory);
        }

        //None of the files should be big enough to actually crash the program by exceeding the .net array limits as the file size is hold by a 4 byte value anyway. Insufficient ram can kill it.
        public void Unpack(int index)
        {
            try
            {
                Stream fileStream = File.OpenRead(path);
                fileStream.Position = CalculateFilePosition(index);
                byte[] buff = new byte[FileList[index].length];
                fileStream.Read(buff, 0, buff.Length);
                Directory.CreateDirectory(Path.GetDirectoryName("Unpacked/" + FileList[index].name));
                File.WriteAllBytes("Unpacked/" + FileList[index].name, buff);
                fileStream.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }

        public byte[] getFileData(PackFile file)
        {
            byte[] ret = new byte[file.length];

            //Get index
            int i = 0;
            while(i < FileList.Length)
            {
                if(FileList[i].name == file.name)
                {
                    break;
                }
                i++;
            }

            long filePosition = CalculateFilePosition(i);

            using(Stream fileStream = File.OpenRead(path))
            {
                fileStream.Position = filePosition;
                fileStream.Read(ret, 0, ret.Length);
            }
            return ret;
        }

        public void UnpackAll()
        {
            try {
                Stream fileStream = File.OpenRead(path);
                int i = 0;
                long streamPos = EndOfDirectory;
                while (i < FileList.Length)
                {
                    fileStream.Position = streamPos;
                    byte[] buff = new byte[FileList[i].length];
                    fileStream.Read(buff, 0, buff.Length);
                    Directory.CreateDirectory(Path.GetDirectoryName("Unpacked/" + FileList[i].name));
                    File.WriteAllBytes("Unpacked/" + FileList[i].name, buff);
                    //Console.WriteLine((i + 1) + "/" + FileList.Length);
                    streamPos += FileList[i].length;
                    i++;
                    //break;
                }
                //Console.WriteLine("DONE");
                //Console.ReadKey();
                fileStream.Flush();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }
    }

    public class PackFile
    {
        public string name { get; }
        public int length { get; }
        public PackFile(string FileName, int FileLength)
        {
            name = FileName;
            length = FileLength;
        }
    }
}
