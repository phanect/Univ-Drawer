using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Ink;
using Microsoft.Win32;
using System.IO;
using System;

namespace VectorGraphicsDrawer_WPF
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>インクの太  さ、色などの属性情報を管理するオブジェクト</summary>
        DrawingAttributes inkAttr = new DrawingAttributes();
        /// <summary>直線を描画中であるか否かを示すフラグ</summary>
        private bool isDrawing = false;
        /// <summary>直線を描画する際の点のコレクション</summary>
        private StylusPointCollection stylusPoints;
        /// <summary>
        /// 描画する直線
        /// </summary>
        Line tmpline = new Line();

        public MainWindow()
        {
            InitializeComponent();
            DrawZone.Children.Add(tmpline); // 移動時のラインを予め追加しておく。
        }

        private void cbLineStroke_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int penradius = cbLineStroke.SelectedIndex + 1;

            inkAttr.Height = penradius;
            inkAttr.Width = penradius;

            DrawZone.DefaultDrawingAttributes = inkAttr;
        }

        private void btColor_Click(object sender, RoutedEventArgs e)
        {
            // Rotate Current Colors when the button clicked
            if (btColor.Content.Equals("黒"))
            {
                btColor.Content = "赤";
                btColor.Foreground = Brushes.Red;
                inkAttr.Color = Colors.Red;
            }
            else if (btColor.Content.Equals("赤"))
            {
                btColor.Content = "青";
                btColor.Foreground = Brushes.Blue;
                inkAttr.Color = Colors.Blue;
            }
            else if (btColor.Content.Equals("青"))
            {
                btColor.Content = "黄";
                btColor.Foreground = Brushes.Yellow;
                inkAttr.Color = Colors.Yellow;
            }
            else if (btColor.Content.Equals("黄"))
            {
                btColor.Content = "緑";
                btColor.Foreground = Brushes.Green;
                inkAttr.Color = Colors.Green;
            }
            else // if(btColor.Content.Equals("緑"))
            {
                btColor.Content = "黒";
                btColor.Foreground = Brushes.Black;
                inkAttr.Color = Colors.Black;
            }
        }

        private void tbHandWrite_Checked(object sender, RoutedEventArgs e)
        {
            DrawZone.EditingMode = InkCanvasEditingMode.Ink;
            if (tbLine != null)
                tbLine.IsChecked = false;
            if (tbSelect != null)
                tbSelect.IsChecked = false;
            if (tbEraser != null)
                tbEraser.IsChecked = false;
        }

        private void tbLine_Checked(object sender, RoutedEventArgs e)
        {
            DrawZone.EditingMode = InkCanvasEditingMode.None;
            tbHandWrite.IsChecked = false;
            tbSelect.IsChecked = false;
            tbEraser.IsChecked = false;
        }

        private void tbSelect_Checked(object sender, RoutedEventArgs e)
        {
            DrawZone.EditingMode = InkCanvasEditingMode.Select;
            tbHandWrite.IsChecked = false;
            tbLine.IsChecked = false;
            tbEraser.IsChecked = false;
        }

        private void tbEraser_Checked(object sender, RoutedEventArgs e)
        {
            DrawZone.EditingMode = InkCanvasEditingMode.EraseByPoint;
            tbHandWrite.IsChecked = false;
            tbLine.IsChecked = false;
            tbSelect.IsChecked = false;
        }

        private void DrawZone_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return; // 左ボタンが押された状態でなければ、なにもしない。

            if (DrawZone.EditingMode == InkCanvasEditingMode.None) // 直線モードの時
            {
                if (!isDrawing) // 現在描画中でなければ
                {

                    Point start = e.GetPosition(DrawZone); // クリックした座標を取得。(引数の DrawZone は、座標の基準にするコンポーネント)

                    stylusPoints = new StylusPointCollection();

                    stylusPoints.Add(new StylusPoint(start.X, start.Y));
                    tmpline.X1 = start.X;
                    tmpline.Y1 = start.Y;
                    /*line = new Line();
                    line.X1 = start.X;
                    line.Y1 = start.Y;
                    line.StrokeThickness = cbLineStroke.SelectedIndex + 1;
                    line.Stroke = btColor.Background;
                    DrawZone.Children.Add(line);*/

                    isDrawing = true; // 描画中フラグを立てる

                }
            }
        }

        private void DrawZone_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDrawing)
            {
                Point end = e.GetPosition(DrawZone); // クリックした座標を取得。(引数の DrawZone は、座標の基準にするコンポーネント)

                stylusPoints.Add(new StylusPoint(end.X, end.Y));
                Stroke stroke = new Stroke(stylusPoints);
                stroke.DrawingAttributes.Color = inkAttr.Color;
                stroke.DrawingAttributes.Width = inkAttr.Width;
                DrawZone.Strokes.Add(stroke);

                stylusPoints = null;

                isDrawing = false;
            }
        }

        private void DrawZone_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {

                Point end = e.GetPosition(DrawZone); // クリックした座標を終点として取得

                //tmpline = new Line();
                tmpline.X2 = end.X;
                tmpline.Y2 = end.Y;
                tmpline.StrokeThickness = cbLineStroke.SelectedIndex + 1;
                tmpline.Stroke = btColor.Background;

                /*
                if (stylusPoints.Count < 2)
                    stylusPoints.Add(new StylusPoint(end.X, end.Y));
                else
                {
                    stylusPoints.ElementAt(1).X = end.X;
                    stylusPoints.ElementAt(1).Y = end.Y;
                }*/
                //            line.X2 = end.X;
                //          line.Y2 = end.Y;
            }
        }

        /// <summary>
        /// 保存ボタンを押したときのイベント
        /// </summary>
        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog(); // 保存ダイアログ

            // デフォルトは ISF (ベクタ) 形式。他に、bmp と jpg で保存が可能。
            saveDialog.DefaultExt = ".isf";
            saveDialog.Filter = "Ink Serialized Format (*.isf)|*.isf|Bitmap (*.bmp)|*.bmp|Jpeg (*.jpg)|*.jpg|全てのファイル|*.*";

            if (saveDialog.ShowDialog() == true)
            {
                string filename = saveDialog.FileName;

                if (File.Exists(filename)) File.Delete(filename); // 当該のファイルが存在する場合は削除

                using (FileStream fs = new FileStream(filename, FileMode.CreateNew))
                {
                    if (filename.EndsWith(".isf")) // ISF 形式で保存する場合
                    {
                        DrawZone.Strokes.Save(fs);
                    }
                    else // bmp か jpg で保存する場合
                    {
                        Rect bounds = new Rect(0, 0, DrawZone.ActualWidth, DrawZone.ActualHeight); // DrawZone.Strokes.GetBounds();
                        DrawingVisual visual = new DrawingVisual();

                        using (DrawingContext context = visual.RenderOpen())
                        {
                            context.PushTransform(new TranslateTransform(-bounds.X, -bounds.Y));
                            context.DrawRectangle(DrawZone.Background, null, bounds);
                            DrawZone.Strokes.Draw(context);
                        }

                        RenderTargetBitmap bitmap = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, 96, 96, PixelFormats.Default);
                        bitmap.Render(visual);

                        BitmapEncoder encoder = null;

                        if (filename.EndsWith(".bmp"))
                        {
                            encoder = new BmpBitmapEncoder();
                        }
                        else if (filename.EndsWith(".jpg"))
                        {
                            encoder = new JpegBitmapEncoder();
                        }
                        else if (filename.EndsWith(".png"))
                        {
                            encoder = new PngBitmapEncoder();
                        }

                        if (encoder != null)
                        {
                            encoder.Frames.Add(BitmapFrame.Create(bitmap));
                            encoder.Save(fs);
                        }

                    }
                }
            }
        }

        private void btOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.DefaultExt = ".isf";
            openDialog.Filter = "Ink Serialized Format (.isf)|*.isf|Bitmap (*.bmp)|*.bmp|Jpeg (*.jpg)|*.jpg|全てのファイル|*.*";

            if (openDialog.ShowDialog() == true)
            {
                string filename = openDialog.FileName;

                if (filename.EndsWith(".isf")) // ISF 形式なら
                {
                    using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                    {
                        DrawZone.Strokes = new StrokeCollection(fs);
                    }
                }
                else // 一般的な画像形式なら
                {
                    DrawingVisual visual = new DrawingVisual();

                    using (DrawingContext context = visual.RenderOpen())
                    {
                        
                            BitmapImage bitmap = new BitmapImage();

                            bitmap.BeginInit();
                            bitmap.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                            bitmap.UriSource = new Uri(@filename, UriKind.Absolute);

                            bitmap.EndInit();

                            Image i = new Image();
                            i.Source = bitmap;
                            DrawZone.Children.Add(i);
                    }
                }


            }
        }

        private void InkCanvas_Gesture(object sender, InkCanvasGestureEventArgs e)
        {

        }

    }
}
