using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wyri.Objects;
using Wyri.Objects.Levels;
using Wyri.Types;
using Wyri.Util;

namespace Wyri.Main
{
    public static class MapDisplay
    {
        const int sizeX = 5;
        const int sizeY = 3;
        const float depth = .9f;

        public static void Draw(SpriteBatch sb)
        {
            Color bgFill = new Color(22, 22, 29);
            Color bgGrid = new Color(24, 24, 31);
            Color unvisitedFill = new Color(105, 106, 106);
            Color unvisitedGrid = new Color(131, 140, 145);
            Color visitedFill = new Color(121, 215, 255);
            Color visitedGrid = new Color(255, 255, 255);

            List<Room> drawn = new List<Room>();

            var map = MainGame.Map;
            var cam = MainGame.Camera;
            var player = MainGame.Player;

            if (map == null || cam == null || player == null)
                return;

            var rmW = (int)((double)map.Width / (double)cam.ViewWidth * (double)G.T);
            var rmH = (int)((double)map.Height / (double)cam.ViewHeight * (double)G.T);

            var xo = cam.ViewX + cam.ViewWidth * .5f - .5f * rmW * sizeX - .5f * sizeX;
            var yo = cam.ViewY + cam.ViewHeight * .5f - .5f * rmH * sizeY - .5f * sizeY;

            for (var i = 0; i < rmW; i++)
            {
                for (var j = 0; j < rmH; j++)
                {
                    sb.Draw(GameResources.Map, new Vector2(xo + i * (sizeX), yo + j * (sizeY)), null, new Color(Color.White, .85f), 0, Vector2.Zero, Vector2.One, SpriteEffects.None, depth - .00005f);

                    var r = Collisions.CollisionPoint<Room>(i * cam.ViewWidth + G.T, j * cam.ViewHeight + G.T).FirstOrDefault();
                    if (r == null || drawn.Contains(r))
                        continue;

                    drawn.Add(r);

                    var w = sizeX * r.Width / (float)MainGame.Camera.ViewWidth;
                    var h = sizeY * r.Height / (float)MainGame.Camera.ViewHeight;

                    var d = depth;
                    Color bgCol;
                    Color fgCol;
                    if (MainGame.SaveGame.VisitedRooms.Contains(r.ID))
                    {
                        bgCol = visitedFill;
                        fgCol = visitedGrid;
                        d += .000005f;
                    }
                    else
                    {
                        bgCol = unvisitedFill;
                        fgCol = unvisitedGrid;
                    }

                    // visited/unvisited rooms
                    sb.DrawRectangle(new RectF(xo + i * sizeX, yo + j * sizeY, w, h), bgCol, true, d - .00004f);
                    sb.DrawRectangle(new RectF(xo + i * sizeX, yo + j * sizeY, w, h), fgCol, false, d - .00003f);

                    // player position
                    var ppx = (player.X / (float)(map.Width)) * sizeX * rmW / (float)G.T;
                    var ppy = (player.Y / (float)(map.Height)) * sizeY * rmH / (float)G.T;

                    if (MainGame.Ticks % 30 > 15)
                    {
                        sb.DrawPixel(new Vector2(xo + ppx, yo + ppy), Color.Red, d);
                    }

                    if (player.Abilities.HasFlag(PlayerAbility.COMPASS))
                    {
                        // items
                        Item item = r.Objects.Where(x => x is Item).FirstOrDefault() as Item;
                        if (item != null)
                        {
                            var itemx = (item.X / (float)(map.Width)) * sizeX * rmW / (float)G.T;
                            var itemy = (item.Y / (float)(map.Height)) * sizeY * rmH / (float)G.T;

                            var itemCol = item.Type == 0 ? GameResources.CollectabledisplayColor : GameResources.ItemDisplayColor;
                            itemCol = (MainGame.Ticks % 60 > 55) ? Color.White : itemCol;

                            sb.DrawPixel(new Vector2(xo + itemx, yo + itemy), itemCol, d);                            
                        }
                    }
                }
            }

            // border
            sb.DrawRectangle(new RectF(xo, yo, rmW * sizeX, rmH * sizeY), Color.White, false, depth - .00002f);            
        }
    }
}
