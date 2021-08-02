using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Photo
{
    class Photo_
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public int Position { get; set; }
    }

    public class Settings
    {
        public double Sensitivity { get; set; }
        public bool Watermark { get; set; }

        public void Save_Settings()
        {
            string jsonString = JsonConvert.SerializeObject(this);
            File.WriteAllText(Environment.CurrentDirectory + "\\Settings.json", jsonString);
        }
        public void Read_Settings()
        {
            if (!File.Exists(Environment.CurrentDirectory + "\\Settings.json")) Set_Default_Settings();
            else
            {
                var str = File.ReadAllText(Environment.CurrentDirectory + "\\Settings.json");
                Settings set = JsonConvert.DeserializeObject<Settings>(str);
                if (set == null)
                    Set_Default_Settings();
                else
                {
                    Sensitivity = set.Sensitivity;
                    Watermark = set.Watermark;
                }
            }
        }
        public void Set_Default_Settings()
        {
            Sensitivity = 50.0;
            Watermark = false;
        }
    }
}
