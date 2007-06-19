'*******************************************************************************************************
'  ConfigurationCell.vb - Protocol Independent Configuration Cell Definition
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
'  06/22/2006 - J. Ritchie Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.Runtime.Serialization
Imports TVA.Common
Imports TVA.Measurements
Imports PhasorProtocols

<CLSCompliant(False), Serializable()> _
Public Class ConfigurationCell

    Inherits ConfigurationCellBase

    Private m_lastReportTime As Long
    Private m_signalSynonyms As Dictionary(Of SignalType, String())
    Private m_analogDataFormat As PhasorProtocols.DataFormat
    Private m_frequencyDataFormat As PhasorProtocols.DataFormat
    Private m_phasorDataFormat As PhasorProtocols.DataFormat
    Private m_phasorCoordinateFormat As PhasorProtocols.CoordinateFormat

    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)

        MyBase.New(info, context)

        m_lastReportTime = info.GetInt64("lastReportTime")
        m_signalSynonyms = info.GetValue("signalSynonyms", GetType(Dictionary(Of SignalType, String())))
        m_analogDataFormat = info.GetValue("analogDataFormat", GetType(DataFormat))
        m_frequencyDataFormat = info.GetValue("frequencyDataFormat", GetType(DataFormat))
        m_phasorDataFormat = info.GetValue("phasorDataFormat", GetType(DataFormat))
        m_phasorCoordinateFormat = info.GetValue("phasorCoordinateFormat", GetType(CoordinateFormat))

    End Sub

    Public Sub New()

        m_signalSynonyms = New Dictionary(Of SignalType, String())
        m_analogDataFormat = DataFormat.FloatingPoint
        m_frequencyDataFormat = DataFormat.FloatingPoint
        m_phasorDataFormat = DataFormat.FloatingPoint
        m_phasorCoordinateFormat = CoordinateFormat.Polar

    End Sub

    Public Sub New(ByVal idCode As UInt16, ByVal idLabel As String)

        MyClass.New()
        MyBase.IDCode = idCode
        MyBase.IDLabel = idLabel

    End Sub

    Public Overrides ReadOnly Property DerivedType() As System.Type
        Get
            Return Me.GetType()
        End Get
    End Property

    Public ReadOnly Property SignalSynonym(ByVal signal As SignalType) As String
        Get
            ' We cache non-indexed signal reference strings so they don't need to be generated at each mapping call.
            ' This helps with performance since the mappings for each signal occur 30 times per second.
            Dim synonyms As String()

            ' Look up synonym in dictionary based on signal type, if found return single element
            If m_signalSynonyms.TryGetValue(signal, synonyms) Then Return synonyms(0)

            ' Create a new signal reference array (for single element)
            synonyms = CreateArray(Of String)(1)

            ' Create and cache new non-indexed signal reference
            synonyms(0) = SignalReference.ToString(IDLabel, signal)

            Return synonyms(0)
        End Get
    End Property

    Public ReadOnly Property SignalSynonym(ByVal signal As SignalType, ByVal signalIndex As Integer, ByVal signalCount As Integer) As String
        Get
            ' We cache indexed signal reference strings so they don't need to be generated at each mapping call.
            ' This helps with performance since the mappings for each signal occur 30 times per second.
            ' For speed purposes we intentionally do not validate that signalIndex falls within signalCount, be
            ' sure calling procedures are very careful with parameters...
            Dim synonyms As String()

            ' Look up synonym in dictionary based on signal type
            If m_signalSynonyms.TryGetValue(signal, synonyms) Then
                ' Verify signal count has not changed (we may have received new configuration from device)
                If signalCount = synonyms.Length Then
                    ' Lookup signal reference "synonym" value of given signal index
                    If synonyms(signalIndex) Is Nothing Then
                        ' Didn't find signal index, create and cache new signal reference
                        synonyms(signalIndex) = SignalReference.ToString(IDLabel, signal, signalIndex + 1)
                    End If

                    Return synonyms(signalIndex)
                End If
            End If

            ' Create a new indexed signal reference array
            synonyms = CreateArray(Of String)(signalCount)

            ' Create and cache new signal reference
            synonyms(signalIndex) = SignalReference.ToString(IDLabel, signal, signalIndex + 1)

            Return synonyms(signalIndex)
        End Get
    End Property

    Public Property LastReportTime() As Long
        Get
            Return m_lastReportTime
        End Get
        Set(ByVal value As Long)
            m_lastReportTime = value
        End Set
    End Property

    Public Overrides Property AnalogDataFormat() As PhasorProtocols.DataFormat
        Get
            Return m_analogDataFormat
        End Get
        Set(ByVal value As PhasorProtocols.DataFormat)
            m_analogDataFormat = value
        End Set
    End Property

    Public Overrides Property FrequencyDataFormat() As PhasorProtocols.DataFormat
        Get
            Return m_frequencyDataFormat
        End Get
        Set(ByVal value As PhasorProtocols.DataFormat)
            m_frequencyDataFormat = value
        End Set
    End Property

    Public Overrides Property PhasorDataFormat() As PhasorProtocols.DataFormat
        Get
            Return m_phasorDataFormat
        End Get
        Set(ByVal value As PhasorProtocols.DataFormat)
            m_phasorDataFormat = value
        End Set
    End Property

    Public Overrides Property PhasorCoordinateFormat() As PhasorProtocols.CoordinateFormat
        Get
            Return m_phasorCoordinateFormat
        End Get
        Set(ByVal value As PhasorProtocols.CoordinateFormat)
            m_phasorCoordinateFormat = value
        End Set
    End Property

    Public Overrides Sub GetObjectData(ByVal info As SerializationInfo, ByVal context As StreamingContext)

        MyBase.GetObjectData(info, context)

        info.AddValue("lastReportTime", m_lastReportTime)
        info.AddValue("signalSynonyms", m_signalSynonyms, GetType(Dictionary(Of SignalType, String())))
        info.AddValue("analogDataFormat", m_analogDataFormat, GetType(DataFormat))
        info.AddValue("frequencyDataFormat", m_frequencyDataFormat, GetType(DataFormat))
        info.AddValue("phasorDataFormat", m_phasorDataFormat, GetType(DataFormat))
        info.AddValue("phasorCoordinateFormat", m_phasorCoordinateFormat, GetType(CoordinateFormat))

    End Sub

End Class
