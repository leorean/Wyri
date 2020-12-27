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
using System.Xml;
using System.Xml.Linq;
using Wyri.Objects;
using Wyri.Objects.Levels;
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

        List<Dictionary<string, object>> ObjectData { get; } = new List<Dictionary<string, object>>();

        public List<Room> Rooms { get; } = new List<Room>();

        public Map(string name)
        {
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
                            tileGrid[i] = new Tile(tileID, OptionsForID(tileID));
                    }

                    LayerData.Add(xmlLayer.Attributes["name"].Value, tileGrid);
                }

                if (xmlLayer.Name == "objectgroup")
                {
                    foreach (XmlNode objectNode in xmlLayer.ChildNodes)
                    {
                        var objProperties = new Dictionary<string, object>();

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
                    MainGame.Player = new Player(new Vector2(x + width * .5f, y + height * .5f));
                    MainGame.Camera.Target = MainGame.Player;
                    break;
                }
            }
        }

        public void CreateAllRooms()
        {
            var camera = MainGame.Camera;

            try
            {
                Room room;

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
                    Rooms.Add(room);
                    //room.Background = bg;
                    //room.Weather = weather;
                    //room.Darkness = darkness;
                }

                // load rooms of standard size when there is none
                for (var i = 0; i < Width * G.T; i += camera.ViewWidth)
                {
                    for (var j = 0; j < Height * G.T; j += camera.ViewHeight)
                    {
                        var c = CollisionExtensions.CollisionPoint<Room>(i + G.T, j + G.T);
                        if (c.Count == 0)
                        {
                            room = new Room(i, j, camera.ViewWidth, camera.ViewHeight);
                            Rooms.Add(room);
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

            foreach(var layer in LayerData)
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
                            if(tile.SwitchState == SwitchState.Switch1)
                            {
                                switchOffset = camera.Room.SwitchState ? 1 : 0;
                                tile.IsSolid = camera.Room.SwitchState ? false : true;
                            }
                            if (tile.SwitchState == SwitchState.Switch2)
                            {
                                switchOffset = camera.Room.SwitchState ? -1 : 0;
                                tile.IsSolid = camera.Room.SwitchState ? true : false;
                            }

                            var partRect = new Rectangle(tix + G.T * tile.AnimationFrame + switchOffset * G.T, tiy, G.T, G.T);

                            sb.Draw(GameResources.Tiles.OriginalTexture, new Vector2(i * G.T, j * G.T), partRect, Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, depth);
                        }
                    }
                }
            }
        }

        public void Update()
        {
        }
    }
}
