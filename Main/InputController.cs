using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wyri
{
    public enum KeyState
    {
        Pressed, Released, Holding
    }

    public static class InputController
    {
        private static KeyboardState prevKeyState;
        private static KeyboardState currentKeyState;

        public static bool IsEnabled { get; set; } = true;

        public static bool IsKeyPressed(Keys key, KeyState keyState = KeyState.Holding)
        {
            if (!IsEnabled) return false;

            switch (keyState)
            {
                case KeyState.Pressed:
                    return currentKeyState.IsKeyDown(key) && !prevKeyState.IsKeyDown(key);
                case KeyState.Released:
                    return !currentKeyState.IsKeyDown(key) && prevKeyState.IsKeyDown(key);
                case KeyState.Holding:
                    return currentKeyState.IsKeyDown(key) && prevKeyState.IsKeyDown(key);
                default: return false;
            }
        }

        public static void Update()
        {
            prevKeyState = currentKeyState;
            currentKeyState = Keyboard.GetState();            
        }
    }
}
