'*******************************************************************************************************
'  ConfigurationFrame.vb - PDCstream Configuration Frame / File
'  Copyright � 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports System.IO
Imports System.Text
Imports System.Reflection.Assembly
Imports System.Security.Principal
Imports System.Threading
Imports System.Buffer
Imports TVA.Shared.String
Imports TVA.Shared.Common
Imports TVA.Shared.Math
Imports TVA.Interop
Imports TVA.Interop.Windows
Imports TVA.EE.Phasor.PDCstream.Common

Namespace EE.Phasor.PDCstream

    ' Note that it is expected that the end user will typically create only one instance of this class per INI file for use by any
    ' number of different threads and a request can be made at anytime to "reload" the config file, so we make sure all publically
    ' accessible methods in the class make proper use of the internal reader-writer lock.  This also allows end user to place a
    ' file-watcher on the INI file so class can "reload" config file when it's updated...
    Public Class ConfigurationFrame

        Inherits ConfigurationFrameBase

        Private m_readWriteLock As ReaderWriterLock
        Private m_iniFile As IniFile
        Private m_defaultPhasorV As PhasorDefinition
        Private m_defaultPhasorI As PhasorDefinition
        Private m_defaultFrequency As FrequencyDefinition
        Private m_rowLength As Int32
        Private m_packetsPerSample As Int16
        Private m_streamType As StreamType
        Private m_revisionNumber As RevisionNumber

        Public Event ConfigFileReloaded()

        Public Sub New(ByVal configFileName As String)

            MyBase.New(New ConfigurationCellCollection)

            m_iniFile = New IniFile(configFileName)
            m_readWriteLock = New ReaderWriterLock
            m_packetsPerSample = 1
            Refresh(True)

        End Sub

        ' If you are going to create multiple data packets, you can use this constructor
        ' Note that this only starts becoming necessary if you start hitting data size
        ' limits imposed by the nature of the protocol...
        Public Sub New(ByVal configFileName As String, ByVal packetsPerSample As Int16)

            Me.New(configFileName)
            m_packetsPerSample = packetsPerSample

        End Sub

        Public Sub New(ByVal configFileName As String, ByVal binaryImage As Byte(), ByVal startIndex As Integer)

            MyBase.New(New ConfigurationFrameParsingState(New ConfigurationCellCollection, GetType(ConfigurationCell), 0), binaryImage, startIndex)
            Refresh(False)

        End Sub

        Public Sub New(ByVal configurationFrame As IConfigurationFrame)

            MyBase.New(configurationFrame)

        End Sub

        Public Overrides ReadOnly Property InheritedType() As System.Type
            Get
                Return Me.GetType
            End Get
        End Property

        Public Shadows ReadOnly Property Cells() As ConfigurationCellCollection
            Get
                Return MyBase.Cells
            End Get
        End Property

        Public Property StreamType() As StreamType
            Get
                Return m_streamType
            End Get
            Set(ByVal Value As StreamType)
                m_streamType = Value
            End Set
        End Property

        Public Property RevisionNumber() As RevisionNumber
            Get
                Return m_revisionNumber
            End Get
            Set(ByVal Value As RevisionNumber)
                m_revisionNumber = Value
            End Set
        End Property

        Public Sub Refresh(ByVal clearExistingPMUList As Boolean)

            ' The only time we need a write lock is when we reload the config file...
            m_readWriteLock.AcquireWriterLock(-1)

            Try
                With m_iniFile
                    If File.Exists(.IniFileName) Then
                        Dim pmuCell As ConfigurationCell
                        Dim x, phasorCount As Integer

                        m_defaultPhasorV = New PhasorDefinition(Nothing, 0, .KeyValue("DEFAULT", "PhasorV", "V,4500.0,0.0060573,0,0,500,Default 500kV"))
                        m_defaultPhasorI = New PhasorDefinition(Nothing, 0, .KeyValue("DEFAULT", "PhasorI", "I,600.00,0.000040382,0,1,1.0,Default Current"))
                        m_defaultFrequency = New FrequencyDefinition(Nothing, .KeyValue("DEFAULT", "Frequency", "F,1000,60,1000,0,0,Frequency"))
                        SampleRate = CInt(.KeyValue("CONFIG", "SampleRate", "30"))

                        Cells.Clear()

                        ' Load phasor data for each section in config file...
                        For Each section As String In .SectionNames()
                            If Len(section) > 0 Then
                                ' Make sure this is not a special section
                                If String.Compare(section, "DEFAULT", True) <> 0 And String.Compare(section, "CONFIG", True) <> 0 Then
                                    ' Create new PMU entry structure from config file settings...
                                    phasorCount = CInt(.KeyValue(section, "NumberPhasors", "0"))

                                    pmuCell = New ConfigurationCell(Me)

                                    pmuCell.IDLabel = section
                                    pmuCell.StationName = .KeyValue(section, "Name", section)
                                    pmuCell.IDCode = CInt(.KeyValue(section, "PMU", Cells.Count))

                                    For x = 0 To phasorCount - 1
                                        pmuCell.PhasorDefinitions.Add(New PhasorDefinition(pmuCell, x + 1, .KeyValue(section, "Phasor" & (x + 1))))
                                    Next

                                    pmuCell.FrequencyDefinition = New FrequencyDefinition(pmuCell, .KeyValue(section, "Frequency"))

                                    Cells.Add(pmuCell)
                                End If
                            End If
                        Next
                    Else
                        Throw New InvalidOperationException("PDC config file """ & .IniFileName & """ does not exist.")
                    End If
                End With
            Catch ex As Exception
                Throw ex
            Finally
                m_readWriteLock.ReleaseWriterLock()
            End Try

            ' In case other classes want to know, we send out a notification that the config file has been reloaded (make sure
            ' you do this after the write lock has been released to avoid possible dead-lock situations)
            RaiseEvent ConfigFileReloaded()

        End Sub

        Public Property PacketsPerSample() As Int16
            Get
                Return m_packetsPerSample
            End Get
            Set(ByVal Value As Int16)
                m_packetsPerSample = Value
            End Set
        End Property

        Public ReadOnly Property ConfigFileName() As String
            Get
                m_readWriteLock.AcquireReaderLock(-1)

                Try
                    Return m_iniFile.IniFileName
                Catch
                    Throw
                Finally
                    m_readWriteLock.ReleaseReaderLock()
                End Try
            End Get
        End Property

        Public ReadOnly Property DefaultPhasorV() As PhasorDefinition
            Get
                m_readWriteLock.AcquireReaderLock(-1)

                Try
                    Return m_defaultPhasorV
                Catch
                    Throw
                Finally
                    m_readWriteLock.ReleaseReaderLock()
                End Try
            End Get
        End Property

        Public ReadOnly Property DefaultPhasorI() As PhasorDefinition
            Get
                m_readWriteLock.AcquireReaderLock(-1)

                Try
                    Return m_defaultPhasorI
                Catch
                    Throw
                Finally
                    m_readWriteLock.ReleaseReaderLock()
                End Try
            End Get
        End Property

        Public ReadOnly Property DefaultFrequency() As FrequencyDefinition
            Get
                m_readWriteLock.AcquireReaderLock(-1)

                Try
                    Return m_defaultFrequency
                Catch
                    Throw
                Finally
                    m_readWriteLock.ReleaseReaderLock()
                End Try
            End Get
        End Property

        Public ReadOnly Property IniFileImage() As String
            Get
                m_readWriteLock.AcquireReaderLock(-1)

                Try
                    With New StringBuilder
                        .Append("; File - " & m_iniFile.IniFileName & vbCrLf)
                        .Append("; Auto-generated on " & Now() & " by TVA DatAWare PDC" & vbCrLf)
                        .Append(";    Assembly: " & GetShortAssemblyName(GetExecutingAssembly) & vbCrLf)
                        .Append(";    Compiled: " & File.GetLastWriteTime(GetExecutingAssembly.Location) & vbCrLf)
                        .Append(";" & vbCrLf)
                        .Append(";" & vbCrLf)
                        .Append("; Format:" & vbCrLf)
                        .Append(";   Each Column in data file is given a bracketed identifier, numbered in the order it" & vbCrLf)
                        .Append(";   appears in the data file, and identified by data type ( PMU, PDC, or other)" & vbCrLf)
                        .Append(";     PMU designates column data format from a single PMU" & vbCrLf)
                        .Append(";     PDC designates column data format from another PDC which is somewhat different from a single PMU" & vbCrLf)
                        .Append(";   Default gives default values for a processing algorithm in case quantities are omitted" & vbCrLf)
                        .Append(";   Name= gives the overall station name for print labels" & vbCrLf)
                        .Append(";   NumberPhasors= :  for PMU data, gives the number of phasors contained in column" & vbCrLf)
                        .Append(";                     for PDC data, gives the number of PMUs data included in the column" & vbCrLf)
                        .Append(";                     Note - for PDC data, there will be 2 phasors & 1 freq per PMU" & vbCrLf)
                        .Append(";   Quantities within the column are listed by PhasorI=, Frequency=, etc" & vbCrLf)
                        .Append(";   Each quantity has 7 comma separated fields followed by an optional comment" & vbCrLf)
                        .Append(";" & vbCrLf)
                        .Append(";   Phasor entry format:  Type, Ratio, Cal Factor, Offset, Shunt, VoltageRef/Class, Label  ;Comments" & vbCrLf)
                        .Append(";    Type:       Type of measurement, V=voltage, I=current, N=don't care, single ASCII character" & vbCrLf)
                        .Append(";    Ratio:      PT/CT ratio N:1 where N is a floating point number" & vbCrLf)
                        .Append(";    Cal Factor: Conversion factor between integer in file and secondary volts, floating point" & vbCrLf)
                        .Append(";    Offset:     Phase Offset to correct for phase angle measurement errors or differences, floating point" & vbCrLf)
                        .Append(";    Shunt:      Current- shunt resistence in ohms, or the equivalent ratio for aux CTs, floating point" & vbCrLf)
                        .Append(";                Voltage- empty, not used" & vbCrLf)
                        .Append(";    VoltageRef: Current- phasor number (1-10) of voltage phasor to use for power calculation, integer" & vbCrLf)
                        .Append(";                Voltage- voltage class, standard l-l voltages, 500, 230, 115, etc, integer" & vbCrLf)
                        .Append(";    Label:      Phasor quantity label for print label, text" & vbCrLf)
                        .Append(";    Comments:   All text after the semicolon on a line are optional comments not for processing" & vbCrLf)
                        .Append(";" & vbCrLf)
                        .Append(";   Voltage Magnitude = MAG(Real,Imaginary) * CalFactor  * PTR    (line-neutral)" & vbCrLf)
                        .Append(";   Current Magnitude = MAG(Real,Imaginary)  * CalFactor * CTR / Shunt   (phase current)" & vbCrLf)
                        .Append(";   Phase Angle = ATAN(Imaginary/Real) + Phase Offset   (usually degrees)" & vbCrLf)
                        .Append(";     Note: Usually phase Offset is 0, but is sometimes required for comparing measurements" & vbCrLf)
                        .Append(";           from different systems or through transformer banks" & vbCrLf)
                        .Append(";" & vbCrLf)
                        .Append(";   Frequency entry format:  scale, offset, dF/dt scale, dF/dt offset, dummy, label  ;Comments" & vbCrLf)
                        .Append(";   Frequency = Number / scale + offset" & vbCrLf)
                        .Append(";   dF/dt = Number / (dF/dt scale) + (dF/dt offset)" & vbCrLf)
                        .Append(";" & vbCrLf)
                        .Append(";" & vbCrLf)

                        .Append("[DEFAULT]" & vbCrLf)
                        .Append("PhasorV=" & PhasorDefinition.ConfigFileFormat(DefaultPhasorV) & vbCrLf)
                        .Append("PhasorI=" & PhasorDefinition.ConfigFileFormat(DefaultPhasorI) & vbCrLf)
                        .Append("Frequency=" & FrequencyDefinition.ConfigFileFormat(DefaultFrequency) & vbCrLf)
                        .Append(vbCrLf)

                        .Append("[CONFIG]" & vbCrLf)
                        .Append("SampleRate=" & SampleRate & vbCrLf)
                        .Append("NumberOfPMUs=" & Cells.Count & vbCrLf)
                        .Append(vbCrLf)

                        For x As Integer = 0 To Cells.Count - 1
                            .Append("[" & Cells(x).IDLabel & "]" & vbCrLf)
                            .Append("Name=" & Cells(x).StationName & vbCrLf)
                            .Append("PMU=" & x & vbCrLf)
                            .Append("NumberPhasors=" & Cells(x).PhasorDefinitions.Count & vbCrLf)
                            For y As Integer = 0 To Cells(x).PhasorDefinitions.Count - 1
                                .Append("Phasor" & (y + 1) & "=" & PhasorDefinition.ConfigFileFormat(Cells(x).PhasorDefinitions(y)) & vbCrLf)
                            Next
                            .Append("Frequency=" & FrequencyDefinition.ConfigFileFormat(Cells(x).FrequencyDefinition) & vbCrLf)
                            .Append(vbCrLf)
                        Next

                        Return .ToString()
                    End With
                Catch
                    Throw
                Finally
                    m_readWriteLock.ReleaseReaderLock()
                End Try
            End Get
        End Property

        Public ReadOnly Property RowLength() As Int32
            Get
                Return RowLength(False)
            End Get
        End Property

        ' RowLength property calculates cell offsets - so it must be called before
        ' accessing cell offsets - this happens automatically since HeaderImage is
        ' called before base class BodyImage which just gets Cells.BinaryImage
        Public ReadOnly Property RowLength(ByVal recalculate As Boolean) As Int32
            Get
                If m_rowLength = 0 OrElse recalculate Then
                    m_rowLength = 0
                    For x As Integer = 0 To Cells.Count - 1
                        With Cells(x)
                            .Offset = m_rowLength
                            m_rowLength += 12 + FrequencyValue.CalculateBinaryLength(.FrequencyDefinition)
                            For y As Integer = 0 To .PhasorDefinitions.Count - 1
                                m_rowLength += PhasorValue.CalculateBinaryLength(.PhasorDefinitions(y))
                            Next
                        End With
                    Next
                End If

                Return m_rowLength
            End Get
        End Property

        Protected Overrides Function CalculateChecksum(ByVal buffer() As Byte, ByVal offset As Integer, ByVal length As Integer) As Int16

            ' PDCstream uses an XOR based check sum
            Return XorCheckSum(buffer, offset, length)

        End Function

        Protected Overrides ReadOnly Property HeaderLength() As Short
            Get
                Return 16
            End Get
        End Property

        Protected Overrides ReadOnly Property HeaderImage() As Byte()
            Get
                Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), HeaderLength)
                Dim index As Integer

                buffer(0) = SyncByte
                buffer(1) = DescriptorPacketFlag
                EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(BinaryLength \ 2), buffer, 2)
                buffer(4) = StreamType
                buffer(5) = RevisionNumber
                EndianOrder.BigEndian.CopyBytes(SampleRate, buffer, 6)
                EndianOrder.BigEndian.CopyBytes(RowLength(True), buffer, 8) ' <-- Important: This step calculates all PMU row offsets!
                EndianOrder.BigEndian.CopyBytes(PacketsPerSample, buffer, 12)
                EndianOrder.BigEndian.CopyBytes(Convert.ToInt16(Cells.Count), buffer, 14)

                Return buffer
            End Get
        End Property

        Protected Overrides Sub ParseHeaderImage(ByVal state As IChannelParsingState, ByVal binaryImage() As Byte, ByVal startIndex As Integer)

            ' We parse the PDC stream specific header image here...
            Dim parsingState As IConfigurationFrameParsingState = DirectCast(state, IConfigurationFrameParsingState)
            Dim wordCount As Int16

            If binaryImage(startIndex) <> Common.SyncByte Then
                Throw New InvalidOperationException("Bad Data Stream: Expected sync byte &HAA as first byte in PDCstream configuration frame, got " & binaryImage(startIndex).ToString("x"c).PadLeft(2, "0"c))
            End If

            If binaryImage(startIndex + 1) <> DescriptorPacketFlag Then
                Throw New InvalidOperationException("Bad Data Stream: This is not a PDCstream configuration frame - looks like a data frame.")
            End If

            wordCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 2)
            StreamType = binaryImage(startIndex + 4)
            RevisionNumber = binaryImage(startIndex + 5)
            SampleRate = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 6)
            m_rowLength = EndianOrder.BigEndian.ToInt32(binaryImage, startIndex + 8)
            PacketsPerSample = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 12)

            parsingState.CellCount = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 14)

            ' The data that's in the data stream will take precedence over what's in the
            ' in the configuration file.  The configuration file may define more PMU's than
            ' are in the stream - in my opinon that's OK - it's when you have PMU's in the
            ' stream that aren't defined in the INI file that you'll have trouble..

        End Sub

    End Class

End Namespace