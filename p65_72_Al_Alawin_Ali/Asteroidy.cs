using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p65_72_Al_Alawin_Ali
{
    internal class Asteroidy
    {
        public Texture2D texture;
       
        public Vector2 position;
        public Vector2 speed;
        
        public bool isVisible = true;

        Random random = new Random();
        int randX_speed, Y_speed;

        public Asteroidy(Texture2D newTexture, Vector2 newPosition)
        {
            texture = newTexture;
            position = newPosition;

            randX_speed = random.Next(-2,2);
            Y_speed = 9;

            speed = new Vector2(randX_speed, Y_speed);
        }

        public void Update(GraphicsDevice graphics)
        {
            position += speed;

            if(position.X <=0 || position.X >= graphics.Viewport.Width - texture.Width)
            {
                speed.X = -speed.X;
            }

            if(position.Y > graphics.Viewport.Height + texture.Height)
            {
                isVisible = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch){
            spriteBatch.Draw(
                texture,
                position,
                null,
                Color.White,
                0f,
                new Vector2(texture.Width / 2, texture.Height / 2),
                Vector2.One,
                SpriteEffects.None,
                0f);

        }

    }
}
