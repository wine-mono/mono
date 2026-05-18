'BEGIN contents of Microsoft.VisualBasic.Compatibility.VB6/LabelArray.vb.cpp
'
' LabelArray.vb.cpp
'
' Authors:
'   Esme Povirk <esme@codeweavers.com>
'
' Copyright (C) 2023 CodeWeavers, Inc
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
Imports System.ComponentModel
Imports System.Windows.Forms

Namespace Microsoft.VisualBasic.Compatibility.VB6


'BEGIN contents of Microsoft.VisualBasic.Compatibility.VB6/ControlArrayCommon.vb.h
'
' ControlArrayCommon.vb.h
'
' Authors:
'   Esme Povirk <esme@codeweavers.com>
'
' Copyright (C) 2023 CodeWeavers, Inc
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


<Obsolete()>
Public Partial Class LabelArray
Inherits BaseControlArray
Implements IExtenderProvider

	Public Sub New(Container as IContainer)
		MyBase.New (Container)
	End Sub

	'BaseControlArray abstract methods
	Protected Overrides Function GetControlInstanceType () As Type
		GetControlInstanceType = GetType(Label)
	End Function

	Protected Overrides Sub HookUpControlEvents (o As Object)
		Control_HookupControlEvents (DirectCast(o,Label))
		Label_HookupControlEvents (DirectCast(o,Label))
	End Sub

	' Public methods
	Default Public ReadOnly Property Item(Index As Short) As Label
		Get
			Return DirectCast(BaseGetItem (Index), Label)
		End Get
	End Property

	Public Function GetIndex (o As Label) As Short
		Return BaseGetIndex (o)
	End Function

	Public Sub SetIndex (o As Label, Index As Short)
		BaseSetIndex (o, Index)
	End Sub

	'IExtenderProvider
	Public Function CanExtend (target As Object) As Boolean Implements IExtenderProvider.CanExtend
		Throw New NotImplementedException()
	End Function

	' Events on Control type
	Private Sub Control_HookupControlEvents (o As Label)
		AddHandler o.AutoSizeChanged, AddressOf Control_AutoSizeChanged
		AddHandler o.Click, AddressOf Control_Click
		AddHandler o.MouseDown, AddressOf Control_MouseDown
		AddHandler o.MouseMove, AddressOf Control_MouseMove
		AddHandler o.MouseUp, AddressOf Control_MouseUp
	End Sub

	Public Event AutoSizeChanged As EventHandler
	Private Sub Control_AutoSizeChanged (sender As Object, e As EventArgs)
		RaiseEvent AutoSizeChanged(sender, e)
	End Sub

	Public Event Click As EventHandler
	Private Sub Control_Click (sender As Object, e As EventArgs)
		RaiseEvent Click(sender, e)
	End Sub

	Public Event MouseDown As MouseEventHandler
	Private Sub Control_MouseDown (sender As Object, e As MouseEventArgs)
		RaiseEvent MouseDown(sender, e)
	End Sub

	Public Event MouseMove As MouseEventHandler
	Private Sub Control_MouseMove (sender As Object, e As MouseEventArgs)
		RaiseEvent MouseMove(sender, e)
	End Sub

	Public Event MouseUp As MouseEventHandler
	Private Sub Control_MouseUp (sender As Object, e As MouseEventArgs)
		RaiseEvent MouseUp(sender, e)
	End Sub


End Class

'END contents of Microsoft.VisualBasic.Compatibility.VB6/ControlArrayCommon.vb.h

Public Partial Class LabelArray
	' Events
	Private Sub Label_HookupControlEvents (o As Label)
		AddHandler o.TextAlignChanged, AddressOf Label_TextAlignChanged
	End Sub

	Public Event TextAlignChanged As EventHandler
	Private Sub Label_TextAlignChanged (sender As Object, e As EventArgs)
		RaiseEvent TextAlignChanged (sender, e)
	End Sub
End Class

End Namespace

'END contents of Microsoft.VisualBasic.Compatibility.VB6/LabelArray.vb.cpp
