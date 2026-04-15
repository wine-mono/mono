' EXPECT: True
' EXPECT: True
' EXPECT: dark
Imports System

Enum Shade
    Light = 1
    Dark = 2
End Enum

Module Test
    Sub Main()
        Dim a As Shade = Shade.Light
        Dim b As Shade = Shade.Dark

        Console.WriteLine(a = Shade.Light)
        Console.WriteLine(a < b)

        Select Case b
            Case Shade.Light
                Console.WriteLine("light")
            Case Shade.Dark
                Console.WriteLine("dark")
        End Select
    End Sub
End Module
