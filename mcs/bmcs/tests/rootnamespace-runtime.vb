' BMCS-ARGS: /rootnamespace:vbnc
' EXPECT: 42

Public Class TypeConverter
    Public Shared Function Answer() As Integer
        Return 42
    End Function
End Class

Module Test
    Sub Main()
        System.Console.WriteLine(vbnc.TypeConverter.Answer())
    End Sub
End Module
