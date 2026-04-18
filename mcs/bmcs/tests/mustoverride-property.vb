' EXPECT: ok
' Tests that MustOverride declarations for Sub, Function and
' Property (ReadOnly) parse correctly inside a MustInherit class.
' The inherited-member matching machinery and 'Public MustOverride
' + [extra modifiers] + Property' modifier flow both get exercised.
' Doesn't drive the methods - just verifies the declaration path.
Imports System

Public MustInherit Class Person
    Public MustOverride Sub Greet()
    Public MustOverride Function Age() As Integer
    Public MustOverride ReadOnly Property Name() As String
    Public MustOverride WriteOnly Property Note() As String
End Class

Module Test
    Sub Main()
        Console.WriteLine("ok")
    End Sub
End Module
