'Microsoft Limited Public Licence

Enum EastAsianWidths

    Unknown = 0

    FullWidth
    HalfWidth
    Wide
    Narrow
    Ambiguous
    Neutral

    'F = FullWidth
    'H = HalfWidth
    'W = Wide
    'Na = Narrow
    'A = Ambiguous
    'N = Neutral
End Enum


Class Range
    Public Sub New(ByVal w As EastAsianWidths, ByVal code1 As Integer, ByVal code2 As Integer)
        Me._Width = w
        Me._Code1 = code1
        Me._Code2 = code2
    End Sub
    Public ReadOnly Property Width As EastAsianWidths
        Get
            Return _Width
        End Get
    End Property
    Private _Width As EastAsianWidths

    Public ReadOnly Property Code1 As Integer
        Get
            Return _Code1
        End Get
    End Property
    Private _Code1 As Integer

    Public ReadOnly Property Code2 As Integer
        Get
            Return _Code2
        End Get
    End Property
    Private _Code2 As Integer

    Public Function Contains(ByVal code As Integer) As Boolean

        If (Code1 <= code AndAlso code <= Code2) Then
            Return True
        End If

        Return False

    End Function

    Public Function Contains(ByVal c As Char) As Boolean
        Return Contains(AscW(c))
    End Function
End Class

Class EastAsianWidthDictionary

    Public Shared ReadOnly Property [Default] As EastAsianWidthDictionary

        Get
            If (_Default Is Nothing) Then
                _Default = EastAsianWidthDictionary.Create()
            End If
            Return _Default
        End Get
    End Property
    Private Shared _Default As EastAsianWidthDictionary

    Private list As List(Of Range)

    Private Sub New()
        Me.list = New List(Of Range)()
    End Sub

    Public Shared Function Create() As EastAsianWidthDictionary

        Dim fileName As String = "_EastAsianWidth.txt"
        If (System.IO.File.Exists(fileName)) Then
            Using sr As New System.IO.StreamReader(fileName, System.Text.Encoding.ASCII)
                Return Create(sr)
            End Using
        Else
            Dim client As New System.Net.WebClient()
            Dim s As String = client.DownloadString("http://www.unicode.org/Public/UCD/latest/ucd/EastAsianWidth.txt")
            Using sr = New System.IO.StringReader(s)
                Return Create(sr)
            End Using
            end if
    End Function

    Public Shared Function Create(ByVal sr As System.IO.TextReader) As EastAsianWidthDictionary

        Dim dic As New EastAsianWidthDictionary()

        Dim reg As New System.Text.RegularExpressions.Regex("^(?<CODE1>[0-9A-F]+)(\.\.(?<CODE2>[0-9A-F]+))?(?<WIDTH>(Na|N|W|F|H|A))\s")

        Dim Dictionary As New EastAsianWidthDictionary()

        Dim line As String

        Do While (sr.Peek() <> -1)

            line = sr.ReadLine()
            If (line.StartsWith("#")) Then
                Continue Do
            End If

            Dim match As System.Text.RegularExpressions.Match = reg.Match(line)
            If (match.Success) Then

                Dim sw As String = match.Groups("WIDTH").Value
                Dim w As EastAsianWidths = EastAsianWidths.Unknown
                Select Case (sw)

                    Case "N"
                        w = EastAsianWidths.Neutral
                    Case "Na"
                        w = EastAsianWidths.Narrow
                    Case "W"
                        w = EastAsianWidths.Wide
                    Case "F"
                        w = EastAsianWidths.FullWidth
                    Case "H"
                        w = EastAsianWidths.HalfWidth
                    Case "A"
                        w = EastAsianWidths.Ambiguous
                    Case Else
                        w = EastAsianWidths.Unknown
                End Select

                Dim code1 As Integer = Integer.Parse(match.Groups("CODE1").Value, System.Globalization.NumberStyles.HexNumber)
                Dim code2 As Integer = code1
                If (match.Groups("CODE2").Success) Then
                    code2 = Integer.Parse(match.Groups("CODE2").Value, System.Globalization.NumberStyles.HexNumber)
                End If

                Dim r As Range = New Range(w, code1, code2)

                dic.List.Add(r)
            End If

        Loop
        Return dic
    End Function

    Public Function GetEastAsianWidth(ByVal c As Char) As EastAsianWidths
        Dim code As Integer = AscW(c)
        For Each r As Range In Me.list
            If (r.Contains(code)) Then
                Return r.Width
            End If
        Next
        Return EastAsianWidths.Unknown
    End Function


