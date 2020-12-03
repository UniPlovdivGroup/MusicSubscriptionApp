using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using SSLClientEssentials;
using SSLConnectionEssentials;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Music_subscription_app
{
    public class MainWindow : Window
    {
        private TextBox NameBox;
        private TextBox Last_nameBox;
        private TextBox EmailBox;
        private TextBox CityBox;
        private ComboBox StateBox;
        private TextBox ZipCodeBox;
        private TextBox ArtistsBox;

        private CheckBox PopCheck;
        private CheckBox SoulCheck;
        private CheckBox IndieCheck;
        private CheckBox OtherCheck;
        private CheckBox RockCheck;
        private CheckBox RnBCheck;
        private CheckBox BluesCheck;
        private CheckBox HipHopCheck;
        private CheckBox ElectronicCheck;
        private CheckBox LatinCheck;
        private CheckBox JazzCheck;
        private CheckBox ExperimentalCheck;
        private CheckBox PunkCheck;

        private Button SendFormButton;

        private TextBox AppConsoleBox;
        private TextBox MessageBox;
        private Button SendMsgButton;
        private string[] array = new string[] { "Not Specified", "US", "Bulgaria", "Serbia", "Ukraine" };

        private SSLClient client { get; set; } = new SSLClient();

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();

#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            LoggerDebug.Logger.Suppress = true;
            NameBox = this.FindControl<TextBox>("Name");
            Last_nameBox = this.FindControl<TextBox>("Last_name");
            EmailBox = this.FindControl<TextBox>("Email");
            CityBox = this.FindControl<TextBox>("City");
            StateBox = this.FindControl<ComboBox>("State");
            ZipCodeBox = this.FindControl<TextBox>("ZipCode");
            ArtistsBox = this.FindControl<TextBox>("Artists");

            PopCheck = this.FindControl<CheckBox>("Pop");
            SoulCheck = this.FindControl<CheckBox>("Soul");
            IndieCheck = this.FindControl<CheckBox>("Indie");
            OtherCheck = this.FindControl<CheckBox>("Other");
            RockCheck = this.FindControl<CheckBox>("Rock");
            RnBCheck = this.FindControl<CheckBox>("RnB");
            BluesCheck = this.FindControl<CheckBox>("Blues");
            HipHopCheck = this.FindControl<CheckBox>("Hip_hop");
            ElectronicCheck = this.FindControl<CheckBox>("Electronic");
            LatinCheck = this.FindControl<CheckBox>("Latin");
            JazzCheck = this.FindControl<CheckBox>("Jazz");
            ExperimentalCheck = this.FindControl<CheckBox>("Experimental");
            PunkCheck = this.FindControl<CheckBox>("Punk");

            SendFormButton = this.FindControl<Button>("SendForm");
            SendFormButton.Click += SendFormButton_Click;
            AppConsoleBox = this.FindControl<TextBox>("AppConsole");
            MessageBox = this.FindControl<TextBox>("messageBox");

            SendMsgButton = this.FindControl<Button>("SendMsg");
            SendMsgButton.Click += SendMsgButton_Click;

            UILogger.ConsoleBox = AppConsoleBox;
        }

        private void SendFormButton_Click(object sender, RoutedEventArgs e)
        {
            List<string> genresList = new List<string>();

            checkBoxPressed(PopCheck, genresList);
            checkBoxPressed(SoulCheck, genresList);
            checkBoxPressed(IndieCheck, genresList);
            checkBoxPressed(OtherCheck, genresList);
            checkBoxPressed(RockCheck, genresList);
            checkBoxPressed(RnBCheck, genresList);
            checkBoxPressed(BluesCheck, genresList);
            checkBoxPressed(HipHopCheck, genresList);
            checkBoxPressed(ElectronicCheck, genresList);
            checkBoxPressed(LatinCheck, genresList);
            checkBoxPressed(JazzCheck, genresList);
            checkBoxPressed(ExperimentalCheck, genresList);
            checkBoxPressed(PunkCheck, genresList);

            StringBuilder genresStringBuilder = new StringBuilder();
            foreach (string genre in genresList)
            {
                genresStringBuilder.Append($"{genre} ");
            }

            string genres = genresStringBuilder.ToString();

            if (string.IsNullOrEmpty(NameBox.Text) || string.IsNullOrEmpty(Last_nameBox.Text)
                || string.IsNullOrEmpty(EmailBox.Text) || string.IsNullOrEmpty(CityBox.Text)
                || StateBox.SelectedIndex == 0 || string.IsNullOrEmpty(ZipCodeBox.Text)
                || string.IsNullOrEmpty(ArtistsBox.Text) || string.IsNullOrEmpty(genres))
            {
                UILogger.Log("Missing person details", false);
                return;
            }

            DataToSend data = new DataToSend(NameBox.Text, Last_nameBox.Text, EmailBox.Text, CityBox.Text, array[StateBox.SelectedIndex], ZipCodeBox.Text, ArtistsBox.Text, genres);

            if (client.IsConnected)
            {
                client.SendData<DataToSend>(data, "DataSent");
                UILogger.Log("Data has been sent", false);
            }
            else
            {
                UILogger.Log("Not connected to server. Cannot send data.", false);
            }
        }
        private void SendMsgButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Text.ToLower() == "self connect")
            {
                ConnectToServer(new IPEndPoint(IPAddress.Loopback, 1258));
            }
            else if (MessageBox.Text.ToLower() == ("disconnect"))
            {
                DisconnectFromCurrentServer();
            }
            else if (MessageBox.Text.ToLower().Contains("connect"))
            {
                string connectCommand = MessageBox.Text;
                string[] commandSplit = connectCommand.Split(' ');

                if (commandSplit.Length == 3)
                {
                    ConnectToServer(new IPEndPoint(IPAddress.Parse(commandSplit[1]), int.Parse(commandSplit[2])));
                }
            }
            else if (MessageBox.Text.ToLower().Contains("help"))
            {
                UILogger.Log("help - shows the list of commands", false);
                UILogger.Log("connect <IPAdress> <port number> - connects to a remote server", false);
                UILogger.Log("disconnect - disconnects from the server", false);
                UILogger.Log("self connect - connects to server on the same computer", false);
            }
            else
            {
                UILogger.Log("Invalid command. Please type \"help\" to get list of commands.", false);
            }
            MessageBox.Text = "";
        }
        private void checkBoxPressed(CheckBox checkBox, List<string> genres)
        {
            if (checkBox.IsChecked == true)
            {
                genres.Add($"{checkBox.Name}");
            }
        }

        private void ConnectToServer(IPEndPoint EndPoint)
        {
            if (client.IsConnected == true)
            {
                UILogger.Log("Already connected", false);
            }
            else
            {
                bool connectionStatus = client.Connect(EndPoint);
                if (connectionStatus)
                {
                    client.StartReceivingData();
                    client.ConnectionLost += ConnectionLostHandler;
                    client.OnNewDataFullyReceived += HandleFullyReceivedPackage;
                    UILogger.Log("Successfully connected to server!", false);
                }
                else UILogger.Log("Unable to connect to server!", false);
            }
        }
        private void DisconnectFromCurrentServer()
        {
            client.Disconnect();
            UILogger.Log("Disconnected from a server", false);
        }

        private void ConnectionLostHandler(object s, EventArgs e)
        {
            UILogger.Log("Connection was lost", false);
        }
        private void HandleFullyReceivedPackage(object sender, EventArgs e)
        {
            ReceivedDataArgs args = (ReceivedDataArgs)e;
            dynamic receivedObject = args.GetReceivedObject();
            if (receivedObject.GetType() == typeof(DataToSend))
            {
                DataToSend reconstructed = (receivedObject as DataToSend);
                UILogger.Log(reconstructed.ToString(), false);
                reconstructed.Dispose();

            }
        }
    }
}
