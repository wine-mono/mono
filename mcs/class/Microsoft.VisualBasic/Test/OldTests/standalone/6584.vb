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
Imports System.IO
Public Class TestClass
    Public Function Test() As String
        
        '// make sure all files are closed
        Microsoft.VisualBasic.FileSystem.Reset()
        '// create a file for the test
        Dim SourceFile As String
        Dim DestinationFile As String
        SourceFile = System.IO.Directory.GetCurrentDirectory() + "/data/textfile.txt"
        DestinationFile = System.IO.Directory.GetCurrentDirectory() + "/data/6584.txt"
        Dim f As System.IO.FileInfo = New System.IO.FileInfo(DestinationFile)
        If (f.Exists) Then
            Kill(DestinationFile)
        End If
        FileCopy(SourceFile, DestinationFile)
        Dim fn1 As Integer
        Dim fn2 As Integer
        Dim fn3 As Integer
        'open and close 3 files
        fn1 = FreeFile()
        FileOpen(fn1, DestinationFile, OpenMode.Input, , OpenShare.Shared)
        fn2 = FreeFile()
        FileOpen(fn2, DestinationFile, OpenMode.Input, , OpenShare.Shared)
        fn3 = FreeFile()
        FileOpen(fn3, DestinationFile, OpenMode.Input, , OpenShare.Shared)
        FileClose(fn1, fn2, fn3)
        Return "success"
    End Function
End Class
