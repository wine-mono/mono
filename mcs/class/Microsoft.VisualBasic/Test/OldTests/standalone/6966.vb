  '
  ' Copyright (c) 2002-2003 Mainsoft Corporation.
  '
  ' Permission is hereby granted, free of charge, to any person obtaining a
  ' copy of this software and associated documentation files (the "Software"),
  ' to deal in the Software without restriction, including without limitation
  ' the rights to use, copy, modify, merge, publish, distribute, sublicense,
  ' and/or sell copies of the Software, and to permit persons to whom the
  ' Software is furnished to do so, subject to the following conditions:
  ' 
  ' The above copyright notice and this permission notice shall be included in
  ' all copies or substantial portions of the Software.
  ' 
  ' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  ' IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  ' FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  ' AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  ' LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
  ' FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
  ' DEALINGS IN THE SOFTWARE.
  '
Imports Microsoft.VisualBasic
Public Class TestClass
    Public Function Test() As String
        Dim fput As Integer
        Dim fget As Integer
        
        '// make sure all files are closed
        Microsoft.VisualBasic.FileSystem.Reset()
        Dim oDT2_1 As Boolean = True
        Dim oDT2_2 As Boolean
        Dim oDT3_1 As Byte = 1
        Dim oDT3_2 As Byte
        Dim oDT4_1 As Short = 100
        Dim oDT4_2 As Short
        Dim oDT5_1 As Integer = 1000
        Dim oDT5_2 As Integer
        Dim oDT6_1 As Long = 100000
        Dim oDT6_2 As Long
        Dim oDT7_1 As Char = "c"c
        Dim oDT7_2 As Char
        Dim oDT8_1 As Single = 2.2
        Dim oDT8_2 As Single
        Dim oDT9_1 As Double = 8.8
        Dim oDT9_2 As Double
        Dim oDT10_1 As Decimal = 10000000
        Dim oDT10_2 As Decimal
        Dim oDT11_1 As String = "zzz"
        Dim oDT11_2 As String
        Dim oDT12_1 As Date = #5/31/1993#
        Dim oDT12_2 As Date
        fput = FreeFile()
        FileOpen(fput, "data/6966.txt", OpenMode.Output)
        PrintLine(fput, oDT2_1)
        PrintLine(fput, oDT3_1)
        PrintLine(fput, oDT4_1)
        PrintLine(fput, oDT5_1)
        PrintLine(fput, oDT6_1)
        PrintLine(fput, oDT7_1)
        PrintLine(fput, oDT8_1)
        PrintLine(fput, oDT9_1)
        PrintLine(fput, oDT10_1)
        PrintLine(fput, oDT11_1)
        PrintLine(fput, oDT12_1)
        FileClose(fput)
        fget = FreeFile()
        FileOpen(fget, "data/6966.txt", OpenMode.Binary)
        Input(fget, oDT2_2)
        Input(fget, oDT3_2)
        Input(fget, oDT4_2)
        Input(fget, oDT5_2)
        Input(fget, oDT6_2)
        Input(fget, oDT7_2)
        Input(fget, oDT8_2)
        Input(fget, oDT9_2)
        Input(fget, oDT10_2)
        Input(fget, oDT11_2)
        Input(fget, oDT12_2)
        FileClose(fget)
        If oDT2_1 <> oDT2_2 Then Return "failed"
        If oDT3_1 <> oDT3_2 Then Return "failed"
        If oDT4_1 <> oDT4_2 Then Return "failed"
        If oDT5_1 <> oDT5_2 Then Return "failed"
        If oDT6_1 <> oDT6_2 Then Return "failed"
        If oDT7_1 <> oDT7_2 Then Return "failed"
        If oDT8_1 <> oDT8_2 Then Return "failed"
        If oDT9_1 <> oDT9_2 Then Return "failed"
        If oDT10_1 <> oDT10_2 Then Return "failed"
        If oDT11_1 <> oDT11_2 Then Return "failed"
        If oDT12_1 <> oDT12_2 Then Return "failed"
        Return "success"
    End Function
End Class
