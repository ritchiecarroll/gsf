//*******************************************************************************************************
//  QueryPacketBase.cs
//  Copyright � 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  07/16/2007 - Pinal C. Patel
//       Generated original version of source code.
//  04/21/2009 - Pinal C. Patel
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;

namespace TVA.Historian.Packets
{
    /// <summary>
    /// A base class for a packet to be used for requesting information from a historian.
    /// </summary>
    public abstract class QueryPacketBase : PacketBase
    {
        // **************************************************************************************************
        // *                                        Binary Structure                                        *
        // **************************************************************************************************
        // * # Of Bytes Byte Index Data Type  Property Name                                                 *
        // * ---------- ---------- ---------- --------------------------------------------------------------*
        // * 2          0-1        Int16      TypeID (packet identifier)                                    *
        // * 4          2-5        Int32      RequestIDs.Count                                              *
        // * 4          6-9        Int32      RequestIDs[0]                                                 *
        // * 4          n1-n2      Int32      RequestIDs[RequestIDs.Count -1 ]                              *
        // **************************************************************************************************

        #region [ Members ]

        // Fields
        private List<int> m_requestIDs;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the query packet.
        /// </summary>
        /// <param name="packetID">Numeric identifier for the packet type.</param>
        protected QueryPacketBase(short packetID)
            : base(packetID)
        {
            m_requestIDs = new List<int>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets a list of historian identifiers whose information is being requested.
        /// </summary>
        /// <remarks>A singe entry with ID of -1 can be used to request information for all defined historian identifiers.</remarks>
        public IList<int> RequestIDs
        {
            get
            {
                return m_requestIDs;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="BinaryImage"/>.
        /// </summary>
        public override int BinaryLength
        {
            get
            {
                return (2 + 4 + (m_requestIDs.Count * 4));
            }
        }

        /// <summary>
        /// Gets the binary representation of the query packet.
        /// </summary>
        public override byte[] BinaryImage
        {
            get
            {
                byte[] image = new byte[BinaryLength];

                Array.Copy(EndianOrder.LittleEndian.GetBytes(TypeID), 0, image, 0, 2);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_requestIDs.Count), 0, image, 2, 4);
                for (int i = 0; i < m_requestIDs.Count; i++)
                {
                    Array.Copy(EndianOrder.LittleEndian.GetBytes(m_requestIDs[i]), 0, image, 6 + (i * 4), 4);
                }

                return image;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes the query packet from the specified <paramref name="binaryImage"/>.
        /// </summary>
        /// <param name="binaryImage">Binary image to be used for initializing the query packet.</param>
        /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="binaryImage"/>.</param>
        /// <param name="length">Valid number of bytes in <paramref name="binaryImage"/> from <paramref name="startIndex"/>.</param>
        /// <returns>Number of bytes used from the <paramref name="binaryImage"/> for initializing the query packet.</returns>
        public override int Initialize(byte[] binaryImage, int startIndex, int length)
        {
            if (length - startIndex >= 6)
            {
                // Binary image has sufficient data.
                short packetID = EndianOrder.LittleEndian.ToInt16(binaryImage, startIndex);
                if (packetID != TypeID)
                    throw new ArgumentException(string.Format("Unexpected packet id '{0}' (expected '{1}').", packetID, TypeID));

                // Ensure that the binary image is complete
                int requestIDCount = EndianOrder.LittleEndian.ToInt32(binaryImage, startIndex + 2);
                if (length - startIndex < 6 + requestIDCount * 4)
                    return 0;

                // We have a binary image with the correct packet id.
                m_requestIDs.Clear();
                for (int i = 0; i < requestIDCount; i++)
                {
                    m_requestIDs.Add(EndianOrder.LittleEndian.ToInt32(binaryImage, startIndex + 6 + (i * 4)));
                }

                return BinaryLength;
            }
            else
            {
                // Binary image does not have sufficient data.
                return 0;
            }
        }

        /// <summary>
        /// Extracts time series data from the query packet type.
        /// </summary>
        /// <returns>A null reference.</returns>
        public override IEnumerable<IDataPoint> ExtractTimeSeriesData()
        {
            return null;
        }

        #endregion
    }
}