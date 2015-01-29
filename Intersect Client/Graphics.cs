﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML;
using SFML.Graphics;
using SFML.Window;
using System.IO;
using BitmapProcessing;


namespace Intersect_Client
{
    public static class Graphics
    {
        public static RenderWindow renderWindow;

        //Screen Values
        public static int ScreenWidth = 0;
        public static int ScreenHeight = 0;
        public static int DisplayMode = 0;
        public static bool FullScreen = false;
        public static bool MustReInit = false;
        public static int fadeStage = 1;
        public static float fadeAmt = 255f;
        public static frmGame myForm;
        public static List<Keyboard.Key> keyStates = new List<Keyboard.Key>();
        public static List<Mouse.Button> mouseState = new List<Mouse.Button>();
        public static SFML.Graphics.Font GameFont;
        public static int FPS = 0;
        private static int fpsCount = 0;
        private static long fpsTimer = 0;

        //Game Textures
        public static Texture menuBG;
        public static List<Texture> tilesets = new List<Texture>();
        public static List<Texture> entities = new List<Texture>();
        public static List<string> entityNames = new List<string>();

        //DayNight Stuff
        public static bool lightsChanged = true;
        public static SFML.Graphics.Color[,] nightColorArray;
        public static SFML.Graphics.Color[,] bnightColorArray;
        public static Texture nightTex;

        private static long fadeTimer = 0;



        public static void InitGraphics()
        {
            InitSFML();
            LoadEntities();
            GameFont = new SFML.Graphics.Font("Arvo-Regular.ttf");

            //Load menu bg
            if (File.Exists("data\\graphics\\bg.png")){
                //menuBG = new Texture("data\\graphics\\bg.png");
            }
        }

        private static void InitSFML()
        {
            if (DisplayMode < 0 || DisplayMode >= GetValidVideoModes().Count) { DisplayMode = 0; }
            myForm = new frmGame();
            if (GetValidVideoModes().Count() > 0)
            {
                myForm.Width = (int)GetValidVideoModes()[DisplayMode].Width;
                myForm.Height = (int)GetValidVideoModes()[DisplayMode].Height;
                myForm.Text = "Intersect Client";
                renderWindow = new RenderWindow(myForm.Handle);
                if (FullScreen)
                {
                    myForm.TopMost = true;
                    myForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                    myForm.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                    renderWindow.SetView(new View(new FloatRect(0, 0, (int)GetValidVideoModes()[DisplayMode].Width, (int)GetValidVideoModes()[DisplayMode].Height)));
                }
                else
                {
                    renderWindow.SetView(new View(new FloatRect(0, 0, myForm.ClientSize.Width, myForm.ClientSize.Height)));
                }
                
            }
            else
            {
                myForm.Width = 800;
                myForm.Height = 640;
                myForm.Text = "Intersect Client";
                renderWindow = new RenderWindow(myForm.Handle);
                renderWindow.SetView(new View(new FloatRect(0, 0, myForm.ClientSize.Width, myForm.ClientSize.Height)));
            }
            if (FullScreen)
            {
                ScreenWidth = (int)GetValidVideoModes()[DisplayMode].Width;
                ScreenHeight = (int)GetValidVideoModes()[DisplayMode].Height;
            }
            else
            {
                ScreenWidth = myForm.ClientSize.Width;
                ScreenHeight = myForm.ClientSize.Height;
            }
            renderWindow.KeyPressed += renderWindow_KeyPressed;
            renderWindow.KeyReleased += renderWindow_KeyReleased;
            renderWindow.MouseButtonPressed += renderWindow_MouseButtonPressed;
            renderWindow.MouseButtonReleased += renderWindow_MouseButtonReleased;
            GUI.InitGwen();
        }

        static void renderWindow_MouseButtonReleased(object sender, MouseButtonEventArgs e)
        {
            while (mouseState.Remove(e.Button)) { }
        }

        static void renderWindow_MouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            mouseState.Add(e.Button);
        }

        static void renderWindow_KeyReleased(object sender, KeyEventArgs e)
        {
            while (keyStates.Remove(e.Code)) { }
        }

        static void renderWindow_KeyPressed(object sender, KeyEventArgs e)
        {
            keyStates.Add(e.Code);
            if (e.Code == Keyboard.Key.Return)
            {
                if (Globals.GameState == 1)
                {
                    if (GUI.HasInputFocus() == false)
                    {
                        GUI.focusChat = true;
                    }
                }
            }
        }

