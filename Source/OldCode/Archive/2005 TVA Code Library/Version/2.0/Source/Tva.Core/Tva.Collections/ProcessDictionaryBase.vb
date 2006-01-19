'*******************************************************************************************************
'  Tva.Collections.ProcessDictionary.vb - Multi-threaded Intervaled Keyed Item Processing Class
'  Copyright � 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2005
'  Primary Developer: James R Carroll, Operations Data Architecture [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2250
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  01/07/2006 - James R Carroll
'       Original version of source code generated
'
'*******************************************************************************************************

Imports System.Threading

Namespace Collections

    ''' <summary>
    ''' <para>This class will process a keyed list of items on independent threads</para>
    ''' </summary>
    ''' <typeparam name="TKey">Type of keys used to references process items</typeparam>
    ''' <typeparam name="TValue">Type of values to process</typeparam>
    ''' <remarks>
    ''' <para>This class acts as a strongly typed dictionary of objects to be processed.</para>
    ''' <para>Note that the queue will not start processing until the Start method is called.</para>
    ''' </remarks>
    Public Class ProcessDictionary(Of TKey, TValue)

        Inherits ProcessList(Of KeyValuePair(Of TKey, TValue))

        Implements IDictionary(Of TKey, TValue), IDictionary

        ''' <summary>
        ''' Create a new keyed asynchronous process queue with the default settings: ProcessInterval = 100, MaximumThreads = 5, ProcessTimeout = Infinite, RequeueOnTimeout = False
        ''' </summary>
        Public Shared Shadows Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature) As ProcessDictionary(Of TKey, TValue)

            Return CreateAsynchronousQueue(processItemFunction, DefaultProcessInterval, DefaultMaximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new keyed asynchronous process queue with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False
        ''' </summary>
        Public Shared Shadows Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal maximumThreads As Integer) As ProcessDictionary(Of TKey, TValue)

            Return CreateAsynchronousQueue(processItemFunction, DefaultProcessInterval, maximumThreads, DefaultProcessTimeout, DefaultRequeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new keyed asynchronous process queue using the specified settings
        ''' </summary>
        Public Shared Shadows Function CreateAsynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processInterval As Integer, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean) As ProcessDictionary(Of TKey, TValue)

            Return New ProcessDictionary(Of TKey, TValue)(processItemFunction, processInterval, maximumThreads, processTimeout, requeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new keyed synchronous process queue (i.e., single process thread) with the default settings: ProcessInterval = 100, ProcessTimeout = Infinite, RequeueOnTimeout = False
        ''' </summary>
        Public Shared Shadows Function CreateSynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature) As ProcessDictionary(Of TKey, TValue)

            Return CreateSynchronousQueue(processItemFunction, DefaultProcessInterval, DefaultProcessTimeout, DefaultRequeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new keyed synchronous process queue (i.e., single process thread) using the specified settings
        ''' </summary>
        Public Shared Shadows Function CreateSynchronousQueue(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processInterval As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean) As ProcessDictionary(Of TKey, TValue)

            Return New ProcessDictionary(Of TKey, TValue)(processItemFunction, processInterval, 1, processTimeout, requeueOnTimeout)

        End Function

        ''' <summary>
        ''' Create a new keyed process list using the specified settings
        ''' </summary>
        Public Sub New(ByVal processItemFunction As ProcessItemFunctionSignature, ByVal processInterval As Integer, ByVal maximumThreads As Integer, ByVal processTimeout As Integer, ByVal requeueOnTimeout As Boolean)

            MyBase.New(processItemFunction, processInterval, maximumThreads, processTimeout, requeueOnTimeout, New SortedDictionary(Of TKey, TValue))

        End Sub

        Protected ReadOnly Property InternalDictionary() As SortedDictionary(Of TKey, TValue)
            Get
                Return DirectCast(InternalList, SortedDictionary(Of TKey, TValue))
            End Get
        End Property

        Public Overloads Sub Add(ByVal key As TKey, ByVal value As TValue) Implements System.Collections.Generic.IDictionary(Of TKey, TValue).Add

            InternalDictionary.Add(key, value)

        End Sub

        Public Function ContainsKey(ByVal key As TKey) As Boolean Implements System.Collections.Generic.IDictionary(Of TKey, TValue).ContainsKey

            Return InternalDictionary.ContainsKey(key)

        End Function

        Default Public Overloads Property Item(ByVal key As TKey) As TValue Implements System.Collections.Generic.IDictionary(Of TKey, TValue).Item
            Get
                Return InternalDictionary(key)
            End Get
            Set(ByVal value As TValue)
                InternalDictionary(key) = value
            End Set
        End Property

        Public Overloads Function Remove(ByVal key As TKey) As Boolean Implements System.Collections.Generic.IDictionary(Of TKey, TValue).Remove

            InternalDictionary.Remove(key)

        End Function

        Public Function TryGetValue(ByVal key As TKey, ByRef value As TValue) As Boolean Implements System.Collections.Generic.IDictionary(Of TKey, TValue).TryGetValue

            Return InternalDictionary.TryGetValue(key, value)

        End Function

        Public ReadOnly Property Keys() As System.Collections.Generic.ICollection(Of TKey) Implements System.Collections.Generic.IDictionary(Of TKey, TValue).Keys
            Get
                Return InternalDictionary.Keys
            End Get
        End Property

        Public ReadOnly Property Values() As System.Collections.Generic.ICollection(Of TValue) Implements System.Collections.Generic.IDictionary(Of TKey, TValue).Values
            Get
                Return InternalDictionary.Values
            End Get
        End Property

        ' **************************************
        '
        '       IDictionary Implementation
        '
        ' **************************************

        Private ReadOnly Property IDictionary() As IDictionary
            Get
                Return DirectCast(InternalDictionary, IDictionary)
            End Get
        End Property

        Private Sub IDictionaryAdd(ByVal key As Object, ByVal value As Object) Implements System.Collections.IDictionary.Add

            IDictionary.Add(key, value)

        End Sub

        Private Function IDictionaryContains(ByVal key As Object) As Boolean Implements System.Collections.IDictionary.Contains

            Return IDictionary.Contains(key)

        End Function

        Private Property IDictionaryItem(ByVal key As Object) As Object Implements System.Collections.IDictionary.Item
            Get
                Return IDictionary(key)
            End Get
            Set(ByVal value As Object)
                IDictionary(key) = value
            End Set
        End Property

        Private ReadOnly Property IDictionaryKeys() As System.Collections.ICollection Implements System.Collections.IDictionary.Keys
            Get
                Return IDictionary.Keys
            End Get
        End Property

        Private ReadOnly Property IDictionaryValues() As System.Collections.ICollection Implements System.Collections.IDictionary.Values
            Get
                Return IDictionary.Values
            End Get
        End Property

        Private Sub IDictionaryRemove(ByVal key As Object) Implements System.Collections.IDictionary.Remove

            IDictionary.Remove(key)

        End Sub

        Private ReadOnly Property IDictionaryIsFixedSize() As Boolean Implements System.Collections.IDictionary.IsFixedSize
            Get
                Return IDictionary.IsFixedSize
            End Get
        End Property

        Private ReadOnly Property IDictionaryIsReadOnly() As Boolean Implements System.Collections.IDictionary.IsReadOnly
            Get
                Return IDictionary.IsReadOnly
            End Get
        End Property

        Private Sub IDictionaryClear() Implements System.Collections.IDictionary.Clear

            IDictionary.Clear()

        End Sub

        Private Function IDictionaryGetEnumerator() As System.Collections.IDictionaryEnumerator Implements System.Collections.IDictionary.GetEnumerator

            Return IDictionary.GetEnumerator

        End Function

    End Class

End Namespace