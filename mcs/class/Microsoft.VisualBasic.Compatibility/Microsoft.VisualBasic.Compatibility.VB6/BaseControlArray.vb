'
' BaseControlArray.vb
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
Imports System.Collections
Imports System.ComponentModel

Namespace Microsoft.VisualBasic.Compatibility.VB6

<Obsolete()>
Public MustInherit Class BaseControlArray
	Inherits Component
	Implements ISupportInitialize

	' Constructors
	Protected Sub New ()
		Throw New NotImplementedException()
	End Sub

	Protected Sub New (Container As IContainer)
		components = Container
	End Sub

	' Abstract methods
	Protected MustOverride Function GetControlInstanceType () As Type
	Protected MustOverride Sub HookUpControlEvents (o As Object)

	' Protected fields
	Protected components As IContainer
	Protected controls As New Hashtable
	Protected fIsEndInitCalled As Boolean
	Protected indices As New Hashtable

	' Protected methods
	Protected Function BaseGetItem (Index As Short) As Object
		Return controls(Index)
	End Function

	Protected Function BaseGetIndex (ctl As Object) As Short
		Return CShort(indices(ctl))
	End Function

	Protected Sub BaseSetIndex (ctl As Object, Index As Short, Optional fIsDynamic as Boolean = False)
		' sync with Unload method
		components.Add (DirectCast(ctl, IComponent))
		controls.Add (Index, ctl)
		indices.Add (ctl, Index)
		HookUpControlEvents (ctl)
	End Sub

	' Public Methods
	Public Function Count () As Short
		Return CShort(controls.Count)
	End Function

	Public Sub Load (Index As Short)
		Dim ctl As Object
		Dim t As Type

		t = GetControlInstanceType()

		ctl = t.GetConstructor(New System.Type() {}).Invoke(New Object() {})

		BaseSetIndex (ctl, Index, true)
	End Sub

	Public Function UBound () As Short
		If controls.Count = 0 Then
			Throw New IndexOutOfRangeException("Control array has no members")
		End If
		UBound = Short.MinValue
		For Each idx In controls.Keys
			Dim i As Short
			i = DirectCast(idx, Short)
			If i > UBound
				UBound = i
			End If
		Next idx
	End Function

	Public Sub Unload (Index As Short)
		Dim ctl As Object
		ctl = BaseGetItem (Index)

		components.Remove (DirectCast(ctl, IComponent))
		controls.Remove (Index)
		indices.Remove (ctl)
		' Unregistering events doesn't seem to be possible
	End Sub

	' Method overrides
	Protected Overrides Sub Dispose (disposing as Boolean)
	End Sub

	' ISupportInitialize

	Sub BeginInit () Implements ISupportInitialize.BeginInit
	End Sub

	Sub EndInit () Implements ISupportInitialize.EndInit
		fIsEndInitCalled = True
	End Sub

End Class

End Namespace
