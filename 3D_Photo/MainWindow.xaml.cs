using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Forms;
using System.IO;

using System.Threading;
using MessageBox = System.Windows.MessageBox;
using System.Text.RegularExpressions;
using System.Windows.Interop;

namespace _3D_Photo
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        List<Photo_> images = new List<Photo_>();
        Settings settings = new Settings();
        
        int scroll_direction = 0;
        bool auto_scroll_enabled = false;

        string logo_path = Environment.CurrentDirectory + "\\Resources\\Logo\\logo.png";
        public MainWindow()
        {
            InitializeComponent();
            Load_Settings();
        }

        void Load_Settings()
        {
            settings.Read_Settings();
            sens_bar.Value = settings.Sensitivity;
            watermark_checkbox.IsChecked = settings.Watermark;
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
        
        BitmapImage Add_Watermark(string path)
        {
            System.Drawing.Image image = (System.Drawing.Image)System.Drawing.Bitmap.FromFile(path);

            System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(image);
            graphics.DrawImage(logo, 20f, image.Height - logo.Height - 20, logo.Width, logo.Height);
            graphics.Dispose();


            MemoryStream m = new MemoryStream();
            image.Save(m, System.Drawing.Imaging.ImageFormat.Jpeg);
            byte[] arr = m.ToArray();
            m.Close();
            MemoryStream stream = new MemoryStream(arr);

            BitmapImage bimage = new BitmapImage();
            bimage.BeginInit();
            bimage.StreamSource = stream;
            bimage.EndInit();

            return bimage;
        }

        byte[] HTMLBitmapArray(string path)
        {
            System.Drawing.Image image = (System.Drawing.Image)System.Drawing.Bitmap.FromFile(path);

            if (settings.Watermark && logo != null)
            {
                System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(image);
                graphics.DrawImage(logo, 20f, image.Height - logo.Height - 20, logo.Width, logo.Height);
                graphics.Dispose();
            }
            else
            {
                System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(image);
                graphics.DrawString("A", new System.Drawing.Font("Arial",1), System.Drawing.Brushes.Transparent,0f,0f);
                graphics.Dispose();
            }
            MemoryStream m = new MemoryStream();
            image.Save(m, System.Drawing.Imaging.ImageFormat.Jpeg);
            return m.ToArray();
        }

        void SaveBitmap(string path, string topath)
        {
            System.Drawing.Image image = (System.Drawing.Image)System.Drawing.Bitmap.FromFile(path);

            if(settings.Watermark && logo != null)
            {
                System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(image);
                graphics.DrawImage(logo, 20f, image.Height - logo.Height - 20, logo.Width, logo.Height);
                graphics.Dispose();
            }
            else
            {
                System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(image);
                graphics.DrawImage(image, 0f,0f,0f,0f);
                graphics.Dispose();
            }

            image.Save(topath);
        }

        bool Fill_ListBox(string path)
        {
            images.Clear();
            try
            {
                DirectoryInfo info = new DirectoryInfo(path);
                string pattern = "([0-9]+)$";
                foreach (var file in info.GetFiles())
                {

                    string[] tmp = file.Name.Split(new Char[] { '.' });

                    if (file.Extension != ".jpg" && file.Extension != ".png" && file.Extension != ".jpeg" && file.Extension != ".bmp" && file.Extension != ".gif"
                        && file.Extension != ".tiff" && file.Extension != ".jpeg 2000" && file.Extension != ".heic" && file.Extension != ".tif" && file.Extension != ".jpe"
                        && file.Extension != ".jfif" && file.Extension != ".dib")
                        continue;

                    if (Regex.IsMatch(tmp[tmp.Length-2], pattern,RegexOptions.IgnoreCase))
                    {
                        Regex regex = new Regex(pattern);
                        MatchCollection matches = regex.Matches(tmp[tmp.Length - 2]);
                        images.Add(new Photo_() { Name = file.Name, Path = file.FullName, Position = Int32.Parse(matches[matches.Count-1].Value) });
                    }

                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            images_box.ItemsSource = null;
            img.Source = null;
            if (images.Count < 1) return false;

            
            images.Sort(delegate (Photo_ photo1, Photo_ photo2) { return photo1.Position.CompareTo(photo2.Position); });
            images_box.ItemsSource = images;
            images_box.SelectedIndex = 0;
            return true;
        }

        void Save_LocalModel()
        {
            try
            {
                prog_bar.Maximum = images.Count;
                prog_bar.Value = 0;
                string cur_dir = Environment.CurrentDirectory;
                if (!Directory.Exists(cur_dir + "\\Model")) Directory.CreateDirectory(cur_dir + "\\Model");
                new Thread(() =>
                {
                    DirectoryInfo info = new DirectoryInfo(cur_dir + "\\Model");
                    foreach (var file in info.GetFiles())
                    {
                        if (file.Name.StartsWith("loc_"))
                            file.Delete();
                    }

                    foreach (var image in images)
                    {
                        SaveBitmap(image.Path, cur_dir + "\\Model\\loc_" + image.Name);
                        SetPorgressBar(1);
                    }
                })
                { IsBackground = true }.Start();
            }
            catch
            {
                Set_Status("Failed");
            }
        }

        private void SetPorgressBar(int val)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke((Action)(() => SetPorgressBar(val)));
            }
            else
            {
                try
                {
                    prog_bar.Value += val;
                    if (prog_bar.Value == prog_bar.Maximum)
                        prog_bar.Value = 0;
                }
                catch (OperationCanceledException) { }
            }
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            OpenFolder();
        }

        private void ListBoxItem_Selected(object sender, RoutedEventArgs e)
        {
            if (auto_scroll_enabled) return;
            Set_Current_Photo();
            ((ListBoxItem)sender).Background = Brushes.BlueViolet;
            
        }

        void Set_Current_Photo()
        {
            try
            {
                if (images_box.SelectedIndex == -1) return;
                if (settings.Watermark && logo != null)
                    img.Source = Add_Watermark(((Photo_)images_box.SelectedItem).Path);
                else
                    img.Source = new BitmapImage(new Uri(((Photo_)images_box.SelectedItem).Path));
            }
            catch { }
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
                if(point.X + (100/ settings.Sensitivity) <= e.GetPosition(img).X)
                {
                    if(images_box.SelectedIndex == 0)
                    {
                        images_box.SelectedIndex = images_box.Items.Count - 1;
                    }
                    else
                    {
                        images_box.SelectedIndex -= 1;
                    }
                    
                    point = e.GetPosition(img);
                }
                else if (point.X - (100 / settings.Sensitivity) >= e.GetPosition(img).X)
                {
                    if (images_box.SelectedIndex == images_box.Items.Count - 1)
                    {
                        images_box.SelectedIndex = 0;
                    }
                    else
                    {
                        images_box.SelectedIndex += 1;
                    }
                    point = e.GetPosition(img);
                }
                Set_Current_Photo();
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
                Set_Current_Photo();
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
            prog_bar.Maximum = images.Count + 6;
            new Thread(() =>
            {
                try
                {
                    Set_Status("Exporting...");
                    string photos_string = "";
                    foreach (var photo in images)
                    {
                        photos_string += $"\"data:image/jpg;base64,{Convert.ToBase64String(HTMLBitmapArray(photo.Path))}\",";
                        SetPorgressBar(1);
                    }
                    photos_string = photos_string.Remove(photos_string.Length - 1, 1);
                    SetPorgressBar(1);

                    Script_Settings script_settings = new Script_Settings()
                    {
                        Photos_String = photos_string,
                        First_Photo = Convert.ToBase64String(HTMLBitmapArray(images[0].Path)),
                        Sens = settings.Sensitivity
                    };
                    HTML_Program program = new HTML_Program();
                    program.Generate_Script(script_settings);

                    File.WriteAllText(path, program.script);
                    SetPorgressBar(5);
                    Set_Status("Success");
                }
                catch
                {
                    
                    Set_Status("Failed");
                }
            }).Start();
        }

        void Set_Status(string status)
        {
            if (!Dispatcher.CheckAccess())
                Dispatcher.Invoke((Action)(() => Set_Status(status)));
            else
            {
                try
                {
                    status_tb.Text = status;
                }
                catch (OperationCanceledException) { }
            }
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
            settings.Sensitivity = sens_bar.Value;
            sens_tb.Text = settings.Sensitivity.ToString("0.0");
        }

        private void local_model_save_Click(object sender, RoutedEventArgs e)
        {
            Save_LocalModel();
        }
        void Copy_Logo(string path)
        {
            var curdir = Environment.CurrentDirectory;
            if (!Directory.Exists(curdir + "\\Resources")) Directory.CreateDirectory(curdir + "\\Resources");
            if (!Directory.Exists(curdir + "\\Resources\\Logo")) Directory.CreateDirectory(curdir + "\\Resources\\Logo");

            var arr = File.ReadAllBytes(path);
            File.Delete(logo_path);
            File.WriteAllBytes(curdir + "\\Resources\\Logo\\logo.png", arr);

        }
        void Read_Logo()
        {
            try
            {
                logo_img.Source = new BitmapImage(new Uri(logo_path));
                noimg_tb.Visibility = Visibility.Hidden;
                watermark_checkbox.IsChecked = true;
            }
            catch { }
        }
        void Clear_logo()
        {
            noimg_tb.Visibility = Visibility.Visible;
            logo = null;
            logo_img.Source = null;
        }
        private void chg_logo_btn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                logo = null;
                logo_img.Source = null;
                dialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    FileInfo info = new FileInfo(dialog.FileName);

                    Clear_logo();

                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    Copy_Logo(info.FullName);
                    logo = (System.Drawing.Image)System.Drawing.Bitmap.FromFile(logo_path);
                    Read_Logo();
                }
                Set_Current_Photo();
            }
            catch
            {
                MessageBox.Show("Ну удалось загрузить изображение!");
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            settings.Save_Settings();
        }
        System.Drawing.Image logo;
        private void watermark_checkbox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                settings.Watermark = true;
                logo_img.Opacity = 1;
                logo = (System.Drawing.Image)System.Drawing.Bitmap.FromFile(logo_path);
                Read_Logo();
            }
            catch
            {
               
            }
            try
            {
                Set_Current_Photo();
            }
            catch { }
        }

        private void watermark_checkbox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                settings.Watermark = false;
                logo_img.Opacity = 0.7;
                Set_Current_Photo();
            }
            catch { }
        }
    }
}
