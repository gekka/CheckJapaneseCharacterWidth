Module Module1

    Sub Main()
        Dim test As String = "A1#Ａ１あｱア亜"

        Console.WriteLine("**** Check by Unicode ****")
        For Each c As Char In test
            Console.WriteLine(String.Format("{0}\t{1}\t{2}", c, c.GetEastAsianWidth(), c.IsWideEastAsianWidth()))
        Next

        Console.WriteLine("**** Check by S-JIS ****")
        For Each c As Char In test
            Console.WriteLine(String.Format("{0}\t{1}\t{2}", c, c.GetEastAsianWidthFrom_SJIS(), c.IsWideEastAsianWidth_SJIS()))
        Next

        Console.WriteLine("**** Check by Microsoft.VisualBasic.Compatibility.dll ****")
        For Each c As Char In test
            Console.WriteLine(String.Format("{0}\t{1}\t{2}", c, c.GetEastAsianWidthFrom_VB6(), c.IsWideEastAsianWidth_VB6()))
        Next
    End Sub

End Module
