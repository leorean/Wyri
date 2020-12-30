using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Wyri.Objects
{
    public abstract class Object
    {
        public Object()
        {
            ObjectController.Add(this);
            IsActive = true;
            IsVisible = true;
        }

        ~Object()
        {            
            ObjectController.Remove(this);
        }

        public bool IsVisible { get; set; }

        private bool isActive;
        public bool IsActive
        {
            get
            {
                return isActive;
            }
            set
            {
                ObjectController.SetActive(this, value);                
                isActive = value;
            }
        }

        public virtual void Destroy()
        {
            ObjectController.Remove(this);
        }

        public abstract void Update();
        
        public abstract void Draw(SpriteBatch sb);
    }
}
