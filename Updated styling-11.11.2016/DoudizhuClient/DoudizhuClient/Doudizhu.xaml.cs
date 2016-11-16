using DoudizhuClient.DoudizhuService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DoudizhuClient
{
    public delegate void trurfalseDelegate(bool b);
    public delegate void tfWithMsgDelegate(string msg, bool b);
    public delegate void updategivencards(Card[] cards);
    public delegate void updateControlsLocation();
    public delegate void updateAvator(bool joinleft, bool male_left, bool landlord_left, bool male_my, bool landlord_my, bool joinright, bool male_right, bool landlord_right);

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
     /*
     * Multiple    服务实例是多线程的。 无同步保证。 因为其他线程可以随时更改服务对象，所以必须始终处理同步与状态一致性。
     * Reentrant   服务实例是单线程的，且接受可重入调用。 可重入服务接受在调用其他服务的同时进行调用；因此在调出之前，您需要负责让对象的状态一致，而在调出之后，必须确认本地操作数据有效。
     * 请注意，只有通过 WCF 通道调用其他服务，才能解除服务实例锁定。 在此情况下，已调用的服务可以通过回调重入第一个服务。 如果第一个服务不可重入，则该调用顺序会导致死锁。 
     * 有关详细信息，请参见ConcurrencyMode。
     * Single  服务实例是单线程的，且不接受可重入调用。 如果 InstanceContextMode 属性为 Single，且其他消息在实例处理调用的同时到达，则这些消息必须等待，直到服务可用或消息超时为止。
     */
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]//writen down in the server side.
    public partial class MainWindow : Window, DoudizhuService.IDeterminteLandlordCallback, DoudizhuService.IUpdateGameCallback, DoudizhuService.IPlayerJoinLeaveRoomCallback,
        DoudizhuService.ISetCancelPlayerReadyCallback, DoudizhuService.IStartGameCallback, DoudizhuService.IGameOverCallback, DoudizhuService.IUpdateChatterCallback,
        DoudizhuService.IUpdateLandlordCardCallback, DoudizhuService.ITimeCounterDownCallback
    {
        #region private variables.

        private string username = "";
        private int roomNumber = 0;
        private int baseScore = 0;
        private int position = 0;

        private int cardheight = 150;
        private int cardWidth = 100;

        #region sound effects
        //sound effects.
        private MediaElement SoundBGM;//background music.
        private MediaElement SoundbtClick;//按按钮的声音
        private MediaElement SoundStart;//开局声音
        private MediaElement SoundClick;//按钮点击声音
        private MediaElement SoundGive;//出牌声音
        private MediaElement SoundLoss;//当前玩家输了声音
        private MediaElement SoundWin;//当前玩家赢了声音
        private MediaElement Soundwjiaodizhu;
        private MediaElement Soundwqiangdizhu;
        private MediaElement Soundwbujiao;
        private MediaElement Soundwbuqiang;
        private MediaElement Soundwpass;
        private MediaElement Soundmjiaodizhu;
        private MediaElement Soundmqiangdizhu;
        private MediaElement Soundmbujiao;
        private MediaElement Soundmpass;
        private MediaElement Soundmbuqiang;
        private MediaElement Soundmqiangdizhu2;
        private MediaElement Soundmqiangdizhu3;
        private MediaElement Soundwqiangdizhu2;
        private MediaElement Soundwqiangdizhu3;
        #endregion

        private List<Image> cardImages;//剩余手牌的图形

        //玩家出的牌。
        List<Image> leftgivencards = null;
        List<Image> rightgivencards = null;
        List<Image> mygivencards = null;

        #endregion

        #region private variables for delegation.

        //抢地主的按钮。
        private trurfalseDelegate delegateDL;
        //出牌，pass，提示的按钮。
        private trurfalseDelegate delegateGPH;
        //出牌按钮。
        private trurfalseDelegate delegateGive;
        //准备按钮。
        private tfWithMsgDelegate delegateReady;
        private trurfalseDelegate delegateleftReadyImage;
        private trurfalseDelegate delegaterightReadyImage;
        private trurfalseDelegate delegatemyReadyImage;
        //显示/隐藏一些label。
        private tfWithMsgDelegate delegateleftmsg;
        private tfWithMsgDelegate delegaterightmsg;
        private tfWithMsgDelegate delegatemymsg;
        private tfWithMsgDelegate delegatelefttimer;
        private tfWithMsgDelegate delegatemytimer;
        private tfWithMsgDelegate delegaterighttimer;
        private tfWithMsgDelegate delegateresult;
        private tfWithMsgDelegate delegateleftovertype;
        private tfWithMsgDelegate delegateleftusername;
        private tfWithMsgDelegate delegatemyusername;
        private tfWithMsgDelegate delegaterightusername;
        private tfWithMsgDelegate delegateleftleftcards;
        private tfWithMsgDelegate delegaterightleftcards;
        private updategivencards delegatemygivencards;
        private updategivencards delegateleftgivencards;
        private updategivencards delegaterightgivencards;
        private updateAvator delegateupdateavator;

        private updateControlsLocation delegateUpdateControlLocations;

        #endregion

        #region proxys.

        private DoudizhuService.DoudizhuServiceClient proxy;

        //proxy of the server.
        private DoudizhuService.PlayerJoinLeaveRoomClient proxyJoinLeaveRoom;
        private DoudizhuService.SetCancelPlayerReadyClient proxySetCancelReady;
        private DoudizhuService.StartGameClient proxyStart;
        private DoudizhuService.DeterminteLandlordClient proxyDeteLandlord;
        private DoudizhuService.UpdateLandlordCardClient proxyUpdateLandlordCard;
        private DoudizhuService.UpdateGameClient proxyUpdateGame;
        private DoudizhuService.TimeCounterDownClient proxyTimeCounter;
        private DoudizhuService.GameOverClient proxyGameOver;
        private DoudizhuService.UpdateChatterClient proxyUpdateChatter;

        #endregion

        //constructor.
        public MainWindow(string username, int tablenumber, int position, int baseScore)
        {
            InitializeComponent();

            delegateDL = new trurfalseDelegate(showDL);
            delegateGPH = new trurfalseDelegate(showHideGPH);
            delegateGive = new trurfalseDelegate(showGive);
            delegateReady = new tfWithMsgDelegate(showReady);
            delegateleftReadyImage = new trurfalseDelegate(showleftready);
            delegaterightReadyImage = new trurfalseDelegate(showrightready);
            delegatemyReadyImage = new trurfalseDelegate(showmyready);
            delegatemymsg = new tfWithMsgDelegate(showmymsg);
            delegateleftmsg = new tfWithMsgDelegate(showleftmsg);
            delegaterightmsg = new tfWithMsgDelegate(showrightmsg);
            delegatelefttimer = new tfWithMsgDelegate(showlefttimer);
            delegatemytimer = new tfWithMsgDelegate(showmytimer);
            delegaterighttimer = new tfWithMsgDelegate(showrighttimer);
            delegateresult = new tfWithMsgDelegate(showresult);
            delegateleftovertype = new tfWithMsgDelegate(showcardtype);
            delegateleftusername = new tfWithMsgDelegate(showleftusername);
            delegatemyusername = new tfWithMsgDelegate(showmyusername);
            delegaterightusername = new tfWithMsgDelegate(showrightusername);
            delegateleftleftcards = new tfWithMsgDelegate(showleftleftcards);
            delegaterightleftcards = new tfWithMsgDelegate(showrightleftcards);
            delegatemygivencards = new updategivencards(UpdateMyGivenCards);
            delegateleftgivencards = new updategivencards(UpdateLefGivenCards);
            delegaterightgivencards = new updategivencards(UpdateRightGivenCards);
            delegateupdateavator = new updateAvator(UpdateAvator);

            delegateUpdateControlLocations = new updateControlsLocation(updateControlLocation);

            this.Closing += MainWindow_Closing;
            this.MouseRightButtonDown += MainWindow_MouseRightButtonDown;
            this.cb_bg_music.Unchecked += Cb_bg_music_CheckedChange;
            this.cb_bg_music.Checked += Cb_bg_music_CheckedChange;

            #region 音效设置。

            SoundbtClick = new MediaElement();
            SoundbtClick.LoadedBehavior = MediaState.Manual;
            SoundbtClick.UnloadedBehavior = MediaState.Manual;
            Doudizhu.Children.Add(SoundbtClick);
            SoundbtClick.Source = (new Uri(@"Resources/sound/bt_click.wav", UriKind.Relative));

            SoundBGM = new MediaElement();
            SoundBGM.LoadedBehavior = MediaState.Manual;
            SoundBGM.UnloadedBehavior = MediaState.Manual;
            Doudizhu.Children.Add(SoundBGM);
            SoundBGM.Source = new Uri(@"Resources/sound/bg_normal.wav", UriKind.Relative);
            SoundBGM.MediaEnded += SoundBGM_MediaEnded;

            SoundStart = new MediaElement();
            SoundStart.LoadedBehavior = MediaState.Manual;
            SoundStart.UnloadedBehavior = MediaState.Manual;
            Doudizhu.Children.Add(SoundStart);
            SoundStart.Source = (new Uri(@"Resources/sound/start.wav", UriKind.Relative));

            SoundClick = new MediaElement();
            SoundClick.LoadedBehavior = MediaState.Manual;
            SoundClick.UnloadedBehavior = MediaState.Manual;
            Doudizhu.Children.Add(SoundClick);
            SoundClick.Source = (new Uri(@"Resources/sound/click.wav", UriKind.Relative));

            SoundGive = new MediaElement();
            SoundGive.LoadedBehavior = MediaState.Manual;
            SoundGive.UnloadedBehavior = MediaState.Manual;
            Doudizhu.Children.Add(SoundGive);
            SoundGive.Source = (new Uri(@"Resources/sound/give.wav", UriKind.Relative));

            SoundLoss = new MediaElement();
            SoundLoss.LoadedBehavior = MediaState.Manual;
            SoundLoss.UnloadedBehavior = MediaState.Manual;
            Doudizhu.Children.Add(SoundLoss);
            SoundLoss.Source = (new Uri(@"Resources/sound/loss.wav", UriKind.Relative));

            SoundWin = new MediaElement();
            SoundWin.LoadedBehavior = MediaState.Manual;
            SoundWin.UnloadedBehavior = MediaState.Manual;
            Doudizhu.Children.Add(SoundWin);
            SoundWin.Source = (new Uri(@"Resources/sound/win.wav", UriKind.Relative));

            Soundwbujiao = new MediaElement();
            Soundwbujiao.LoadedBehavior = MediaState.Manual;
            Soundwbujiao.UnloadedBehavior = MediaState.Manual;
            Doudizhu.Children.Add(Soundwbujiao);
            Soundwbujiao.Source = (new Uri(@"Resources/sound/w_bujiao.wav", UriKind.Relative));

            Soundwjiaodizhu = new MediaElement();
            Soundwjiaodizhu.LoadedBehavior = MediaState.Manual;
            Soundwjiaodizhu.UnloadedBehavior = MediaState.Manual;
            Doudizhu.Children.Add(Soundwjiaodizhu);
            Soundwjiaodizhu.Source = (new Uri(@"Resources/sound/w_jiaodizhu.wav", UriKind.Relative));

            Soundwpass = new MediaElement();
            Soundwpass.LoadedBehavior = MediaState.Manual;
            Soundwpass.UnloadedBehavior = MediaState.Manual;
            Doudizhu.Children.Add(Soundwpass);
            Soundwpass.Source = (new Uri(@"Resources/sound/w_pass.wav", UriKind.Relative));

            Soundwqiangdizhu = new MediaElement();
            Soundwqiangdizhu.LoadedBehavior = MediaState.Manual;
            Soundwqiangdizhu.UnloadedBehavior = MediaState.Manual;
            Doudizhu.Children.Add(Soundwqiangdizhu);
            Soundwqiangdizhu.Source = (new Uri(@"Resources/sound/w_qiangdizhu.wav", UriKind.Relative));

            Soundwbuqiang = new MediaElement();
            Soundwbuqiang.LoadedBehavior = MediaState.Manual;
            Soundwbuqiang.UnloadedBehavior = MediaState.Manual;
            Doudizhu.Children.Add(Soundwbuqiang);
            Soundwbuqiang.Source = (new Uri(@"Resources/sound/w_buqiang.wav", UriKind.Relative));

            Soundmbuqiang = new MediaElement();
            Soundmbuqiang.LoadedBehavior = MediaState.Manual;
            Soundmbuqiang.UnloadedBehavior = MediaState.Manual;
            Doudizhu.Children.Add(Soundmbuqiang);
            Soundmbuqiang.Source = (new Uri(@"Resources/sound/m_buqiang.wav", UriKind.Relative));

            Soundmbujiao = new MediaElement();
            Soundmbujiao.LoadedBehavior = MediaState.Manual;
            Soundmbujiao.UnloadedBehavior = MediaState.Manual;
            Doudizhu.Children.Add(Soundmbujiao);
            Soundmbujiao.Source = (new Uri(@"Resources/sound/m_bujiao.wav", UriKind.Relative));

            Soundmjiaodizhu = new MediaElement();
            Soundmjiaodizhu.LoadedBehavior = MediaState.Manual;
            Soundmjiaodizhu.UnloadedBehavior = MediaState.Manual;
            Doudizhu.Children.Add(Soundmjiaodizhu);
            Soundmjiaodizhu.Source = (new Uri(@"Resources/sound/m_jiaodizhu.wav", UriKind.Relative));

            Soundmpass = new MediaElement();
            Soundmpass.LoadedBehavior = MediaState.Manual;
            Soundmpass.UnloadedBehavior = MediaState.Manual;
            Doudizhu.Children.Add(Soundmpass);
            Soundmpass.Source = (new Uri(@"Resources/sound/m_pass.wav", UriKind.Relative));

            Soundmqiangdizhu = new MediaElement();
            Soundmqiangdizhu.LoadedBehavior = MediaState.Manual;
            Soundmqiangdizhu.UnloadedBehavior = MediaState.Manual;
            Doudizhu.Children.Add(Soundmqiangdizhu);
            Soundmqiangdizhu.Source = (new Uri(@"Resources/sound/m_qiangdizhu.wav", UriKind.Relative));

            Soundmqiangdizhu2 = new MediaElement();
            Soundmqiangdizhu2.LoadedBehavior = MediaState.Manual;
            Soundmqiangdizhu2.UnloadedBehavior = MediaState.Manual;
            Doudizhu.Children.Add(Soundmqiangdizhu2);
            Soundmqiangdizhu2.Source = (new Uri(@"Resources/sound/m_qiangdizhu2.wav", UriKind.Relative));

            Soundmqiangdizhu3 = new MediaElement();
            Soundmqiangdizhu3.LoadedBehavior = MediaState.Manual;
            Soundmqiangdizhu3.UnloadedBehavior = MediaState.Manual;
            Doudizhu.Children.Add(Soundmqiangdizhu3);
            Soundmqiangdizhu3.Source = (new Uri(@"Resources/sound/m_qiangdizhu3.wav", UriKind.Relative));

            Soundwqiangdizhu3 = new MediaElement();
            Soundwqiangdizhu3.LoadedBehavior = MediaState.Manual;
            Soundwqiangdizhu3.UnloadedBehavior = MediaState.Manual;
            Doudizhu.Children.Add(Soundwqiangdizhu3);
            Soundwqiangdizhu3.Source = (new Uri(@"Resources/sound/w_qiangdizhu3.wav", UriKind.Relative));

            Soundwqiangdizhu2 = new MediaElement();
            Soundwqiangdizhu2.LoadedBehavior = MediaState.Manual;
            Soundwqiangdizhu2.UnloadedBehavior = MediaState.Manual;
            Doudizhu.Children.Add(Soundwqiangdizhu2);
            Soundwqiangdizhu2.Source = (new Uri(@"Resources/sound/w_qiangdizhu2.wav", UriKind.Relative));

            #endregion

            this.username = username;
            this.roomNumber = tablenumber;
            this.baseScore = baseScore;
            this.position = position;

            //initialize the proxy.
            proxy = new DoudizhuService.DoudizhuServiceClient();
            proxy.CreateRoom(this.roomNumber, this.baseScore);

            proxyJoinLeaveRoom = new DoudizhuService.PlayerJoinLeaveRoomClient(new InstanceContext(this));
            proxyJoinLeaveRoom.playerJoinLeaveRoomSubscribe(roomNumber);

            proxySetCancelReady = new DoudizhuService.SetCancelPlayerReadyClient(new InstanceContext(this));
            proxySetCancelReady.playerSetCancelReadySubscribe(roomNumber);

            proxyStart = new DoudizhuService.StartGameClient(new InstanceContext(this));
            proxyStart.startGameSubscribe(roomNumber);

            proxyDeteLandlord = new DoudizhuService.DeterminteLandlordClient(new InstanceContext(this));
            proxyDeteLandlord.determinteLandlordSubscribe(roomNumber);

            proxyUpdateLandlordCard = new DoudizhuService.UpdateLandlordCardClient(new InstanceContext(this));
            proxyUpdateLandlordCard.updateLandlordCardSubscribe(roomNumber);

            proxyUpdateGame = new DoudizhuService.UpdateGameClient(new InstanceContext(this));
            proxyUpdateGame.updateGameSubscribe(roomNumber);

            proxyTimeCounter = new DoudizhuService.TimeCounterDownClient(new InstanceContext(this));
            proxyTimeCounter.timeCounterDownSubscribe(roomNumber);

            proxyGameOver = new DoudizhuService.GameOverClient(new InstanceContext(this));
            proxyGameOver.gameoverSubscribe(roomNumber);

            proxyUpdateChatter = new DoudizhuService.UpdateChatterClient(new InstanceContext(this));
            proxyUpdateChatter.updateChatterSubscribe(roomNumber);

            initialize();
        }

        private void SoundBGM_MediaEnded(object sender, RoutedEventArgs e)
        {
            SoundBGM.Position = TimeSpan.Zero;
            SoundBGM.Play();
        }

        #region private functions

        #region delegate functions.
        //for resizing.
        private void updateControlLocation()
        {
            //the form is temperoary non-resizeable.
            //pic_1.Margin = new Thickness(Doudizhu.Width / 2 - cardWidth / 2 * 3 - cardWidth / 10, Doudizhu.Height / 20, 0, 0);
            //pic_2.Margin = new Thickness(Doudizhu.Width / 2 - cardWidth / 2, Doudizhu.Height / 20, 0, 0);
            //pic_3.Margin = new Thickness(Doudizhu.Width / 2 + cardWidth / 2 + cardWidth / 10, Doudizhu.Height / 20, 0, 0);
            //mycards.Margin = new Thickness(Doudizhu.Width / 5, Doudizhu.Height * 2 / 3, 0, 0);
            //leftlastcards.Margin = new Thickness(-Doudizhu.Width * 3 / 8, -Doudizhu.Height / 5, 0, 0);
            //rightlastcards.Margin = new Thickness(Doudizhu.Width * 3 / 8, -Doudizhu.Height / 5, 0, 0);
            //mylastcards.Margin = new Thickness(0, Doudizhu.Height / 5, 0, 0);
        }

        private void showHideGPH(bool b)
        {
            if (b)
            {
                bt_give.Visibility = Visibility.Visible;
                bt_pass.Visibility = Visibility.Visible;
                bt_hint.Visibility = Visibility.Visible;
            }
            else
            {
                bt_give.Visibility = Visibility.Hidden;
                bt_pass.Visibility = Visibility.Hidden;
                bt_hint.Visibility = Visibility.Hidden;
            }
        }

        private void showGive(bool b)
        {
            if (b)
            {
                bt_give.Visibility = Visibility.Visible;
            }
            else
            {
                bt_give.Visibility = Visibility.Hidden;
            }
        }

        private void showDL(bool b)
        {
            if (b)
            {
                bt_jiaodizhu.Visibility = Visibility.Visible;
                bt_bujiao.Visibility = Visibility.Visible;
            }
            else
            {
                bt_jiaodizhu.Visibility = Visibility.Hidden;
                bt_bujiao.Visibility = Visibility.Hidden;
            }
        }

        private void showReady(string msg, bool b)
        {
            bt_ready.Content = msg;
            if (b)
            {
                bt_ready.Visibility = Visibility.Visible;
            }
            else
            {
                bt_ready.Visibility = Visibility.Hidden;
            }
        }

        private void showleftready(bool b)
        {
            if (b)
            {
                im_left_ready.Visibility = Visibility.Visible;
            }
            else
            {
                im_left_ready.Visibility = Visibility.Hidden;
            }
        }

        private void showmyready(bool b)
        {
            if (b)
            {
                im_my_ready.Visibility = Visibility.Visible;
            }
            else
            {
                im_my_ready.Visibility = Visibility.Hidden;
            }
        }

        private void showmymsg(string msg, bool b)
        {
            lb_my_msg.Content = msg;
            if (b)
            {
                lb_my_msg.Visibility = Visibility.Visible;
            }
            else
            {
                lb_my_msg.Visibility = Visibility.Hidden;
            }
        }

        private void showleftmsg(string msg, bool b)
        {
            lb_left_msg.Content = msg;
            if (b)
            {
                lb_left_msg.Visibility = Visibility.Visible;
            }
            else
            {
                lb_left_msg.Visibility = Visibility.Hidden;
            }
        }

        private void showrightmsg(string msg, bool b)
        {
            lb_right_msg.Content = msg;
            if (b)
            {
                lb_right_msg.Visibility = Visibility.Visible;
            }
            else
            {
                lb_right_msg.Visibility = Visibility.Hidden;
            }
        }

        private void showrightready(bool b)
        {
            if (b)
            {
                im_right_ready.Visibility = Visibility.Visible;
            }
            else
            {
                im_right_ready.Visibility = Visibility.Hidden;
            }
        }

        private void showlefttimer(string msg, bool b)
        {
            lb_left_timer.Content = msg;
            if (b)
            {
                lb_left_timer.Visibility = Visibility.Visible;
            }
            else
            {
                lb_left_timer.Visibility = Visibility.Hidden;
            }
        }

        private void showmytimer(string msg, bool b)
        {
            lb_my_timer.Content = msg;
            if (b)
            {
                lb_my_timer.Visibility = Visibility.Visible;
            }
            else
            {
                lb_my_timer.Visibility = Visibility.Hidden;
            }
        }

        private void showrighttimer(string msg, bool b)
        {
            lb_right_timer.Content = msg;
            if (b)
            {
                lb_right_timer.Visibility = Visibility.Visible;
            }
            else
            {
                lb_right_timer.Visibility = Visibility.Hidden;
            }
        }

        private void showresult(string msg, bool b)
        {
            lb_result.Content = msg;
            if (b)
                lb_result.Visibility = Visibility.Visible;
            else
                lb_result.Visibility = Visibility.Hidden;
        }

        private void showcardtype(string msg, bool b)
        {
            lb_leftoverType.Content = msg;
            if (b)
                lb_leftoverType.Visibility = Visibility.Visible;
            else
                lb_leftoverType.Visibility = Visibility.Hidden;

        }

        private void showleftusername(string msg, bool b)
        {
            lb_left_username.Content = msg;
            if (b)
                lb_left_username.Visibility = Visibility.Visible;
            else
                lb_left_username.Visibility = Visibility.Hidden;
        }

        private void showmyusername(string msg, bool b)
        {
            lb_my_username.Content = msg;
            if (b)
                lb_my_username.Visibility = Visibility.Visible;
            else
                lb_my_username.Visibility = Visibility.Hidden;
        }

        private void showrightusername(string msg, bool b)
        {
            lb_right_username.Content = msg;
            if (b)
                lb_right_username.Visibility = Visibility.Visible;
            else
                lb_right_username.Visibility = Visibility.Hidden;
        }

        private void showleftleftcards(string msg, bool b)
        {
            lb_left_leftcards.Content = msg;
            if (b)
                lb_left_leftcards.Visibility = Visibility.Visible;
            else
                lb_left_leftcards.Visibility = Visibility.Hidden;
        }

        private void showrightleftcards(string msg, bool b)
        {
            lb_right_leftcards.Content = msg;
            if (b)
                lb_right_leftcards.Visibility = Visibility.Visible;
            else
                lb_right_leftcards.Visibility = Visibility.Hidden;
        }

        private void UpdateMyGivenCards(Card[] cards)
        {
            int smaller = 2;
            int distance = 8;
            mylastcards.Children.Clear();
            mygivencards.Clear();
            for (int i = 0; i < cards.Length; i++)
            {
                mygivencards.Add(new Image());
                mygivencards[i].Stretch = Stretch.Fill;
                mygivencards[i].Source = new BitmapImage(new Uri(@"Resources/cards/" + cards[i].Image + ".png", UriKind.Relative));
                mygivencards[i].Width = cardWidth / smaller;
                mygivencards[i].Height = cardheight / smaller;
                //这里的数字以后需要修改。
                mygivencards[i].Margin = new Thickness(mylastcards.Width / 2 - cardWidth / 2 + cardWidth / distance * i - cardWidth / (distance * 2) * cards.Length, 0, 0, 0);
                mylastcards.Children.Add(mygivencards[i]);
            }
            mylastcards.InvalidateVisual();
            mylastcards.UpdateLayout();
            mylastcards.Visibility = Visibility.Visible;
        }

        private void UpdateLefGivenCards(Card[] cards)
        {
            int smaller = 2;
            int distance = 8;
            leftlastcards.Children.Clear();
            leftgivencards.Clear();
            if (cards.Length <= 10)
            {
                for (int i = 0; i < cards.Length; i++)
                {
                    leftgivencards.Add(new Image());
                    leftgivencards[i].Stretch = Stretch.Fill;
                    leftgivencards[i].Source = new BitmapImage(new Uri(@"Resources/cards/" + cards[i].Image + ".png", UriKind.Relative));
                    leftgivencards[i].Width = cardWidth / smaller;
                    leftgivencards[i].Height = cardheight / smaller;
                    //这里的数字以后需要修改。
                    leftgivencards[i].Margin = new Thickness(cardWidth / distance * i, 0, 0, 0);
                    leftlastcards.Children.Add(leftgivencards[i]);
                }
            }
            else
            {
                for (int i = 0; i < 9; i++)
                {
                    leftgivencards.Add(new Image());
                    leftgivencards[i].Stretch = Stretch.Fill;
                    leftgivencards[i].Source = new BitmapImage(new Uri(@"Resources/cards/" + cards[i].Image + ".png", UriKind.Relative));
                    leftgivencards[i].Width = cardWidth / smaller;
                    leftgivencards[i].Height = cardheight / smaller;
                    //这里的数字以后需要修改。
                    leftgivencards[i].Margin = new Thickness(cardWidth / distance * i, 0, 0, 0);
                    leftlastcards.Children.Add(leftgivencards[i]);
                }
                for (int i = 9; i < cards.Length; i++)
                {
                    leftgivencards.Add(new Image());
                    leftgivencards[i].Stretch = Stretch.Fill;
                    leftgivencards[i].Source = new BitmapImage(new Uri(@"Resources/cards/" + cards[i].Image + ".png", UriKind.Relative));
                    leftgivencards[i].Width = cardWidth / smaller;
                    leftgivencards[i].Height = cardheight / smaller;
                    //这里的数字以后需要修改。
                    leftgivencards[i].Margin = new Thickness(cardWidth / distance * (i - 9), cardheight / 4, 0, 0);
                    leftlastcards.Children.Add(leftgivencards[i]);
                }
            }
            leftlastcards.InvalidateVisual();
            leftlastcards.UpdateLayout();
            leftlastcards.Visibility = Visibility.Visible;
        }

        private void UpdateRightGivenCards(Card[] cards)
        {
            int smaller = 2;
            int distance = 8;
            rightlastcards.Children.Clear();
            rightgivencards.Clear();
            if (cards.Length <= 10)
            {
                for (int i = 0; i < cards.Length; i++)
                {
                    rightgivencards.Add(new Image());
                    rightgivencards[i].Stretch = Stretch.Fill;
                    rightgivencards[i].Source = new BitmapImage(new Uri(@"Resources/cards/" + cards[i].Image + ".png", UriKind.Relative));
                    rightgivencards[i].Width = cardWidth / smaller;
                    rightgivencards[i].Height = cardheight / smaller;
                    //这里的数字以后需要修改。
                    rightgivencards[i].Margin = new Thickness(rightlastcards.Width - cardWidth / 2 - cards.Length * cardWidth / distance + cardWidth / distance * i, 0, 0, 0);
                    rightlastcards.Children.Add(rightgivencards[i]);
                }
            }
            else
            {
                for (int i = 0; i < 9; i++)
                {
                    rightgivencards.Add(new Image());
                    rightgivencards[i].Stretch = Stretch.Fill;
                    rightgivencards[i].Source = new BitmapImage(new Uri(@"Resources/cards/" + cards[i].Image + ".png", UriKind.Relative));
                    rightgivencards[i].Width = cardWidth / smaller;
                    rightgivencards[i].Height = cardheight / smaller;
                    //这里的数字以后需要修改。
                    rightgivencards[i].Margin = new Thickness(rightlastcards.Width + cardWidth / 2 - cards.Length * cardWidth / distance + cardWidth / distance * i, 0, 0, 0);
                    rightlastcards.Children.Add(rightgivencards[i]);
                }
                for (int i = 9; i < cards.Length; i++)
                {
                    rightgivencards.Add(new Image());
                    rightgivencards[i].Stretch = Stretch.Fill;
                    rightgivencards[i].Source = new BitmapImage(new Uri(@"Resources/cards/" + cards[i].Image + ".png", UriKind.Relative));
                    rightgivencards[i].Width = cardWidth / smaller;
                    rightgivencards[i].Height = cardheight / smaller;
                    //这里的数字以后需要修改。
                    rightgivencards[i].Margin = new Thickness(rightlastcards.Width + cardWidth / 2 - cards.Length * cardWidth / distance + cardWidth / distance * (i - 9), cardheight / 4, 0, 0);
                    rightlastcards.Children.Add(rightgivencards[i]);
                }
            }
            rightlastcards.InvalidateVisual();
            rightlastcards.UpdateLayout();
            rightlastcards.Visibility = Visibility.Visible;
        }

        private void UpdateAvator(bool jl, bool ml, bool ll, bool mm, bool lm, bool jr, bool mr, bool lr)
        {
            //set the left player.
            if (jl)
            {
                if (ml)
                {
                    if (ll)
                    {
                        leftAvator.Source = new BitmapImage(new Uri(@"Resources/landlord_m_r.png", UriKind.Relative));
                    }
                    else
                    {
                        leftAvator.Source = new BitmapImage(new Uri(@"Resources/farmer_m_r.png", UriKind.Relative));
                    }
                }
                else
                {
                    if (ll)
                    {
                        leftAvator.Source = new BitmapImage(new Uri(@"Resources/landlord_w_r.png", UriKind.Relative));
                    }
                    else
                    {
                        leftAvator.Source = new BitmapImage(new Uri(@"Resources/farmer_w_r.png", UriKind.Relative));
                    }
                }
            }
            else
            {
                leftAvator.Source = null;
            }
            //set the right player.
            if (jr)
            {
                if (mr)
                {
                    if (lr)
                    {
                        rightAvator.Source = new BitmapImage(new Uri(@"Resources/landlord_m_l.png", UriKind.Relative));
                    }
                    else
                    {
                        rightAvator.Source = new BitmapImage(new Uri(@"Resources/farmer_m_l.png", UriKind.Relative));
                    }
                }
                else
                {
                    if (lr)
                    {
                        rightAvator.Source = new BitmapImage(new Uri(@"Resources/landlord_w_l.png", UriKind.Relative));
                    }
                    else
                    {
                        rightAvator.Source = new BitmapImage(new Uri(@"Resources/farmer_w_l.png", UriKind.Relative));
                    }
                }
            }
            else
            {
                rightAvator.Source = null;
            }
            //set myself.
            if (mm)
            {
                if (lm)
                {
                    myAvator.Source = new BitmapImage(new Uri(@"Resources/landlord_m_r.png", UriKind.Relative));
                }
                else
                {
                    myAvator.Source = new BitmapImage(new Uri(@"Resources/farmer_m_r.png", UriKind.Relative));
                }
            }
            else
            {
                if (lm)
                {
                    myAvator.Source = new BitmapImage(new Uri(@"Resources/landlord_w_r.png", UriKind.Relative));
                }
                else
                {
                    myAvator.Source = new BitmapImage(new Uri(@"Resources/farmer_w_r.png", UriKind.Relative));
                }
            }
        }

        #endregion

        private void initialize()
        {
            leftgivencards = new List<Image>();
            rightgivencards = new List<Image>();
            mygivencards = new List<Image>();
            cardImages = new List<Image>();

            //重置reset
            this.Dispatcher.Invoke(() =>
            {
                pic_1.Source = null;
                pic_2.Source = null;
                pic_3.Source = null;

                delegateGPH(false);
                delegateDL(false);

                delegateleftleftcards("0", false);
                delegaterightleftcards("0", false);

                if (proxy != null)
                {
                    lb_basescore_value.Content = proxy.GetBaseScore(roomNumber).ToString();
                    lb_bet_value.Content = proxy.GetBetNumber(roomNumber).ToString();
                }

                delegatelefttimer("", false);
                delegatemytimer("", false);
                delegaterighttimer("", false);

                delegateleftovertype("", false);
                delegateresult("", false);

                delegateUpdateControlLocations();
            });
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (SoundBGM != null)
                SoundBGM.Stop();

            proxyJoinLeaveRoom.playerJoinLeaveRoomUnSubscribe(roomNumber);
            proxySetCancelReady.playerSetCancelReadyUnSubscribe(roomNumber);
            proxyStart.startGameUnSubscribe(roomNumber);
            proxyDeteLandlord.determinteLandlordUnSubscribe(roomNumber);
            proxyUpdateLandlordCard.updateLandlordCardUnSubscribe(roomNumber);
            proxyUpdateGame.updateGameUnSubscribe(roomNumber);
            proxyTimeCounter.timeCounterDownUnSubscribe(roomNumber);
            proxyGameOver.gameoverUnSubscribe(roomNumber);
            proxyUpdateChatter.updateChatterUnSubscribe(roomNumber);
            //先unsubscribe所有东西之后，再退出。
            if (proxy.InGame(roomNumber))
                proxy.PlayerQuit(username, roomNumber);
            proxy.PlayerLeaveRoom(username, roomNumber);
        }

        #endregion

        #region callbacks.

        public void PlayerJoinLeaveRoom_Client(Player p1, Player p2, Player p3, int roomNr)
        {
            this.Dispatcher.Invoke(() =>
            {
                delegatemyusername("", false);
                delegateleftusername("", false);
                delegaterightusername("", false);

                delegatemymsg("", false);
                delegatemyReadyImage(false);
                delegateleftReadyImage(false);
                delegaterightReadyImage(false);

                if (p1 != null)
                {
                    if (p1.Account.Username == username)
                        delegateupdateavator(p3 == null ? false : true, p3 == null ? false : p3.Account.IsMale, p3 == null ? false : p3.Landlord, p1.Account.IsMale, p1.Landlord, p2 == null ? false : true, p2 == null ? false : p2.Account.IsMale, p2 == null ? false : p2.Landlord);
                }
                if (p2 != null)
                {
                    if (p2.Account.Username == username)
                        delegateupdateavator(p1 == null ? false : true, p1 == null ? false : p1.Account.IsMale, p1 == null ? false : p1.Landlord, p2.Account.IsMale, p2.Landlord, p3 == null ? false : true, p3 == null ? false : p3.Account.IsMale, p3 == null ? false : p3.Landlord);
                }
                if (p3 != null)
                {
                    if (p3.Account.Username == username)
                        delegateupdateavator(p2 == null ? false : true, p2 == null ? false : p2.Account.IsMale, p2 == null ? false : p2.Landlord, p3.Account.IsMale, p3.Landlord, p1 == null ? false : true, p1 == null ? false : p1.Account.IsMale, p1 == null ? false : p1.Landlord);
                }
            });

            //如果我是a1玩家，那么a2在我的右边，a3在我的左边，因为是逆时针出牌，而在server端，设置的是，从player1-2-3这样的出牌顺序。
            //下面两种情况和这个类似。
            this.Dispatcher.Invoke(() =>
            {
                if (p1 != null)
                {
                    if (p1.Account.Username == username)
                    {
                        delegatemyusername(username, true);
                        if (p2 != null)
                            delegaterightusername(p2.Account.Username, true);
                        if (p3 != null)
                            delegateleftusername(p3.Account.Username, true);
                    }
                }
                if (p2 != null)
                {
                    if (p2.Account.Username == username)
                    {
                        delegatemyusername(username, true);
                        if (p3 != null)
                            delegaterightusername(p3.Account.Username, true);
                        if (p1 != null)
                            delegateleftusername(p1.Account.Username, true);
                    }
                }
                if (p3 != null)
                {
                    if (p3.Account.Username == username)
                    {
                        delegatemyusername(username, true);
                        if (p1 != null)
                            delegaterightusername(p1.Account.Username, true);
                        if (p2 != null)
                            delegateleftusername(p2.Account.Username, true);
                    }
                }
            });

            this.SetCancelPlayerReady_Client(p1, p2, p3, roomNr);
        }

        public void SetCancelPlayerReady_Client(Player p1, Player p2, Player p3, int roomNr)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (p1 != null)
                {
                    if (p1.Account.Username == username)
                    {
                        if (p1.Ready)
                            delegatemyReadyImage(true);
                        else
                            delegatemyReadyImage(false);
                        if (p2 != null)
                            if (p2.Ready)
                                delegaterightReadyImage(true);
                            else
                                delegaterightReadyImage(false);
                        if (p3 != null)
                            if (p3.Ready)
                                delegateleftReadyImage(true);
                            else
                                delegateleftReadyImage(false);
                    }
                }
                if (p2 != null)
                {
                    if (p2.Account.Username == username)
                    {
                        if (p2.Ready)
                            delegatemyReadyImage(true);
                        else
                            delegatemyReadyImage(false);
                        if (p3 != null)
                            if (p3.Ready)
                                delegaterightReadyImage(true);
                            else
                                delegaterightReadyImage(false);
                        if (p1 != null)
                            if (p1.Ready)
                                delegateleftReadyImage(true);
                            else
                                delegateleftReadyImage(false);
                    }
                }
                if (p3 != null)
                {
                    if (p3.Account.Username == username)
                    {
                        if (p3.Ready)
                            delegatemyReadyImage(true);
                        else
                            delegatemyReadyImage(false);
                        if (p1 != null)
                            if (p1.Ready)
                                delegaterightReadyImage(true);
                            else
                                delegaterightReadyImage(false);
                        if (p2 != null)
                            if (p2.Ready)
                                delegateleftReadyImage(true);
                            else
                                delegateleftReadyImage(false);
                    }
                }
            });

            //update the lobby.
            this.proxy.UpdateLobby(roomNr);
        }

        public void DeterminteLandlord_Client(Player[] tempdizhu, bool[] dizhuorNot, Player current, bool determined)
        {
            var thread = new Thread(() =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    lb_bet_value.Content = proxy.GetBetNumber(roomNumber).ToString();

                    delegateDL(false);

                    if (current != null)
                    {
                        if (current.Account.Username == username)
                        {
                            delegateDL(true);
                        }
                    }

                    int count = 0;
                    foreach (bool b in dizhuorNot)
                    {
                        if (b)
                            count++;
                    }

                    #region 音效。

                    if (tempdizhu.Length >= 1)
                    {
                        if (dizhuorNot[dizhuorNot.Length - 1])
                        {
                            if (count == 1)
                            {
                                if (cb_soundeffect.IsChecked == true)
                                {
                                    if (tempdizhu[tempdizhu.Length - 1].Account.IsMale)
                                    {
                                        Soundmjiaodizhu.Position = TimeSpan.Zero;
                                        Soundmjiaodizhu.Play();
                                    }
                                    else
                                    {
                                        Soundwjiaodizhu.Position = TimeSpan.Zero;
                                        Soundwjiaodizhu.Play();
                                    }
                                }
                            }
                            if (count == 2)
                            {
                                if (cb_soundeffect.IsChecked == true)
                                {

                                    if (tempdizhu[tempdizhu.Length - 1].Account.IsMale)
                                    {
                                        Soundmqiangdizhu.Position = TimeSpan.Zero;
                                        Soundmqiangdizhu.Play();
                                    }
                                    else
                                    {
                                        Soundwqiangdizhu.Position = TimeSpan.Zero;
                                        Soundwqiangdizhu.Play();
                                    }
                                }
                            }
                            if (count == 3)
                            {
                                if (cb_soundeffect.IsChecked == true)
                                {

                                    if (tempdizhu[tempdizhu.Length - 1].Account.IsMale)
                                    {
                                        Soundmqiangdizhu2.Position = TimeSpan.Zero;
                                        Soundmqiangdizhu2.Play();
                                    }
                                    else
                                    {
                                        Soundwqiangdizhu2.Position = TimeSpan.Zero;
                                        Soundwqiangdizhu2.Play();
                                    }
                                }
                            }
                            if (count == 4)
                            {
                                if (cb_soundeffect.IsChecked == true)
                                {

                                    if (tempdizhu[tempdizhu.Length - 1].Account.IsMale)
                                    {
                                        Soundmqiangdizhu3.Position = TimeSpan.Zero;
                                        Soundmqiangdizhu3.Play();
                                    }
                                    else
                                    {
                                        Soundwqiangdizhu3.Position = TimeSpan.Zero;
                                        Soundwqiangdizhu3.Play();
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (count == 0)
                            {
                                if (tempdizhu[tempdizhu.Length - 1].Account.IsMale)
                                {
                                    Soundmbujiao.Position = TimeSpan.Zero;
                                    Soundmbujiao.Play();
                                }
                                else
                                {
                                    Soundwbujiao.Position = TimeSpan.Zero;
                                    Soundwbujiao.Play();
                                }
                            }
                            else
                            {
                                if (tempdizhu[tempdizhu.Length - 1].Account.IsMale)
                                {
                                    Soundmbuqiang.Position = TimeSpan.Zero;
                                    Soundmbuqiang.Play();
                                }
                                else
                                {
                                    Soundwbuqiang.Position = TimeSpan.Zero;
                                    Soundwbuqiang.Play();
                                }
                            }
                        }
                    }
                    #endregion

                    for (int i = 0; i < tempdizhu.Length; i++)
                    {
                        if (dizhuorNot[i])
                        {

                            if (tempdizhu[i].Account.Username == username)
                            {
                                if (count == 1)
                                {
                                    delegatemymsg("jiao di zhu!", true);
                                }
                                else
                                {
                                    delegatemymsg("qiang di zhu!", true);
                                }
                            }
                            if (tempdizhu[i].Account.Username == lb_left_username.Content.ToString())
                            {
                                if (count == 1)
                                {
                                    delegateleftmsg("jiao di zhu!", true);
                                }
                                else
                                {
                                    delegateleftmsg("qiang di zhu!", true);
                                }
                            }
                            if (tempdizhu[i].Account.Username == lb_right_username.Content.ToString())
                            {
                                if (count == 1)
                                {
                                    delegaterightmsg("jiao di zhu!", true);
                                }
                                else
                                {
                                    delegaterightmsg("qiang di zhu!", true);
                                }
                            }
                        }
                        else
                        {
                            if (tempdizhu[i].Account.Username == username)
                            {
                                if (count == 0)
                                {
                                    delegatemymsg("bu jiao!", true);
                                }
                                else
                                {
                                    delegatemymsg("bu qiang!", true);
                                }
                            }
                            if (tempdizhu[i].Account.Username == lb_left_username.Content.ToString())
                            {
                                if (count == 0)
                                {
                                    delegateleftmsg("bu jiao!", true);
                                }
                                else
                                {
                                    delegateleftmsg("bu qiang!", true);
                                }
                            }
                            if (tempdizhu[i].Account.Username == lb_right_username.Content.ToString())
                            {
                                if (count == 0)
                                {
                                    delegaterightmsg("bu jiao!", true);
                                }
                                else
                                {
                                    delegaterightmsg("bu qiang!", true);
                                }
                            }
                        }
                    }

                    if (determined)
                    {
                        //取消抢地主的提示。
                        delegatemymsg("", false);
                        delegateleftmsg("", false);
                        delegaterightmsg("", false);
                    }
                }));
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        public void UpdateGame_Client(Player current, int bet)
        {
            Player previous = proxy.GetPlayerByUsername(current.PreviousPlayer, roomNumber);

            //这边可以播放各种牌的种类，比如，对A，对2，王炸，等等。
            this.Dispatcher.Invoke(() =>
            {
                if (cb_soundeffect.IsChecked == true)
                {
                    if (previous != null && current != null)
                    {
                        if (current.LastCards != null)
                        {
                            if (previous.Account.IsMale)
                            {
                                if (current.LastCards.Length == 0)
                                {
                                    Soundmpass.Position = TimeSpan.Zero;
                                    Soundmpass.Play();
                                }
                            }
                            else
                            {
                                if (current.LastCards.Length == 0)
                                {
                                    Soundwpass.Position = TimeSpan.Zero;
                                    Soundwpass.Play();
                                }
                            }
                        }
                    }
                }
            });

            //in order to prevent cross thread, the last given cards are shown in "game_over_client".
            //为了防止线程干扰，这边的最后一手牌，在game_over_client里面显示。
            if (previous.LeftCards.Length == 0)
                return;

            this.Dispatcher.Invoke(() =>
            {
                delegateGPH(false);

                delegatemymsg("", false);
                delegateresult("", false);
                //若是出了炸弹什么的，要加倍。
                lb_bet_value.Content = bet.ToString();

                //更新剩余牌的数量。仅仅需要更新左右玩家的即可。
                if (current.PreviousPlayer != null)
                {
                    if (current.Account.Username == username)
                    {
                        delegateleftleftcards(previous.LeftCards.Length.ToString(), true);
                    }
                    if (current.Account.Username == lb_left_username.Content.ToString())
                    {
                        delegaterightleftcards(previous.LeftCards.Length.ToString(), true);
                    }
                }

                //如果是这个账户，那么显示出牌按钮。
                if (current.Account.Username == username)
                {
                    //判断是否有人出完牌了，来判断是否需要显示出牌按钮。
                    bool gameIsOver = false;

                    if (previous.LeftCards.Length == 0)
                        gameIsOver = true;
                    //如果地主一手出完所有牌（几乎不可能的情况）。
                    if (current.LastCards != null)
                        if (current.LastCards.Length == 20)
                            gameIsOver = true;

                    //游戏已经结束，也就是说上一个玩家出牌出完了。那么就不显示出牌按钮了。
                    //如果还没结束，那么就显示出牌按钮。
                    if (!gameIsOver)
                    {
                        if (current.IsFreeGive)
                        {
                            delegateGive(true);
                        }
                        else
                        {
                            delegateGPH(true);
                        }
                    }
                }

                //如果这是第一手牌，也就是地主出第一手牌。那么久不会显示任何东西了。如果这不是地主的第一手牌，那么就会显示以下已出的牌或者是pass的提示等等。
                if (current.LeftCards.Length != 20)
                {
                    //如果last出牌的人是我，也就是说，current是你右边的那个player在出牌。
                    //所以因为现在是你右边的人再出牌，要把你左边的人的牌和你的牌给显示出来，然而你右边的人的牌要消除，并且显示，一个倒计时（如果以后可以实现的话。）
                    if (previous.Account.Username == username)
                    {
                        #region 设置我的牌。
                        if (current.LastCards != null)
                            if (current.LastCards.Length == 0)
                            {
                                delegatemymsg("pass", true);
                            }
                            else
                            {
                                #region 画出的牌
                                delegatemygivencards(current.LastCards);
                                #endregion

                                #region 画剩余牌
                                //把刚才出的牌去掉。重新画所有的牌。
                                mycards.Children.Clear();
                                cardImages.Clear();
                                //显示新的牌。
                                for (int i = 0; i < previous.LeftCards.Length; i++)
                                {
                                    cardImages.Add(new Image());
                                    cardImages[i].Stretch = Stretch.Fill;
                                    cardImages[i].Source = new BitmapImage(new Uri(@"Resources/cards/" + previous.LeftCards[i].Image + ".png", UriKind.Relative));
                                    cardImages[i].MouseDown += MainWindow_MouseDown;
                                    cardImages[i].Width = cardWidth;
                                    cardImages[i].Height = cardheight;
                                    //这里的数字以后需要修改。
                                    cardImages[i].Margin = new Thickness((20 - previous.LeftCards.Length) * cardWidth / 8 + cardWidth / 4 * i, cardheight / 5, 0, 0);
                                    mycards.Children.Add(cardImages[i]);
                                }
                                #endregion
                            }
                        #endregion

                        #region 设置我左边的玩家的牌。
                        if (previous.LastCards != null)
                            if (previous.LastCards.Length == 0)
                            {
                                delegateleftmsg("pass", true);
                            }
                            else
                            {
                                #region 画出的牌
                                delegateleftgivencards(previous.LastCards);
                                #endregion
                            }
                        #endregion

                        #region 设置在我右边玩家的牌。
                        delegaterightmsg("", false);

                        #region 清除出的牌。
                        rightlastcards.Children.Clear();
                        rightgivencards.Clear();
                        #endregion

                        #endregion
                    }
                    //如果last出牌的人是我左边的人，也就是说，current是我。
                    //所以因为现在我在出牌，要把你左边的人的牌和你右边的人的牌给显示出来，然而你的牌要消除，并且显示，一个倒计时（如果以后可以实现的话。）
                    if (previous.Account.Username == lb_left_username.Content.ToString())
                    {
                        #region 设置我左边的玩家的牌。
                        if (current.LastCards != null)
                            if (current.LastCards.Length == 0)
                            {
                                delegateleftmsg("pass", true);
                            }
                            else
                            {
                                #region 画出的牌
                                delegateleftgivencards(current.LastCards);
                                #endregion
                            }
                        #endregion

                        #region 设置我右边的玩家的牌。
                        if (previous.LastCards != null)
                            if (previous.LastCards.Length == 0)
                            {
                                delegaterightmsg("pass", true);
                            }
                            else
                            {
                                #region 画出的牌
                                delegaterightgivencards(previous.LastCards);
                                #endregion
                            }
                        #endregion

                        #region 设置在我的牌。这边不需要重新画我的牌，因为没有变动。
                        delegatemymsg("", false);

                        #region 清除出的牌。
                        mylastcards.Children.Clear();
                        mygivencards.Clear();

                        #endregion
                        #endregion
                    }
                    //如果last出牌的人是我右边的人，也就是说，current是我左边的人。
                    //所以因为现在我左边的人出牌，要把你的牌和你右边的人的牌给显示出来，然而你左边的人的牌要消除，并且显示，一个倒计时（如果以后可以实现的话。）
                    if (previous.Account.Username == lb_right_username.Content.ToString())
                    {
                        #region 设置我右边的玩家的牌。
                        if (current.LastCards != null)
                            if (current.LastCards.Length == 0)
                            {
                                delegaterightmsg("pass", true);
                            }
                            else
                            {
                                #region 画出的牌
                                delegaterightgivencards(current.LastCards);
                                #endregion
                            }
                        #endregion

                        #region 设置我的牌。
                        if (previous.LastCards != null)
                            if (previous.LastCards.Length == 0)
                            {
                                delegatemymsg("pass", true);
                            }
                            else
                            {
                                #region 画出的牌
                                delegatemygivencards(previous.LastCards);
                                #endregion
                            }
                        #endregion

                        #region 设置在我左边的人的牌。
                        delegateleftmsg("", false);

                        #region 清除出的牌。
                        leftlastcards.Children.Clear();
                        leftgivencards.Clear();

                        #endregion
                        #endregion
                    }
                }
            });
        }

        public void StartGame_Client(Player p1, Player p2, Player p3)
        {
            #region 发牌，给每个人都发牌，17张。
            this.Dispatcher.Invoke(() =>
            {
                delegatemyReadyImage(true);
                delegaterightReadyImage(true);
                delegateleftReadyImage(true);
            });
            Thread.Sleep(500);
            this.Dispatcher.Invoke(() =>
            {
                delegateReady("Ready", false);
                delegatemyReadyImage(false);
                delegaterightReadyImage(false);
                delegateleftReadyImage(false);

                cardImages.Clear();
                if (cb_soundeffect.IsChecked == true)
                {
                    SoundStart.Position = TimeSpan.Zero;
                    SoundStart.Play();
                }
            });
            this.Dispatcher.Invoke(() =>
            {
                pic_1.Source = new BitmapImage(new Uri(@"Resources/back.png", UriKind.Relative));
                pic_2.Source = new BitmapImage(new Uri(@"Resources/back.png", UriKind.Relative));
                pic_3.Source = new BitmapImage(new Uri(@"Resources/back.png", UriKind.Relative));
            });
            //每张牌间隔时间
            int interval = 240;

            if (p1 != null)
            {
                if (p1.Account.Username == username)
                {
                    for (int i = 0; i < p1.LeftCards.Length; i++)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            cardImages.Add(new Image());
                            cardImages[i].Stretch = Stretch.Fill;
                            cardImages[i].Source = new BitmapImage(new Uri(@"Resources/cards/" + p1.LeftCards[i].Image + ".png", UriKind.Relative));
                            cardImages[i].MouseDown += MainWindow_MouseDown;
                            cardImages[i].Width = cardWidth;
                            cardImages[i].Height = cardheight;
                            //这里的数字以后需要修改。
                            cardImages[i].Margin = new Thickness(cardWidth / 4 * i, cardheight / 5, 0, 0);
                            mycards.Children.Add(cardImages[i]);
                        });
                        Thread.Sleep(interval);
                    }
                    proxy.StartDetermineLandlord(username, roomNumber);
                }
            }
            if (p2 != null)
            {
                if (p2.Account.Username == username)
                {
                    for (int i = 0; i < p2.LeftCards.Length; i++)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            cardImages.Add(new Image());
                            cardImages[i].Stretch = Stretch.Fill;
                            cardImages[i].Source = new BitmapImage(new Uri(@"Resources/cards/" + p2.LeftCards[i].Image + ".png", UriKind.Relative));
                            cardImages[i].MouseDown += MainWindow_MouseDown;
                            cardImages[i].Width = cardWidth;
                            cardImages[i].Height = cardheight;
                            //这里的数字以后需要修改。
                            cardImages[i].Margin = new Thickness(cardWidth / 4 * i, cardheight / 5, 0, 0);
                            mycards.Children.Add(cardImages[i]);
                        });
                        Thread.Sleep(interval);
                    }
                    proxy.StartDetermineLandlord(username, roomNumber);
                }
            }
            if (p3 != null)
            {
                if (p3.Account.Username == username)
                {
                    for (int i = 0; i < p3.LeftCards.Length; i++)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            cardImages.Add(new Image());
                            cardImages[i].Stretch = Stretch.Fill;
                            cardImages[i].Source = new BitmapImage(new Uri(@"Resources/cards/" + p3.LeftCards[i].Image + ".png", UriKind.Relative));
                            cardImages[i].MouseDown += MainWindow_MouseDown;
                            cardImages[i].Width = cardWidth;
                            cardImages[i].Height = cardheight;
                            //这里的数字以后需要修改。
                            cardImages[i].Margin = new Thickness(cardWidth / 4 * i, cardheight / 5, 0, 0);
                            mycards.Children.Add(cardImages[i]);
                        });
                        Thread.Sleep(interval);
                    }
                    proxy.StartDetermineLandlord(username, roomNumber);
                }
            }
            this.Dispatcher.Invoke(() =>
            {
                //最开始剩余的牌是17张。
                delegateleftleftcards("17", true);
                delegaterightleftcards("17", true);
            });

            #endregion
        }
        private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //这里以后需要改。
            if (cb_soundeffect.IsChecked == true)
            {
                SoundClick.Position = TimeSpan.Zero;
                SoundClick.Play();
            }
            ((Image)sender).Margin = new Thickness(((Image)sender).Margin.Left, ((Image)sender).Margin.Top == ((Image)sender).Height / 5 ? 0 : ((Image)sender).Height / 5, 0, 0);
        }

        public void UpdateLandlordCard_Client(Player landlord, string cardsType)
        {
            #region 如果是地主之后的开局，就光光只对地主的那个玩家进行变动(插入3张牌)就行。

            Card[] leftovercards = proxy.GetLeftoverCards(roomNumber);

            var thread = new Thread(() =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    #region 翻底牌
                    pic_1.Source = new BitmapImage(new Uri(@"Resources/cards/" + proxy.GetLeftoverCards(roomNumber)[0].Image + ".png", UriKind.Relative));
                    pic_2.Source = new BitmapImage(new Uri(@"Resources/cards/" + proxy.GetLeftoverCards(roomNumber)[1].Image + ".png", UriKind.Relative));
                    pic_3.Source = new BitmapImage(new Uri(@"Resources/cards/" + proxy.GetLeftoverCards(roomNumber)[2].Image + ".png", UriKind.Relative));
                    showcardtype(cardsType, true);
                    #endregion

                    #region 给地主重新发牌。我是地主。
                    if (landlord.Account.Username == username)
                    {
                        //清除所有原来的牌。
                        mycards.Children.Clear();
                        cardImages.Clear();
                        //显示新的牌。
                        for (int i = 0; i < landlord.LeftCards.Length; i++)
                        {
                            cardImages.Add(new Image());
                            cardImages[i].Stretch = Stretch.Fill;
                            cardImages[i].Source = new BitmapImage(new Uri(@"Resources/cards/" + landlord.LeftCards[i].Image + ".png", UriKind.Relative));
                            cardImages[i].MouseDown += MainWindow_MouseDown;
                            cardImages[i].Width = cardWidth;
                            cardImages[i].Height = cardheight;
                            //这里的数字以后需要修改。
                            if ((leftovercards[0].Size == landlord.LeftCards[i].Size && leftovercards[0].Color == landlord.LeftCards[i].Color) ||
                            (leftovercards[1].Size == landlord.LeftCards[i].Size && leftovercards[1].Color == landlord.LeftCards[i].Color) ||
                            (leftovercards[2].Size == landlord.LeftCards[i].Size && leftovercards[2].Color == landlord.LeftCards[i].Color))
                                cardImages[i].Margin = new Thickness(cardWidth / 4 * i, 0, 0, 0);
                            else
                                cardImages[i].Margin = new Thickness(cardWidth / 4 * i, cardheight / 5, 0, 0);
                            mycards.Children.Add(cardImages[i]);
                        }
                        Player left = proxy.GetPlayerByUsername(lb_left_username.Content.ToString(), roomNumber);
                        Player right = proxy.GetPlayerByUsername(lb_right_username.Content.ToString(), roomNumber);
                        delegateupdateavator(true, left.Account.IsMale, left.Landlord, landlord.Account.IsMale, landlord.Landlord, true, right.Account.IsMale, right.Landlord);
                        return;
                    }

                    #endregion

                    //更新地主牌的数量。同时更新倍率，有可能底牌是顺子什么的。
                    //更新地主牌的数量。
                    if (landlord.Account.Username == lb_left_username.Content.ToString())
                    {
                        delegateleftleftcards("20", true);
                        Player me = proxy.GetPlayerByUsername(username, roomNumber);
                        Player right = proxy.GetPlayerByUsername(lb_right_username.Content.ToString(), roomNumber);
                        delegateupdateavator(true, landlord.Account.IsMale, landlord.Landlord, me.Account.IsMale, me.Landlord, true, right.Account.IsMale, right.Landlord);
                    }
                    if (landlord.Account.Username == lb_right_username.Content.ToString())
                    {
                        delegaterightleftcards("20", true);
                        Player me = proxy.GetPlayerByUsername(username, roomNumber);
                        Player left = proxy.GetPlayerByUsername(lb_left_username.Content.ToString(), roomNumber);
                        delegateupdateavator(true, left.Account.IsMale, left.Landlord, me.Account.IsMale, me.Landlord, true, landlord.Account.IsMale, landlord.Landlord);
                    }
                    //更新倍率。
                    this.lb_bet_value.Content = proxy.GetBetNumber(roomNumber).ToString();
                }));

            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            #endregion
        }

        public void TimeCounterDown_Client(Player current, int timeLeft)
        {
            var thread = new Thread(() =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    delegatelefttimer("", false);
                    delegatemytimer("", false);
                    delegaterighttimer("", false);

                    if (current.Account.Username == username)
                    {
                        delegatemytimer(timeLeft.ToString(), true);
                    }
                    if (current.Account.Username == lb_left_username.Content.ToString())
                    {
                        delegatelefttimer(timeLeft.ToString(), true);
                    }
                    if (current.Account.Username == lb_right_username.Content.ToString())
                    {
                        delegaterighttimer(timeLeft.ToString(), true);
                    }
                }));

            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }

        public void GameOver_Client(Player previous, Player winner, Player next, Card[] leftovercards, Player quitter)
        {
            //if one player quit in the middle of the game.
            if (quitter != null)
            {
                var thread = new Thread(() =>
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        delegateresult("Game over, " + quitter.Account.Username + " quits during the game!", true);

                        //翻底牌
                        pic_1.Source = new BitmapImage(new Uri(@"Resources/cards/" + leftovercards[0].Image + ".png", UriKind.Relative));
                        pic_2.Source = new BitmapImage(new Uri(@"Resources/cards/" + leftovercards[1].Image + ".png", UriKind.Relative));
                        pic_3.Source = new BitmapImage(new Uri(@"Resources/cards/" + leftovercards[2].Image + ".png", UriKind.Relative));

                        if (quitter.Account.Username != username)
                        {
                            if (quitter.Account.Username != lb_right_username.Content.ToString())
                            {
                                this.delegateleftgivencards(quitter.LeftCards);
                                if (previous.Account.Username == lb_right_username.Content.ToString())
                                    this.delegaterightgivencards(previous.LeftCards);
                                if (winner.Account.Username == lb_right_username.Content.ToString())
                                    this.delegaterightgivencards(winner.LeftCards);
                                if (next.Account.Username == lb_right_username.Content.ToString())
                                    this.delegaterightgivencards(next.LeftCards);
                            }
                            if (quitter.Account.Username != lb_left_username.Content.ToString())
                            {
                                if (previous.Account.Username == lb_left_username.Content.ToString())
                                    this.delegaterightgivencards(previous.LeftCards);
                                if (winner.Account.Username == lb_left_username.Content.ToString())
                                    this.delegaterightgivencards(winner.LeftCards);
                                if (next.Account.Username == lb_left_username.Content.ToString())
                                    this.delegaterightgivencards(next.LeftCards);
                                this.delegateleftgivencards(quitter.LeftCards);
                            }
                        }
                    }));
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
            }
            //游戏正常结束。
            else
            {
                //如果这里有赢家,不是没人抢地主的情况。
                if (winner.LeftCards.Length == 0)
                {
                    var thread = new Thread(() =>
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            SoundBGM.Stop();
                            if (winner.Landlord)
                                delegateresult("game over, LANDLORD wins!", true);
                            else
                                delegateresult("game over, FARMER wins!", true);

                            //如果是你赢了。
                            if (winner.Account.Username == username)
                            {
                                delegaterightgivencards(next.LeftCards);
                                delegateleftgivencards(previous.LeftCards);
                                //显示赢家出的最后一手牌。
                                delegatemygivencards(next.LastCards);
                                mycards.Children.Clear();

                                if (cb_soundeffect.IsChecked == true)
                                {
                                    SoundWin.Position = TimeSpan.Zero;
                                    SoundWin.Play();
                                }
                            }
                            //如果是你右边的人赢了。
                            if (previous.Account.Username == username)
                            {
                                delegateleftgivencards(next.LeftCards);
                                //显示赢家出的最后一手牌。
                                delegaterightgivencards(next.LastCards);
                                delegaterightleftcards("0", true);

                                if (cb_soundeffect.IsChecked == true)
                                {
                                    //如果他是地主。
                                    if (winner.Landlord)
                                    {
                                        SoundLoss.Position = TimeSpan.Zero;
                                        SoundLoss.Play();
                                    }
                                    else
                                    {
                                        //如果你是地主。
                                        if (previous.Landlord)
                                        {
                                            SoundLoss.Position = TimeSpan.Zero;
                                            SoundLoss.Play();
                                        }
                                        //如果你和他是一伙的。
                                        else
                                        {
                                            SoundWin.Position = TimeSpan.Zero;
                                            SoundWin.Play();
                                        }
                                    }
                                }
                            }
                            //如果是你左边的人赢了。
                            if (next.Account.Username == username)
                            {
                                delegaterightgivencards(previous.LeftCards);
                                //显示赢家出的最后一手牌。
                                delegateleftgivencards(next.LastCards);
                                delegateleftleftcards("0", true);

                                if (cb_soundeffect.IsChecked == true)
                                {
                                    //如果他是地主。
                                    if (winner.Landlord)
                                    {
                                        SoundLoss.Position = TimeSpan.Zero;
                                        SoundLoss.Play();
                                    }
                                    else
                                    {
                                        //如果你是地主。
                                        if (next.Landlord)
                                        {
                                            SoundLoss.Position = TimeSpan.Zero;
                                            SoundLoss.Play();
                                        }
                                        //如果你和他是一伙的。
                                        else
                                        {
                                            SoundWin.Position = TimeSpan.Zero;
                                            SoundWin.Play();
                                        }
                                    }
                                }
                            }
                        }));
                    });
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                    thread.Join();
                }
                //如果这是没人抢地主的情况，没有赢家。
                else
                {
                    var thread = new Thread(() =>
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            delegateresult("game over, nobody calls for landlord.", true);

                            //翻底牌
                            pic_1.Source = new BitmapImage(new Uri(@"Resources/cards/" + leftovercards[0].Image + ".png", UriKind.Relative));
                            pic_2.Source = new BitmapImage(new Uri(@"Resources/cards/" + leftovercards[1].Image + ".png", UriKind.Relative));
                            pic_3.Source = new BitmapImage(new Uri(@"Resources/cards/" + leftovercards[2].Image + ".png", UriKind.Relative));

                            if (winner.Account.Username == username)
                            {
                                this.delegateleftgivencards(previous.LeftCards);
                                this.delegaterightgivencards(next.LeftCards);
                            }
                            if (previous.Account.Username == username)
                            {
                                this.delegateleftgivencards(next.LeftCards);
                                this.delegaterightgivencards(winner.LeftCards);
                            }
                            if (next.Account.Username == username)
                            {
                                this.delegaterightgivencards(previous.LeftCards);
                                this.delegateleftgivencards(winner.LeftCards);
                            }
                        }));
                    });
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                    thread.Join();
                }
            }

            var thread_1 = new Thread(() =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    delegateGPH(false);

                    delegateReady("Ready", true);

                    delegatemyReadyImage(false);
                    delegateleftReadyImage(false);
                    delegaterightReadyImage(false);
                }));
            });

            thread_1.SetApartmentState(ApartmentState.STA);
            thread_1.Start();
            thread_1.Join();

            //update the lobby.
            this.proxy.UpdateLobby(roomNumber);
        }
        
        public void UpdateChatter_Client(string message)
        {
            this.Dispatcher.Invoke(() =>
            {
                lb_messages.Items.Add(message);
            });
        }

        #endregion

        #region button functions.

        private void bt_ready_Click(object sender, RoutedEventArgs e)
        {
            if (cb_soundeffect.IsChecked == true)
            {
                SoundbtClick.Position = TimeSpan.Zero;
                SoundbtClick.Play();
            }
            if (cb_bg_music.IsChecked == true)
            {
                if (SoundBGM.Position == TimeSpan.Zero)
                    SoundBGM.Play();
            }
            if (bt_ready.Content.ToString() == "Ready")
            {
                delegateReady("Wait", true);
                proxy.SetPlayerReady(username, roomNumber);
            }
            else
            {
                delegateReady("Ready", true);
                proxy.SetPlayerWait(username, roomNumber);
            }

            //把当前场上所有的牌给去掉。
            //去除前面玩家出的牌。
            delegatemygivencards(new Card[0]);
            delegateleftgivencards(new Card[0]);
            delegaterightgivencards(new Card[0]);
            //去除剩余手牌。
            mycards.Children.Clear();
            cardImages.Clear();

            initialize();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            proxy.PlayerJoinRoom(this.username, this.roomNumber, this.position);
            SoundBGM.Play();
        }

        private void bt_give_Click(object sender, RoutedEventArgs e)
        {
            List<int> cards = new List<int>();
            for (int i = 0; i < cardImages.Count; i++)
            {
                if (cardImages[i].Margin.Top == 0)
                {
                    cards.Add(i);
                }
            }
            int[] index = new int[cards.Count];
            for (int i = 0; i < cards.Count; i++)
            {
                index[i] = cards[i];
            }

            //如果仅仅是give按钮现实的话，说明上面没有人大的过他，所以他必须要出牌。
            if (index.Length == 0 && bt_pass.Visibility == Visibility.Hidden)
                return;

            if (index.Length > 0)
            {
                if (!proxy.ValidateCardsCombination(username, index, roomNumber))
                {
                    delegateresult("Invalid combination cards", true);
                    return;
                }
                if (!proxy.ValidateCardsComparison(username, index, roomNumber))
                {
                    delegateresult("your combination is not big enough to beat your opponent", true);
                    return;
                }
            }

            if (index.Length != 0)
            {
                if (cb_soundeffect.IsChecked == true)
                {
                    SoundGive.Position = TimeSpan.Zero;
                    SoundGive.Play();
                }
            }
            proxy.GiveCards(username, index, roomNumber);
            //把所有牌复位。
            foreach (Image image in mycards.Children)
            {
                image.Margin = new Thickness(image.Margin.Left, image.Height / 5, 0, 0);
            }
        }

        private void bt_jiaodizhu_Click(object sender, RoutedEventArgs e)
        {
            proxy.DetermindLandlord(username, true, roomNumber);
        }

        private void bt_bujiao_Click(object sender, RoutedEventArgs e)
        {
            bt_jiaodizhu.Visibility = Visibility.Hidden;
            bt_bujiao.Visibility = Visibility.Hidden;
            proxy.DetermindLandlord(username, false, roomNumber);
        }

        private void bt_pass_Click(object sender, RoutedEventArgs e)
        {
            proxy.GiveCards(username, new int[0], roomNumber);
            //把所有牌复位。
            foreach (Image image in mycards.Children)
            {
                image.Margin = new Thickness(image.Margin.Left, image.Height / 5, 0, 0);
            }
        }

        private void bt_hint_Click(object sender, RoutedEventArgs e)
        {
            int[] hitIndex = proxy.GiveHints(username, roomNumber);
            //必须pass.
            if (hitIndex == null)
            {
                bt_pass_Click(sender, e);
                return;
            }

            for (int i = 0; i < mycards.Children.Count; i++)
            {
                bool contained = false;
                foreach (int index in hitIndex)
                {
                    if (index == i)
                        contained = true;
                }
                if (contained)
                    ((Image)mycards.Children[i]).Margin = new Thickness(((Image)mycards.Children[i]).Margin.Left, 0, 0, 0);
                else
                    ((Image)mycards.Children[i]).Margin = new Thickness(((Image)mycards.Children[i]).Margin.Left, ((Image)mycards.Children[i]).Height / 5, 0, 0);
            }
        }

        //右键出牌。
        private void MainWindow_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (bt_give.Visibility == Visibility.Visible)
            {
                bt_give_Click(sender, e);
            }
        }

        private void bt_send_msg_Click(object sender, RoutedEventArgs e)
        {
            if (cb_message.SelectedItem != null)
                proxy.SendMessage(username, ((ComboBoxItem)cb_message.SelectedItem).Content.ToString(), roomNumber);
            cb_message.SelectedItem = null;
        }

        private void bt_message_clear_Click(object sender, RoutedEventArgs e)
        {
            cb_message.SelectedItem = null;
            this.lb_messages.Items.Clear();
        }
        
        private void Cb_bg_music_CheckedChange(object sender, RoutedEventArgs e)
        {
            if (cb_bg_music.IsChecked == true)
            {
                if (SoundBGM != null)
                    SoundBGM.Play();
            }
            else
            {
                if (SoundBGM != null)
                    SoundBGM.Stop();
            }
        }

        #endregion
    }
}
