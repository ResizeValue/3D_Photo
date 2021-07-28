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
using System.Windows.Forms;
using System.IO;
using System.Windows.Threading;
using System.Threading;
using MessageBox = System.Windows.MessageBox;

namespace _3D_Photo
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Photo_> images = new List<Photo_>();
        
        int scroll_direction = 0;
        bool auto_scroll_enabled = false;
        int curIndex = -1;
        double sens = 5;
        public MainWindow()
        {
            InitializeComponent();
            
        }

        void OpenFolder()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Fill_ListBox(dialog.SelectedPath);
                empty_model_btn.Visibility = Visibility.Hidden;
            }
        }

        void Fill_ListBox(string path)
        {
            images.Clear();
            DirectoryInfo info = new DirectoryInfo(path);

            foreach (var file in info.GetFiles())
            {
                if(file.Name[0] == '0' && file.Name[1] == '_')
                {
                    string[] tmp = file.Name.Split(new Char[] { '_', '.' });
                    images.Add(new Photo_() { Name = file.Name, Path = file.FullName, Position = Int32.Parse(tmp[1]) });
                }

            }
            images_box.ItemsSource = null;
            images.Sort(delegate (Photo_ photo1, Photo_ photo2) { return photo1.Position.CompareTo(photo2.Position); });
            images_box.ItemsSource = images;
            images_box.SelectedIndex = 0;
            no_active_model_tb.Visibility = Visibility.Hidden;
            no_photo.Visibility = Visibility.Hidden;
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            OpenFolder();
        }

        private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {
            if (auto_scroll_enabled) return;
            Photo_ photo = ((ListBoxItem)sender).Content as Photo_;
            img.Source = new BitmapImage(new Uri(photo.Path));
            ((ListBoxItem)sender).Background = Brushes.BlueViolet;
            
        }

        private void ListBoxItem_Unselected(object sender, RoutedEventArgs e)
        {
            ((ListBoxItem)sender).Background = Brushes.LightGray;
        }
        bool click = false;
        Point point = new Point();

        private void img_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (auto_scroll_enabled) Clear_Scroll();
            click = true;
            point = e.GetPosition(img);
        }

        private void img_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            click = false;
        }

        private void img_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (click)
            {
                if(point.X + sens <= e.GetPosition(img).X)
                {
                    if(images_box.SelectedIndex == 0)
                    {
                        images_box.SelectedIndex = images_box.Items.Count - 1;
                    }
                    else
                    {
                        images_box.SelectedIndex -= 1;
                    }
                    img.Source = new BitmapImage(new Uri(((Photo_)images_box.SelectedItem).Path));
                    point = e.GetPosition(img);
                }
                else if (point.X - sens >= e.GetPosition(img).X)
                {
                    if (images_box.SelectedIndex == images_box.Items.Count - 1)
                    {
                        images_box.SelectedIndex = 0;
                    }
                    else
                    {
                        images_box.SelectedIndex += 1;
                    }
                    img.Source = new BitmapImage(new Uri(((Photo_)images_box.SelectedItem).Path));
                    point = e.GetPosition(img);
                }
            }
        }


        private void img_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            click = false;
        }


        public delegate void NextImage(int param);
        void Print_Image(int param)
        {
            if (!Dispatcher.CheckAccess())
            {
                NextImage primeEv = new NextImage(Print_Image);
                Dispatcher.Invoke(primeEv, new object[] { param });
            }
            else
            {
                images_box.SelectedIndex += scroll_direction;
                if (images_box.SelectedIndex == images_box.Items.Count - 1) { 
                    images_box.SelectedIndex = 0;
                }
                else if (images_box.SelectedIndex == 0) {
                    images_box.SelectedIndex = images_box.Items.Count - 1;
                }
                img.Source = new BitmapImage(new Uri(((Photo_)images_box.SelectedItem).Path));
            }
        }

        void Img_Timer_Change()
        {
            while (true)
            {
                if (!auto_scroll_enabled) break;
                Print_Image(0);
                Thread.Sleep(40);
            }
            
        }

        private void auto_scroll_Disabled()
        {
            auto_scroll_enabled = false;
        }

        private void export_HTML_Click(object sender, RoutedEventArgs e)
        {
            if (images.Count < 1) {
                MessageBox.Show("Модель не загружена"); 
                return; 
            }
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "HTML files (*.html)|*.html";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ExportToHTML(dialog.FileName);
            }
        }

        void ExportToHTML(string path)
        {
            HTML_Program program = new HTML_Program();
            program.Write_To_File(path, images, sens);
        }

        void Clear_Scroll()
        {
            auto_scroll_Disabled();
            left_scroll.Background = Brushes.Transparent;
            right_scroll.Background = Brushes.Transparent;
            scroll_direction = 0;
        }

        private void btn_scroll_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Border bord = sender as Border;
            bord.Background = Brushes.LightBlue;
        }
        Thread scroll_thread;
        private void btn_scroll_Click(object sender, MouseButtonEventArgs e)
        {
            Clear_Scroll();
            if (images_box.Items.Count < 1) return;
           
            Border bord = sender as Border;
            bord.Tag = "Check";
            if(bord.Name == "right_scroll") scroll_direction = -1;
            else if(bord.Name == "left_scroll") scroll_direction = 1;
            Console.WriteLine(scroll_direction);
            auto_scroll_enabled = true;
            curIndex = images_box.SelectedIndex;

            scroll_thread = new Thread(Img_Timer_Change);

            scroll_thread.Start();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Console.WriteLine(scroll_thread.ThreadState);
        }

        private void btn_scroll_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Border bord = sender as Border;
            if (bord.Tag.ToString() == "Check") return;
            bord.Background = Brushes.Transparent;
        }
    }
}
