using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            string test = "A1#Ａ１あｱア亜";

            Console.WriteLine("**** Check by Unicode ****");
            foreach (char c in test)
            {
                Console.WriteLine(string.Format("{0}\t{1}\t{2}", c, c.GetEastAsianWidth(), c.IsWideEastAsianWidth()));
            }

            Console.WriteLine("**** Check by S-JIS ****");
            foreach (char c in test)
            {
                Console.WriteLine(string.Format("{0}\t{1}\t{2}", c, c.GetEastAsianWidthFrom_SJIS(), c.IsWideEastAsianWidth_SJIS()));
            }

            Console.WriteLine("**** Check by Microsoft.VisualBasic.Compatibility.dll ****");
            foreach (char c in test)
            {
                Console.WriteLine(string.Format("{0}\t{1}\t{2}", c, c.GetEastAsianWidthFrom_VB6(), c.IsWideEastAsianWidth_VB6()));
            }
        }
    }
}
