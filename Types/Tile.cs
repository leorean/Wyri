using System;
using System.Collections.Generic;
using System.Text;
using Wyri.Main;

namespace Wyri.Types
{
    public enum TileType
    {
        Default,        
        Platform,
        /*Save,
        SpikeUp,
        SpikeDown,
        SpikeLeft,
        SpikeRight,
        Smoke,
        // ... etc.*/
    }

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
        public SwitchState SwitchState { get; set; } = SwitchState.None;
        
        public TileType Type { get; private set; }

        public string typeData { get; private set; }

        public Tile(int id)
        {
            ID = id;
            if (id == -1)
            {
                Type = TileType.Default;
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
                    if (o.StartsWith("A"))
                    {
                        string[] animationOptions = o.Remove(0, 1).Split('-');
                        AnimationLength = int.Parse(animationOptions[0]);
                        AnimationTimeout = int.Parse(animationOptions[1]);
                    }

                    switch (o)
                    {
                        case "S-":
                            IsSolid = false;
                            break;
                        case "V-":
                            IsVisible = false;
                            break;
                        case "P":
                            Type = TileType.Platform;
                            IsSolid = false;
                            break;
                        case "SB1":
                            SwitchState = SwitchState.Switch1;
                            break;
                        case "SB2":
                            SwitchState = SwitchState.Switch2;
                            break;
                        default:
                            typeData = o;
                            break;
                    }

                    //if (o == "S-")
                    //    IsSolid = false;
                    
                    //if (o == "V-")
                    //    IsVisible = false;

                    //if (o == "P")
                    //{
                    //    Type = TileType.Platform;
                    //    IsSolid = false;
                    //}

                    

                    //if (o == "SAVE")
                    //{
                    //    Type = TileType.Save;
                    //    IsVisible = false;
                    //    IsSolid = false;
                    //}

                    //if (o == "SB1")
                    //    SwitchState = SwitchState.Switch1;

                    //if (o == "SB2")
                    //    SwitchState = SwitchState.Switch2;

                    //if (o == "SPIKE_UP")
                    //{
                    //    Type = TileType.SpikeUp;
                    //    IsSolid = false;
                    //}
                    //if (o == "SPIKE_DOWN")
                    //{
                    //    Type = TileType.SpikeDown;
                    //    IsSolid = false;
                    //}
                    //if (o == "SPIKE_LEFT")
                    //{
                    //    Type = TileType.SpikeLeft;
                    //    IsSolid = false;
                    //}
                    //if (o == "SPIKE_RIGHT")
                    //{
                    //    Type = TileType.SpikeRight;
                    //    IsSolid = false;
                    //}

                    //if (o == "SMOKE")
                    //{
                    //    typeData = o;
                    //    IsVisible = false;
                    //    IsSolid = false;
                    //}
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
