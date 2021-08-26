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

        public string script;

        public void Generate_Script(Script_Settings settings)
        {

            string arrow = "";
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("_3D_Photo.Arrow_Transp_100.png"))
            {
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                arrow = Convert.ToBase64String(buffer);
            }

            
            script = $@"<head>
                            <meta charset=""UTF-8"">
                            <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
                            <meta name = ""viewport"" content=""width=device-width, initial-scale=1.0"">
                            <title>Wolf Griman 3Dex</title>

                            <script>
                                
                                let photos = [{settings.Photos_String}];

                                let x = 0;
                                
                                let cur_arrow = ""none"";

                                var sens = {settings.Sens * 2};

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
                                    document.getElementById('img_rotate').style.cursor = ""grabbing"";
                                    point = [event.pageX, event.pageY];
                                    }}
                                function imgMove() {{
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
                                    document.getElementById('img_rotate').style.cursor = ""grab"";
                                    event.stopPropagation();
                                }}

                                function stop_Auto_Scroll(){{
                                    autoScrollDirection = 0;
                                    IsScroll = false;
                                    cur_arrow = 'none';
                                    document.getElementById(""auto_text"").style.opacity = 0.5;
                                    document.getElementById(""left"").style.opacity = 0.5;
                                    document.getElementById(""right"").style.opacity = 0.5;
                                    clearInterval(auto_scroll_timer);
                                }}

                                function arrow_Click()
                                {{
                                    stop_Auto_Scroll();
                                    document.getElementById(""auto_text"").style.opacity = 1.0;
                                    if(event.currentTarget.id == ""left""){{
                                        autoScrollDirection = 1;
                                        cur_arrow = 'left';
                                        document.getElementById(""left"").style.opacity = 1.0;
                                    }}
                                    else{{
                                        autoScrollDirection = -1;
                                        cur_arrow = 'right';
                                        document.getElementById(""right"").style.opacity = 1.0;
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
                                    var el = document.getElementById(""img_rotate"");
                                    el.addEventListener(""touchstart"", touchstart, false);
                                    el.addEventListener(""touchcancel"", imgBtnUp, false);
                                    el.addEventListener(""touchmove"", touchmove, false);
                                }}

                                let is_touched = false
                                function touchstart(){{
                                    event.preventDefault();
                                    if(IsScroll)
                                    stop_Auto_Scroll();
                                    var str = event.changedTouches;
                                    var touch = str[str.length-1];
                                    point = [touch.pageX,touch.pageY];
                                    is_touched = true;
                                }}

                                function touchmove(){{
                                    event.preventDefault();
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
                                    event.preventDefault();
                                    is_touched = false;
                                }}

                                function arrow_mouseenter(arrow){{
                                    if(arrow == ""left"") document.getElementById(""left"").style.opacity = 1.0;
                                    if(arrow == ""right"") document.getElementById(""right"").style.opacity = 1.0;
                                }}

                                function arrow_mouseleave(arrow){{
                                    if(arrow == ""left"" && cur_arrow != 'left') document.getElementById(""left"").style.opacity = 0.5;
                                    if(arrow == ""right""&& cur_arrow != 'right') document.getElementById(""right"").style.opacity = 0.5;
                                }}
                            </script>

                            <style>
                                .container{{
                                    display: flex;

                                    justify-content: center;
                                }}

                                main{{
                                    height: max-content;
                                    padding-right: 10px;
                                }}

                                body{{
                                    display: flex;
                                    align-items: center;
                                    justify-content: center;
                                }}

                                .auto_container{{
                                    display: flex;
                                    flex-direction: column;
                                    justify-items: center;
                                    margin-left: -155px;
                                    z-index: 2;
                                    align-self: flex-end;
                                    background-color: {(settings.Auto_Background == true ? "rgba(126, 126, 126, 0.2)" : "transparent")};
                                    padding: 5px;
                                    margin-bottom: 10px;
                                    border-radius: 7px;
                                }}
                                
                                #auto_text{{
                                    cursor:default;
                                    text-align: center;
                                    font-size: 20px;
                                    font-weight: 600;
                                    color: white;
                                    opacity: 0.5;
                                    font-family:Arial, Helvetica, sans-serif;
                                    -webkit-text-stroke: 1.0px gray;
                                }}

                                @media screen and (max-width: 800px) {{
                                    #main_img{{
                                        width: 100%;
                                        height: auto;
                                    }}
                                    body{{
                                        margin-left: 0px !important;
                                        padding: 0px !important;
                                    }}
                                }}

                                .arrow_container{{
                                    display: flex;
                                    flex-direction: row;
                                    justify-content: center;
                                    
                                    background-color: transparent;
                                }}
                                .arrow{{
                                    width: 35;
                                    height: 20;
                                    padding-left: 20px;
                                    margin-top: 10px;
                                    margin-bottom: 5px;
                                }}
                                .arrow_back{{
                                    transform: rotate(180deg);
                                }}

                                #img_rotate{{
                                    width: 100%;
                                    height: 100%;
                                    position: absolute;
                                    left: 0;
                                    top: 0;
                                    z-index: 2;
                                    background: transparent;
                                }}
                                
                            </style>
                        </head>
                        <body onload=""touch_init()"">
                            <main>
                                <div id=""img_rotate"" ondragstart=""stopDrag()"" onmouseout=""imgBtnUp()"" onmousedown=""imgBtnDown()""
                                    onmouseup = ""imgBtnUp()"" onmousemove = ""imgMove()"" style = ""cursor: grab;""></div>
                                <div class=""container"">
                                    <img id=""main_img"" src=""data:image/jpg;base64,{settings.First_Photo}"" >
                                <div class=""auto_container"">
                                        <div id =""auto_text"" >AUTOROTATE</div>
 
                                         <div class=""arrow_container"" >
  
                                              <div id = ""left"" class=""arrow_container"" onmouseenter=""arrow_mouseenter('left')""
                                                onmouseleave=""arrow_mouseleave('left')"" onclick=""arrow_Click()"" style=""opacity:0.5"">
                                                <img class=""arrow arrow_back"" src=""data:image/png;base64,{arrow}"">
                                            </div>
                                            <div id = ""right"" class=""arrow_container"" onmouseenter=""arrow_mouseenter('right')""
                                                onmouseleave=""arrow_mouseleave('right')"" onclick=""arrow_Click()"" style=""opacity:0.5"">
                                                <img class=""arrow"" src=""data:image/png;base64,{arrow}"">
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </main>
                        </body>";
        }

    }
    public class Script_Settings
    {
        public string Photos_String { get; set; }
        public bool Auto_Background { get; set; }
        public string First_Photo { get; set; }
        public double Sens { get; set; }
    }
}
