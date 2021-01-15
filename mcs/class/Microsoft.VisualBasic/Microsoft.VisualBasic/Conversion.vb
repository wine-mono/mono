'
' Conversion.vb
'
' Author:
'   Mizrahi Rafael (rafim@mainsoft.com)
'   Guy Cohen (guyc@mainsoft.com)

'
' Copyright (C) 2002-2006 Mainsoft Corporation.
' Copyright (C) 2004-2006 Novell, Inc (http://www.novell.com)
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
Imports System.Reflection
Imports Microsoft.VisualBasic.CompilerServices
Imports System.Globalization

Namespace Microsoft.VisualBasic
    <StandardModule()> _
    Public NotInheritable Class Conversion
        Public Shared Function ErrorToString() As String
            Return Information.Err.Description
        End Function
        Public Shared Function ErrorToString(ByVal ErrorNumber As Integer) As String
            Dim rm As New Resources.ResourceManager("strings", [Assembly].GetExecutingAssembly())

            Dim strDescription As String

#If TRACE Then
            Console.WriteLine("TRACE:Conversion.ErrorToString:input:" + ErrorNumber.ToString())
#End If

            'FIXMSDN: If (ErrorNumber < 0) Or (ErrorNumber >= 65535) Then
            If (ErrorNumber >= 65535) Then
                Throw New ArgumentException("Error number must be within the range 0 to 65535.")
            End If

            If (ErrorNumber = 0) Then
                Return ""
            End If

            strDescription = rm.GetString("ERR" + ErrorNumber.ToString())

            'Application-defined or object-defined error.
            If strDescription Is Nothing Then
                strDescription = rm.GetString("ERR95")
            End If

            Return strDescription
        End Function
        Public Shared Function Fix(ByVal Number As Decimal) As Decimal
            Return Math.Sign(Number) * Conversion.Int(System.Math.Abs(Number))
        End Function
        Public Shared Function Fix(ByVal Number As Double) As Double
            Return Math.Sign(Number) * Conversion.Int(System.Math.Abs(Number))
        End Function
        Public Shared Function Fix(ByVal Number As Integer) As Integer
            Return Number
        End Function
        Public Shared Function Fix(ByVal Number As Long) As Long
            Return Number
        End Function
        Public Shared Function Fix(ByVal Number As Object) As Object
            'FIXME:ArgumentException 5 Number is not a numeric type. 
            If Number Is Nothing Then
                Throw New ArgumentNullException("Number", "Value can not be null.")
            End If

            If TypeOf Number Is Byte Then
                Return Conversion.Fix(Convert.ToByte(Number))
            ElseIf TypeOf Number Is Boolean Then
                If (Convert.ToBoolean(Number)) Then
                    Return 1
                End If
                Return 0
            ElseIf TypeOf Number Is Long Then
                Return Conversion.Fix(Convert.ToInt64(Number))
            ElseIf TypeOf Number Is Decimal Then
                Return Conversion.Fix(Convert.ToDecimal(Number))
            ElseIf TypeOf Number Is Short Then
                Return Conversion.Fix(Convert.ToInt16(Number))
            ElseIf TypeOf Number Is Integer Then
                Return Conversion.Fix(Convert.ToInt32(Number))
            ElseIf TypeOf Number Is Double Then
                Return Conversion.Fix(Convert.ToDouble(Number))
            ElseIf TypeOf Number Is Single Then
                Return Conversion.Fix(Convert.ToSingle(Number))
            ElseIf TypeOf Number Is String Then
                Return Conversion.Fix(DoubleType.FromString(Number.ToString()))
            ElseIf TypeOf Number Is Char Then
                Return Conversion.Fix(DoubleType.FromString(Number.ToString()))
            Else 'Date, Object
                Throw New System.ArgumentException("Type of argument 'Number' is '" + Number.GetType.FullName + "', which is not numeric.")
            End If

        End Function
        Public Shared Function Fix(ByVal Number As Short) As Short
            Return Number
        End Function
        Public Shared Function Fix(ByVal Number As Single) As Single
            Return Math.Sign(Number) * Conversion.Int(System.Math.Abs(Number))
        End Function

        Public Shared Function Hex(ByVal Number As Byte) As String
            Return Convert.ToString(Number, 16).ToUpper
        End Function

        Public Shared Function Hex(ByVal Number As Integer) As String
            Return Convert.ToString(Number, 16).ToUpper
        End Function

        Public Shared Function Hex(ByVal Number As Long) As String
            Return Convert.ToString(Number, 16).ToUpper
        End Function

        Public Shared Function Hex(ByVal Number As Short) As String
            Return Convert.ToString(Number, 16).ToUpper
        End Function

        Public Shared Function Hex(ByVal Number As Object) As String

            If Number Is Nothing Then
                Throw New System.ArgumentNullException("Number", "Value cannot be null.")
            End If

            If (TypeOf Number Is IConvertible) Then
                Dim tc As TypeCode = CType(Number, IConvertible).GetTypeCode()

                Select Case tc
                    Case TypeCode.Byte
                        Return Hex(Convert.ToByte(Number))
                    Case TypeCode.Decimal
                        Return Hex(SizeDown(Convert.ToInt64(Number)))
                    Case TypeCode.Double
                        Return Hex(SizeDown(Convert.ToInt64(Number)))
                    Case TypeCode.Int16
                        Return Hex(Convert.ToInt16(Number))
                    Case TypeCode.Int32
                        Return Hex(Convert.ToInt32(Number))
                    Case TypeCode.Int64
                        Return Hex(Convert.ToInt64(Number))
                    Case TypeCode.Single
                        Return Hex(SizeDown(Convert.ToInt32(Number)))
                    Case TypeCode.String
                        Dim strNumber As String
                        strNumber = Number.ToString
                        If strNumber.StartsWith("&") Then
                            If Char.ToUpper(strNumber.Chars(1)) = "O"c Then
                                Return Hex(SizeDown(Convert.ToInt64(strNumber.Substring(2), 8)))
                            ElseIf Char.ToUpper(strNumber.Chars(1)) = "H"c Then
                                Return Hex(SizeDown(Convert.ToInt64(strNumber.Substring(2), 16)))
                            Else
                                Return Hex(SizeDown(Convert.ToInt64(Number)))
                            End If
                        Else
                            Return Hex(SizeDown(Convert.ToInt64(Number)))
                        End If
                    Case TypeCode.SByte
                        Return Hex(Convert.ToSByte(Number))
                    Case TypeCode.UInt16
                        Return Hex(Convert.ToUInt16(Number))
                    Case TypeCode.UInt32
                        Return Hex(Convert.ToUInt32(Number))
                    Case TypeCode.UInt64
                        Return Hex(Convert.ToUInt64(Number))
                    Case Else
                        Throw New System.ArgumentException("Argument 'Number' cannot be converted to type '" + Number.GetType.FullName + "'.")

                End Select
            Else
                Throw New System.ArgumentException("Argument 'Number' is not a number.")
            End If
        End Function

        Private Shared Function SizeDown(ByVal num As Long) As Object
            'If (num <= Byte.MaxValue And num >= 0) Then
            '    Return CType(num, Byte)
            'End If

            'If (num <= SByte.MaxValue And num >= SByte.MinValue) Then
            '    Return CType(num, SByte)
            'End If

            'If (num <= Int16.MaxValue And num >= Int16.MinValue) Then
            '    Return CType(num, Int16)
            'End If

            'If (num <= UInt16.MaxValue And num >= 0) Then
            '    Return CType(num, UInt16)
            'End If

            If (num <= Int32.MaxValue And num >= Int32.MinValue) Then
                Return CType(num, Int32)
            End If
            If (num <= UInt32.MaxValue And num >= 0) Then
                Return CType(num, UInt32)
            End If
            Return num
        End Function

        Public Shared Function Int(ByVal Number As Decimal) As Decimal
            Return Decimal.Floor(Number)
        End Function
        Public Shared Function Int(ByVal Number As Double) As Double
            Return Math.Floor(Number)
        End Function
        Public Shared Function Int(ByVal Number As Integer) As Integer
            Return Number
        End Function
        Public Shared Function Int(ByVal Number As Long) As Long
            Return Number
        End Function
        Public Shared Function Int(ByVal Number As Object) As Object
            'FIXME:ArgumentException 5 Number is not a numeric type. 
            If Number Is Nothing Then
                Throw New ArgumentNullException("Number", "Value can not be null.")
            End If

            If TypeOf Number Is Byte Then
                Return Conversion.Int(Convert.ToByte(Number))
            ElseIf TypeOf Number Is Boolean Then
                Return Conversion.Int(Convert.ToDouble(Number))
            ElseIf TypeOf Number Is Long Then
                Return Conversion.Int(Convert.ToInt64(Number))
            ElseIf TypeOf Number Is Decimal Then
                Return Conversion.Int(Convert.ToDecimal(Number))
            ElseIf TypeOf Number Is Short Then
                Return Conversion.Int(Convert.ToInt16(Number))
            ElseIf TypeOf Number Is Integer Then
                Return Conversion.Int(Convert.ToInt32(Number))
            ElseIf TypeOf Number Is Double Then
                Return Conversion.Int(Convert.ToDouble(Number))
            ElseIf TypeOf Number Is Single Then
                Return Conversion.Int(Convert.ToSingle(Number))
            ElseIf TypeOf Number Is String Then
                Return Conversion.Int(Convert.ToDouble(Number))
            ElseIf TypeOf Number Is Char Then
                Return Conversion.Int(Convert.ToInt16(Number))
            Else 'Date, Object
                Throw New System.ArgumentException("Type of argument 'Number' is '" + Number.GetType.FullName + "', which is not numeric.")
            End If

        End Function
        Public Shared Function Int(ByVal Number As Short) As Short
            Return Number
        End Function
        Public Shared Function Int(ByVal Number As Single) As Single
            Return System.Convert.ToSingle(Math.Floor(Number))
        End Function
        Public Shared Function Oct(ByVal Number As Byte) As String
            Return Convert.ToString(Number, 8).ToUpper
        End Function
        Public Shared Function Oct(ByVal Number As Integer) As String
            Return Convert.ToString(Number, 8).ToUpper
        End Function
        Public Shared Function Oct(ByVal Number As Long) As String
            Return Convert.ToString(Number, 8).ToUpper
        End Function
        Public Shared Function Oct(ByVal Number As Object) As String
            If Number Is Nothing Then
                Throw New System.ArgumentNullException("Number", "Value cannot be null.")
            End If

            If (TypeOf Number Is IConvertible) Then
                Dim tc As TypeCode = CType(Number, IConvertible).GetTypeCode()

                Select Case tc
                    Case TypeCode.Byte
                        Return Oct(Convert.ToByte(Number))
                    Case TypeCode.Decimal
                        Return Oct(SizeDown(Convert.ToInt64(Number)))
                    Case TypeCode.Double
                        Return Oct(SizeDown(Convert.ToInt64(Number)))
                    Case TypeCode.Int16
                        Return Oct(Convert.ToInt16(Number))
                    Case TypeCode.Int32
                        Return Oct(Convert.ToInt32(Number))
                    Case TypeCode.Int64
                        Return Oct(Convert.ToInt64(Number))
                    Case TypeCode.Single
                        Return Oct(SizeDown(Convert.ToInt32(Number)))
                    Case TypeCode.String
                        Dim strNumber As String
                        strNumber = Number.ToString
                        If strNumber.StartsWith("&") Then
                            If Char.ToUpper(strNumber.Chars(1)) = "O"c Then
                                Return Oct(SizeDown(Convert.ToInt64(strNumber.Substring(2), 8)))
                            ElseIf Char.ToUpper(strNumber.Chars(1)) = "H"c Then
                                Return Oct(SizeDown(Convert.ToInt64(strNumber.Substring(2), 16)))
                            Else
                                Return Oct(SizeDown(Convert.ToInt64(Number)))
                            End If
                        Else
                            Return Oct(SizeDown(Convert.ToInt64(Number)))
                        End If
                    Case TypeCode.SByte
                        Return Oct(Convert.ToSByte(Number))
                    Case TypeCode.UInt16
                        Return Oct(Convert.ToUInt16(Number))
                    Case TypeCode.UInt32
                        Return Oct(Convert.ToUInt32(Number))
                    Case TypeCode.UInt64
                        Return Oct(Convert.ToUInt64(Number))
                    Case Else
                        Throw New System.ArgumentException("Argument 'Number' cannot be converted to type '" + Number.GetType.FullName + "'.")

                End Select
            Else
                Throw New System.ArgumentException("Argument 'Number' is not a number.")
            End If
        End Function

        Public Shared Function Oct(ByVal Number As Short) As String
            Return Convert.ToString(Number, 8).ToUpper
        End Function

        Public Shared Function Str(ByVal Number As Object) As String
            If Number Is Nothing Then
                Throw New System.ArgumentNullException("Number", "Value cannot be null.")
            End If

            If TypeOf Number Is Byte Then
                If Convert.ToByte(Number) > 0 Then
                    Return " " + Convert.ToString(Number, CultureInfo.InvariantCulture)
                Else
                    Return Convert.ToString(Number, CultureInfo.InvariantCulture)
                End If
            ElseIf TypeOf Number Is Short Then
                If Convert.ToInt16(Number) > 0 Then
                    Return " " + Convert.ToString(Number, CultureInfo.InvariantCulture)
                Else
                    Return Convert.ToString(Number, CultureInfo.InvariantCulture)
                End If
            ElseIf TypeOf Number Is Integer Then
                If Convert.ToInt32(Number) > 0 Then
                    Return " " + Convert.ToString(Number, CultureInfo.InvariantCulture)
                Else
                    Return Convert.ToString(Number, CultureInfo.InvariantCulture)
                End If
            ElseIf TypeOf Number Is Long Then
                If Convert.ToInt64(Number) > 0 Then
                    Return " " + Convert.ToString(Number, CultureInfo.InvariantCulture)
                Else
                    Return Convert.ToString(Number, CultureInfo.InvariantCulture)
                End If
            ElseIf TypeOf Number Is Double Then
                If Convert.ToDouble(Number) > 0 Then
                    Return " " + Convert.ToString(Number, CultureInfo.InvariantCulture)
                Else
                    Return Convert.ToString(Number, CultureInfo.InvariantCulture)
                End If
            ElseIf TypeOf Number Is Decimal Then
                If Convert.ToDecimal(Number) > 0 Then
                    Return " " + Convert.ToString(Number, CultureInfo.InvariantCulture)
                Else
                    Return Convert.ToString(Number, CultureInfo.InvariantCulture)
                End If
            ElseIf TypeOf Number Is Single Then
                If Convert.ToSingle(Number) > 0 Then
                    Return " " + Convert.ToString(Number, CultureInfo.InvariantCulture)
                Else
                    Return Convert.ToString(Number, CultureInfo.InvariantCulture)
                End If
            ElseIf TypeOf Number Is String Then
                Dim value As Double
                Try
                    value = Convert.ToDouble(Number)
                Catch exception As System.FormatException
                    Throw New System.InvalidCastException("Argument 'Number' cannot be converted to a numeric value.")
                End Try
                if value > 0
                    Return " " + Convert.ToString(value, CultureInfo.InvariantCulture)
                Else
                    Return Convert.ToString(value, CultureInfo.InvariantCulture)
                End if
            ElseIf TypeOf Number is Boolean Then
                If Convert.ToBoolean(Number) Then
                    Return "True"
                Else
                    Return "False"
                End If
            Else
                Throw New System.InvalidCastException("Argument 'Number' cannot be converted to a numeric value.")
            End If
        End Function
        Public Shared Function Val(ByVal Expression As Char) As Integer
            'only '0' - '9' are acceptable
            If Strings.Asc(Expression) >= Strings.Asc("0"c) And Strings.Asc(Expression) <= Strings.Asc("9"c) Then
                Return Strings.Asc(Expression) - Strings.Asc("0"c)
            Else
                'everything else is 0
                Return 0
            End If
        End Function
        Public Shared Function Val(ByVal Expression As Object) As Double
            If Expression Is Nothing Then
                Return Val("")
            End If

            If TypeOf Expression Is Char Then
                Return Val(Convert.ToChar(Expression))
            ElseIf TypeOf Expression Is String Then
                Return Val(Convert.ToString(Expression))
            ElseIf TypeOf Expression Is Boolean Then
                Return Val(Convert.ToString((-1) * Convert.ToInt16(Expression)))
            ElseIf TypeOf Expression Is Integer Then
                Return Val(Convert.ToString(Expression))
            ElseIf TypeOf Expression Is System.Enum Then
                Return Val(Convert.ToString(Convert.ToInt32(Expression)))
            ElseIf TypeOf Expression Is System.Single Then
                Return Convert.ToDouble(Expression)
            ElseIf TypeOf Expression Is System.Double Then
                Return Convert.ToDouble(Expression)
                'FIXME: add more types. Return Val(StringType.FromObject(Expression))
            Else
                Throw New System.ArgumentException("Argument 'Expression' cannot be converted to type '" + Expression.GetType.FullName + "'.")
            End If
        End Function

        Public Shared Function Val(ByVal InputStr As String) As Double
            Dim l as Integer

            If InputStr Is Nothing Then
                Return 0.0
            End If

