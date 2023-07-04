using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using JRPG.Data;

namespace JRPG
{
    public enum GameState {OverworldPlay, OverworldInteract, OverworldPause, Battle};

    public class GameJRPG : Game
    {
        public static GameState currentState = GameState.OverworldPlay;
        public static Room currentRoom { get; private set; }
        protected static GameJRPG mainGame;

        public const int TILE_SIZE = 8;
        public const int PIXEL_SIZE = 2;

        public const int TARGET_WIDTH = 160;
        public const int TARGET_HEIGHT = 144;

        Matrix Scale;


        // Loaded Data Collections //
        static public Dictionary<string, Texture2D> tilesets = new Dictionary<string, Texture2D>();
        static public Dictionary<string, MapData> maps = new Dictionary<string, MapData>();

        // Object Management //
        public Entity interactedEntity { get; protected set; }
        public PlayerEntity player;

        public List<TextObject> textPool = new List<TextObject>();
        public List<Entity> entityPool = new List<Entity>();

        public List<TextObject> inactiveTextPool = new List<TextObject>();
        public List<Entity> inactiveEntityPool = new List<Entity>();

        public static Texture2D blankSprite;
        public static SpriteFont mainFont;

        static protected GraphicsDeviceManager _graphics;
        private SpriteBatch spriteBatch;

        public int ScrWidth => Window.ClientBounds.Width;
        public int ScrHeight => Window.ClientBounds.Height;

        
        public static Color backgroundColor0 = Color.DarkMagenta;
        public static Color backgroundColor1 = Color.DarkViolet;


        int numberOfLogs = 0;
        public GameJRPG()
        {
            _graphics = new GraphicsDeviceManager(this);

            _graphics.PreferredBackBufferWidth = 640;
            _graphics.PreferredBackBufferHeight = 576;
            float scaleX = (float)_graphics.PreferredBackBufferWidth / TARGET_WIDTH;
            float scaleY = (float)_graphics.PreferredBackBufferHeight / TARGET_HEIGHT;
            Scale = Matrix.CreateScale(new Vector3(scaleX, scaleY, 1));

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            mainGame = this;
        }

        protected override void Initialize()
        {
            // claim memory for object pools
            for (int i = 0; i < 64; i++)
                inactiveTextPool.Add(new TextObject());
            for (int i = 0; i < 128; i++)
                inactiveEntityPool.Add(new Entity());

            base.Initialize();

            // creating player //
            player = new PlayerEntity();
            player.Initialize(currentRoom);

            // miscellaneous entity setup //
            interactedEntity = null;

            // tilemap and backgrounds //         

            Entity interactible = CreateEntity(new Point(4, 12));
            TextObject textObject = null;

            interactible.OnInteractStart = () =>
            {
                textObject = CreateText("I'm actually evil\nFight me, you teal curr!", new Point(TARGET_WIDTH / 2, TARGET_HEIGHT / 2 - 32), TextAlign.Center, true, Color.White, true, .035F);
                textObject.skippable = true;
            };

            interactible.OnInteract = (GameTime gameTime) =>
            {
                return InputHandler.IsPressed(Button.Action, false) && textObject.Complete;
            };

            interactible.OnInteractEnd = () =>
            {
                currentRoom.backgroundColorA = backgroundColor0;

                for (int i = textPool.Count - 1; i >=0; i--)  // clear all text objects, as the battle system uses the same pool
                {
                    RemoveText(textPool[i]);
                }

                BattleCharacter foe = new BattleCharacter("Frostmite", new[] { "it", "it", "its", "itself" });
                foe.maxHealth = 20;
                foe.moveset[0] = BattleOption.FreezerBurn;
                foe.moveset[1] = BattleOption.FrostBuild;
                foe.defenseStat = 1;
                foe.attackStat = 2;
                BattleSystem.StartFight(new[] { foe, new BattleCharacter(foe)});
            };

            BattleSystem.InitializeParty();


            InputHandler.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            blankSprite = new Texture2D(GraphicsDevice, 1, 1);
            blankSprite.SetData(new[] { Color.White });
            mainFont = Content.Load<SpriteFont>("mainfont");

            StreamReader reader;
            string json;

            /*=================*\
            | OGMO Project Load |
            \*=================*/
            reader = new StreamReader("Content/Level/JRPG-Map.ogmo");         
            json = reader.ReadToEnd();
            MapProjectData project = JsonSerializer.Deserialize<MapProjectData>(json);
            reader.Close();


            /*===================*\
            | apply gathered data |
            \*===================*/
            // - background color
            backgroundColor0 = ColorFromStringHTML(project.backgroundColor);

            // - tileset loading and indexing
            Texture2D texture;
            foreach (TilesetData tileset in project.tilesets)
            {
                texture = Content.Load<Texture2D>("Level/" + tileset.label);
                if (texture != null)
                    tilesets.Add(tileset.label, texture);
            }

            // - loading all maps
            string[] mapFiles = Directory.GetFiles("Content/Level", "*.json");
            MapData map;

            foreach (string mapFile in mapFiles)
            {
                reader = new StreamReader(mapFile);
                json = reader.ReadToEnd();
                map = JsonSerializer.Deserialize<MapData>(json);
                reader.Close();

                int start = mapFile.IndexOf("\\");
                int end = mapFile.IndexOf(".json");
                string mapName = mapFile.Substring(start + 1, end - start - 1);
                maps.Add(mapName, map);
            }

            // once all content is loaded, initialize the room //
            currentRoom = new Room(new Point(TARGET_WIDTH / TILE_SIZE, TARGET_HEIGHT / TILE_SIZE), new[] { false, false });
            currentRoom.Load(maps["test-level"]);
            currentRoom.Initialize();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // input update //
            InputHandler.Update();

            foreach (TextObject textObject in textPool)
            {
                textObject.Update(gameTime);
            }

            if (currentState == GameState.Battle)
            {
                BattleSystem.Update(gameTime);
            }
            else
            {
                // player update //
                Point oldMapIndex = player.currentTile;
                player.Update(gameTime);
                currentRoom.RemapEntity(player, oldMapIndex);

                // entity updates //
                if (currentState == GameState.OverworldPlay)
                    for (int i = entityPool.Count - 1; i >= 0; i--)
                    {
                        oldMapIndex = entityPool[i].currentTile;
                        entityPool[i].Update(gameTime);
                        currentRoom.RemapEntity(entityPool[i], oldMapIndex);
                    }

                // interaction updates //
                if (interactedEntity != null)   // if there is an entity that the player ia currently interacting with
                {
                    currentState = GameState.OverworldInteract;
                    bool interactionResult = interactedEntity.OnInteract(gameTime);
                    if (interactionResult)
                    {
                        interactedEntity.OnInteractEnd();
                        interactedEntity = null;
                    }
                }
                else if (currentState == GameState.OverworldInteract)
                    currentState = GameState.OverworldPlay;

                // update extra effects //
                currentRoom.backgroundColorA = Color.Lerp(backgroundColor0, backgroundColor1, (float)(Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 360) / (Math.PI) + .5F));
                currentRoom.backgroundColorB = Color.Lerp(backgroundColor1, backgroundColor0, (float)(Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 360 - 440) / (Math.PI) + .5F));

