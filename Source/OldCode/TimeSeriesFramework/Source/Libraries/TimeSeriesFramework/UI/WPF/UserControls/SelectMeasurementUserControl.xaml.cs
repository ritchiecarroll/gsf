﻿//******************************************************************************************************
//  SelectMeasurementUserControl.xaml.cs - Gbtc
//
//  Copyright © 2010, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  05/17/2011 - Mehulbhai P Thakkar
//       Generated original version of source code.
//
//******************************************************************************************************

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TimeSeriesFramework.UI.ViewModels;

namespace TimeSeriesFramework.UI.UserControls
{
    /// <summary>
    /// Interaction logic for SelectMeasurementUserControl.xaml
    /// </summary>
    public partial class SelectMeasurementUserControl : UserControl
    {
        #region [ Members ]

        // Delegates

        /// <summary>
        /// Method signature for function used to handle SourceCollectionChanged event.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        public delegate void OnSourceCollectionChanged(object sender, RoutedEventArgs e);

        // Events

        /// <summary>
        /// Event to notify subcribers when collection changes.
        /// </summary>
        public event OnSourceCollectionChanged SourceCollectionChanged;

        // Fields
        private SelectMeasurements m_dataContext;
        private ObservableCollection<TimeSeriesFramework.UI.DataModels.Measurement> m_itemsSource;

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets updated measurement list.
        /// </summary>
        public ObservableCollection<TimeSeriesFramework.UI.DataModels.Measurement> UpdatedMeasurements
        {
            get
            {
                return m_dataContext.ItemsSource;
            }
        }

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates an instance of <see cref="SelectMeasurementUserControl"/> class.
        /// </summary>
        public SelectMeasurementUserControl()
        {
            InitializeComponent();
            // TODO: Think about the placement of try catch statement.
            // It was added here since designed did not work without it.
            try
            {
                m_dataContext = new SelectMeasurements(17);
                m_itemsSource = m_dataContext.ItemsSource;

                foreach (TimeSeriesFramework.UI.DataModels.Measurement measurement in m_itemsSource)
                    measurement.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(measurement_PropertyChanged);

                this.DataContext = m_dataContext;
            }
            catch
            {

            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Handles property changed event for measurement.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        void measurement_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (SourceCollectionChanged != null)
                SourceCollectionChanged(this, null);
        }

        /// <summary>
        /// Hanldes checked event on the select all check box.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void CheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (TimeSeriesFramework.UI.DataModels.Measurement measurement in m_itemsSource)
                measurement.Selected = true;
        }

        /// <summary>
        /// Handles unchecked event on the select all check box.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void CheckBox_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            foreach (TimeSeriesFramework.UI.DataModels.Measurement measurement in m_itemsSource)
                measurement.Selected = false;
        }

        /// <summary>
        /// Method to uncheck check boxes checked by user.
        /// </summary>
        public void UncheckSelection()
        {
            foreach (TimeSeriesFramework.UI.DataModels.Measurement measurement in m_itemsSource)
                measurement.Selected = false;
        }

        #endregion
    }
}
