using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using System.Security.Cryptography.X509Certificates;

namespace p65_72_Al_Alawin_Ali
{
    public class Game1 : Game
    {
        Texture2D rocketTexture;
        Texture2D asteroidTexture;
        Texture2D backgroundTexture;
        Texture2D panelTexture;
        Texture2D heartTexture;
        Texture2D spent_heartTexture;

        int life_count = 3;
        int limit;
        float immortal = 0;
        bool isImmortal = false;

        Vector2 rocketPosition;

        float rocketSpeed;
        float asteroidSpawn = 0;

        bool isFullScreen = false;

        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;


        List<Asteroidy> asteroids = new List<Asteroidy>();

        Random random = new Random();

        bool rocketHit = false;

        public Game1()
        {
            Window.AllowAltF4 = true;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            //Inicializácia

            graphics.PreferredBackBufferHeight = 1000;
            graphics.PreferredBackBufferWidth = 1600;
            graphics.ApplyChanges();

            //Počiatočná pozícia rakety - stred okna ; počiat. rýchlosť rakety
            rocketPosition = new Vector2(graphics.PreferredBackBufferWidth / 2,
            graphics.PreferredBackBufferHeight / 2);
            rocketSpeed = 300f;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice); //Načítanie sprite batchu -- obsahuje metódy na kreslenie

            //Načítanie obrázkov
            rocketTexture = Content.Load<Texture2D>("rocket");
            backgroundTexture = Content.Load<Texture2D>("background");
            panelTexture = Content.Load<Texture2D>("panel");
            heartTexture = Content.Load<Texture2D>("heart");
            spent_heartTexture = Content.Load<Texture2D>("spent_heart");

        }

        void ControlFullScreenMode(bool becomeFullscreen)
        {
            graphics.IsFullScreen = becomeFullscreen;
            isFullScreen = true;
            graphics.ApplyChanges();
        }

        protected override void Update(GameTime gameTime) //Prebehne niekoľko krát za sekundu
        {
            //Generovanie asteroidov
            asteroidSpawn += (float)gameTime.ElapsedGameTime.TotalSeconds; //Asteroid spawn sa zmení na 1 každú sekundu

            if (isImmortal)
            {
                immortal += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if( immortal >= 3 ) {
                    isImmortal = false;
                    immortal = 0;
                }
            }

            //update vykreslenia v classe Asteroidy za každý asteroid v liste
            foreach (Asteroidy asteroid in asteroids)
            {
                asteroid.Update(graphics.GraphicsDevice);
            }

            LoadAsteroids(); //volanie metódy na generovanie asteroidov

            //Inputy z klávesnice
            var ks = Keyboard.GetState();

            //Pohyb lode
            if (ks.IsKeyDown(Keys.Up))
            {
                rocketPosition.Y -= rocketSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (ks.IsKeyDown(Keys.Down))
            {
                rocketPosition.Y += rocketSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (ks.IsKeyDown(Keys.Left))
            {
                rocketPosition.X -= rocketSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            if (ks.IsKeyDown(Keys.Right))
            {
                rocketPosition.X += rocketSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            //Fullscreen ovládanie klávesnicou F5 (Minecraft)
            if (ks.IsKeyDown(Keys.F5)) {
                if(isFullScreen == false)
                {
                    ControlFullScreenMode(true);
                }
                else
                {
                    ControlFullScreenMode(false);
                }
            }

            //Exit na esc
            if (ks.IsKeyDown(Keys.Escape))
                Exit();

            //Raketa nemôže opustiť rozhranie
            if (rocketPosition.X > graphics.PreferredBackBufferWidth - rocketTexture.Width / 2)
            {
                rocketPosition.X = graphics.PreferredBackBufferWidth - rocketTexture.Width / 2;
            }
            else if (rocketPosition.X < rocketTexture.Width / 2)
            {
                rocketPosition.X = rocketTexture.Width / 2;
            }

            if (rocketPosition.Y > (graphics.PreferredBackBufferHeight - 150) - rocketTexture.Height / 2)
            {
                rocketPosition.Y = (graphics.PreferredBackBufferHeight - 150) - rocketTexture.Height / 2;
            }
            else if (rocketPosition.Y < rocketTexture.Height / 2)
            {
                rocketPosition.Y = rocketTexture.Height / 2;
            }

            base.Update(gameTime);
        }

        public void LoadAsteroids()
        {
            asteroidTexture = Content.Load<Texture2D>("asteroid");
            int randX = random.Next(0, 1600);
            int Y = -50;

            //Hitbox rakety
            Rectangle rocketRectangle = new Rectangle(
                (int)rocketPosition.X ,
                (int)rocketPosition.Y ,
                rocketTexture.Width,
                rocketTexture.Height
                );

            
            //Detekovanie nárazu rakety na asteroid
            for (int ix = 0;  ix < asteroids.Count; ix++)
            {
                //Hitbox asteroidu
                Rectangle asteroidRectangle = new Rectangle(
                    (int)asteroids[ix].position.X,
                    (int)asteroids[ix].position.Y,
                    asteroidTexture.Width,
                    asteroidTexture.Height
                    );

                if (rocketRectangle.Intersects(asteroidRectangle))
                {
                    if(!isImmortal)
                    {
                        life_count -= 1;
                        rocketHit = true;
                        isImmortal = true;
                    }

                }
            }

            if (asteroidSpawn >= 1)
            {
                asteroidSpawn = 0;
                if (asteroids.Count < 4)
                {
                    asteroids.Add(
                        new Asteroidy(
                            asteroidTexture,
                            new Vector2(randX, Y)
                            )
                        );
                }


                for (int i = 0; i < asteroids.Count; i++)
                {
                    //Odstránenie z listu po odídení z okna
                    if (!asteroids[i].isVisible)
                    {
                        asteroids.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {

            // Pozadie
            if (rocketHit){
                GraphicsDevice.Clear(Color.Red);
            }
            else{
                GraphicsDevice.Clear(Color.Black);
            }

            // Kreslenie
            spriteBatch.Begin(); //Otvorenie sprite batchu

            // Pozadie
            spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, 1600, 1000), Color.White);

            // Raketa
            spriteBatch.Draw(
                rocketTexture, // img rakety
                rocketPosition, // pozícia rakety
                null, // Štvorec ?
                Color.White, 
                0f, // Uhol otočenia
                new Vector2(rocketTexture.Width / 2, rocketTexture.Height / 2),// prenastavenie pozície rakety relatívne na stred namiesto na roh
                Vector2.One,// Veľkosť (scale)
                SpriteEffects.None,// Efekty spritov 
                0f); // hĺbka vo vrstvách

            // Asteroid(y)
            foreach (Asteroidy asteroid in asteroids)
            {
                asteroid.Draw(spriteBatch);
            }


            // InfoPanel
            spriteBatch.Draw(panelTexture, new Rectangle(300, 850, 1000, 150), Color.White);

            // Životy
            if (life_count == 3)
            {
                limit = 850;
            }
            if (life_count == 2)
            {
                limit = 750;
            }
            if (life_count == 1)
            {
                limit = 650;
            }
            if (life_count == 0)
            {
                limit = 0;
            }

            for (int i = 650; i <= limit ; i += 100)
            {
                spriteBatch.Draw(heartTexture, new Rectangle(i, 885, 80, 80), Color.White);
            }

            

            // Minuté životy

           


            spriteBatch.End(); // Zatvorenie sprite batchu
            base.Draw(gameTime);
        }
    }
}