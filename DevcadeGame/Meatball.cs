using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace DevcadeGame
{
    internal class Meatball
    {
        //Fields
        private Rectangle rectangle;
        private Texture2D texture;

        public int x { get { return rectangle.X; } }
        public int y { get { return rectangle.Y; } }

        public Meatball(Texture2D texture, int x, int y, int Width, int Height)
        {
            this.texture = texture;
            rectangle = new Rectangle(x, y, Width, Height);
        }

        public void Update(GameTime gameTime)
        {

        }

        public bool equals(Meatball other)
        {
            return texture.Equals(other.texture);
        }
    }
}