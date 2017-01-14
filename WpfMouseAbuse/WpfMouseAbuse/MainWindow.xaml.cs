using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

namespace WpfMouseAbuse
{
   public partial class MainWindow : Window
   {
      public ConcurrentQueue<Line> m_lines;
      public Stopwatch ti = new Stopwatch();
      long last_n = 0;
      int color_idx = 0;
      double x0, y0;
      public const int color_max = 5;

      public MainWindow()
      {
         m_lines = new ConcurrentQueue<Line>();
         ti.Start();
         InitializeComponent();
         Canvas.MouseLeftButtonDown += Canvas_MouseButtonDown;
         Canvas.MouseLeftButtonUp += Canvas_MouseButtonUp;
         EventManager.RegisterClassHandler(typeof(Window), Window.PreviewMouseDownEvent, new MouseButtonEventHandler(Canvas_MouseButtonDown));
         EventManager.RegisterClassHandler(typeof(Window), Window.PreviewMouseUpEvent, new MouseButtonEventHandler(Canvas_MouseButtonUp));
      }

      void Canvas_MouseButtonUp(object sender, MouseButtonEventArgs e)
      {
         long n = ti.ElapsedMilliseconds;
         Line _line;
         bool ok = m_lines.TryDequeue(out _line);
         if (ok)
         {            
            _line.X2 = x0 + n;
            _line.Y2 = y0
               - Math.Abs(x0 - e.GetPosition(this.Canvas).X)
               - Math.Abs(y0 - e.GetPosition(this.Canvas).Y);
            _line.Loaded += _line_Loaded;
            Canvas.Children.Add(_line);
         }
      }

      void _line_Loaded(object sender, RoutedEventArgs e)
      {
         Line line = sender as Line;
         Color col = GetColorFromIdx(color_idx);
         Cls_Barriere.LineAnimation(line, col);
         // Canvas.Children.Add(line);
      }
      Color GetColorFromIdx(int col_idx)
      {
         Color ret_val;
         switch (col_idx)
         {
            case 0:
               ret_val= Colors.Red;
               break;
            case 1:
               ret_val = Colors.Orange;
               break;
            case 2:
               ret_val= Colors.Yellow;
               break;
            case 3:
               ret_val= Colors.DarkGray;
               break;
            case 4:
               ret_val= Colors.Green;
               break;
            case 5:
            default:
               ret_val= Colors.Blue;
               System.Diagnostics.Debug.Assert(color_max == col_idx);
               break;
         }
         return ret_val;
      }

      void Canvas_MouseButtonDown(object sender, MouseButtonEventArgs e)
      {
         Line _line;
         long n = ti.ElapsedMilliseconds;
         if (e.ChangedButton == MouseButton.Right)
            if (++color_idx > color_max)
               color_idx = 0;
         if (e.ChangedButton == MouseButton.Middle)
            Canvas.Children.Clear();
         if (e.ChangedButton == MouseButton.Left)
         {
            if ((n - last_n) > 500)
            {
               ti.Restart();
               while (m_lines.TryDequeue(out _line))
                  ;
               n = 0;
            }
            last_n = n;
            _line = new Line();
            _line.Stroke = new SolidColorBrush(Colors.White);
            _line.StrokeThickness = 5;
            _line.StrokeStartLineCap = PenLineCap.Round;

            _line.StrokeEndLineCap = PenLineCap.Round;
            _line.StrokeDashCap = PenLineCap.Round;
            if (n == 0)
            {
               x0 = e.GetPosition(this.Canvas).X;
               y0 = e.GetPosition(this.Canvas).Y;
            }
            _line.X1 = x0 + n;
            _line.Y1 = y0;
            m_lines.Enqueue(_line);
         }
      }
   }

#if false
   public partial class App : Application
   {
      protected override void OnStartup(StartupEventArgs e)
      {
         EventManager.RegisterClassHandler(typeof(Window), Window.PreviewMouseDownEvent, new MouseButtonEventHandler(OnPreviewMouseDown));
         EventManager.RegisterClassHandler(typeof(Window), Window.PreviewMouseUpEvent, new MouseButtonEventHandler(OnPreviewMouseUp));

         base.OnStartup(e);
      }

      static void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
      {
         Trace.WriteLine("Up!!!");
      }
      static void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
      {
         string s = String.Format("Down: {0}", e.ChangedButton);
         Trace.WriteLine(s);
      }
   }
#endif
   public class Cls_Barriere
   {
      // animazione periferica
      public static void LineAnimation(Line _line, Color col)
      {
         Storyboard result = new Storyboard();
         Duration duration = new Duration(TimeSpan.FromSeconds(2));

         ColorAnimation animation = new ColorAnimation();
         animation.RepeatBehavior = RepeatBehavior.Forever;
         animation.Duration = duration;
         animation.From = col;
         animation.To = col; // Colors.Gray;
         Storyboard.SetTarget(animation, _line);
         Storyboard.SetTargetProperty(animation, new PropertyPath("(Line.Stroke).(SolidColorBrush.Color)"));
         result.Children.Add(animation);
         result.Begin();
      }
   }
}
