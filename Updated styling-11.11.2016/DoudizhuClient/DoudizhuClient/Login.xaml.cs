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

namespace DoudizhuClient
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private DoudizhuService.DoudizhuServiceClient proxy;

        public Login()
        {
            InitializeComponent();
            proxy = new DoudizhuService.DoudizhuServiceClient();
            this.CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, this.OnCloseWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, this.OnMaximizeWindow, this.OnCanResizeWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, this.OnMinimizeWindow, this.OnCanMinimizeWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, this.OnRestoreWindow, this.OnCanResizeWindow));
        }

        private void OnCanResizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ResizeMode == ResizeMode.CanResize || this.ResizeMode == ResizeMode.CanResizeWithGrip;
        }

        private void OnCanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.ResizeMode != ResizeMode.NoResize;
        }

        private void OnCloseWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        private void OnMaximizeWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        private void OnMinimizeWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void OnRestoreWindow(object target, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }

        private void bt_Login_Click(object sender, RoutedEventArgs e)
        {
            string username = tb_username.Text;
            string password = tb_password.Password;

            //if (username == "" || password == "")
            //{
            //    MessageBox.Show("Please input necessary information.");
            //    return;
            //}
            username = "this is a test!";

            Lobby lobby = new Lobby(username);
            ////这个login下次直接在这个form里面检测。
            //int result = proxy.Login(username, password);
            //if (result == 1)
            //{
                lobby.Show();
                this.Hide();
            //}
            //else
            //{
            //    if (result == 0)
            //        MessageBox.Show("Wrong username and password combination.");
            //    if (result == 2)
            //        MessageBox.Show("The user has already logged in.");
            //}

            lobby.Closing += Doudizhu_Closing;
        }

        private void Doudizhu_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Show();
        }

        private void bt_register_Click(object sender, RoutedEventArgs e)
        {
            string username = tb_username.Text;
            string password = tb_password.Password;

            if (username == "" || password == "")
            {
                MessageBox.Show("Please input necessary information.");
                return;
            }

            if (proxy.Register(username, password))
            {
                MessageBox.Show("Successfully registered!");
            }
            else
            {
                MessageBox.Show("Failed! Username already taken!");
            }
        }

        private void tb_username_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.Text = string.Empty;
            tb.GotFocus -= tb_username_GotFocus;
        }

        private void tb_password_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordBox pb = (PasswordBox)sender;
            pb.Password = string.Empty;
            pb.GotFocus -= tb_password_GotFocus;
        }
    }
}