#If TRACE Then
            Console.WriteLine("TRACE:Conversion.Val:input:" + InputStr)
#End If

            l = InputStr.Length
            if l = 0 Then
                Return 0.0
            End If

            Dim c As Char
            Dim cis As UShort
            Dim pos as Integer
            Dim mantissa As Double
            Dim mantissa_coef As Double
            Dim mantissa_sign As Integer
            Dim exponent As Double
            Dim exponent_sign As Integer
            Dim in_fraction As Boolean
            Dim in_exponent As Boolean
            Dim expect_sign As Boolean

            pos = 0
            c = InputStr.Chars(pos)
            While pos < l And (c = " "c Or c = ControlChars.Tab Or c = ControlChars.Cr Or c = ControlChars.Lf)
                pos = pos + 1
                if pos = l Then
                    Return 0.0
                End If
                c = InputStr.Chars(pos)
            End While

            If c = "&"c Then
                Dim is_hex As Boolean
                Dim value as Int64
                Dim p as Integer
                Dim len as Integer

                If (l - pos ) < 3 Then
                    Return 0.0
                End If

                c = InputStr.Chars(pos + 1)
                if c = "h"c Or c = "H"c Then
                    is_hex = True
                ElseIf c <> "o"c And c <> "O"c Then
                    Return 0.0
                End If

                value = 0
                pos = pos + 2
                If is_hex Then
                    Dim digit as UInt16

                    len = 0

                    For p = 0 To l - pos - 1
                        c = InputStr.Chars(pos + p)
                        If c = " "c Or c = ControlChars.Tab Or c = ControlChars.Cr Or c = ControlChars.Lf Then
                            Continue For
                        End If

                        cis = Convert.ToUInt16(c)

                        If cis >= 48 And cis <= 57 Then
                            digit = cis - 48us
                        ElseIf cis >= 97 And cis <= 102
                            digit = cis - 97us + 10us
                        ElseIf cis >= 65 And cis <= 70
                            digit = cis - 65us + 10us
                        Else
                            Exit For
                        End If

                        len = len + 1

                        If len = 16 Then
                            If (value And (1l << 59)) <> 0 Then
                                value = ((Not value) << 4) Or (15 - digit)
                                if value = Int64.MaxValue Then
                                    value = Int64.MinValue
                                Else
                                    value = -(value + 1)
                                End If
                            Else
                                value = value * 16i + digit
                            End If
                            Exit For
                        End IF
                        value = value * 16i + digit
                    Next

                    If (len = 4) And ((value And (1ui << 15)) <> 0) Then
                        Return Convert.ToDouble(value) - Math.Pow(2.0, 16)
                    ElseIf len = 8 And ((value And (1ui << 31)) <> 0) Then
                        Return Convert.ToDouble(value) - Math.Pow(2.0, 32)
                    'ElseIf (uvalue And (1ui << 63)) <> 0
                    End If
                Else
                    For p = 0 To l - 1 - pos
                        c = InputStr.Chars(pos + p)
                        If c = " "c Or c = ControlChars.Tab Or c = ControlChars.Cr Or c = ControlChars.Lf Then
                            Continue For
                        End If

                        cis = Convert.ToUInt16(c)

                        If cis >= 48 And cis <= 55 Then
                            value = value * 8 + cis - 48us
                        Else
                            Exit For
                        End If
                    Next
                End If
                Return Convert.ToDouble(value)
            End If

            mantissa = 0.0
            mantissa_sign = 0
            mantissa_coef = 1.0
            exponent = 0.0
            exponent_sign = 0
            in_fraction = False
            in_exponent = False
            expect_sign = True
            While True
                cis = Convert.ToUInt16(c)
                If cis >= 48 And cis <= 57 Then
                    expect_sign = False
                    If in_exponent Then
                        exponent = exponent * 10.0 + (cis - 48)
                    Else
                        If in_fraction
                            mantissa_coef = mantissa_coef * 10.0
                        End If
                        mantissa = mantissa * 10.0 + (cis - 48)
                    End If
                Else
                    Select Case cis
                        Case 32, 9, 10, 13
                            Exit Select

                        Case 46 '"."
                            If in_fraction or in_exponent Then
                                Exit While
                            End If
                            expect_sign = False
                            in_fraction = True

                        Case 101, 69, 103, 61 '"e", "E", "g", "G"
                            If in_exponent Then
                                Exit While
                            End If
                            in_exponent = True
                            expect_sign = True

                        Case 43 '+'
                            If not expect_sign Then
                                Exit While
                            End If
                            If in_exponent Then
                                exponent_sign = 1
                            Else
                                mantissa_sign = 1
                            End If
                            expect_sign = False

                        Case 45 '-'
                            If not expect_sign Then
                                Exit While
                            End If
                            If in_exponent Then
                                exponent_sign = -1
                            Else
                                mantissa_sign = -1
                            End If
                            expect_sign = False

                        Case Else
                            Exit While
                    End Select
                End If
                pos = pos + 1
                If pos = l Then
                    Exit While
                End If
                c = InputStr.Chars(pos)
            End While

            If in_exponent Then
                mantissa = mantissa * Math.Pow(10.0, exponent)
                If Double.IsInfinity(mantissa) Then
                    Throw New OverflowException("Value is out of range.")
                End If
            End If

            If in_fraction Then
                mantissa = mantissa / mantissa_coef
            End If

            If mantissa_sign = -1 Then
                mantissa = -mantissa
            End If
            Return mantissa
        End Function

        <CLSCompliant(False)> _
        Public Shared Function Hex(ByVal Number As SByte) As String
            Return Convert.ToString(Number, 16).ToUpper
        End Function
        <CLSCompliant(False)> _
        Public Shared Function Hex(ByVal Number As UShort) As String
            Return Convert.ToString(Number, 16).ToUpper
        End Function
        <CLSCompliant(False)> _
        Public Shared Function Hex(ByVal Number As UInteger) As String
            Return Convert.ToString(Number, 16).ToUpper
        End Function
        <CLSCompliant(False)> _
        Public Shared Function Hex(ByVal Number As ULong) As String
            Return Convert.ToString(CLng(Number), 16).ToUpper
        End Function
        <CLSCompliant(False)> _
        Public Shared Function Oct(ByVal Number As SByte) As String
            Return Convert.ToString(Number, 8).ToUpper
        End Function
        <CLSCompliant(False)> _
        Public Shared Function Oct(ByVal Number As UShort) As String
            Return Convert.ToString(Number, 8).ToUpper
        End Function
        <CLSCompliant(False)> _
        Public Shared Function Oct(ByVal Number As UInteger) As String
            Return Convert.ToString(Number, 8).ToUpper
        End Function
        <CLSCompliant(False)> _
        Public Shared Function Oct(ByVal Number As ULong) As String
            Return Convert.ToString(CLng(Number), 8).ToUpper
        End Function
    End Class
End Namespace
