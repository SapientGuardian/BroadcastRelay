// -----------------------------------------------------------------------
//  <copyright file="AdapterSelection.cs" company="Outbreak Labs">
//     Copyright (c) Outbreak Labs. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace OutbreakLabs.BroadcastRelay
{
    using System.ComponentModel;

    using SharpPcap;

    /// <summary>
    /// View model for an adapter selection
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class AdapterSelection : INotifyPropertyChanged
    {
        private bool _IsSelected;

        /// <summary>
        /// Gets or sets the text of the selection entry.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this adapter is selected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this adapter is selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsSelected
        {
            get
            {
                return this._IsSelected;
            }
            set
            {
                this._IsSelected = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsSelected"));
            }
        }

        /// <summary>
        /// Gets or sets the device tracked by this selection.
        /// </summary>
        /// <value>
        /// The device.
        /// </value>
        public ICaptureDevice Device { get; set; }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}