using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Wyri.Objects;
using Wyri.Objects.Levels;
using Wyri.Objects.Levels.Effects;
using Wyri.Objects.Levels.Enemies;
using Wyri.Types;

namespace Wyri.Main
{
    public static class MapExtensions
    {
        public static string Find(this XmlAttributeCollection attribs, string name)
        {
            foreach (XmlAttribute a in attribs)
            {
                if (a.Name == name)
                {
                    return a.Value;
                }
            }
            return null;
        }
    }

    public class Map
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Dictionary<string, Grid<Tile>> LayerData { get; private set; } = new Dictionary<string, Grid<Tile>>();

        public Dictionary<int, string> TileOptionsDictionary { get; private set; } = new Dictionary<int, string>();
        private string OptionsForID(int tileID) => TileOptionsDictionary.ContainsKey(tileID) ? TileOptionsDictionary[tileID] : null;

        private List<Dictionary<string, object>> ObjectData { get; set; } = new List<Dictionary<string, object>>();

        public List<Room> Rooms { get; } = new List<Room>();
        
        public async Task LoadMapContentAsync(string name)
        {
            await Task.Run(() =>
            {
                MainGame.SaveGame = new SaveGame();
                var loadSuccess = SaveManager.Load(ref MainGame.SaveGame);

                XmlDocument xmlRoot = new XmlDocument();

                using (var s = typeof(Map).Assembly.GetManifestResourceStream($"Wyri.Content.{name}"))
                {
                    xmlRoot.Load(s);
                }

                var xmlMap = xmlRoot["map"];
                Width = int.Parse(xmlMap.Attributes["width"].Value);
                Height = int.Parse(xmlMap.Attributes["height"].Value);

                // parse options to dictionary
                foreach (XmlNode tilesetNode in xmlMap.ChildNodes)
                {
                    if (tilesetNode.Name == "tileset")
                    {
                        foreach (XmlNode tileNode in tilesetNode.ChildNodes)
                        {
                            if (tileNode.Name == "tile")
                            {
                                string val = tileNode.Attributes["type"].Value;
                                TileOptionsDictionary.Add(int.Parse(tileNode.Attributes["id"].Value), val);
                            }
                        }
                    }
                }

                foreach (XmlNode xmlLayer in xmlMap.ChildNodes)
                {
                    if (xmlLayer.Name == "layer")
                    {
                        string[] layerData = xmlLayer["data"].ChildNodes[0].Value.Split(',');

                        Grid<Tile> tileGrid = new Grid<Tile>(Width, Height);

                        for (int i = 0; i < layerData.Length; i++)
                        {
                            var tileID = int.Parse(layerData[i]) - 1;
                            if (tileID != -1)
                            {
                                tileGrid[i] = new Tile(tileID, OptionsForID(tileID));
                            }
                        }

                        LayerData.Add(xmlLayer.Attributes["name"].Value, tileGrid);
                    }

                    if (xmlLayer.Name == "objectgroup")
                    {
                        foreach (XmlNode objectNode in xmlLayer.ChildNodes)
                        {
                            var objProperties = new Dictionary<string, object>();

                            // text etc will be ignored
                            if (objectNode.Attributes.GetNamedItem("type") == null)
                                continue;

                            var objName = objectNode.Attributes["type"].Value;
                            int x = int.Parse(objectNode.Attributes["x"].Value);
                            int y = int.Parse(objectNode.Attributes["y"].Value);
                            int width = int.Parse(objectNode.Attributes["width"].Value);
                            int height = int.Parse(objectNode.Attributes["height"].Value);

                            objProperties.Add("name", objName); // is actually the type name!
                            objProperties.Add("x", x);
                            objProperties.Add("y", y);
                            objProperties.Add("width", width);
                            objProperties.Add("height", height);

                            if (objectNode.HasChildNodes)
                            {
                                foreach (XmlNode node in objectNode.ChildNodes)
                                {
                                    foreach (XmlNode prop in node.ChildNodes)
                                    {
                                        var propName = prop.Attributes["name"].Value;

                                        var propValueString = prop.Attributes.Find("value");

                                        if (propValueString == null)
                                        {
                                            propValueString = prop.InnerText;
                                        }

                                        // unknown value types are treated as strings
                                        var propValueType = "string";

                                        if (prop.Attributes.Find("type") != null)
                                            propValueType = prop.Attributes["type"].Value;

                                        object propValue;

                                        switch (propValueType)
                                        {
                                            case "int":
                                                propValue = int.Parse(propValueString);
                                                break;
                                            case "float":
                                                float res;
                                                if (float.TryParse(propValueString, out res))
                                                    propValue = res;
                                                else
                                                    propValue = float.Parse(propValueString.Replace('.', ','));
                                                break;
                                            case "bool":
                                                propValue = bool.Parse(propValueString);
                                                break;
                                            default:
                                                propValue = propValueString;
                                                break;
                                        }

                                        objProperties.Add(propName, propValue);
                                    }
                                }
                            }
                            ObjectData.Add(objProperties);
                        }
                    }
                }

                // create rooms
                CreateAllRooms();

                foreach (var data in ObjectData)
                {
                    var x = (int)data["x"];
                    var y = (int)data["y"];
                    var width = (int)data["width"];
                    var height = (int)data["height"];
                    var type = data["name"].ToString();

                    if (type == "player")
                    {
                        if (!loadSuccess)
                        {
                            MainGame.Player = new Player(new Vector2(x + width * .5f, y + height * .5f));
                            MainGame.Camera.Target = MainGame.Player;
                            MainGame.Camera.Position = MainGame.Player.Position;
                            MainGame.Player.SetCameraRoom();
                            MainGame.Player.State = PlayerState.StandUp;
                        }                        
                    }

                    //if (type == "effect")
                    //{
                    //    var effectType = data.ContainsKey("type") ? (int)data["type"] : -1;
                    //    switch (effectType)
                    //    {
                    //        case 0:
                    //            new ElectricSparkEmitter(new Vector2(x + 4, y + 4));
                    //            break;
                    //        default:
                    //            break;
                    //    }
                    //}
                }
                
                if (loadSuccess)
                {
                    MainGame.Player = new Player(MainGame.SaveGame.Position);                    
                    MainGame.Player.Direction = MainGame.SaveGame.Direction;

                    MainGame.Camera.Background = MainGame.SaveGame.Background;
                    MainGame.Camera.Weather = MainGame.SaveGame.Weather;
                    MainGame.Camera.Darkness = MainGame.SaveGame.Darkness;

                    MainGame.Camera.Target = MainGame.Player;
                    MainGame.Camera.Position = MainGame.Player.Position;
                    MainGame.Player.SetCameraRoom();
                }
                
            });
        }

