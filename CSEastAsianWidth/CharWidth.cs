using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConsoleApplication1
{
    enum EastAsianWidths
    {
        Unknown = 0,

        FullWidth,
        HalfWidth,
        Wide,
        Narrow,
        Ambiguous,
        Neutral,

        //F = FullWidth,
        //H = HalfWidth,
        //W = Wide,
        //Na = Narrow,
        //A = Ambiguous,
        //N = Neutral
    }

    class Range
    {
        public Range(EastAsianWidths w, int code1, int code2)
        {
            this.Width = w;
            this.Code1 = code1;
            this.Code2 = code2;
        }
        public EastAsianWidths Width { get; private set; }
        public int Code1 { get; private set; }
        public int Code2 { get; private set; }

        public bool Contains(int code)
        {
            if (Code1 <= code && code <= Code2)
            {
                return true;
            }

            return false;
        }
        public bool Contains(char c)
        {
            return Contains((int)c);
        }
    }

    class EastAsianWidthDictionary
    {
        public static EastAsianWidthDictionary Default
        {
            get
            {
                if (_Default == null)
                {
                    _Default = EastAsianWidthDictionary.Create();
                }
                return _Default;
            }
        }
        private static EastAsianWidthDictionary _Default;

        private List<Range> List;

        private EastAsianWidthDictionary()
        {
            List = new List<Range>();
        }

        public static EastAsianWidthDictionary Create()
        {
            string fileName = "EastAsianWidth.txt";
            if (System.IO.File.Exists(fileName))
            {
                using (System.IO.StreamReader sr = new System.IO.StreamReader(fileName, Encoding.ASCII))
                {
                    return Create(sr);
                }
            }
            else
            {
                System.Net.WebClient client = new System.Net.WebClient();
                string s = client.DownloadString("http://www.unicode.org/Public/UCD/latest/ucd/EastAsianWidth.txt");
                using (System.IO.StringReader sr = new System.IO.StringReader(s))
                {
                    return Create(sr);
                }
            }
        }

        public static EastAsianWidthDictionary Create(System.IO.TextReader sr)
        {
            EastAsianWidthDictionary dic = new EastAsianWidthDictionary();

            System.Text.RegularExpressions.Regex reg
                = new System.Text.RegularExpressions.Regex(@"^(?<CODE1>[0-9A-F]+)(\.\.(?<CODE2>[0-9A-F]+))?;(?<WIDTH>(Na|N|W|F|H|A))\s");

            EastAsianWidthDictionary dictionary = new EastAsianWidthDictionary();

            string line;

            while (sr.Peek() != -1)
            {
                line = sr.ReadLine();
                if (line.StartsWith("#"))
                {
                    continue;
                }

                var match = reg.Match(line);
                if (match.Success)
                {
                    string sw = match.Groups["WIDTH"].Value;
                    EastAsianWidths w = EastAsianWidths.Unknown;
                    switch (sw[0])
                    {
                    case 'N': w = sw.Length == 1 ? EastAsianWidths.Neutral : EastAsianWidths.Narrow; break;
                    case 'W': w = EastAsianWidths.Wide; break;
                    case 'F': w = EastAsianWidths.FullWidth; break;
                    case 'H': w = EastAsianWidths.HalfWidth; break;
                    case 'A': w = EastAsianWidths.Ambiguous; break;
                    default: w = EastAsianWidths.Unknown; break;
                    }

                    int code1 = int.Parse(match.Groups["CODE1"].Value, System.Globalization.NumberStyles.HexNumber);
                    int code2 = code1;
                    if (match.Groups["CODE2"].Success)
                    {
                        code2 = int.Parse(match.Groups["CODE2"].Value, System.Globalization.NumberStyles.HexNumber);
                    }

                    Range r = new Range(w, code1, code2);

                    dic.List.Add(r);
                }
                else
                {
                }
            }
            return dic;
        }

        public EastAsianWidths GetEastAsianWidth(char c)
        {
            int code = (int)c;

            foreach (Range r in this.List)
            {
                if (r.Contains(code))
                {
                    return r.Width;
                }
            }
            return EastAsianWidths.Unknown;
        }


    }


    static class CharExtention
    {
        public static EastAsianWidths GetEastAsianWidth(this char c)
        {
            return EastAsianWidthDictionary.Default.GetEastAsianWidth(c);
        }

        public static bool IsWideEastAsianWidth(this char c)
        {
            var w = EastAsianWidthDictionary.Default.GetEastAsianWidth(c);
            return w == EastAsianWidths.Wide || w == EastAsianWidths.FullWidth;
        }
        public static bool IsFullWidthEastAsianWidth(this char c)
        {
            var w = EastAsianWidthDictionary.Default.GetEastAsianWidth(c);
            return w == EastAsianWidths.FullWidth;
        }

        public static bool IsNarrowEastAsianWidth(this char c)
        {
            var w = EastAsianWidthDictionary.Default.GetEastAsianWidth(c);
            return w == EastAsianWidths.Narrow || w == EastAsianWidths.HalfWidth;
        }

        public static bool IsHalfEastAsianWidth(this char c)
        {
            var w = EastAsianWidthDictionary.Default.GetEastAsianWidth(c);
            return w == EastAsianWidths.HalfWidth;
        }

        private static System.Text.Encoding sjis = System.Text.Encoding.GetEncoding("shift_JIS");

        public static EastAsianWidths GetEastAsianWidthFrom_SJIS(this char c)
        {
            byte[] bs = sjis.GetBytes(c.ToString());
            int byteCount = bs.Length;
            if (byteCount == 1)
            {
                if (bs[0] <= 0x1F || bs[0] == 0x7F)
                {
                    return EastAsianWidths.Neutral;
                }
                else if (0xA1 <= bs[0] && bs[0] <= 0xDF)
                {
                    return EastAsianWidths.HalfWidth;
                }
                else
                {
                    return EastAsianWidths.Narrow;
                }
            }
            else if (byteCount == 2)
            {
                return EastAsianWidths.Wide; //include FullWidth
            }
            return EastAsianWidths.Unknown;
        }
        public static bool IsWideEastAsianWidth_SJIS(this char c)
        {
            int byteCount = sjis.GetByteCount(c.ToString());
            return byteCount == 2;
        }


        public static EastAsianWidths GetEastAsianWidthFrom_VB6(this char c)
        {
            if ((int)c <= 0x1F || (int)c == 0x7F)
            {
                return EastAsianWidths.Neutral;
            }
            string s = c.ToString();
            if (s == Microsoft.VisualBasic.Strings.StrConv(s, Microsoft.VisualBasic.VbStrConv.Wide, 0x0411))
            {
                return EastAsianWidths.Wide;  //include FullWidth
            }
            else
            {
                return EastAsianWidths.Narrow; //include HalfWidth
            }
        }
        public static bool IsWideEastAsianWidth_VB6(this char c)
        {
            string s = c.ToString();
            return s == Microsoft.VisualBasic.Strings.StrConv(s, Microsoft.VisualBasic.VbStrConv.Wide, 0x0411);
        }
    }
}
