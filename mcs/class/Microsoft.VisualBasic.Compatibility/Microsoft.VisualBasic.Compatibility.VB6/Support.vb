'
' Support.vb
'
' Authors:
'   Esme Povirk <esme@codeweavers.com>
'
' Copyright (C) 2026 CodeWeavers, Inc
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
Imports System.Drawing
Imports System.Globalization
Imports System.Windows.Forms
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.CompilerServices

Namespace Microsoft.VisualBasic.Compatibility.VB6

<StandardModule()> <Obsolete()>
Public Module Support
	' Private Fields

	Private ScreenDpiX As Single = 0
	Private ScreenDpiY As Single = 0

	' Private Methods
	Private Sub InitDpi
		If ScreenDpiX = 0 Then
			Using bmp As New Bitmap (1, 1)
				ScreenDpiX = bmp.HorizontalResolution
				ScreenDpiY = bmp.VerticalResolution
			End Using
		End If
	End Sub

	' Public Methods
	Public Function CopyArray (SourceArray As Array) As Array
		Throw New NotImplementedException()
	End Function

	Public Function CursorToIPicture (curs As Cursor) As Object
		Throw New NotImplementedException()
	End Function

	Public Function Eqv (a As Boolean, b As Boolean) As Boolean
		Throw New NotImplementedException()
	End Function

	Public Function Eqv (a As Byte, b As Byte) As Byte
		Throw New NotImplementedException()
	End Function

	Public Function Eqv (a As Short, b As Short) As Short
		Throw New NotImplementedException()
	End Function

	Public Function Eqv (a As Integer, b As Integer) As Integer
		Throw New NotImplementedException()
	End Function

	Public Function Eqv (a As Long, b As Long) As Long
		Throw New NotImplementedException()
	End Function

	Public Function Eqv (a As Object, b As Object) As Object
		Throw New NotImplementedException()
	End Function

	Public Function FontChangeBold (CurrentFont As Font, Bold As Boolean) As Font
		Throw New NotImplementedException()
	End Function

	Public Function FontChangeGdiCharSet (CurrentFont As Font, GdiCharSet As Byte) As Font
		Throw New NotImplementedException()
	End Function

	Public Function FontChangeItalic (CurrentFont As Font, Italic As Boolean) As Font
		Throw New NotImplementedException()
	End Function

	Public Function FontChangeName (CurrentFont As Font, Name As String) As Font
		Throw New NotImplementedException()
	End Function

	Public Function FontChangeSize (CurrentFont As Font, Size As Single) As Font
		Throw New NotImplementedException()
	End Function

	Public Function FontChangeStrikeout (CurrentFont As Font, Strikeout As Boolean) As Font
		Throw New NotImplementedException()
	End Function

	Public Function FontChangeUnderline (CurrentFont As Font, Underline As Boolean) As Font
		Throw New NotImplementedException()
	End Function

	Public Function FontToIFont (fnt As Font) As Object
		Throw New NotImplementedException()
	End Function

	Public Function Format (Expression As Object, Optional Style As String = "", Optional DayOfWeek As FirstDayOfWeek = FirstDayOfWeek.Sunday, Optional WeekOfYear As FirstWeekOfYear = FirstWeekOfYear.Jan1) As String
		Throw New NotImplementedException()
	End Function

	Public Function FromPixelsUserHeight (Height As Double, ScaleHeight As Double, OriginalHeightInPixels As Integer) As Double
		Throw New NotImplementedException()
	End Function

	Public Function FromPixelsUserWidth (Width As Double, ScaleWidth As Double, OriginalWidthInPixels As Integer) As Double
		Throw New NotImplementedException()
	End Function

	Public Function FromPixelsUserX (X As Double, ScaleLeft As Double, ScaleWidth As Double, OriginalWidthInPixels As Integer) As Double
		Throw New NotImplementedException()
	End Function

	Public Function FromPixelsUserY (Y As Double, ScaleTop As Double, ScaleHeight As Double, OriginalHeightInPixels As Integer) As Double
		Throw New NotImplementedException()
	End Function

	Public Function FromPixelsX (X As Double, ToScale As ScaleMode) As Double
		Throw New NotImplementedException()
	End Function

	Public Function FromPixelsY (Y As Double, ToScale As ScaleMode) As Double
		Throw New NotImplementedException()
	End Function

	Public Function GetActiveControl () As Control
		Throw New NotImplementedException()
	End Function

	Public Function GetCancel (btn As Button) As Boolean
		Throw New NotImplementedException()
	End Function

	Public Function GetDefault (btn As Button) As Boolean
		Throw New NotImplementedException()
	End Function

	Public Function GetEXEName () As String
		Throw New NotImplementedException()
	End Function

	Public Function GetHInstance () As IntPtr
		Throw New NotImplementedException()
	End Function

	Public Function GetItemData (Control As Control, Index As Integer) As Integer
		Throw New NotImplementedException()
	End Function

	Public Function GetItemString (Control As Control, Index As Integer) As String
		Throw New NotImplementedException()
	End Function

	Public Function GetPath () As String
		Throw New NotImplementedException()
	End Function

	Public Function IconToIPicture (ico As Icon) As Object
		Throw New NotImplementedException()
	End Function

	Public Function IFontToFont (objFnt As Object) As Font
		Throw New NotImplementedException()
	End Function

	Public Function ImageToIPicture (img As Image) As Object
		Throw New NotImplementedException()
	End Function

	Public Function ImageToIPictureDisp (img As Image) As Object
		Throw New NotImplementedException()
	End Function

	Public Function Imp (a As Boolean, b As Boolean) As Boolean
		Throw New NotImplementedException()
	End Function

	Public Function Imp (a As Byte, b As Byte) As Byte
		Throw New NotImplementedException()
	End Function

	Public Function Imp (a As Short, b As Short) As Short
		Throw New NotImplementedException()
	End Function

	Public Function Imp (a As Integer, b As Integer) As Integer
		Throw New NotImplementedException()
	End Function

	Public Function Imp (a As Long, b As Long) As Long
		Throw New NotImplementedException()
	End Function

	Public Function Imp (a As Object, b As Object) As Object
		Throw New NotImplementedException()
	End Function

	Public Function IPictureDispToImage (pict As Object) As Image
		Throw New NotImplementedException()
	End Function

	Public Function IPictureToImage (pict As Object) As Image
		Throw New NotImplementedException()
	End Function

	Public Function LoadResData (ID As Object, restype As Object) As Object
		Throw New NotImplementedException()
	End Function

	Public Function LoadResData (ID As Object, restype As Object, Culture As CultureInfo) As Object
		Throw New NotImplementedException()
	End Function

	Public Function LoadResPicture (ID As Object, restype As LoadResConstants) As Object
		Throw New NotImplementedException()
	End Function

	Public Function LoadResPicture (ID As Object, restype As LoadResConstants, Culture As CultureInfo) As Object
		Throw New NotImplementedException()
	End Function

	Public Function LoadResString (ID As Integer) As String
		Throw New NotImplementedException()
	End Function

	Public Function LoadResString (ID As Integer, Culture As CultureInfo) As String
		Throw New NotImplementedException()
	End Function

	Public Function PixelsToTwipsX (X As Double) As Double
		InitDpi
		Return X * 1440 / ScreenDpiX
	End Function

	Public Function PixelsToTwipsY (Y As Double) As Double
		InitDpi
		Return Y * 1440 / ScreenDpiY
	End Function

	Public Sub SendKeys (Keys As String, Optional Wait As Boolean = False)
		Throw New NotImplementedException()
	End Sub

	Public Sub SetCancel (btn As Button, Cancel As Boolean)
		Throw New NotImplementedException()
	End Sub

	Public Sub SetDefault (btn As Button, [Default] As Boolean)
		Throw New NotImplementedException()
	End Sub

	Public Sub SetItemData (Control As Control, Index As Integer, ItemData As Integer)
		Throw New NotImplementedException()
	End Sub

	Public Sub SetItemString (Control As Control, Index As Integer, ItemString As String)
		Throw New NotImplementedException()
	End Sub

	Public Sub SetResourceBaseName (BaseFileName As String)
		Throw New NotImplementedException()
	End Sub

	Public Sub SetResourceBaseName (ProjectNamespace As String, BaseFileName As String)
		Throw New NotImplementedException()
	End Sub

	Public Sub ShowForm (Form As Form, Optional Modal As Integer = 0, Optional OwnerForm As Form = Nothing)
		Throw New NotImplementedException()
	End Sub

	Public Function TabLayout (ParamArray Args As Object()) As String
		Throw New NotImplementedException()
	End Function

	Public Function ToPixelsUserHeight (Height As Double, ScaleHeight As Double, OriginalHeightInPixels As Integer) As Double
		Throw New NotImplementedException()
	End Function

	Public Function ToPixelsUserWidth (Width As Double, ScaleWidth As Double, OriginalWidthInPixels As Integer) As Double
		Throw New NotImplementedException()
	End Function

	Public Function ToPixelsUserX (X As Double, ScaleLeft As Double, ScaleWidth As Double, OriginalWidthInPixels As Integer) As Double
		Throw New NotImplementedException()
	End Function

	Public Function ToPixelsUserY (Y As Double, ScaleTop As Double, ScaleHeight As Double, OriginalHeightInPixels As Integer) As Double
		Throw New NotImplementedException()
	End Function

	Public Function ToPixelsX (X As Double, FromScale As ScaleMode) As Double
		Throw New NotImplementedException()
	End Function

	Public Function ToPixelsY (Y As Double, FromScale As ScaleMode) As Double
		Throw New NotImplementedException()
	End Function

	Public Function TwipsPerPixelX () As Single
		InitDpi
		Return 1440 / ScreenDpiX
	End Function

	Public Function TwipsPerPixelY () As Single
		InitDpi
		Return 1440 / ScreenDpiY
	End Function

	Public Function TwipsToPixelsX (X As Double) As Double
		InitDpi
		Return X * ScreenDpiX / 1440
	End Function

	Public Function TwipsToPixelsY (Y As Double) As Double
		InitDpi
		Return Y * ScreenDpiY / 1440
	End Function

	Public Sub ValidateControls (Form As ContainerControl)
		Throw New NotImplementedException()
	End Sub

	Public Sub WhatsThisMode (Form As Form)
		Throw New NotImplementedException()
	End Sub

	Public Sub ZOrder (Control As Control, Position As Integer)
		Throw New NotImplementedException()
	End Sub

End Module

End Namespace