                // room update //
                currentRoom.Update(gameTime);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, null, Scale);


            if (currentState == GameState.Battle)
            {
                BattleSystem.Draw(spriteBatch, gameTime);
            }
            else
            {
                // draw background //
                currentRoom.DrawBackground(spriteBatch, gameTime, currentRoom.Camera);

                // draw generic entities //
                for (int i = 0; i < entityPool.Count; i++)
                    entityPool[i].Draw(spriteBatch, gameTime, currentRoom.Camera);

                // draw foreground //
                currentRoom.DrawForeground(spriteBatch, gameTime, currentRoom.Camera);

                // draw player //
                player.Draw(spriteBatch, gameTime, currentRoom.Camera);

                spriteBatch.DrawString(mainFont, "cam: " + currentRoom.Camera, Vector2.Zero, Color.White);
            }

            // draw text //
            for (int i = 0; i < textPool.Count; i++)
                textPool[i].Draw(spriteBatch, gameTime);

            spriteBatch.End();
            base.Draw(gameTime);
        }

        public static TextObject CreateText(string text, Point position, TextAlign alignment, bool dropShadow, Color color, bool bounce = false, float? textDelay = null)
        {
            List<TextObject> textObjects = new List<TextObject>();

            TextObject textObject = mainGame.inactiveTextPool[0];
            textObject.Initialize(text, position, alignment, color);
            textObject.bounce = bounce;
            textObject.lineBounceOffset = 10;
            if (textDelay != null)
                textObject.textDelay = (float)textDelay;

            if (dropShadow)
                textObject.dropshadowColor = Color.Black;
            mainGame.inactiveTextPool.Remove(textObject);
            mainGame.textPool.Add(textObject);
            textObjects.Add(textObject);

            return textObject;
        }

        public Entity CreateEntity(Point startPoint)
        {
            Entity entity = inactiveEntityPool[0];
            inactiveEntityPool.Remove(entity);
            entityPool.Add(entity);
            entity.Initialize(currentRoom);
            entity.SetPosition(currentRoom.TileToSpace(startPoint));
            if (!currentRoom.entityMap.ContainsKey(startPoint))
            {
                currentRoom.entityMap.Add(startPoint, entity);
            }
            return entity;
        }

        static public Entity GetTileEntity(Point tileIndex)
        {
            if (currentRoom.entityMap.ContainsKey(tileIndex))
                return currentRoom.entityMap[tileIndex];
            else
                return null;
        }

        static public void RemoveEntity(Entity entity)
        {
            currentRoom.entityMap.Remove(entity.currentTile);
            mainGame.entityPool.Remove(entity);
            mainGame.inactiveEntityPool.Add(entity);
            entity.Clear();
        }
        
        static public void RemoveText(TextObject textObject)
        {
            mainGame.textPool.Remove(textObject);
            mainGame.inactiveTextPool.Add(textObject);
            textObject.Clear();
        }

        static public void SetInteractedEntity(Entity entity)
        {
            if (currentState == GameState.OverworldPlay)
            {
                entity.OnInteractStart();
                mainGame.interactedEntity = entity;
            }
        }

        static public Color ColorFromStringHTML(string html)
        {
            System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(html.Substring(0, html.Length - 2));
            return new Color(color.R, color.G, color.B);
        }

        static public Texture2D[] SplitTexture(Texture2D sourceTexture, int frameWidth = TILE_SIZE, int frameHeight = TILE_SIZE)
        {
            List<Texture2D> result = new List<Texture2D>();
            for (int x = 0; x < sourceTexture.Width; x += frameWidth)
            {
                for (int y = 0; y < sourceTexture.Height; y += frameHeight)
                {
                    Rectangle sourceRectangle = new Rectangle(y, x, frameHeight, frameWidth);
                    Texture2D newTexture = new Texture2D(_graphics.GraphicsDevice, sourceRectangle.Width, sourceRectangle.Height);
                    Color[] data = new Color[sourceRectangle.Width * sourceRectangle.Height];
                    sourceTexture.GetData(0, sourceRectangle, data, 0, data.Length);
                    newTexture.SetData(data);
                    result.Add(newTexture);
                    // TODO put new texture into an array
                }
            }

            return result.ToArray();
        }
    }
}
