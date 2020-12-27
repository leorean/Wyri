using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wyri.Objects
{
    public static class ObjectController
    {
        private static readonly List<Object> objects;
        private static readonly List<Object> activeObjects;

        static ObjectController()
        {
            objects = new List<Object>();
            activeObjects = new List<Object>();
        }

        public static void Add(Object o)
        {
            if (!objects.Contains(o))
                objects.Add(o);
        }

        public static void Remove(Object o)
        {
            activeObjects.Remove(o);
            objects.Remove(o);            
        }

        public static void Update()
        {
            foreach (var o in activeObjects)
                o.Update();
        }

        public static void SetActive(Object o, bool value)
        {
            if (value)
            {
                if (!activeObjects.Contains(o))
                {
                    activeObjects.Add(o);
                }
            }
            else
            {
                activeObjects.Remove(o);
            }            
        }

        public static void Draw(SpriteBatch sb)
        {
            foreach (var o in activeObjects)
            {
                if (o.IsVisible)
                {
                    o.Draw(sb);
                }
            }
        }

        public static List<T> FindAll<T>() where T: Object
        {
            List<T> found = new List<T>();

            foreach(var o in objects)
            {
                if (o is T t)
                    found.Add(t);
            }

            return found;
        }

        public static List<T> FindActive<T>() where T : Object
        {
            List<T> found = new List<T>();

            foreach (var o in activeObjects)
            {
                if (o is T t)
                    found.Add(t);
            }

            return found;
        }
    }
}
