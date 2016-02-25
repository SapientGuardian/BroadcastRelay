// -----------------------------------------------------------------------
//  <copyright file="IPersistence.cs" company="Outbreak Labs">
//     Copyright (c) Outbreak Labs. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace OutbreakLabs.BroadcastRelay.Core
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface for a provider to handle saving and loading
    /// </summary>
    public interface IPersistence
    {
        /// <summary>
        /// Saves a set of adapter selections.
        /// </summary>
        /// <param name="adapterSelections">The adapter selections.</param>
        void SaveAdapterSelections(IEnumerable<string> adapterSelections);

        /// <summary>
        /// Saves a set of destinations.
        /// </summary>
        /// <param name="destinationEntries">The destination entries.</param>
        void SaveDestinations(IEnumerable<string> destinationEntries);

        /// <summary>
        /// Loads the adapter selections.
        /// </summary>
        /// <returns>The selected adapters.</returns>
        IEnumerable<string> LoadAdapterSelections();

        /// <summary>
        /// Loads the destinations.
        /// </summary>
        /// <returns>The destinations</returns>
        IEnumerable<string> LoadDestinations();
    }
}