// -----------------------------------------------------------------------
//  <copyright file="SqlitePersistenceProvider.cs" company="Outbreak Labs">
//     Copyright (c) Outbreak Labs. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace OutbreakLabs.BroadcastRelay.Core
{
    using System;
    using System.Collections.Generic;
    using System.Data.SQLite;
    using System.IO;

    /// <summary>
    /// Implementation of IPersistence for Sqlite
    /// </summary>
    /// <seealso cref="IPersistence" />
    /// <seealso cref="System.IDisposable" />
    public class SqlitePersistenceProvider : IPersistence, IDisposable
    {
        private const string filename = "Settings.sqlite";

        private readonly SQLiteConnection m_dbConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlitePersistenceProvider"/> class. Creates Settings.sqlite and initializes the schema.
        /// </summary>
        public SqlitePersistenceProvider()
        {
            
            if (!File.Exists(filename))
            {
                SQLiteConnection.CreateFile(filename);
                this.m_dbConnection = new SQLiteConnection($"Data Source={filename};Version=3;");
            }
            else
            {
                this.m_dbConnection = new SQLiteConnection($"Data Source={filename};Version=3;");
            }

            this.CreateSchema();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.m_dbConnection.Dispose();
        }

        /// <summary>
        /// Saves a set of adapter selections.
        /// </summary>
        /// <param name="adapterSelections">The adapter selections.</param>
        public void SaveAdapterSelections(IEnumerable<string> adapterSelections)
        {
            try
            {
                this.m_dbConnection.Open();
                new SQLiteCommand("delete from AdapterSelection", this.m_dbConnection).ExecuteNonQuery();
                foreach (var adapter in adapterSelections)
                {
                    var cmd = new SQLiteCommand(
                        "insert into AdapterSelection  ([Text]) values (@Text)",
                        this.m_dbConnection);
                    cmd.Parameters.Add(new SQLiteParameter("@Text", adapter));
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                this.m_dbConnection.Close();
            }
        }

        /// <summary>
        /// Saves the destinations.
        /// </summary>
        /// <param name="destinationEntries">The destination entries.</param>
        public void SaveDestinations(IEnumerable<string> destinationEntries)
        {
            try
            {
                this.m_dbConnection.Open();
                new SQLiteCommand("delete from Destination", this.m_dbConnection).ExecuteNonQuery();
                foreach (var destination in destinationEntries)
                {
                    var cmd = new SQLiteCommand(@"insert into Destination  (IPAddress) 
                                                values (@IPAddress)", this.m_dbConnection);
                    cmd.Parameters.Add(new SQLiteParameter("@IPAddress", (object)destination ?? DBNull.Value));
                    cmd.ExecuteNonQuery();
                }
            }
            finally
            {
                this.m_dbConnection.Close();
            }
        }

        /// <summary>
        /// Loads the adapter selections.
        /// </summary>
        /// <returns>
        /// The selected adapters.
        /// </returns>
        public IEnumerable<string> LoadAdapterSelections()
        {
            try
            {
                this.m_dbConnection.Open();
                var reader =
                    new SQLiteCommand("select [Text] from AdapterSelection", this.m_dbConnection).ExecuteReader();
                while (reader.Read())
                {
                    yield return reader.GetString(0);
                }
            }
            finally
            {
                this.m_dbConnection.Close();
            }
        }

        /// <summary>
        /// Loads the destinations.
        /// </summary>
        /// <returns>
        /// The destinations
        /// </returns>
        public IEnumerable<string> LoadDestinations()
        {
            try
            {
                this.m_dbConnection.Open();
                var reader = new SQLiteCommand("select * from Destination", this.m_dbConnection).ExecuteReader();
                while (reader.Read())
                {
                    yield return reader["IPAddress"] == DBNull.Value ? null : (string)reader["IPAddress"];
                }
            }
            finally
            {
                this.m_dbConnection.Close();
            }
        }

        /// <summary>
        /// Creates the schema.
        /// </summary>
        private void CreateSchema()
        {
            try
            {
                this.m_dbConnection.Open();
                try
                {
                    new SQLiteCommand("create table AdapterSelection  ([Text] varchar(255))", this.m_dbConnection)
                        .ExecuteNonQuery();
                }
                catch
                {
                }

                try
                {
                    new SQLiteCommand("create table Destination  (IPAddress varchar(16))", this.m_dbConnection)
                        .ExecuteNonQuery();
                }
                catch
                {
                }
            }
            finally
            {
                this.m_dbConnection.Close();
            }
        }
    }
}