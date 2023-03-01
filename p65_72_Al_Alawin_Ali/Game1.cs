using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using System.Security.Cryptography.X509Certificates;
using static System.Formats.Asn1.AsnWriter;
using System.Threading.Tasks.Sources;

namespace p65_72_Al_Alawin_Ali
{
    public class Game1 : Game
    {
        // User Interface
        MouseState currentMouse;
        MouseState previousMouse;
        Texture2D backgroundTexture;
        Texture2D mildOverlayTexture;
        Texture2D hardOverlayTexture;
        Texture2D playButtonTexture;
        Texture2D exitButtonTexture;
        Texture2D endgameButtonTexture;
        Texture2D playagainButtonTexture;
        Texture2D gameoverTexture;
        Texture2D panelTexture;
        Texture2D pauseTexture;
        Texture2D startTexture;
        SpriteFont font;
        bool isFullScreen = false;
        bool gameStarted = false;
        bool gamePaused = false;
        bool gameOver = false;
        int score = 0;


        // Raketa
        Texture2D rocketTexture;
        Vector2 rocketPosition;
        Color rocketColor = Color.White;
        float rocketSpeed;

        // Asteroidy
        List<Asteroidy> asteroids = new List<Asteroidy>();
        Texture2D asteroidTexture;
        float asteroidSpawn = 0;

        // Mince
        List<Coins> coins = new List<Coins>();
        Texture2D coinTexture;
        float coinSpawn = 0;
        int coinCount = 0;

        // Životy
        List<Hearts> hearts = new List<Hearts>();
        Texture2D heartTexture;
        Texture2D spent_heartTexture;
        float heartSpawnOdds = 0;
        float heartSpawn = 0;
        float immortal = 0;
        bool isImmortal = false;
        int lifeCount = 3;

        // Ostatné
        Random random = new Random();
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

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

            // UI
            backgroundTexture = Content.Load<Texture2D>("background");
            hardOverlayTexture = Content.Load<Texture2D>("hardOverlay");
            mildOverlayTexture = Content.Load<Texture2D>("mildOverlay");
            panelTexture = Content.Load<Texture2D>("panel");
            pauseTexture = Content.Load<Texture2D>("pauseMenu");
            font = Content.Load<SpriteFont>("font");
            playButtonTexture = Content.Load<Texture2D>("play");
            endgameButtonTexture = Content.Load<Texture2D>("endgame");
            playagainButtonTexture = Content.Load<Texture2D>("playagain");
            gameoverTexture = Content.Load<Texture2D>("gameoverMenu");
            exitButtonTexture = Content.Load<Texture2D>("exit");
            startTexture = Content.Load<Texture2D>("startMenu");

            // Životy
            heartTexture = Content.Load<Texture2D>("heart");
            spent_heartTexture = Content.Load<Texture2D>("spent_heart");

            // Raketa
            rocketTexture = Content.Load<Texture2D>("rocket");

            // Objekty
            asteroidTexture = Content.Load<Texture2D>("asteroid");
            coinTexture = Content.Load<Texture2D>("coin");
        }

        void ControlFullScreenMode(bool becomeFullscreen)
        {
            graphics.IsFullScreen = becomeFullscreen;
            isFullScreen = true;
            graphics.ApplyChanges();
        }

