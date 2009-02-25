//*******************************************************************************************************
//  IDataFrame.cs
//  Copyright © 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: James R Carroll
//      Office: PSO TRAN & REL, CHATTANOOGA - MR BK-C
//       Phone: 423/751-4165
//       Email: jrcarrol@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  01/14/2005 - James R Carroll
//       Generated original version of source code.
//
//*******************************************************************************************************

namespace PCS.PhasorProtocols
{
    /// <summary>
    /// Represents a protocol independent representation of any data frame.
    /// </summary>
    public interface IDataFrame : IChannelFrame
    {
        /// <summary>
        /// Gets or sets <see cref="IConfigurationFrame"/> associated with this <see cref="IDataFrame"/>.
        /// </summary>
        IConfigurationFrame ConfigurationFrame { get; set; }

        /// <summary>
        /// Gets reference to the <see cref="DataCellCollection"/> for this <see cref="IDataFrame"/>.
        /// </summary>
        new DataCellCollection Cells { get; }
    }
}
