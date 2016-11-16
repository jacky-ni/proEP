using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DoudizhuClient.DoudizhuService;
using System.ServiceModel;

namespace DoudizhuClient
{
    public delegate void buttondelegateLobby(bool b, Button button);
    public delegate void labelDelegateLobby(string msg, bool b, Label label);
    public delegate void tableDelegateLobby(bool b, Ellipse elli);

    /// <summary>
    /// Interaction logic for Lobby.xaml
    /// </summary>
    public partial class Lobby : Window,DoudizhuService.ILoadLobbyCallback
    {
        private string username;

        private labelDelegateLobby delegatelabels;
        private buttondelegateLobby delegateSitdown;
        private tableDelegateLobby delegateTable;

        private DoudizhuService.DoudizhuServiceClient proxy;
        private DoudizhuService.PlayerJoinLeaveRoomClient proxyJoinLeaveRoom;
        private DoudizhuService.LoadLobbyClient proxyLoadLobby;

        public Lobby(string username)
        {
            InitializeComponent();
            proxy = new DoudizhuService.DoudizhuServiceClient();
            proxyJoinLeaveRoom = new DoudizhuService.PlayerJoinLeaveRoomClient(new InstanceContext(this));
            proxyLoadLobby = new DoudizhuService.LoadLobbyClient(new InstanceContext(this));
            proxyLoadLobby.loadLobbySubscribe();

            this.username = username;
            this.Closing += Lobby_Closing;

            delegatelabels = new labelDelegateLobby(showlabel);
            delegateSitdown = new buttondelegateLobby(enablesitdown);
            delegateTable = new tableDelegateLobby(showTable);
        }

        #region private functions
        
        private void Lobby_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            proxyLoadLobby.loadLobbyUnSubscribe();
        }
        
        private void grid_Loaded(object sender, RoutedEventArgs e)
        {
            proxy.LoadLobby();
        }

        private void Doudizhu_Closed(object sender, EventArgs e)
        {
            this.Show();
        }

        #region delegates

        private void showlabel(string msg, bool b, Label label)
        {
            label.Content = msg;
            if (b)
                label.Visibility = Visibility.Visible;
            else
                label.Visibility = Visibility.Hidden;
        }

        private void enablesitdown(bool b, Button button)
        {
            if (b)
                button.IsEnabled = true;
            else
                button.IsEnabled = false;
        }

        private void showTable(bool b, Ellipse elli)
        {
            if (b)
                elli.Fill = Brushes.Green;
            else
                elli.Fill = Brushes.Yellow;
        }

        #endregion

        #endregion

        private void bt_1_1_Click(object sender, RoutedEventArgs e)
        {
            string[] splits = ((Button)sender).Name.Split('_');
            MainWindow doudizhu = new MainWindow(username.ToString() + "_1", Convert.ToInt32(splits[1]), Convert.ToInt32(splits[2]), 100);
            doudizhu.Closed += Doudizhu_Closed;
            doudizhu.Show();
            //this.Hide();
        }

        private void bt_1_2_Click(object sender, RoutedEventArgs e)
        {
            string[] splits = ((Button)sender).Name.Split('_');
            MainWindow doudizhu = new MainWindow(username.ToString() + "_2", Convert.ToInt32(splits[1]), Convert.ToInt32(splits[2]), 100);
            doudizhu.Show();
            //this.Hide();
        }

        private void bt_1_3_Click(object sender, RoutedEventArgs e)
        {
            string[] splits = ((Button)sender).Name.Split('_');
            MainWindow doudizhu = new MainWindow(username.ToString() + "_3", Convert.ToInt32(splits[1]), Convert.ToInt32(splits[2]), 100);
            doudizhu.Show();
            //this.Hide();
        }

        private void bt_2_1_Click(object sender, RoutedEventArgs e)
        {
            string[] splits = ((Button)sender).Name.Split('_');
            MainWindow doudizhu = new MainWindow(username.ToString() + "_4", Convert.ToInt32(splits[1]), Convert.ToInt32(splits[2]), 100);
            doudizhu.Show();
            //this.Hide();
        }

        private void bt_2_2_Click(object sender, RoutedEventArgs e)
        {
            string[] splits = ((Button)sender).Name.Split('_');
            MainWindow doudizhu = new MainWindow(username.ToString() + "_5", Convert.ToInt32(splits[1]), Convert.ToInt32(splits[2]), 100);
            doudizhu.Show();
            //this.Hide();
        }

        private void bt_2_3_Click(object sender, RoutedEventArgs e)
        {
            string[] splits = ((Button)sender).Name.Split('_');
            MainWindow doudizhu = new MainWindow(username.ToString() + "_6", Convert.ToInt32(splits[1]), Convert.ToInt32(splits[2]), 100);
            doudizhu.Show();
            //this.Hide();
        }

        public void LoadLobby_Client(Player p1, Player p2, Player p3, int roomNr)
        {
            //name labels.
            Label p1Name = grid.FindName("lb_" + roomNr + "_1") as Label;
            Label p2Name = grid.FindName("lb_" + roomNr + "_2") as Label;
            Label p3Name = grid.FindName("lb_" + roomNr + "_3") as Label;
            delegatelabels(p1 == null ? "" : p1.Account.Username, true, p1Name);
            delegatelabels(p2 == null ? "" : p2.Account.Username, true, p2Name);
            delegatelabels(p3 == null ? "" : p3.Account.Username, true, p3Name);

            //ready labels.
            Label p1Ready = grid.FindName("lb_" + roomNr + "_1_ready") as Label;
            Label p2Ready = grid.FindName("lb_" + roomNr + "_2_ready") as Label;
            Label p3Ready = grid.FindName("lb_" + roomNr + "_3_ready") as Label;
            delegatelabels("Ready", p1 == null ? false : p1.Ready, p1Ready);
            delegatelabels("Ready", p2 == null ? false : p2.Ready, p2Ready);
            delegatelabels("Ready", p3 == null ? false : p3.Ready, p3Ready);

            //button labels.
            Button p1sitdown = grid.FindName("bt_" + roomNr + "_1") as Button;
            Button p2sitdown = grid.FindName("bt_" + roomNr + "_2") as Button;
            Button p3sitdown = grid.FindName("bt_" + roomNr + "_3") as Button;
            delegateSitdown(p1 == null ? true : false, p1sitdown);
            delegateSitdown(p2 == null ? true : false, p2sitdown);
            delegateSitdown(p3 == null ? true : false, p3sitdown);

            //the table(room).
            if (p1 != null && p2 != null && p3 != null)
            {
                if (p1.Ready && p2.Ready && p3.Ready)
                    showTable(true, grid.FindName("table_" + roomNr) as Ellipse);
                else
                    showTable(false, grid.FindName("table_" + roomNr) as Ellipse);
            }
            else
            {
                showTable(false, grid.FindName("table_" + roomNr) as Ellipse);
            }
        }
    }
}
