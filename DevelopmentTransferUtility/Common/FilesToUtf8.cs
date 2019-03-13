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
        
        static public void convert(string fpath)
        {

            string[] files = Directory.GetFiles(fpath, "*", SearchOption.AllDirectories);

            foreach (string f in files)
            {
                convertfile(f);
                
            }

        }

        static private void convertfile(string file)
        {
            var t = File.ReadAllText(file, Encoding.Default);

            File.WriteAllText(file, t, Encoding.UTF8);
        }
    }
}