        protected override void Update(GameTime gameTime) //Prebehne niekoľko krát za sekundu
        {
            score = coinCount * 10;

            //Inputy z klávesnice
            var ks = Keyboard.GetState();

            //Fullscreen - F5 (Minecraft)
            if (ks.IsKeyDown(Keys.F5)) {
                ControlFullScreenMode(!isFullScreen);
            }

            if (!gamePaused && lifeCount > 0 && gameStarted)
            {
                //Generovanie asteroidov
                asteroidSpawn += (float)gameTime.ElapsedGameTime.TotalSeconds; //Asteroid sa pokúša o spawn každú sekundu
                heartSpawn += (float)gameTime.ElapsedGameTime.TotalMinutes; //Srdiečko sa pokúša o spawn každú minútu
                coinSpawn += (float)gameTime.ElapsedGameTime.TotalSeconds; //Minca sa pokúša o spawn každú sekundu


                // 2 Sekundy nesmrtelnosti (ošetrenie opakovanej kolízie s asteroidom viac krát za sekundu)
                if (isImmortal)
                {
                    immortal += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    rocketColor = Color.Gray;
                    if (immortal >= 2) {
                        rocketColor = Color.White;
                        isImmortal = false;
                        immortal = 0;
                    }

                }

                //update vykreslenia jednotlivých asteroidov
                foreach (Asteroidy asteroid in asteroids)
                {
                    asteroid.Update(graphics.GraphicsDevice);
                }

                LoadHearts(); //volanie metódny na generovanie srdiečok
                LoadAsteroids(); //volanie metódy na generovanie asteroidov
                LoadCoins(); //volanie metódy na generovanie mincí


                //Pohyb lode
                if (!isImmortal)
                {
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
                }


                //Pause
                CustomKeyboard.GetState();
                if (CustomKeyboard.SinglePress(Keys.Escape)) {
                    gamePaused = true;
                }


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
            else if (!gameStarted)
            {
                LoadPlayButton();
            }
            else if (gamePaused)
            {
                LoadPlayButton();
                CustomKeyboard.GetState();
                if (CustomKeyboard.SinglePress(Keys.Escape))
                {
                    gamePaused = false;
                }
            }
            else if (lifeCount <= 0)
            {
                gameOver = true;
                LoadEndgameButton();
            }
        }


        public void LoadCoins()
        {
            int randX = random.Next(100, 1500);
            int randY = random.Next(100, 700);

            //Hitbox rakety
            Rectangle rocketRectangle = new Rectangle(
                (int)rocketPosition.X,
                (int)rocketPosition.Y,
                rocketTexture.Width,
                rocketTexture.Height
                );

            //Detekovanie nárazu rakety na mincu
            for (int ix = 0; ix < coins.Count; ix++)
            {
                //Hitbox srdiečka
                Rectangle coinRectangle = new Rectangle(
                    (int)coins[ix].position.X,
                    (int)coins[ix].position.Y,
                    coinTexture.Width,
                    coinTexture.Height
                    );

                if (rocketRectangle.Intersects(coinRectangle))
                {
                    coins.RemoveAt(ix);
                    coinCount++;
                }
            }

            //Spawnovanie mincí
            if (coinSpawn >= 3)
            {
                coinSpawn = 0;
                if (coins.Count < 6)
                {
                    coins.Add(
                        new Coins(
                            coinTexture,
                            new Vector2(randX, randY)
                            )
                        );
                }
            }
        }

        public void LoadHearts()
        {
            int randX = random.Next(100, 1500);
            int randY = random.Next(100, 700);

            //Hitbox rakety
            Rectangle rocketRectangle = new Rectangle(
                (int)rocketPosition.X,
                (int)rocketPosition.Y,
                rocketTexture.Width,
                rocketTexture.Height
                );

            //Detekovanie nárazu rakety na srdiečko
            for (int ix = 0; ix < hearts.Count; ix++)
            {
                //Hitbox srdiečka
                Rectangle heartRectangle = new Rectangle(
                    (int)hearts[ix].position.X,
                    (int)hearts[ix].position.Y,
                    heartTexture.Width,
                    heartTexture.Height
                    );

                if (rocketRectangle.Intersects(heartRectangle))
                {
                    hearts.RemoveAt(ix);
                    if (lifeCount < 3)
                    {
                        lifeCount++;
                    }
                }
            }

            //Spawnovanie srdiečok
            if (heartSpawn >= 1)
            {
                heartSpawnOdds = random.Next(0, 2); //Šanca na spawn 50%
                heartSpawn = 0;

                if (heartSpawnOdds == 1)
                {
                    if (hearts.Count < 1)
                    {
                        hearts.Add(
                            new Hearts(
                                heartTexture,
                                new Vector2(randX, randY)
                                )
                            );
                    }
                }
            }

        }

        public void LoadAsteroids()
        {
            asteroidTexture = Content.Load<Texture2D>("asteroid");
            int randX = random.Next(0, 1600);
            int Y = -50;

            //Hitbox rakety
            Rectangle rocketRectangle = new Rectangle(
                (int)rocketPosition.X,
                (int)rocketPosition.Y,
                rocketTexture.Width,
                rocketTexture.Height
                );


            //Detekovanie nárazu rakety na asteroid
            for (int ix = 0; ix < asteroids.Count; ix++)
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
                    if (!isImmortal)
                    {
                        rocketPosition.X = graphics.PreferredBackBufferWidth / 2;
                        rocketPosition.Y = graphics.PreferredBackBufferHeight - 200;
                        lifeCount -= 1;
                        isImmortal = true;
                    }

                }
            }

