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
    class Square
    {
        public Rectangle Rect { get; set; }
        public Double Width { get; set; }
        public Double Height { get; set; }

        public Square(double width, double height, double posX, double posY)
        {
            this.Width = width;
            this.Height = height;

            Rect = new Rectangle();
            Rect.Width = width;
            Rect.Height = height;
            Rect.Fill = Brushes.Orange;            
            Panel.SetZIndex(Rect, 10);

            Canvas.SetLeft(Rect, posX);
            Canvas.SetTop(Rect, posY);
        }

        public void SetNewPos(double posX, double posY)
        {
            Canvas.SetLeft(Rect, posX);
            Canvas.SetTop(Rect, posY);   
        }

        public void Draw(Canvas c)
        {
            if (!c.Children.Contains(Rect))
            {
                c.Children.Add(Rect);
            }
        }

        public void UnDraw(Canvas c)
        {
            if (c.Children.Contains(Rect))
            {
                c.Children.Remove(Rect);
            }
        }
    }
}
