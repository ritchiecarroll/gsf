'*******************************************************************************************************
'  Tva.Text.Common.vb - Common Text Functions
'  Copyright � 2006 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: J. Ritchie Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  02/23/2003 - J. Ritchie Carroll
'       Original version of source code generated
'  01/24/2006 - J. Ritchie Carroll
'       2.0 version of source code migrated from 1.1 source (TVA.Shared.String)
'  06/01/2006 - J. Ritchie Carroll
'       Added ParseBoolean function to parse strings representing booleans that may be numeric
'
'*******************************************************************************************************

Imports System.Text
Imports Tva.Common

Namespace Text

    ''' <summary>Defines common global functions related to string manipulation</summary>
    Public NotInheritable Class Common

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        ''' <summary>Performs a fast concatenation of given string array</summary>
        ''' <param name="values">String array to concatenate</param>
        ''' <returns>The concatenated string representation of the values of the elements in <paramref name="values" /> string array.</returns>
        ''' <remarks>
        ''' This is a replacement for the String.Concat function.  Tests show that the system implemenation of this function is slow:
        ''' http://www.developer.com/net/cplus/article.php/3304901
        ''' </remarks>
        Public Shared Function Concat(ByVal ParamArray values As String()) As String

            If values Is Nothing Then
                Return ""
            Else
                With New StringBuilder
                    For x As Integer = 0 To values.Length - 1
                        If Not String.IsNullOrEmpty(values(x)) Then .Append(values(x))
                    Next

                    Return .ToString
                End With
            End If

        End Function

        ''' <summary>Parses a string intended to represent a boolean value</summary>
        ''' <param name="value">String representing a boolean value</param>
        ''' <returns>Parsed boolean value</returns>
        ''' <remarks>
        ''' This function, unlike Boolean.Parse, correctly parses a boolean value even if the string value
        ''' specified is a number (e.g., 0 or -1).  Boolean.Parse expects a string to be represented as
        ''' "True" or "False" (i.e., Boolean.TrueString or Boolean.FalseString respectively)
        ''' </remarks>
        Public Shared Function ParseBoolean(ByVal value As String) As Boolean

            If String.IsNullOrEmpty(value) Then Return False
            value = value.Trim()

            If value.Length > 0 Then
                If Char.IsNumber(value.Chars(0)) Then
                    ' String contains a number
                    Dim result As Integer

                    If Integer.TryParse(value, result) Then
                        Return (result <> 0)
                    Else
                        Return False
                    End If
                Else
                    ' String contains text
                    Dim result As Boolean

                    If Boolean.TryParse(value, result) Then
                        Return result
                    Else
                        Return False
                    End If
                End If
            Else
                Return False
            End If

        End Function

        ''' <summary>Ensures parameter is not an empty or null string - returns a single space if test value is empty</summary>
        ''' <param name="testValue">Value to test for null or empty</param>
        ''' <returns>A non-empty string</returns>
        Public Shared Function NotEmpty(ByVal testValue As String) As String

            Return NotEmpty(testValue, " ")

        End Function

        ''' <summary>Ensures parameter is not an empty or null string</summary>
        ''' <param name="testValue">Value to test for null or empty</param>
        ''' <param name="nonEmptyReturnValue">Value to return if <paramref name="testValue">testValue</paramref> is null or empty</param>
        ''' <returns>A non-empty string</returns>
        Public Shared Function NotEmpty(ByVal testValue As String, ByVal nonEmptyReturnValue As String) As String

            If String.IsNullOrEmpty(nonEmptyReturnValue) Then Throw New ArgumentException("nonEmptyReturnValue cannot be empty!")
            If String.IsNullOrEmpty(testValue) Then Return nonEmptyReturnValue Else Return testValue

        End Function

        ''' <summary>Removes duplicate character strings (adjoining replication) in a string</summary>
        ''' <param name="value">Input string</param>
        ''' <param name="duplicatedValue">String whose duplicates are to be removed</param>
        ''' <returns>Returns <paramref name="value" /> with all duplicated <paramref name="duplicatedValue" /> removed</returns>
        Public Shared Function RemoveDuplicates(ByVal value As String, ByVal duplicatedValue As String) As String

            If String.IsNullOrEmpty(value) Then Return ""
            If String.IsNullOrEmpty(duplicatedValue) Then Return value

            Dim duplicate As String = Concat(duplicatedValue, duplicatedValue)

            Do While value.IndexOf(duplicate) > -1
                value = value.Replace(duplicate, duplicatedValue)
            Loop

            Return value

        End Function

        ''' <summary>Removes the terminator (Chr(0)) from a null terminated string - useful for strings returned from Windows API call</summary>
        ''' <param name="value">Input string</param>
        ''' <returns>Returns <paramref name="value" /> with all characters to the left of the terminator</returns>
        Public Shared Function RemoveNull(ByVal value As String) As String

            If String.IsNullOrEmpty(value) Then Return ""

            Dim nullPos As Integer = value.IndexOf(Chr(0))

            If nullPos > -1 Then
                Return value.Substring(0, nullPos)
            Else
                Return value
            End If

        End Function

        ''' <summary>Removes all carriage returns and line feeds (CrLf, Cr, and Lf's) from a string</summary>
        ''' <param name="value">Input string</param>
        ''' <returns>Returns <paramref name="value" /> with all CR and LF characters removed.</returns>
        Public Shared Function RemoveCrLfs(ByVal value As String) As String

            If String.IsNullOrEmpty(value) Then Return ""
            Return value.Replace(Environment.NewLine, "").Replace(Convert.ToChar(13), "").Replace(Convert.ToChar(10), "")

        End Function

        ''' <summary>Removes all white space (as defined by IsWhiteSpace) from a string</summary>
        ''' <param name="value">Input string</param>
        ''' <returns>Returns <paramref name="value" /> with all white space removed</returns>
        Public Shared Function RemoveWhiteSpace(ByVal value As String) As String

            If String.IsNullOrEmpty(value) Then Return ""

            With New StringBuilder
                Dim character As Char

                For x As Integer = 0 To value.Length - 1
                    character = value(x)

                    If Not Char.IsWhiteSpace(character) Then
                        .Append(character)
                    End If
                Next

                Return .ToString
            End With

        End Function

        ''' <summary>Replaces all repeating white space with a single space</summary>
        ''' <param name="value">Input string</param>
        ''' <returns>Returns <paramref name="value" /> with all duplicate white space removed</returns>
        Public Shared Function RemoveDuplicateWhiteSpace(ByVal value As String) As String

            Return RemoveDuplicateWhiteSpace(value, " "c)

        End Function

        ''' <summary>Replaces all repeating white space with specified spacing character</summary>
        ''' <param name="value">Input string</param>
        ''' <param name="spacingCharacter">Character value to use to insert as single white space value</param>
        ''' <returns>Returns <paramref name="value" /> with all duplicate white space removed</returns>
        ''' <remarks>This function allows you to specify spacing character (e.g., you may want to use a non-breaking space: Convert.ToChar(160))</remarks>
        Public Shared Function RemoveDuplicateWhiteSpace(ByVal value As String, ByVal spacingCharacter As Char) As String

            If String.IsNullOrEmpty(value) Then Return ""

            With New StringBuilder
                Dim character As Char
                Dim lastCharWasSpace As Boolean

                For x As Integer = 0 To value.Length - 1
                    character = value(x)

                    If Char.IsWhiteSpace(character) Then
                        lastCharWasSpace = True
                    Else
                        If lastCharWasSpace Then
                            .Append(spacingCharacter)
                        End If
                        .Append(character)
                        lastCharWasSpace = False
                    End If
                Next

                Return .ToString
            End With

        End Function

        ''' <summary>Counts the total number of the occurances of <paramref name="characterToCount" /> in the given string</summary>
        ''' <param name="value">Input string</param>
        ''' <param name="characterToCount">Character to be counted</param>
        ''' <returns>Total number of the occurances of <paramref name="characterToCount" /> in the given string</returns>
        Public Shared Function CharCount(ByVal value As String, ByVal characterToCount As Char) As Integer

            If String.IsNullOrEmpty(value) Then Return 0

            Dim total As Integer

            For x As Integer = 0 To value.Length - 1
                If value(x) = characterToCount Then total += 1
            Next

            Return total

        End Function

        ''' <summary>Tests to see if a string is all digits</summary>
        ''' <param name="value">Input string</param>
        ''' <returns>True if all string's characters are digits, otherwise false</returns>
        Public Shared Function IsAllDigits(ByVal value As String) As Boolean

            If String.IsNullOrEmpty(value) Then Return False

            value = value.Trim
            If value.Length = 0 Then Return False

            For x As Integer = 0 To value.Length - 1
                If Not Char.IsDigit(value(x)) Then Return False
            Next

            Return True

        End Function

        ''' <summary>Tests to see if a string is all numbers</summary>
        ''' <param name="value">Input string</param>
        ''' <returns>True if all string's characters are numbers, otherwise false</returns>
        Public Shared Function IsAllNumbers(ByVal value As String) As Boolean

            IsNumeric(

            If String.IsNullOrEmpty(value) Then Return False

            value = value.Trim
            If value.Length = 0 Then Return False

            For x As Integer = 0 To value.Length - 1
                If Not Char.IsNumber(value(x)) Then Return False
            Next

            Return True

        End Function

        ''' <summary>Tests to see if a string's letters are all upper case</summary>
        ''' <param name="value">Input string</param>
        ''' <returns>True if all string's letter characters are upper case, otherwise false</returns>
        Public Shared Function IsAllUpper(ByVal value As String) As Boolean

            If String.IsNullOrEmpty(value) Then Return False

            value = value.Trim
            If value.Length = 0 Then Return False

            For x As Integer = 0 To value.Length - 1
                If Char.IsLetter(value(x)) AndAlso Not Char.IsUpper(value(x)) Then Return False
            Next

            Return True

        End Function

        ''' <summary>Tests to see if a string's letters are all lower case</summary>
        ''' <param name="value">Input string</param>
        ''' <returns>True if all string's letter characters are lower case, otherwise false</returns>
        Public Shared Function IsAllLower(ByVal value As String) As Boolean

            If String.IsNullOrEmpty(value) Then Return False

            value = value.Trim
            If value.Length = 0 Then Return False

            For x As Integer = 0 To value.Length - 1
                If Char.IsLetter(value(x)) AndAlso Not Char.IsLower(value(x)) Then Return False
            Next

            Return True

        End Function

        ''' <summary>Tests to see if a string is all letters</summary>
        ''' <param name="value">Input string</param>
        ''' <returns>True if all string's characters are letters, otherwise false</returns>
        ''' <remarks>Any non letter (e.g., punctuation marks) would cause this function to return False - see overload to ignore punctuation marks</remarks>
        Public Shared Function IsAllLetters(ByVal value As String) As Boolean

            Return IsAllLetters(value, False)

        End Function

        ''' <summary>Tests to see if a string is all letters</summary>
        ''' <param name="value">Input string</param>
        ''' <param name="ignorePunctuation">Set to True to ignore punctuation</param>
        ''' <returns>True if all string's characters are letters, otherwise false</returns>
        Public Shared Function IsAllLetters(ByVal value As String, ByVal ignorePunctuation As Boolean) As Boolean

            If String.IsNullOrEmpty(value) Then Return False

            value = value.Trim
            If value.Length = 0 Then Return False

            For x As Integer = 0 To value.Length - 1
                If ignorePunctuation Then
                    If Not (Char.IsLetter(value(x)) OrElse Char.IsPunctuation(value(x))) Then Return False
                Else
                    If Not Char.IsLetter(value(x)) Then Return False
                End If
            Next

            Return True

        End Function

        ''' <summary>Tests to see if a string is all letters or digits</summary>
        ''' <param name="value">Input string</param>
        ''' <returns>True if all string's characters are letters or digits, otherwise false</returns>
        ''' <remarks>Any non letter or digit (e.g., punctuation marks) would cause this function to return False - see overload to ignore punctuation marks</remarks>
        Public Shared Function IsAllLettersOrDigits(ByVal value As String) As Boolean

            Return IsAllLettersOrDigits(value, False)

        End Function

        ''' <summary>Tests to see if a string is all letters or digits</summary>
        ''' <param name="value">Input string</param>
        ''' <param name="ignorePunctuation">Set to True to ignore punctuation</param>
        ''' <returns>True if all string's characters are letters or digits, otherwise false</returns>
        Public Shared Function IsAllLettersOrDigits(ByVal value As String, ByVal ignorePunctuation As Boolean) As Boolean

            If String.IsNullOrEmpty(value) Then Return False

            value = value.Trim
            If value.Length = 0 Then Return False

            For x As Integer = 0 To value.Length - 1
                If ignorePunctuation Then
                    If Not (Char.IsLetterOrDigit(value(x)) OrElse Char.IsPunctuation(value(x))) Then Return False
                Else
                    If Not Char.IsLetterOrDigit(value(x)) Then Return False
                End If
            Next

            Return True

        End Function

        ''' <summary>Encodes the specified Unicode character in proper Regular Expression format</summary>
        ''' <param name="item">Unicode character to encode in Regular Expression format</param>
        ''' <returns>Specified Unicode character in proper Regular Expression format</returns>
        Public Shared Function EncodeRegexChar(ByVal item As Char) As String

            Return "\u" & Convert.ToInt16(item).ToString("x"c).PadLeft(4, "0"c)

        End Function

        ''' <summary>Decodes the specified Regular Expression character back into a standard Unicode character</summary>
        ''' <param name="value">Regular Expression character to decode back into a Unicode character</param>
        ''' <returns>Standard Unicode character representation of specified Regular Expression character</returns>
        Public Shared Function DecodeRegexChar(ByVal value As String) As Char

            Return Convert.ToChar(Convert.ToInt16(value.Replace("\u", "0x"), 16))

        End Function

        ''' <summary>Encodes a string into a base-64 string</summary>
        ''' <param name="value">Input string</param>
        ''' <remarks>
        ''' <para>This performs a base-64 style of string encoding useful for data obfuscation or safe XML data string transmission</para>
        ''' <para>Note: this function encodes a "String", use the Convert.ToBase64String function to encode a binary data buffer</para>
        ''' </remarks>
        Public Shared Function Base64Encode(ByVal value As String) As String

            Return Convert.ToBase64String(System.Text.Encoding.Unicode.GetBytes(value))

        End Function

        ''' <summary>Decodes given base-64 encoded string encoded with <see cref="Base64Encode" /></summary>
        ''' <param name="value">Input string</param>
        ''' <remarks>Note: this function decodes value back into a "String", use the Convert.FromBase64String function to decode a base-64 encoded string back into a binary data buffer</remarks>
        Public Shared Function Base64Decode(ByVal value As String) As String

            Return System.Text.Encoding.Unicode.GetString(Convert.FromBase64String(value))

        End Function

    End Class

End Namespace
