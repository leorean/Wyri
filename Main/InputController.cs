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
        private static MouseState prevMouseState;
        private static MouseState currentMouseState;

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

        public static bool IsAnyKeyPressed()
        {
            var defaultState = new KeyboardState();
            return currentKeyState != defaultState;
        }

        public static bool IsMousePressed(KeyState keyState = KeyState.Holding)
        {
            switch (keyState)
            {
                case KeyState.Pressed:
                    return currentMouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton != ButtonState.Pressed;
                case KeyState.Released:
                    return currentMouseState.LeftButton != ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Pressed;
                case KeyState.Holding:
                    return currentMouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Pressed;
                default: return false;
            }
        }

        public static void Update()
        {
            prevKeyState = currentKeyState;
            currentKeyState = Keyboard.GetState();

            prevMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();
        }
    }
}
