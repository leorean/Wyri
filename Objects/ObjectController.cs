using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
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
            for(int i = 0; i < activeObjects.Count; i++)
            {
                var o = activeObjects[i];
                o.Update();
            }
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

        public static void SetAllActive<T>(bool active) where T: Object
        {
            for (int i = 0; i < objects.Count; i++)
            {
                var obj = objects[i];
                if (obj is T o)
                {
                    o.IsActive = active;
                }
            }
        }

        public static void SetRegionActive<T>(float x, float y, float width, float height, bool active) where T : SpatialObject
        {
            foreach (var obj in objects)
            {
                if (obj is T o)
                {
                    if (o.X >= x && o.Y >= y && o.X < x + width && o.Y < y + height) o.IsActive = active;
                }
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
