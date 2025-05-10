'
' LinuxDriver.vb
'
' Authors:
'   Rolf Bjarne Kvinge (RKvinge@novell.com>
'
' Copyright (C) 2007 Novell (http://www.novell.com)
'
' Permission is hereby granted, free of charge, to any person obtaining
' a copy of this software and associated documentation files (the
' "Software"), to deal in the Software without restriction, including
' without limitation the rights to use, copy, modify, merge, publish,
' distribute, sublicense, and/or sell copies of the Software, and to
' permit persons to whom the Software is furnished to do so, subject to
' the following conditions:
' 
' The above copyright notice and this permission notice shall be
' included in all copies or substantial portions of the Software.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
' EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
' MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
' NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
' LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
' OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
' WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
'
Imports System
Imports System.Globalization
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text

Namespace Microsoft.VisualBasic.OSSpecific
    Friend Class LinuxDriver
        Inherits OSDriver

        Public Overrides Sub SetDate(ByVal Value As Date)
            Dim Now As System.DateTime = DateTime.Now
            Dim NewDate As System.DateTime = New DateTime(Value.Year, Value.Month, Value.Day, Now.Hour, Now.Minute, Now.Second, Now.Millisecond)
            Dim secondsTimeSpan As System.TimeSpan = NewDate.ToUniversalTime().Subtract(New DateTime(1970, 1, 1, 0, 0, 0))
            Dim seconds As Integer = CType(secondsTimeSpan.TotalSeconds, Integer)

#If TARGET_JVM = False Then
            If (stime(seconds) = -1) Then
                Throw New UnauthorizedAccessException("The caller is not the super-user.")
            End If
#Else
            MyBase.SetTime (Value)
#End If
        End Sub


        Public Overrides Sub SetTime(ByVal Value As Date)
            Dim Now As System.DateTime = DateTime.Now
            Dim NewDate As System.DateTime = New DateTime(Now.Year, Now.Month, Now.Day, Value.Hour, Value.Minute, Value.Second, Value.Millisecond)
            Dim secondsTimeSpan As System.TimeSpan = NewDate.ToUniversalTime().Subtract(New DateTime(1970, 1, 1, 0, 0, 0))
            Dim seconds As Integer = CType(secondsTimeSpan.TotalSeconds, Integer)

#If TARGET_JVM = False Then
            If (stime(seconds) = -1) Then
                Throw New UnauthorizedAccessException("The caller is not the super-user.")
            End If
#Else
            MyBase.SetTime (Value)
#End If
        End Sub

#If TARGET_JVM = False Then
        <DllImport("libc", EntryPoint:="stime", _
           SetLastError:=True, CharSet:=CharSet.Unicode, _
           ExactSpelling:=True, _
           CallingConvention:=CallingConvention.StdCall)> _
        Friend Shared Function stime(ByRef t As Integer) As Integer
            ' Leave function empty - DllImport attribute forwards calls to stime to
            ' stime in libc.dll
        End Function
#End If

        Private Function GetXdgDataHome() As String
            GetXdgDataHome = Environment.GetEnvironmentVariable("XDG_DATA_HOME")

            If GetXdgDataHome Is Nothing Then
                GetXdgDataHome = Environment.GetEnvironmentVariable("HOME") & "/.local/share"
            End If
        End Function

        Private Sub WriteDesktopGroup(ByVal output As StreamWriter, ByVal name As String)
            output.Write ("[")
            output.Write (name)
            output.Write ("]")
            output.Write (Constants.vbLf)
        End Sub

        Private Sub WriteDesktopString(ByVal output As StreamWriter, ByVal key As String, ByVal val As String)
            output.Write (key)
            output.Write ("=")
            output.Write (val.Replace("\", "\\").Replace(Constants.vbLf, "\n").Replace(Constants.vbCr, "\r").Replace(Constants.vbTab, "\t"))
            output.Write (Constants.vbLf)
        End Sub

        Private Function CreateNewUtf8File(ByVal pathname As String) As StreamWriter
            Dim stream As FileStream
            stream = New FileStream(pathname, FileMode.CreateNew, FileAccess.Write, FileShare.Read)
            Return New StreamWriter(stream, new UTF8Encoding())
        End Function

        Private Function CreateUniqueUtf8File(ByVal directory As String, ByVal filename As String,
                ByVal extension As String, ByVal hashcode as Integer, ByRef outfilename As String) As StreamWriter
            Dim result As StreamWriter

            outfilename = Nothing

            Try
                outfilename = Path.Combine(directory, filename & extension)
                result = CreateNewUtf8File(outfilename)
                Return result
            Catch e As IOException
                ' file exists
            End Try

            While True
                outfilename = Path.Combine(directory, filename & hashcode & extension)
                Try
                    result = CreateNewUtf8File(outfilename)
                    Return result
                Catch e As IOException
                    ' file exists
                End Try
                hashcode = hashcode + 1
            End While
        End Function

        Public Overrides Sub TrashPath(ByVal pathname As String)
            ' Identify user's trash directory
            Dim xdgTrash As String
            xdgTrash = Path.Combine(GetXdgDataHome(), "Trash")
            ' Ensure important paths exist
            Directory.CreateDirectory (Path.Combine(xdgTrash, "info"))
            Directory.CreateDirectory (Path.Combine(xdgTrash, "files"))
            ' Normalize path
            pathname = Path.GetFullPath(pathname).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            ' Create info file
            Dim infoFile As StreamWriter
            Dim infoFileName As String
            Dim succeeded as Boolean
            infoFile = CreateUniqueUtf8File(Path.Combine(xdgTrash, "info"), Path.GetFileName(pathname), ".trashinfo", pathname.GetHashCode() Xor GetType(LinuxDriver).GetHashCode(), infoFileName)
            Try
                ' Write info file
                WriteDesktopGroup (infoFile, "Trash Info")
                WriteDesktopString (infoFile, "Path", Uri.EscapeUriString(pathname))
                WriteDesktopString (infoFile, "DeletionDate", DateTime.Now.ToString("yyyyMMddTHH:mm:ss", CultureInfo.InvariantCulture))
                infoFile.Flush
                ' Move item to trash
                Dim trashFileName As String
                trashFileName = Path.Combine(XdgTrash, "files", Path.GetFileNameWithoutExtension(Path.GetFileName(infoFileName)))
                If Directory.Exists (pathname) Then
                    Directory.Move (pathname, trashFileName)
                Else
                    File.Move (pathname, trashFileName)
                End If
                succeeded = True
            Finally
                infoFile.Dispose
                If Not succeeded
                    File.Delete (infoFileName)
                End If
            End Try
        End Sub

    End Class
End Namespace