        public static List<VideoMode> GetValidVideoModes()
        {
            List<VideoMode> myList = new List<VideoMode>();
            for (int i = 0; i < VideoMode.FullscreenModes.Length; i++)
            {
                if (VideoMode.FullscreenModes[i].BitsPerPixel == 32)
                {
                    myList.Add(VideoMode.FullscreenModes[i]);
                }
            }
            myList.Reverse();
            return myList;
        }

        public static void DrawGame()
        {
            RectangleShape myShape;
            if (MustReInit)
            {
                GUI.DestroyGwen();
                renderWindow.Close();
                myForm.Close();
                myForm.Dispose();
                GUI.setupHandlers = false;
                InitSFML();
                MustReInit = false;
            }
            if (renderWindow.IsOpen())
            {
                renderWindow.DispatchEvents();
                renderWindow.Clear(Color.Black);

                if (Globals.GameState == 1 && Globals.gameLoaded == true)
                {
                    if (lightsChanged) { GenerateLightTexture(); }
                    //Render players, names, maps, etc.
                    for (int i = 0; i < 9; i++)
                    {
                        if (Globals.localMaps[i] > -1)
                        {
                            DrawMap(i, false); //Lower only
                        }
                    }

                    for (int i = 0; i < 9; i++)
                    {
                        if (Globals.localMaps[i] > -1)
                        {
                            for (int y = 0; y < Constants.MAP_HEIGHT ; y++)
                            {
                                for (int z = 0; z < Globals.entities.Count; z++)
                                {
                                    if (Globals.entities[z] != null)
                                    {
                                        if (Globals.entities[z].currentMap == Globals.localMaps[i])
                                        {
                                            if (Globals.entities[z].currentY == y)
                                            {
                                                Globals.entities[z].Draw(i);
                                            }
                                        }
                                    }
                                }
                                for (int z = 0; z < Globals.events.Count; z++)
                                {
                                    if (Globals.events[z] != null)
                                    {
                                        if (Globals.events[z].currentMap == Globals.localMaps[i])
                                        {
                                            if (Globals.events[z].currentY == y)
                                            {
                                                Globals.events[z].Draw(i);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
                    for (int i = 0; i < 9; i++)
                    {
                        if (Globals.localMaps[i] > -1)
                        {

                            DrawMap(i, true); //Upper Layers

                            for (int y = 0; y < Constants.MAP_HEIGHT; y++)
                            {
                                for (int z = 0; z < Globals.entities.Count; z++)
                                {
                                    if (Globals.entities[z] != null)
                                    {
                                        if (Globals.entities[z].currentMap == Globals.localMaps[i])
                                        {
                                            if (Globals.entities[z].currentY == y)
                                            {
                                                Globals.entities[z].DrawName(i,false);
                                                Globals.entities[z].DrawHPBar(i);
                                            }
                                        }
                                    }
                                }
                                for (int z = 0; z < Globals.events.Count; z++)
                                {
                                    if (Globals.events[z] != null)
                                    {
                                        if (Globals.events[z].currentMap == Globals.localMaps[i])
                                        {
                                            if (Globals.events[z].currentY == y)
                                            {
                                                Globals.events[z].DrawName(i,true);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    DrawNight();
                }
                else {
                    if (menuBG != null) {
                        Sprite tmpSprite = new Sprite(menuBG);
                        tmpSprite.Position = new Vector2f(0,0);
                        float scaleX = 1;
                        float scaleY = 1;
                        if (ScreenWidth > menuBG.Size.X) { scaleX = (float)ScreenWidth / menuBG.Size.X; }
                        if (ScreenHeight > menuBG.Size.Y) { scaleY = (float)ScreenHeight / menuBG.Size.Y; }
                        tmpSprite.Scale = new Vector2f(scaleX, scaleY);
                        renderWindow.Draw(tmpSprite);
                    }
                }
                
                GUI.DrawGUI();

                
                if (fadeStage != 0)
                {
                    if (fadeTimer < Environment.TickCount)
                    {
                        if (fadeStage == 1)
                        {
                            fadeAmt -= 2f;
                            if (fadeAmt <= 0)
                            {
                                fadeStage = 0;
                                fadeAmt = 0f;
                            }
                        }
                        else
                        {
                            fadeAmt += 2f;
                            if (fadeAmt >= 255)
                            {
                                fadeAmt = 255f;
                            }
                        }
                        fadeTimer = Environment.TickCount + 10;
                    }
                    myShape = new RectangleShape(new Vector2f(ScreenWidth, ScreenHeight));
                    myShape.FillColor = new SFML.Graphics.Color(0, 0, 0, (byte)fadeAmt);
                    renderWindow.Draw(myShape);
                }

                renderWindow.Display();
                fpsCount++;
                if (fpsTimer < Environment.TickCount)
                {
                    FPS = fpsCount;
                    fpsCount = 0;
                    fpsTimer = Environment.TickCount + 1000;
                    renderWindow.SetTitle("Intersect Engine - FPS: " + FPS);
                }
            }
        }

        private static void DrawMap(int index, bool upper = false)
        {
            int mapoffsetx = CalcMapOffsetX(index);
            int mapoffsety = CalcMapOffsetY(index);
            
            if (Globals.localMaps[index] <= Globals.GameMaps.Count() && Globals.localMaps[index] >= 0)
            {
                if (Globals.GameMaps[Globals.localMaps[index]] != null)
                {
                    if (Globals.GameMaps[Globals.localMaps[index]].mapLoaded == true)
                    {
                        Globals.GameMaps[Globals.localMaps[index]].Draw(mapoffsetx, mapoffsety, upper);
                    }
                }
            }
        }

        public static int CalcMapOffsetX(int i)
        {
            if (i < 3)
            {
                return ((-Constants.MAP_WIDTH * 32) + ((i) * (Constants.MAP_WIDTH * 32))) + (ScreenWidth / 2) - Globals.entities[Globals.myIndex].currentX * 32 - (int)Math.Ceiling(Globals.entities[Globals.myIndex].offsetX);
            }
            else if (i < 6)
            {
                return ((-Constants.MAP_WIDTH * 32) + ((i - 3) * (Constants.MAP_WIDTH * 32))) + (ScreenWidth/2) - Globals.entities[Globals.myIndex].currentX * 32 - (int)Math.Ceiling(Globals.entities[Globals.myIndex].offsetX);
            }
            else
            {
                return ((-Constants.MAP_WIDTH * 32) + ((i - 6) * (Constants.MAP_WIDTH * 32))) + (ScreenWidth / 2) - Globals.entities[Globals.myIndex].currentX * 32 - (int)Math.Ceiling(Globals.entities[Globals.myIndex].offsetX);
            }
        }

        public static int CalcMapOffsetY(int i)
        {
            if (i < 3)
            {

                return -Constants.MAP_HEIGHT * 32 + (ScreenHeight / 2) - Globals.entities[Globals.myIndex].currentY * 32 - (int)Math.Ceiling(Globals.entities[Globals.myIndex].offsetY);
            }
            else if (i < 6)
            {
                return 0 + (ScreenHeight / 2) - Globals.entities[Globals.myIndex].currentY * 32 - (int)Math.Ceiling(Globals.entities[Globals.myIndex].offsetY);
            }
            else
            {
                return Constants.MAP_HEIGHT * 32 + (ScreenHeight / 2) - Globals.entities[Globals.myIndex].currentY * 32 - (int)Math.Ceiling(Globals.entities[Globals.myIndex].offsetY) ;
            }
        }

        public static void LoadTilesets(string[] tilesetnames)
        {
            for (int i = 0; i < tilesetnames.Length; i++)
            {
                if (tilesetnames[i] == "")
                {
                    tilesets.Add(null);
                }
                else
                {
                    if (!System.IO.File.Exists("data/graphics/tilesets/" + tilesetnames[i]))
                    {
                        tilesets.Add(null);
                    }
                    else
                    {
                        tilesets.Add(new Texture("data/graphics/tilesets/" + tilesetnames[i]));                    
                    }
                }
            }
        }

        public static void LoadEntities()
        {
            string[] entityPaths = Directory.GetFiles("data/graphics/entities/","*.png");
            for (int i = 0; i < entityPaths.Length; i++)
            {
                entityNames.Add(entityPaths[i].Split('/')[(int)(entityPaths[i].Split('/').Count() - 1)].ToLower());
                entities.Add(new Texture(entityPaths[i]));
            }
            
        }

        //Lighting
        private static void GenerateLightTexture()
        {
            //If we don't have a light texture, make a base/blank one.
            if (bnightColorArray == null)
            {
                bnightColorArray = new SFML.Graphics.Color[30 * 32 * 3, 30 * 32 * 3];
                for (int x = 0; x < 30 * 32 * 3; x++)
                {
                    for (int y = 0; y < 30 * 32 * 3; y++)
                    {
                        bnightColorArray[x, y] = new SFML.Graphics.Color(0, 0, 0, 180);
                    }
                }
            }

            //Wipe our render array by cloning the blank one.
            nightColorArray = (SFML.Graphics.Color[,])bnightColorArray.Clone();
            double w = 1;
            //Render each light.
            for (int i = 0; i < Globals.GameMaps[Globals.currentMap].Lights.Count; i++)
            {
                w = CalcLightWidth(Globals.GameMaps[Globals.currentMap].Lights[i].range);
                int x = (30 * 32) + (Globals.GameMaps[Globals.currentMap].Lights[i].tileX * 32 + Globals.GameMaps[Globals.currentMap].Lights[i].offsetX) - (int)w / 2 + 32 + 16;
                int y = (int)(30 * 32) + (int)(Globals.GameMaps[Globals.currentMap].Lights[i].tileY * 32 + Globals.GameMaps[Globals.currentMap].Lights[i].offsetY) - (int)w / 2 + 32 + 16;
                AddLight(x, y, (int)w, Globals.GameMaps[Globals.currentMap].Lights[i].intensity, nightColorArray, Globals.GameMaps[Globals.currentMap].Lights[i]);
            }
            //Convert our pixel array into a renderable image.
            SFML.Graphics.Image img = new SFML.Graphics.Image(nightColorArray);
            nightTex = new Texture(img);
            lightsChanged = false;
        }
        private static int CalcLightWidth(int range)
        {
            //Formula that is ~equilivant to Unity spotlight widths, this is so future Unity lighting is possible.
            int[] xVals = { 0, 5, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180 };
            int[] yVals = { 1, 8, 18, 34, 50, 72, 92, 114, 135, 162, 196, 230, 268, 320, 394, 500, 658, 976, 1234, 1600 };
            int w = 0;
            int x = 0;
            while (range >= xVals[x])
            {
                x++;
            }
            if (x > yVals.Length)
            {
                w = yVals[yVals.Length - 1];
            }
            else
            {
                w = yVals[x - 1];
                w += (int)((float)(range - xVals[x - 1]) / ((float)xVals[x] - xVals[x - 1])) * (yVals[x] - yVals[x - 1]);
            }
            return w;
        }
        private static void DrawNight()
        {
            if (Globals.GameMaps[Globals.currentMap].isIndoors) { return; }
            Sprite nightSprite = new Sprite(nightTex);
            nightSprite.Position = new Vector2f(CalcMapOffsetX(4) + -32 * 30, CalcMapOffsetY(4) + -32 * 30);
            nightSprite.Draw(renderWindow, RenderStates.Default);
            nightSprite.Dispose();
        }
        private static void AddLight(int x1, int y1, int size, double intensity, SFML.Graphics.Color[,] colorArr, LightObj light)
        {
            System.Drawing.Bitmap tmpLight = null;
            //If not cached, create a radial gradent for the light.
            if (light.graphic == null)
            {
                tmpLight = new System.Drawing.Bitmap(size, size);
                System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(tmpLight);
                System.Drawing.Drawing2D.GraphicsPath pth = new System.Drawing.Drawing2D.GraphicsPath();
                pth.AddEllipse(0, 0, size - 1, size - 1);
                System.Drawing.Drawing2D.PathGradientBrush pgb = new System.Drawing.Drawing2D.PathGradientBrush(pth);
                pgb.CenterColor = System.Drawing.Color.FromArgb((int)((double)(255 * intensity)), 0, 0, 0);
                pgb.SurroundColors = new System.Drawing.Color[] { System.Drawing.Color.Transparent };
                pgb.FocusScales = new System.Drawing.PointF(0.8f, 0.8f);
                g.FillPath(pgb, pth);
                g.Dispose();
                light.graphic = tmpLight;
            }
            else
            {
                tmpLight = light.graphic;
            }

            //Quickly subtract the inverse of light (darkness) from our darkness texture.
            SFML.Graphics.Color emptyBlack;
            FastBitmap fastBitmap = new FastBitmap(tmpLight);
            fastBitmap.LockImage();
            for (int x = x1; x < x1 + size; x++)
            {
                for (int y = y1; y < y1 + size; y++)
                {
                    if (y >= 0 && y < 30 * 32 * 3 && x >= 0 && x < 30 * 32 * 3)
                    {
                        emptyBlack = nightColorArray[y, x];
                        System.Drawing.Color tmpPixel = fastBitmap.GetPixel(x - x1, y - y1);
                        int a = emptyBlack.A - tmpPixel.A;
                        int b = emptyBlack.R + tmpPixel.R;
                        int c = emptyBlack.G + tmpPixel.G;
                        int d = emptyBlack.B + tmpPixel.B;
                        if (a > 255) { a = 255; }
                        if (a < 0) { a = 0; }
                        if (b > 255) { b = 255; }
                        if (c > 255) { c = 255; }
                        if (d > 255) { d = 255; }
                        nightColorArray[y, x] = new SFML.Graphics.Color((byte)b, (byte)c, (byte)d, (byte)a);
                    }
                }
            }
            fastBitmap.UnlockImage();
        }



    }
}