using System;
using System.Collections.Generic;
using System.Text;
using Wyri.Main;

namespace Wyri.Types
{
    //public class TileAnimation
    //{
    //    public bool IsAnimated { get { return AnimationLength > 0 && AnimationTimeout > 0; } }
    //    public int AnimationLength { get; set; } = 0;
    //    public int AnimationTimeout = 0;        
    //    private int timeout;        
    //    public int AnimationFrame { get; private set; } = 0;

    //    public void Update()
    //    {
    //        if (!IsAnimated) return;

    //        timeout = Math.Max(timeout - 1, 0);
    //        if (timeout == 0)
    //        {
    //            AnimationFrame = (AnimationFrame + 1) % AnimationLength;
    //            timeout = AnimationTimeout;
    //        }
    //    }

    //    public static TileAnimation Parse(string expression)
    //    {
    //        TileAnimation t = new TileAnimation();

    //        if (!string.IsNullOrEmpty(expression))
    //        {
    //            string[] options = expression.Split(';');
    //            foreach (var o in options)
    //            {
    //                if (o.StartsWith("A"))
    //                {
    //                    string[] animationOptions = o.Remove(0, 1).Split('-');
    //                    t.AnimationLength = int.Parse(animationOptions[0]);
    //                    t.AnimationTimeout = int.Parse(animationOptions[1]);
    //                }
    //            }
    //        }

    //        return t;
    //    }
    //}

    /// <summary>
    /// a visual, collidable object at a certain position in a TileMap
    /// </summary>
    public class Tile
    {
        public int ID { get; private set; }

        public bool IsSolid { get; set; } = true;
        public bool IsVisible { get; set; } = true;
        public bool IsAnimated { get { return AnimationLength > 0 && AnimationTimeout > 0; } }
        public int AnimationLength { get; set; } = 0;
        public int AnimationTimeout = 0;
        private int timeout;
        public int AnimationFrame { get; private set; } = 0;
        public bool IsPlatform { get; set; } = false;        
        public SwitchState SwitchState { get; set; } = SwitchState.None;

        public Tile(int id)
        {
            ID = id;
            if (id == -1)
            {
                IsVisible = false;
                IsSolid = false;
            }
        }

        public Tile(int id, string expression) : this(id)
        {
            if (!string.IsNullOrEmpty(expression))
            {
                string[] options = expression.Split(';');
                foreach(var o in options)
                {
                    if (o == "S-")
                        IsSolid = false;
                    
                    if (o == "V-")
                        IsVisible = false;

                    if (o == "P")
                        IsPlatform = true;

                    if (o.StartsWith("A"))
                    {
                        string[] animationOptions = o.Remove(0, 1).Split('-');
                        AnimationLength = int.Parse(animationOptions[0]);
                        AnimationTimeout = int.Parse(animationOptions[1]);
                    }

                    if (o == "SB1")
                        SwitchState = SwitchState.Switch1;

                    if (o == "SB2")
                        SwitchState = SwitchState.Switch2;

                }
            }
        }

        public void UpdateAnimation()
        {
            if (!IsAnimated) return;

            timeout = Math.Max(timeout - 1, 0);
            if (timeout == 0)
            {
                AnimationFrame = (AnimationFrame + 1) % AnimationLength;
                timeout = AnimationTimeout;
            }
        }
    }
}
