// -----------------------------------------------------------------------
//  <copyright file="MainWindow.xaml.cs" company="Outbreak Labs">
//     Copyright (c) Outbreak Labs. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace OutbreakLabs.BroadcastRelay
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Net;
    using System.Timers;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Interop;
    using System.Windows.Media.Imaging;

    using Hardcodet.Wpf.TaskbarNotification;

    using OutbreakLabs.BroadcastRelay.Core;

    using SharpPcap;

    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Timer packetsRelayedTimer;

        private readonly IPersistence persistenceProvider;

        private readonly RelayManager relayManager;

        private TaskbarIcon taskbarIcon;

        public MainWindow()
        {
            this.InitializeComponent();
            this.Icon = Imaging.CreateBitmapSourceFromHIcon(
                Properties.Resources.MainIco.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            this.ListenAdapters = new ObservableCollection<AdapterSelection>();
            this.DestinationEntries = new ObservableCollection<DestinationEntry>();
            this.persistenceProvider = new SqlitePersistenceProvider();
            this.relayManager = new RelayManager();
            this.DataContext = this;
            this.packetsRelayedTimer = new Timer(1000);
            this.packetsRelayedTimer.Elapsed += this.packetsRelayedTimer_Elapsed;
            this.packetsRelayedTimer.Start();
        }

        /// <summary>
        /// Gets or sets the adapters on which to listen.
        /// </summary>
        /// <value>
        /// The adapters.
        /// </value>
        public ObservableCollection<AdapterSelection> ListenAdapters { get; set; }

        /// <summary>
        /// Gets or sets the destinations to relay packets.
        /// </summary>
        /// <value>
        /// The destinations.
        /// </value>
        public ObservableCollection<DestinationEntry> DestinationEntries { get; set; }

        /// <summary>
        /// Handles the Elapsed event of the packetsRelayedTimer. Used to update the Packets Relayed counter
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private void packetsRelayedTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.Invoke(() => { this.lblPacketsRelayed.Content = this.relayManager.PacketsRelayed; });
        }

        /// <summary>
        /// Handles the Loaded event of the Window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.LoadAdapters();

                this.LoadDestinations();

                this.SetupTray();

                if (this.ListenAdapters.Any(a => a.IsSelected))
                {
                    if (this.DestinationEntries.Any(r => r.IsValid))
                    {
                        this.Hide();
                    }
                    else
                    {
                        this.tclTabs.SelectedIndex = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// Configures the systray icon.
        /// </summary>
        private void SetupTray()
        {
            this.taskbarIcon = new TaskbarIcon();
            this.taskbarIcon.Icon = Properties.Resources.MainIco;
            this.taskbarIcon.ToolTipText = "Broadcast Relay";
            this.taskbarIcon.TrayMouseDoubleClick += this.tbi_TrayMouseDoubleClick;
        }

        /// <summary>
        /// Handles the TrayMouseDoubleClick event of the systray icon.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void tbi_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        /// <summary>
        /// Loads destinations from storage and adds them to the relay manager.
        /// </summary>
        private void LoadDestinations()
        {
            foreach (var destination in this.persistenceProvider.LoadDestinations())
            {
                var destinationEntry = new DestinationEntry { IPAddress = destination };

                this.DestinationEntries.Add(destinationEntry);
                if (destinationEntry.IsValid)
                {
                    this.relayManager.AddDestination(IPAddress.Parse(destinationEntry.IPAddress));
                }
            }
        }

        /// <summary>
        /// Loads the adapter selections from storage, ultimately adding them to the relay manager.
        /// </summary>
        private void LoadAdapters()
        {
            var savedAdapters = this.persistenceProvider.LoadAdapterSelections().ToArray();

            foreach (var dev in CaptureDeviceList.Instance)
            {
                var adapterSelection = new AdapterSelection { Text = dev.Description, Device = dev };
                adapterSelection.PropertyChanged += this.adapterSelection_PropertyChanged;
                this.ListenAdapters.Add(adapterSelection);
                if (savedAdapters.Contains(adapterSelection.Text))
                {
                    adapterSelection.IsSelected = true;
                }
            }

            //If we only have one adapter, it's obviously the one we want
            if (CaptureDeviceList.Instance.Count == 1)
            {
                this.ListenAdapters[0].IsSelected = true;
            }
        }

        /// <summary>
        /// Handles the PropertyChanged event of the adapter selection entries, namely IsSelected.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void adapterSelection_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var typed = sender as AdapterSelection;
            if (typed != null && e.PropertyName == "IsSelected")
            {
                try
                {
                    if (typed.IsSelected)
                    {
                        this.relayManager.EnableCaptureDevice(typed.Device);
                    }
                    else
                    {
                        this.relayManager.DisableCaptureDevice(typed.Device);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the Add Destination button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnAddDestination_Click(object sender, RoutedEventArgs e)
        {
            this.DestinationEntries.Add(new DestinationEntry());
        }

        /// <summary>
        /// Handles the Click event of the X button on the destination entries.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnDeleteDestination(object sender, RoutedEventArgs e)
        {
            var DestinationEntry = ((Button)sender).DataContext as DestinationEntry;
            if (DestinationEntry != null)
            {
                IPAddress ip;
                if (DestinationEntry.IPAddress != null && IPAddress.TryParse(DestinationEntry.IPAddress, out ip))
                {
                    this.relayManager.RemoveDestination(ip);
                }
                this.DestinationEntries.Remove(DestinationEntry);
            }
        }

        /// <summary>
        /// Handles the Closed event of the Window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                this.relayManager.Dispose();
                this.packetsRelayedTimer.Stop();
                this.persistenceProvider.SaveAdapterSelections(this.ListenAdapters.Where(a => a.IsSelected).Select(a=>a.Text));
                this.persistenceProvider.SaveDestinations(this.DestinationEntries.Select(a=>a.IPAddress));
                this.taskbarIcon.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// Handles the StateChanged event of the Window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
        }

        /// <summary>
        /// Handles the Closing event of the Window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            var messageBoxResult = MessageBox.Show(
                "Are you sure you want to exit?",
                "Exit Confirmation",
                MessageBoxButton.YesNo);
            e.Cancel = messageBoxResult == MessageBoxResult.No;
        }

        /// <summary>
        /// Handles the Click event of the Lock button on the destination entries.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnLock_Click(object sender, RoutedEventArgs e)
        {
            var destinationEntry = ((Button)sender).DataContext as DestinationEntry;
            destinationEntry?.Validate();
            if (destinationEntry != null && destinationEntry.IsValid)
            {
                this.relayManager.AddDestination(IPAddress.Parse(destinationEntry.IPAddress));
            }
            else
            {
                MessageBox.Show("IP address is not valid");
            }
        }
    }
}