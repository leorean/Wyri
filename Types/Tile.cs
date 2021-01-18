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
        DestroyBlock,
        Card_A,
        Card_B,
        Card_C,
        Move_Up,
        Move_Down,
        Move_Left,
        Move_Right
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
                        continue;
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
                        case "D":
                            Type = TileType.DestroyBlock;
                            break;
                        case "SB1":
                            SwitchState = SwitchState.Switch1;
                            break;
                        case "SB2":
                            SwitchState = SwitchState.Switch2;
                            break;
                        case "CARD_A":
                            Type = TileType.Card_A;
                            break;
                        case "CARD_B":
                            Type = TileType.Card_B;
                            break;
                        case "CARD_C":
                            Type = TileType.Card_C;
                            break;
                        case "MOVE_U":
                            Type = TileType.Move_Up;
                            break;
                        case "MOVE_D":
                            Type = TileType.Move_Down;
                            break;
                        case "MOVE_L":
                            Type = TileType.Move_Left;
                            break;
                        case "MOVE_R":
                            Type = TileType.Move_Right;
                            break;
                        default:
                            typeData = o;
                            break;
                    }
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
