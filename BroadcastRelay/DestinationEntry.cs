// -----------------------------------------------------------------------
//  <copyright file="DestinationEntry.cs" company="Outbreak Labs">
//     Copyright (c) Outbreak Labs. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace OutbreakLabs.BroadcastRelay
{
    using System.ComponentModel;
    using System.Net;

    /// <summary>
    /// View model for destinations
    /// </summary>
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    public class DestinationEntry : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the ip address.
        /// </summary>
        /// <value>
        /// The ip address.
        /// </value>
        public string IPAddress { get; set; }

        /// <summary>
        /// Gets a value indicating whether the IP address is invalid; defined as !Valid.
        /// </summary>
        /// <value>
        /// <c>true</c> if the IP address is invalid; otherwise, <c>false</c>.
        /// </value>
        public bool IsInvalid => !this.IsValid;

        /// <summary>
        /// Returns true if the IP address is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the IP address is valid; otherwise, <c>false</c>.
        /// </value>
        public bool IsValid
        {
            get
            {
                IPAddress temp;
                return System.Net.IPAddress.TryParse(this.IPAddress, out temp);
            }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Triggers the PropertyChanged event for IsValid and IsInvalid.
        /// </summary>
        public void Validate()
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs("IsValid"));
            this.PropertyChanged(this, new PropertyChangedEventArgs("IsInvalid"));
        }
    }
}