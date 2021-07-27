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

namespace _3D_Photo
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Photo_> images = new List<Photo_>();
        DispatcherTimer scroll_timer;
        int scroll_direction = 1;
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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenFolder();
        }

        private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {
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

        private void auto_scroll_Checked(object sender, RoutedEventArgs e)
        {
            scroll_timer = new DispatcherTimer();
            scroll_timer.Interval = TimeSpan.FromSeconds(0.04);
            scroll_timer.Tick += Scroll_timer_Tick;
            scroll_timer.Start();
        }

        private void Scroll_timer_Tick(object sender, EventArgs e)
        {
            if(images_box.Items.Count > 1)
            {
                if (images_box.SelectedIndex == images_box.Items.Count - 1) images_box.SelectedIndex = 0;
                images_box.SelectedIndex += scroll_direction;
                img.Source = new BitmapImage(new Uri(((Photo_)images_box.SelectedItem).Path));
            }
        }

        private void auto_scroll_Unchecked(object sender, RoutedEventArgs e)
        {
            scroll_timer.Stop();
        }

        private void export_HTML_Click(object sender, RoutedEventArgs e)
        {
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
            program.Write_To_File(path, images);
        }
    }
}
