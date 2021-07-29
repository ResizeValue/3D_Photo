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
            string arrow = "";
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("_3D_Photo.left_arr.png"))
            {
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                arrow = Convert.ToBase64String(buffer);
            }

            photos_string = photos_string.Remove(photos_string.Length - 1, 1);
            script = $@"<head>
                            <meta charset=""UTF-8"">
                            <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
                            <meta name = ""viewport"" content=""width=device-width, initial-scale=1.0"">
                            <title>Wolf Griman 3Dex</title>

                            <script>
                                
                                let photos = [{photos_string}];

                                let x = 0;

                                var sens = {sens};

                                let IsScroll = false;
                                var autoScrollDirection = 0;
                                let auto_scroll_timer;

                                let point = null;
                                function stopDrag(){{
                                    event.preventDefault();
                                }}
                                var click = false;
                                function imgBtnDown(){{
                                    if(IsScroll)
                                    stop_Auto_Scroll();
                                    click = true;
                                    document.getElementById('main_img').style.cursor = ""grabbing"";
                                    point = [event.pageX, event.pageY];
                                    }}
                                    function imgMove()
                                    {{
                                        if (!click) return;

                                        if (point[0] - event.pageX > (100/sens)){{
                                        x++;
                                        if (x == photos.length) x = 0;
                                        changeImg();
                                    }}
                                    if(point[0] - event.pageX < -(100/sens)){{
                                        x--;
                                        if (x == -1) x = photos.length - 1;
                                        changeImg();
                                    }}
                                    event.stopPropagation();
                                }}

                                function changeImg()
                                {{
                                    document.getElementById('main_img').setAttribute('src', photos[x]);
                                    if(autoScrollDirection == 0){{
                                        if(is_touched){{
                                            var str = event.changedTouches;
                                            var touch = str[str.length-1];
                                            point = [touch.pageX, touch.pageY];
                                        }}
                                        else
                                            point = [event.pageX, event.pageY];
                                    }}
                                }}

                                function imgBtnUp()
                                {{
                                    click = false;
                                    document.getElementById('main_img').style.cursor = ""grab"";
                                    event.stopPropagation();
                                }}

                                function stop_Auto_Scroll(){{
                                    autoScrollDirection = 0;
                                    IsScroll = false;
                                    document.getElementById(""auto_text"").style.color = ""black"";
                                    clearInterval(auto_scroll_timer);
                                }}

                                function arrow_Click()
                                {{
                                    stop_Auto_Scroll();
                                    document.getElementById(""auto_text"").style.color = ""blue"";
                                    if(event.currentTarget.id == ""left""){{
                                        autoScrollDirection = 1;
                                    }}
                                    else{{
                                        autoScrollDirection = -1;
                                    }}
                                    IsScroll = true;
                                    auto_scroll_timer = setInterval(()=>{{
                                        x += autoScrollDirection;
                                        if (x == photos.length) x = 0;
                                        else if (x == -1) x = photos.length - 1;
                                        changeImg();
                                    }},40)
                                }}

                                function touch_init(){{
                                    var el = document.getElementById(""main_img"");
                                    el.addEventListener(""touchstart"", touchstart, false);
                                    el.addEventListener(""touchcancel"", imgBtnUp, false);
                                    el.addEventListener(""touchmove"", touchmove, false);
                                }}

                                let is_touched = false
                                function touchstart(){{
                                    var str = event.changedTouches;
                                    var touch = str[str.length-1];
                                    point = [touch.pageX,touch.pageY];
                                    is_touched = true;
                                }}

                                function touchmove(){{
                                    var str = event.changedTouches;
                                    var touch = str[str.length-1];
                                    if (point[0] - touch.pageX > (100/sens)){{
                                        x++;
                                        if (x == photos.length) x = 0;
                                        changeImg();
                                    }}
                                    if(point[0] - touch.pageX < -(100/sens)){{
                                        x--;
                                        if (x == -1) x = photos.length - 1;
                                        changeImg();
                                    }}
                                    
                                }}

                                function touchcancel(){{
                                    is_touched = false;
                                }}
                            </script>

                            <style>
                                .container{{
                                    display: flex;

                                    justify-content: center;
                                }}

                                .auto_container{{
                                    display: flex;
                                    flex-direction: column;
                                    justify-items: center;
                                    margin-left: -160px;
                                    z-index: 2;
                                    align-self: flex-end;
                                    background-color: rgba(159, 89, 206, 0.199);
                                    margin-bottom: 10px;
                                    border-radius: 25%;
                                }}
                                
                                #auto_text{{
                                    cursor:default;
                                    text-align: center;
                                    font-size: 40px;
                                    font-weight: 900;
                                }}

                                @media screen and (max-width: 600px) {{
                                    #main_img{{
                                        width: 475px;
                                    }}
                                }}
                                @media screen and (max-width: 400px) {{
                                    #main_img{{
                                        width: 100%;
                                    }}
                                }}
                                @media screen and (max-height: 800px) {{
                                    #main_img{{
                                        height: 600px;
                                    }}
                                }}

                                .arrow_container{{
                                    background-color: transparent;
                                    border-radius: 25%;
                                }}
                                .arrow{{
                                    width: 75;
                                    height: 40;
                                }}
                                .arrow_back{{
                                    transform: rotate(180deg);
                                }}

                                .arrow_container:hover{{
                                    border: 1px solid black;
                                }}
                            </style>
                        </head>
                        <body onload=""touch_init()"">
                            <div class=""container"">
                                <img id=""main_img"" style=""cursor: grab; "" ondragstart=""stopDrag()"" onmouseout=""imgBtnUp()"" onmousedown=""imgBtnDown()""
                                onmouseup = ""imgBtnUp()"" onmousemove = ""imgMove()"" src=""data:image/jpg;base64,{Convert.ToBase64String(File.ReadAllBytes(photos[0].Path))}"" >
                            <div class=""auto_container"">
                                    <div id =""auto_text"" >Auto</div>
 
                                     <div style = ""display: flex; flex-direction: row;"">
  
                                          <div id = ""left"" class=""arrow_container"" onclick=""arrow_Click()"">
                                            <img class=""arrow"" src=""data:image/png;base64,{arrow}"">
                                        </div>
                                        <div id = ""right"" class=""arrow_container"" onclick=""arrow_Click()"">
                                            <img class=""arrow arrow_back"" src=""data:image/png;base64,{arrow}"">
                                        </div>
                                    </div>
                                </div>
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
