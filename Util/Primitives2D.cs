﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Wyri.Types;

namespace Wyri
{
    public static class Primitives2D
    {
        public static Texture2D Pixel { get; private set; } //our pixel texture we will be using to draw primitives

        private static bool initialized = false;

        public static void Setup(GraphicsDevice gd)
        {
            if (initialized) return;

            //creating our simple pixel
            Pixel = new Texture2D(gd, 1, 1);
            Pixel.SetData(new Color[] { Color.White });

            initialized = true;
        }

        static Primitives2D()
        {
        }

        //draws a pixel
        public static void DrawPixel(this SpriteBatch sb, float x, float y, Color col, float depth = 1f)
        {
            Setup(sb.GraphicsDevice);
            sb.Draw(Pixel, new Vector2(x, y), null, col, 0, Vector2.Zero/*new Vector2(.5f)*/, 1f, SpriteEffects.None, depth);
        }

        public static void DrawPixel(this SpriteBatch sb, Vector2 vec, Color col, float depth = 1f) => DrawPixel(sb, vec.X, vec.Y, col, depth);

        public static void DrawLine(this SpriteBatch sb, Vector2 p1, Vector2 p2, Color col, float depth = 1f)
        {
            sb.DrawLine(p1.X, p1.Y, p2.X, p2.Y, col, depth);
        }

        //draws a line 
        public static void DrawLine(this SpriteBatch sb, float x1, float y1, float x2, float y2, Color col, float depth = 1f)
        {
            float deltax, deltay, x, y, xinc1, xinc2, yinc1, yinc2, den, num, numadd, numpixels, curpixel;
            deltax = Math.Abs(x2 - x1);        // The difference between the x's
            deltay = Math.Abs(y2 - y1);        // The difference between the y's
            x = x1;                       // Start x off at the first pixel
            y = y1;                       // Start y off at the first pixel

            if (x2 >= x1)                 // The x-values are increasing
            {
                xinc1 = 1;
                xinc2 = 1;
            }
            else                          // The x-values are decreasing
            {
                xinc1 = -1;
                xinc2 = -1;
            }

            if (y2 >= y1)                 // The y-values are increasing
            {
                yinc1 = 1;
                yinc2 = 1;
            }
            else                          // The y-values are decreasing
            {
                yinc1 = -1;
                yinc2 = -1;
            }

            if (deltax >= deltay)         // There is at least one x-value for every y-value
            {
                xinc1 = 0;                  // Don't change the x when numerator >= denominator
                yinc2 = 0;                  // Don't change the y for every iteration
                den = deltax;
                num = deltax / 2;
                numadd = deltay;
                numpixels = deltax;         // There are more x-values than y-values
            }
            else                          // There is at least one y-value for every x-value
            {
                xinc2 = 0;                  // Don't change the x for every iteration
                yinc1 = 0;                  // Don't change the y when numerator >= denominator
                den = deltay;
                num = deltay / 2;
                numadd = deltax;
                numpixels = deltay;         // There are more y-values than x-values
            }

            for (curpixel = 0; curpixel <= numpixels; curpixel++)
            {
                DrawPixel(sb, x, y, col, depth);
                num += numadd;              // Increase the numerator by the top of the fraction
                if (num >= den)             // Check if numerator >= denominator
                {
                    num -= den;               // Calculate the new numerator value
                    x += xinc1;               // Change the x as appropriate
                    y += yinc1;               // Change the y as appropriate
                }
                x += xinc2;                 // Change the x as appropriate
                y += yinc2;                 // Change the y as appropriate
            }
        }

        public static void DrawRectangle(this SpriteBatch sb, RectF rect, Color col, Boolean filled, float depth = 1f)
        {
            if (filled)
            {
                sb.Draw(Pixel, new Vector2(rect.x, rect.y), null, col, 0, Vector2.Zero, new Vector2(rect.w, rect.h), SpriteEffects.None, depth);
            }
            else
            {
                DrawLine(sb, rect.x, rect.y, rect.x + rect.w, rect.y, col, depth);
                DrawLine(sb, rect.x, rect.y, rect.x, rect.y + rect.h, col, depth);
                DrawLine(sb, rect.x + rect.w, rect.y, rect.x + rect.w, rect.y + rect.h, col, depth);
                DrawLine(sb, rect.x, rect.y + rect.h, rect.x + rect.w, rect.y + rect.h, col, depth);
            }
        }
    }
}
