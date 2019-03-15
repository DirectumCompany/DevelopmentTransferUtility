using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NpoComputer.DevelopmentTransferUtility.Common
{
   static class filestoutf8
    {
        static public void export(string folder_path)
        {
            convertToUTF8(folder_path);
        }
        static public string import(string folder_path)
        {
            string tmp_path = createTmpDir();

            if (string.IsNullOrEmpty(tmp_path))
                return "";

            ConvertTo1251(folder_path, tmp_path);

            return tmp_path;
        }

        static public void convertToUTF8(string fpath)
        {
            convert(fpath, Encoding.Default, Encoding.UTF8);
        }

        static public void convert(string fpath_src, string fpath_dest, Encoding src, Encoding dest)
        {
            if (string.IsNullOrEmpty(fpath_dest))
                fpath_dest = fpath_src;

            string[] files = Directory.GetFiles(fpath_src, "*", SearchOption.AllDirectories);

            foreach (string f in files)
                convertfile(f, f.Replace(fpath_src, fpath_dest), src, dest);

        }

        static private void convert(string fpath, Encoding src, Encoding dest)
        {
            convert(fpath, fpath, src,dest);
        }

        static private void convertfile(string filesrc, string filedest, Encoding src, Encoding dest)
        {

            var t = File.ReadAllText(filesrc, src);

            FileInfo fi = new FileInfo(filedest);

            Directory.CreateDirectory(fi.DirectoryName);

            File.WriteAllText(filedest, t, dest);
        }

        static public void ConvertTo1251(string pathSrc, string pathDest)
        {
            convert(pathSrc,pathDest, Encoding.UTF8, Encoding.Default);
        }

        static private string createTmpDir()
        {
            string tmp_path = Path.GetTempPath() + Guid.NewGuid();

            try
            {
                Directory.CreateDirectory(tmp_path);
            }

            catch(Exception e)
            {
                tmp_path = "";
            }

            return tmp_path;
        }

    }
}