            //Spawnovanie asteroidov
            if (asteroidSpawn >= 0.5)
            {
                asteroidSpawn = 0;
                if (asteroids.Count < 15)
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
        public void LoadPlayButton()
        {
            previousMouse = currentMouse;
            currentMouse = Mouse.GetState();
            Rectangle mouseRectangle = new Rectangle(
                currentMouse.X,
                currentMouse.Y,
                1,
                1);

            Rectangle playButtonRectangle = new Rectangle(
                520,
                340,
                520,
                160
                );

            Rectangle exitButtonRectangle = new Rectangle(
                520,
                600,
                520,
                160
                );
            if (mouseRectangle.Intersects(playButtonRectangle))
            {
                if (currentMouse.LeftButton == ButtonState.Released && previousMouse.LeftButton == ButtonState.Pressed)
                {
                    gameStarted = true;
                    gamePaused = false;
                }
            }
            if (mouseRectangle.Intersects(exitButtonRectangle))
            {
                if (currentMouse.LeftButton == ButtonState.Released && previousMouse.LeftButton == ButtonState.Pressed)
                {
                    Exit();
                }
            }

        }

        public void LoadEndgameButton()
        {
            previousMouse = currentMouse;
            currentMouse = Mouse.GetState();
            Rectangle mouseRectangle = new Rectangle(
                currentMouse.X,
                currentMouse.Y,
                1,
                1);

            Rectangle endgameButtonRectangle = new Rectangle(
                890,
                800,
                520,
                160
                );

            Rectangle playAgainButtonRectangle = new Rectangle(
                200,
                800,
                520,
                160
                );

            if (mouseRectangle.Intersects( endgameButtonRectangle ) )
            {
                if (currentMouse.LeftButton == ButtonState.Released && previousMouse.LeftButton == ButtonState.Pressed) {
                    Exit();
                }
            }
            if (mouseRectangle.Intersects(playAgainButtonRectangle))
            {
                if (currentMouse.LeftButton == ButtonState.Released && previousMouse.LeftButton == ButtonState.Pressed)
                {
                    rocketPosition.X = graphics.PreferredBackBufferWidth / 2;
                    rocketPosition.Y = graphics.PreferredBackBufferHeight - 200;
                    score = 0;
                    coinCount = 0;

                    asteroids.Clear();
                    coins.Clear();

                    immortal = 0;
                    isImmortal = false;
                    gameOver = false;
                    gamePaused = false;
                    lifeCount = 3;
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {

            // Pozadie
            GraphicsDevice.Clear(Color.Black);
           
            // Kreslenie
            spriteBatch.Begin(); //Otvorenie sprite batchu

            // Pozadie img
            spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, 1600, 1000), Color.White);


            // Asteroid(y)
            foreach (Asteroidy asteroid in asteroids)
            {
                asteroid.Draw(spriteBatch);
            }

            // Srdiečko
            foreach (Hearts heart in hearts)
            {
                heart.Draw(spriteBatch);
            }

            // Mince
            foreach (Coins coin in coins)
            {
                coin.Draw(spriteBatch);
            }

            // Raketa
            spriteBatch.Draw(
                rocketTexture, // img rakety
                rocketPosition, // pozícia rakety
                null, // Štvorec ?
                rocketColor, 
                0f, // Uhol otočenia
                new Vector2(rocketTexture.Width / 2, rocketTexture.Height / 2),// prenastavenie pozície rakety relatívne na stred namiesto na roh
                Vector2.One,// Veľkosť (scale)
                SpriteEffects.None,// Flip 
                0f); // hĺbka vo vrstvách

            // InfoPanel
            spriteBatch.Draw(panelTexture, new Rectangle(300, 850, 1000, 150), Color.White);

            //Skóre
            spriteBatch.DrawString(font, "Score: " + score, new Vector2(800, 895), Color.Black);

            // Životy
            switch (lifeCount)
            {
                case 0:
                    for (int i = 400; i <= 600; i += 100)
                    {
                        spriteBatch.Draw(spent_heartTexture, new Rectangle(i, 885, 80, 80), Color.White);
                    }

                    break;

                case 1:
                    spriteBatch.Draw(spent_heartTexture, new Rectangle(600, 885, 80, 80), Color.White);
                    spriteBatch.Draw(spent_heartTexture, new Rectangle(500, 885, 80, 80), Color.White);
                    spriteBatch.Draw(heartTexture, new Rectangle(400, 885, 80, 80), Color.White);
                    spriteBatch.Draw(hardOverlayTexture, new Rectangle(0, 0, 1600, 1000), Color.White);
                    break;

                case 2:
                    spriteBatch.Draw(spent_heartTexture, new Rectangle(600, 885, 80, 80), Color.White);
                    spriteBatch.Draw(heartTexture, new Rectangle(500, 885, 80, 80), Color.White);
                    spriteBatch.Draw(heartTexture, new Rectangle(400, 885, 80, 80), Color.White);
                    spriteBatch.Draw(mildOverlayTexture, new Rectangle(0, 0, 1600, 1000), Color.White);
                    break;

                case 3:
                    for (int i = 400; i <= 600; i += 100)
                    {
                        spriteBatch.Draw(heartTexture, new Rectangle(i, 885, 80, 80), Color.White);
                    }
                    break;
            }

            if (gamePaused)
            {
                spriteBatch.Draw(pauseTexture,new Rectangle(0, 0, 1600, 1000), Color.White);
                spriteBatch.Draw(playButtonTexture, new Vector2(520, 340), Color.White);
                spriteBatch.Draw(exitButtonTexture, new Vector2(520, 600), Color.White);
            }

            if (!gameStarted)
            {
                spriteBatch.Draw(startTexture, new Rectangle(0, 0, 1600, 1000), Color.White);
                spriteBatch.Draw(playButtonTexture, new Vector2(520, 340), Color.White);
                spriteBatch.Draw(exitButtonTexture, new Vector2(520, 600), Color.White);
            }

            if (gameOver)
            {
                spriteBatch.Draw(backgroundTexture, new Rectangle(0, 0, 1600, 1000), Color.White);
                spriteBatch.Draw(gameoverTexture, new Rectangle(0, 0, 1600, 1000), Color.White);
                spriteBatch.DrawString(font, "New score: " + score, new Vector2(120, 350), Color.Black);
                spriteBatch.DrawString(font, "Best score: " + score, new Vector2(120, 350), Color.Black);


                spriteBatch.Draw(playagainButtonTexture, new Vector2(200, 800), Color.White);
                spriteBatch.Draw(endgameButtonTexture, new Vector2(890, 800), Color.White);

            }

            spriteBatch.End(); // Zatvorenie sprite batchu

            base.Draw(gameTime);
        }
    }
}