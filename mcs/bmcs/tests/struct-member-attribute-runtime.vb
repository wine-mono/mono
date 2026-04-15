' EXPECT: True
Imports System

Structure Sample
    <Obsolete("marker")> _
    Public Overrides Function ToString() As String
        Return "marked"
    End Function
End Structure

Module Test
    Sub Main()
        Dim marker As Reflection.MethodInfo = GetType(Sample).GetMethod("ToString")
        Console.WriteLine(marker.IsDefined(GetType(ObsoleteAttribute), False))
    End Sub
End Module
