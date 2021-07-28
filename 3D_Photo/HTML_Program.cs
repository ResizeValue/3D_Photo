using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3D_Photo
{
    class HTML_Program
    {

        string script;

        void Generate_Script(List<Photo_> photos, double sens)
        {
            string photos_string = "";
            foreach (var photo in photos)
            {
                photos_string += $"\"data:image/jpg;base64,{Convert.ToBase64String(File.ReadAllBytes(photo.Path))}\",";
            }
            photos_string = photos_string.Remove(photos_string.Length - 1, 1);
            script = $@"<head>
                            <meta charset=""UTF-8"">
                            <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
                            <meta name = ""viewport"" content=""width=device-width, initial-scale=1.0"">
                            <title>Wolf Griman 3Dex</title>

                            <script>
                                //data:image/jpg;base64,
                                let photos = [{photos_string}];

                                let x = 0;

                                var sens = {sens};

                                let point = null;
                                function stopDrag(){{
                                    event.preventDefault();
                                }}
                                var click = false;
                                function imgBtnDown(){{
                                    click = true;
                                    document.getElementById('main_img').style.cursor = ""grabbing"";
                                    point = [event.pageX, event.pageY];
                                    }}
                                    function imgMove()
                                    {{
                                        if (!click) return;

                                        if (point[0] - event.pageX > sens){{
                                        x++;
                                        if (x == photos.length) x = 0;
                                        changeImg();
                                    }}
                                    if(point[0] - event.pageX < -sens){{
                                        x--;
                                        if (x == -1) x = photos.length - 1;
                                        changeImg();
                                    }}
                                    event.stopPropagation();
                                }}

                                function changeImg()
                                {{
                                    document.getElementById('main_img').setAttribute('src', photos[x]);
                                    console.log(x);
                                    point = [event.pageX, event.pageY];
                                }}

                                function imgLoad(){{
                                            changeImg();
                                            console.log(""Loaded"")
                                        }}

                                function imgBtnUp()
                                {{
                                    click = false;
                                    document.getElementById('main_img').style.cursor = ""grab"";
                                    event.stopPropagation();
                                }}
                            </script>

                            <style>
                                .container{{
                                    display: flex;
                                    margin-left: auto;
                                    margin-right: auto;
                                }}
                            </style>
                        </head>
                        <body>
                            <div>
                                <img id=""main_img"" style=""cursor: grab; "" ondragstart=""stopDrag()"" onmouseout=""imgBtnUp()"" onmousedown=""imgBtnDown()""
                                onmouseup = ""imgBtnUp()"" onmousemove = ""imgMove()"" class=""container"" src=""data:image/jpg;base64,{Convert.ToBase64String(File.ReadAllBytes(photos[0].Path))}"" >
                            </div>
                        </body>";
        }

        public void Write_To_File(string path, List<Photo_> photos, double sens)
        {
            Generate_Script(photos, sens);
            File.WriteAllText(path, script);
        }
    }
}