        public Map() { }

        private void CreateAllRooms()
        {            
            var camera = MainGame.Camera;
            Room room;

            try
            {
                foreach (var data in ObjectData)
                {
                    if (data["name"].ToString() != "room")
                        continue;

                    var x = (int)data["x"];
                    var y = (int)data["y"];

                    var w = data.ContainsKey("width") ? (int)data["width"] : camera.ViewWidth;
                    var h = data.ContainsKey("height") ? (int)data["height"] : camera.ViewHeight;
                    var bg = data.ContainsKey("bg") ? (int)data["bg"] : -1;
                    var weather = data.ContainsKey("weather") ? (int)data["weather"] : -1;
                    var darkness = data.ContainsKey("darkness") ? (float)Convert.ToDouble(data["darkness"]) : -1;
                    
                    Math.DivRem(x, camera.ViewWidth, out int remX);
                    Math.DivRem(y, camera.ViewHeight, out int remY);
                    Math.DivRem(w, camera.ViewWidth, out int remW);
                    Math.DivRem(h, camera.ViewHeight, out int remH);

                    if (remX != 0 || remY != 0 || remW != 0 || remH != 0)
                        throw new ArgumentException($"The room at ({x},{y}) has an incorrect size or position!");

                    room = new Room(x, y, w, h);
                    room.Background = bg;
                    room.Weather = weather;
                    room.Darkness = darkness;
                    Rooms.Add(room);

                    for (var i = M.Div(x, G.T); i < M.Div(x + w, G.T); i++) {
                        for (var j = M.Div(y, G.T); j < M.Div(y + h, G.T); j++)
                        {
                            var t = LayerData["FG"][i, j];

                            if (t == null)
                                continue;

                            switch (t.typeData)
                            {
                                case "SPIKE_UP":
                                    new SpikeUp(new Vector2(i * G.T, j * G.T), room);
                                    break;
                                case "SPIKE_DOWN":
                                    new SpikeDown(new Vector2(i * G.T, j * G.T), room);
                                    break;
                                case "SPIKE_LEFT":
                                    new SpikeLeft(new Vector2(i * G.T, j * G.T), room);
                                    break;
                                case "SPIKE_RIGHT":
                                    new SpikeRight(new Vector2(i * G.T, j * G.T), room);
                                    break;
                                case "SPIKE_CORNER":
                                    new SpikeCorner(new Vector2(i * G.T, j * G.T), room);
                                    break;
                                case "SAVE":
                                    new SavePoint(new Vector2((i - .5f) * G.T, (j - 1) * G.T), room);
                                    break;
                                case "SMOKE":
                                    new SmokeEmitter(new Vector2((i + .5f) * G.T, (j + .5f) * G.T), room);
                                    break;
                                case "SPARK":
                                    new ElectricSparkEmitter(new Vector2((i + .5f) * G.T, (j + .5f) * G.T), room);
                                    break;
                                case "E1":
                                    new Enemy1(new Vector2((i + .5f) * G.T, j * G.T), room);
                                    break;
                                case "E2":
                                    new Enemy2(new Vector2((i + .5f) * G.T, j * G.T), room);
                                    break;
                                case "E3_U":
                                    new Enemy3(new Vector2((i + .5f) * G.T, (j + .5f) * G.T), Enemy3.Direction.Up, room);
                                    break;
                                case "E3_D":
                                    new Enemy3(new Vector2((i + .5f) * G.T, (j + .5f) * G.T), Enemy3.Direction.Down, room);
                                    break;
                                case "E3_L":
                                    new Enemy3(new Vector2((i + .5f) * G.T, (j + .5f) * G.T), Enemy3.Direction.Left, room);
                                    break;
                                case "E3_R":
                                    new Enemy3(new Vector2((i + .5f) * G.T, (j + .5f) * G.T), Enemy3.Direction.Right, room);
                                    break;
                                case "I1":
                                    new Item(new Vector2((i + .5f) * G.T, (j + .5f) * G.T), 0, room);
                                    break;
                                case "I2":
                                    new Item(new Vector2((i + .5f) * G.T, (j + .5f) * G.T), 1, room);
                                    break;
                                case "I3":
                                    new Item(new Vector2((i + .5f) * G.T, (j + .5f) * G.T), 2, room);
                                    break;
                                case "I4":
                                    new Item(new Vector2((i + .5f) * G.T, (j + .5f) * G.T), 3, room);
                                    break;
                                case "I5":
                                    new Item(new Vector2((i + .5f) * G.T, (j + .5f) * G.T), 4, room);
                                    break;
                                case "I6":
                                    new Item(new Vector2((i + .5f) * G.T, (j + .5f) * G.T), 5, room);
                                    break;
                                case "I7":
                                    new Item(new Vector2((i + .5f) * G.T, (j + .5f) * G.T), 6, room);
                                    break;
                                case "I8":
                                    new Item(new Vector2((i + .5f) * G.T, (j + .5f) * G.T), 7, room);
                                    break;
                                case "I9":
                                    new Item(new Vector2((i + .5f) * G.T, (j + .5f) * G.T), 8, room);
                                    break;
                                case "I10":
                                    new Item(new Vector2((i + .5f) * G.T, (j + .5f) * G.T), 9, room);
                                    break;
                                case "I11":
                                    new Item(new Vector2((i + .5f) * G.T, (j + .5f) * G.T), 10, room);
                                    break;
                                case "TB1":
                                    var tb = new TriggerBlock(new Vector2(i * G.T, j * G.T), t.ID, false, room);
                                    break;
                            }
                        }
                    }
                }
            }            
            catch (Exception e)
            {
                Debug.WriteLine("Unable to initialize room from data: " + e.Message);
                throw;
            }
        }

