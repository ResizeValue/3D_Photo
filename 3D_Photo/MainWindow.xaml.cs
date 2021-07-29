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
        double sens = 50;
        public MainWindow()
        {
            InitializeComponent();
            
        }

        void OpenFolder()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (Fill_ListBox(dialog.SelectedPath))
                {
                    empty_model_btn.Visibility = Visibility.Hidden;
                    no_active_model_tb.Visibility = Visibility.Hidden;
                    no_photo.Visibility = Visibility.Hidden;
                }
                else
                {
                    MessageBox.Show("По данному пути не найдено нужных файлов!");
                    empty_model_btn.Visibility = Visibility.Visible;
                    no_active_model_tb.Visibility = Visibility.Visible;
                    no_photo.Visibility = Visibility.Visible;
                }
            }
        }

        bool Fill_ListBox(string path)
        {
            images.Clear();
            try
            {
                DirectoryInfo info = new DirectoryInfo(path);

                foreach (var file in info.GetFiles())
                {
                    if (file.Name[0] == '0' && file.Name[1] == '_')
                    {
                        string[] tmp = file.Name.Split(new Char[] { '_', '.' });
                        images.Add(new Photo_() { Name = file.Name, Path = file.FullName, Position = Int32.Parse(tmp[1]) });
                    }

                }
            }
            catch
            {
            }
            images_box.ItemsSource = null;
            img.Source = null;
            if (images.Count < 1) return false;

            
            images.Sort(delegate (Photo_ photo1, Photo_ photo2) { return photo1.Position.CompareTo(photo2.Position); });
            images_box.ItemsSource = images;
            images_box.SelectedIndex = 0;
            return true;
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
                if(point.X + (100/sens) <= e.GetPosition(img).X)
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
                else if (point.X - (100 / sens) >= e.GetPosition(img).X)
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
                if (images_box.SelectedIndex == images_box.Items.Count-1) { 
                    images_box.SelectedIndex = 0;
                }
                else if (images_box.SelectedIndex == -1) {
                    images_box.SelectedIndex = images_box.Items.Count - 1;
                }
                img.Source = new BitmapImage(new Uri(((Photo_)images_box.SelectedItem).Path));
            }
        }

        void Img_Timer_Change()
        {
            while (auto_scroll_enabled)
            {
                Print_Image(0);
                Thread.Sleep(40);
            }
           
        }

        private void auto_scroll_Disabled()
        {
            try
            {
                auto_scroll_enabled = false;
                if (scroll_thread != null)
                {
                    Console.WriteLine("Abort");
                    scroll_thread.Abort();
                    scroll_thread = null;
                }
            }
            catch { }
        }

        private void export_HTML_Click(object sender, RoutedEventArgs e)
        {
            if (images.Count < 1)
            {
                MessageBox.Show("Модель не загружена");
                return;
            }
            try
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.Filter = "HTML files (*.html)|*.html";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ExportToHTML(dialog.FileName);
                }
            }
            catch {
                MessageBox.Show("Ошибка при сохранении файла! Проверьте количество свободного места на диске.");
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
            left_scroll.Tag = "Unchecked";
            left_scroll.BorderThickness = new Thickness(1);
            right_scroll.Background = Brushes.Transparent;
            right_scroll.Tag = "Unchecked";
            right_scroll.BorderThickness = new Thickness(1);
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
            try
            {
                Clear_Scroll();
                if (images_box.Items.Count < 1) return;

                Border bord = sender as Border;
                bord.Tag = "Checked";
                bord.Background = Brushes.LightBlue;
                bord.BorderThickness = new Thickness(2);
                if (bord.Name == "right_scroll") scroll_direction = -1;
                else if (bord.Name == "left_scroll") scroll_direction = 1;
                auto_scroll_enabled = true;

                if (scroll_thread != null)
                    Console.WriteLine("ST:" + scroll_thread.ManagedThreadId);

                scroll_thread = new Thread(Img_Timer_Change);
                scroll_thread.Start();
            }
            catch { }
        }

        private void btn_scroll_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Border bord = sender as Border;
            if (bord.Tag.ToString() == "Checked") return;
            bord.Background = Brushes.Transparent;
        }
        private void sens_bar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            sens = sens_bar.Value;
            sens_tb.Text = sens.ToString("0.0");
        }
    }
}