End Class


Module CharExtention

    <System.Runtime.CompilerServices.Extension> _
    Public Function GetEastAsianWidth(ByVal c As Char) As EastAsianWidths
        Return EastAsianWidthDictionary.Default.GetEastAsianWidth(c)
    End Function

    <System.Runtime.CompilerServices.Extension> _
    Public Function IsWideEastAsianWidth(ByVal c As Char) As Boolean
        Dim w As EastAsianWidths = EastAsianWidthDictionary.Default.GetEastAsianWidth(c)
        Return w = EastAsianWidths.Wide OrElse w = EastAsianWidths.FullWidth
    End Function

    <System.Runtime.CompilerServices.Extension> _
    Public Function IsFullWidthEastAsianWidth(ByVal c As Char) As Boolean

        Dim w = EastAsianWidthDictionary.Default.GetEastAsianWidth(c)
        Return w = EastAsianWidths.FullWidth
    End Function

    <System.Runtime.CompilerServices.Extension> _
    Public Function IsNarrowEastAsianWidth(ByVal c As Char) As Boolean
        Dim w As EastAsianWidths = EastAsianWidthDictionary.Default.GetEastAsianWidth(c)
        Return w = EastAsianWidths.Narrow OrElse w = EastAsianWidths.HalfWidth
    End Function

    <System.Runtime.CompilerServices.Extension> _
    Public Function IsHalfEastAsianWidth(ByVal c As Char) As Boolean
        Dim w = EastAsianWidthDictionary.Default.GetEastAsianWidth(c)
        Return w = EastAsianWidths.HalfWidth
    End Function

    Private sjis As System.Text.Encoding = System.Text.Encoding.GetEncoding("shift_JIS")

    <System.Runtime.CompilerServices.Extension> _
    Public Function GetEastAsianWidthFrom_SJIS(ByVal c As Char) As EastAsianWidths

        Dim bs() As Byte = sjis.GetBytes(c.ToString())
        Dim byteCount As Integer = bs.Length
        If (byteCount = 1) Then
            If (bs(0) <= &H1F OrElse bs(0) = &H7F) Then
                Return EastAsianWidths.Neutral
            ElseIf (&HA1 <= bs(0) AndAlso bs(0) <= &HDF) Then
                Return EastAsianWidths.HalfWidth
            Else
                Return EastAsianWidths.Narrow
            End If

        ElseIf (byteCount = 2) Then
            Return EastAsianWidths.Wide 'include FullWidth
        End If
        Return EastAsianWidths.Unknown
    End Function

    <System.Runtime.CompilerServices.Extension> _
    Public Function IsWideEastAsianWidth_SJIS(ByVal c As Char) As Boolean

        Dim byteCount As Integer = sjis.GetByteCount(c.ToString())
        Return byteCount = 2

    End Function

    <System.Runtime.CompilerServices.Extension> _
    Public Function GetEastAsianWidthFrom_VB6(ByVal c As Char) As EastAsianWidths

        If (AscW(c) <= &H1F OrElse AscW(c) = &H7F) Then
            Return EastAsianWidths.Neutral
        End If

        Dim s As String = c.ToString()
        If (s = Microsoft.VisualBasic.Strings.StrConv(s, Microsoft.VisualBasic.VbStrConv.Wide, &H411)) Then
            Return EastAsianWidths.Wide  'include FullWidth
        Else

            Return EastAsianWidths.Narrow 'include HalfWidth
        End If
    End Function

    <System.Runtime.CompilerServices.Extension> _
    Public Function IsWideEastAsianWidth_VB6(ByVal c As Char) As Boolean
        Dim s As String = c.ToString()
        Return s = Microsoft.VisualBasic.Strings.StrConv(s, Microsoft.VisualBasic.VbStrConv.Wide, &H411)
    End Function
End Module