        public async Task UnloadAsync()
        {
            await Task.Run(() =>
            {
                foreach (var r in Rooms.ToList())
                {
                    r.Destroy();
                    Rooms.Remove(r);
                }

                LayerData.Clear();
                TileOptionsDictionary.Clear();
                ObjectData.Clear();
            });            
        }

        public static Texture2D Crop(Texture2D original, Rectangle rect)
        {
            //Color[] originalData = new Color[original.Width * original.Height];
            //original.GetData(originalData);

            Texture2D newTexture = new Texture2D(original.GraphicsDevice, rect.Width, rect.Height);
            Color[] newTextureData = new Color[rect.Width * rect.Height];

            original.GetData(0, rect, newTextureData, 0, newTextureData.Length);
            newTexture.SetData(newTextureData);

            return newTexture;
        }

        public void Draw(SpriteBatch sb)
        {
            var camera = MainGame.Camera;
            if (camera == null || camera.Room == null)
                return;

            if (GameResources.Tiles == null)
                throw new InvalidOperationException("The map cannot be drawn without a tileset!");

            int minX = (int)Math.Max(camera.Position.X - camera.ViewWidth * .5f, 0f);
            int maxX = (int)Math.Min(camera.Position.X + camera.ViewWidth * .5f + G.T, Width * G.T);
            int minY = (int)Math.Max(camera.Position.Y - camera.ViewHeight * .5f, 0f);
            int maxY = (int)Math.Min(camera.Position.Y + camera.ViewHeight * .5f + G.T, Height * G.T);

            minX = M.Div(minX, G.T);
            minY = M.Div(minY, G.T);
            maxX = M.Div(maxX, G.T);
            maxY = M.Div(maxY, G.T);

            //Grid<Texture2D> textureBuffer = new Grid<Texture2D>(16 * G.T, 9 * G.T);

            foreach (var layer in LayerData)
            {
                float depth = 0f;
                if (layer.Key == "FG") depth = G.D_FG;
                if (layer.Key == "WATER") depth = G.D_WATER;
                if (layer.Key == "BG1") depth = G.D_BG1;
                if (layer.Key == "BG2") depth = G.D_BG2;                

                for (int i = M.Div(camera.Room.X, G.T); i < M.Div(camera.Room.X + camera.Room.Width, G.T); i++)
                {
                    for (int j = M.Div(camera.Room.Y, G.T); j < M.Div(camera.Room.Y + camera.Room.Height, G.T); j++)
                    {
                        var tile = layer.Value[i, j];

                        if (tile == null)
                            continue;

                        tile.UpdateAnimation();

                        if (i < minX || i >= maxX || j < minY || j >= maxY)
                            continue;

                        if (tile.IsVisible)
                        {
                            var tid = tile.ID;

                            var tix = (tid * G.T) % GameResources.Tiles.Width;
                            var tiy = M.Div(tid * G.T, GameResources.Tiles.Width) * G.T;

                            int switchOffset = 0;
                            float d = depth;
                            if(tile.SwitchState == SwitchState.Switch1)
                            {
                                switchOffset = camera.Room.SwitchState ? 1 : 0;
                                tile.IsSolid = camera.Room.SwitchState ? false : true;
                                if (!tile.IsSolid) d = G.D_BG1;
                            }
                            if (tile.SwitchState == SwitchState.Switch2)
                            {
                                switchOffset = camera.Room.SwitchState ? -1 : 0;
                                tile.IsSolid = camera.Room.SwitchState ? true : false;
                                if (!tile.IsSolid) d = G.D_BG1;
                            }

                            var tileColor = Color.White;

                            if (tile.IsHidden)
                            {
                                if(MainGame.Player != null)
                                {
                                    var dist = M.Euclidean(MainGame.Player.Center, new Vector2((i * G.T) + 4, (j * G.T) + 4));
                                    var alpha = Math.Max(Math.Min(1 - dist / 32f, 1), 0);

                                    tileColor = new Color(Color.White, alpha);
                                }
                            }

                            var partRect = new Rectangle(tix + G.T * tile.AnimationFrame + switchOffset * G.T, tiy, G.T, G.T);
                            sb.Draw(GameResources.Tiles.OriginalTexture, new Vector2(i * G.T, j * G.T), partRect, tileColor, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, d);
                        }
                    }
                }
            }
        }
    }
}
