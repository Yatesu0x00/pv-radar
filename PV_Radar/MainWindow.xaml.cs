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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Windows.Threading;
using System.Threading;

namespace PV_Radar
{
    public partial class MainWindow : Window
    {
        MyDlg dlg;
        Socket s;
        IPAddress host;
        DispatcherTimer timer;
        bool connectStatus;
        byte[] bytes;
        double ticks_old;
        Circle blip;
        string[] requestData;
        string request;
        Square square;
        double posX, posY;
        double x, y;
        double vX, vY;
        Thread thread_1;

        public MainWindow()
        {
            InitializeComponent();
            disconnect.IsEnabled = false;
            Background = Brushes.Gainsboro;
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 60);

            ticks_old = Environment.TickCount;

            blip = new Circle(10, -20, -20);
            blip.Draw(cvs);

            square = new Square(100, 40, -20, -20);
            square.Draw(cvs);

            r.Visibility = Visibility.Hidden;
            v.Visibility = Visibility.Hidden;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            double ticks = Environment.TickCount;

            var point = Mouse.GetPosition(window);
            new Point(window.Left + point.X, window.Top + point.Y);

            posX = point.X;
            posY = point.Y;

            request = "send_data";
            bytes = new byte[256];

            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
            {
                square.SetNewPos(posX + 15, posY - 15);

                Canvas.SetLeft(r, posX + 15);
                Canvas.SetTop(r, posY - 18);
                Canvas.SetLeft(v, posX + 15);
                Canvas.SetTop(v, posY - 4);             
            }));
            
            if (connectStatus)
            {
                try
                {                   
                    s.Send(Encoding.ASCII.GetBytes(request));
                    s.Receive(bytes);

                    string requestValue = Encoding.ASCII.GetString(bytes);
                    requestData = requestValue.Split(':');
                    
                    x = Convert.ToDouble(requestData[0]);
                    y = Convert.ToDouble(requestData[1]);
                    vX = Convert.ToDouble(requestData[2]);
                    vY = Convert.ToDouble(requestData[3]);                  

                    Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                    {
                        blip.UpdatePos(Convert.ToDouble(requestData[0]), Convert.ToDouble(requestData[1]));
                    }));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Fehler: " + ex.Message, "Eingabefehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    disconnect.IsEnabled = false;
                    connect.IsEnabled = true;
                    connectStatus = false;
                    blip.UnDraw(cvs);
                }
            }

            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
            {
                r.Content = "r= " + Math.Round(Math.Sqrt((x * x) + (y * y)), 2) + " km";
                v.Content = "v= " + Math.Round(Math.Sqrt((vX * vX) + (vY * vY)), 2) + " km/h"; 
                Console.WriteLine(requestData[2]);
                Console.WriteLine(requestData[3]);
            }));

            ticks_old = ticks;
        }

        private void Thread_function()
        {           
            s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                     
            try
            {
                s.Connect(host, dlg.port);
                connectStatus = true;            

                Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    disconnect.IsEnabled = true;
                    connect.IsEnabled = false;
                }));
            }
            catch (SocketException se)
            {
                MessageBox.Show("Fehler: " + se.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);

                Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    disconnect.IsEnabled = false;
                    connect.IsEnabled = true;
                    connectStatus = false;
                    blip.UnDraw(cvs);
                }));
            }
            Thread.Sleep(50);            
        }

        private void connect_Click(object sender, RoutedEventArgs e)
        {
            dlg = new MyDlg();
            dlg.ShowDialog();

            if (dlg.DialogResult == true)
            {
                host = IPAddress.Parse(dlg.ipAddress);

                thread_1 = new Thread(Thread_function);
                thread_1.Start();

                timer.Start();
            }
        }

        private void window_MouseMove(object sender, MouseEventArgs e)
        {
            if (blip.Elli.IsMouseDirectlyOver)
            { 
                square.Rect.Visibility = Visibility.Visible;
                r.Visibility = Visibility.Visible;
                v.Visibility = Visibility.Visible;
            }
            else
            {
                square.Rect.Visibility = Visibility.Hidden;
                r.Visibility = Visibility.Hidden;
                v.Visibility = Visibility.Hidden;
            }
        }

        private void disconnect_Click(object sender, RoutedEventArgs e)
        {
            connectStatus = false;
            s.Close();
            connect.IsEnabled = true;
            disconnect.IsEnabled = false;
            blip.UnDraw(cvs);
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                s.Close();

                Environment.Exit(0);
            }
            catch { }
        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                s.Close();
                Environment.Exit(0);
            }
            catch { }
        }

        /*
         * f) Als erstes Eventhandler für "Canvas_SizeChanged" erstellen und dann eine Resize Methode jeweils für beide Klassen programmieren, dann sie in den "Eventhandler Canvas_SizeChanged" in Mainwindow.cs aufrufen
         * Zusätzlich dort die statischen Elemente Resizen(Ellipsen, Labels, Rectangles)
        */
    }
}
