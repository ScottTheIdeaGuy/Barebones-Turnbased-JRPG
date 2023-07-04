using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using JRPG.Data;
using System.Windows.Forms;
using Microsoft.VisualBasic.Logging;

namespace JRPG
{
	public class Room
	{
		public Point size { get; private set; }
		public bool[] scrollable { get; private set; }

        public int[][] tilemapBackground;   // tilemapBackground contains an int that tells you what image to render
        public int[][] tilemapForeground;   // tilemapForeground contains an int that tells you what image to render. Negative are non-collidable tiles

        public Color backgroundColorA;
        public Color backgroundColorB;

        private Texture2D[][] slicedTilesets;

        public Dictionary<Point, Entity> entityMap { get; private set; }


        private Vector2 _camera;
        private float cameraSpeed = 1.0F;
        private Vector2 _cameraTarget = new Vector2();
        public Vector2 Camera => _camera;

        public Room(Point size, bool[] scrollable)
		{
			this.size = size;
			if (scrollable != null && scrollable.Length == 2)
				this.scrollable = scrollable;
			else
				this.scrollable = new[] {false, false};
		}

        public void Load(MapData map)
        {
            this.size = new Point(map.width / GameJRPG.TILE_SIZE, map.height / GameJRPG.TILE_SIZE);
            this.scrollable = new[]{
                map.width > GameJRPG.TARGET_WIDTH,
                map.height > GameJRPG.TARGET_HEIGHT
            };

            CreateBlankMap();

            slicedTilesets = new Texture2D[map.layers.Length][];

            for (int layerIndex = 0; layerIndex < map.layers.Length; layerIndex++)
            {
                MapLayerData layer = map.layers[layerIndex];
                slicedTilesets[layerIndex] = GameJRPG.SplitTexture(GameJRPG.tilesets[layer.tileset]);
                Debug.WriteLine(slicedTilesets[layerIndex]);

                // todo?: differentiate layers
                for (int i = 0; i < size.X * size.Y; i++)
                {
                    int x = i % size.X;
                    int y = i / size.X;
                    tilemapForeground[x][y] = layer.data[i];
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            Vector2 diff = _cameraTarget - _camera;
            cameraSpeed = MathF.Sqrt(diff.X * diff.X + diff.Y * diff.Y) / 15F;
            if (cameraSpeed < .5F)
                cameraSpeed = .5F;

            // update camera //
            if (Math.Abs(_camera.X - _cameraTarget.X) >= .5F)
            {
                if (_camera.X < _cameraTarget.X)
                    _camera.X += cameraSpeed;
                if (_camera.X > _cameraTarget.X)
                    _camera.X -= cameraSpeed;
            }
            else
                _camera.X = _cameraTarget.X;


            if (Math.Abs(_camera.Y - _cameraTarget.Y) >= .5F)
            {
                if (_camera.Y < _cameraTarget.Y)
                    _camera.Y += cameraSpeed;
                if (_camera.Y > _cameraTarget.Y)
                    _camera.Y -= cameraSpeed;
            }
            else
                _camera.Y = _cameraTarget.Y;
        }

        public void LoadDefault()
        {
            CreateBlankMap();
        }

		public void Initialize()
		{
            backgroundColorA = GameJRPG.backgroundColor0;
            backgroundColorB = GameJRPG.backgroundColor1;

            entityMap = new Dictionary<Point, Entity>();

            _camera = Vector2.Zero;
        }

        public void DrawBackground(SpriteBatch _spriteBatch, GameTime time, Vector2 camera)
        {
            // draw background //
            for (int x = 0; x < tilemapBackground.Length; x++)
                for (int y = 0; y < tilemapBackground[x].Length; y++)
                {
                    Rectangle destRect = new Rectangle(TileToSpace(new Point(x, y)) - new Point((int)camera.X, (int)camera.Y), new Point(GameJRPG.TILE_SIZE, GameJRPG.TILE_SIZE));
                    Color tileColor;

                    switch (tilemapBackground[x][y])
                    {
                        case 0:
                            tileColor = backgroundColorA;
                            break;

                        case 1:
                            tileColor = backgroundColorB;
                            break;

                        default:
                            tileColor = Color.Black;
                            break;
                    }

                    _spriteBatch.Draw(GameJRPG.blankSprite, destRect, tileColor);
                }
        }

        public void DrawForeground(SpriteBatch _spriteBatch, GameTime time, Vector2 camera)
        {
            // draw foreground //
            for (int x = 0; x < tilemapForeground.Length; x++)
                for (int y = 0; y < tilemapForeground[x].Length; y++)
                {
                    Rectangle destRect = new Rectangle(TileToSpace(new Point(x, y)) - new Point((int)camera.X, (int)camera.Y), new Point(GameJRPG.TILE_SIZE, GameJRPG.TILE_SIZE));
                    Color tileColor = Color.Transparent;
                    Texture2D texture = GameJRPG.blankSprite;

                    int tilemapIndex = tilemapForeground[x][y];

                    if (tilemapIndex >= 0)
                    {
                        texture = slicedTilesets[0][tilemapIndex];
                        tileColor = Color.White;
                    }
                    else
                    {
                        texture = GameJRPG.blankSprite;
                    }

                    if (tileColor.A > 0)
                    {
                        _spriteBatch.Draw(texture, destRect, tileColor);
                    }
                }
        }

        private void CreateBlankMap()
        {
            tilemapForeground = new int[size.X][];
            tilemapBackground = new int[size.X][];
            for (int x = 0; x < size.X; x++)
            {
                tilemapForeground[x] = new int[size.Y];
                tilemapBackground[x] = new int[size.Y];
                for (int y = 0; y < size.Y; y++)
                {
                    tilemapForeground[x][y] = -1;
                    // to make a checkered pattern //
                    tilemapBackground[x][y] = (x + (y % 2)) % 2 == 0 ? 0 : 1;
                }
            }
        }

        public void SetCameraTarget(Point target, Point direction)
        {
            int cameraPadding = GameJRPG.TILE_SIZE * 5;

            Vector2 position = _cameraTarget;
            Vector2 targetCentered = new Vector2(target.X, target.Y) - new Vector2(GameJRPG.TARGET_WIDTH / 2, GameJRPG.TARGET_HEIGHT / 2);

            int cameraLeft = (int)_cameraTarget.X - GameJRPG.TARGET_WIDTH / 2;
            int cameraRight = (int)_cameraTarget.X + GameJRPG.TARGET_WIDTH / 2 - GameJRPG.TILE_SIZE;
            int cameraUp = (int)_cameraTarget.Y - GameJRPG.TARGET_HEIGHT / 2;
            int cameraDown = (int)_cameraTarget.Y + GameJRPG.TARGET_HEIGHT / 2 - GameJRPG.TILE_SIZE;

            if (direction.X < 0)
            {
                if (targetCentered.X - cameraPadding < cameraLeft)
                {
                    position.X = target.X - cameraPadding;
                }
            }

            if (direction.X > 0)
            {
                if (targetCentered.X + cameraPadding >= cameraRight)
                {
                    position.X = target.X + cameraPadding - GameJRPG.TARGET_WIDTH + GameJRPG.TILE_SIZE;
                }
            }


            if (direction.Y < 0)
            {
                if (targetCentered.Y - cameraPadding < cameraUp)
                {
                    position.Y = target.Y - cameraPadding;
                }
            }

            if (direction.Y > 0)
            {
                if (targetCentered.Y + cameraPadding >= cameraDown)
                {
                    position.Y = target.Y + cameraPadding - GameJRPG.TARGET_HEIGHT + GameJRPG.TILE_SIZE;
                }
            }


            int xMin = 0;
            int xMax = size.X * GameJRPG.TILE_SIZE - 1 - GameJRPG.TARGET_WIDTH;
            int yMin = 0;
            int yMax = size.Y * GameJRPG.TILE_SIZE - 1 - GameJRPG.TARGET_HEIGHT;

            if (position.X < xMin)
                position.X = xMin;

            if (position.X > xMax)
                position.X = xMax;

            if (position.Y < yMin)
                position.Y = yMin;

            if (position.Y > yMax)
                position.Y = yMax;


            _cameraTarget = position;
        }

        public Point TileToSpace(Point tileIndex)
        {
            Vector2 vector = TileToSpaceVector(tileIndex);
            return new Point((int)vector.X, (int)vector.Y);
        }

        public Point SpaceToTile(Point space)
        {
            Vector2 vector = SpaceToTileVector(space);
            return new Point((int)vector.X, (int)vector.Y);
        }

        public Vector2 TileToSpaceVector(Point tileIndex)
        {
            return new Vector2(tileIndex.X, tileIndex.Y) * GameJRPG.TILE_SIZE;
        }

        public Vector2 SpaceToTileVector(Point space)
        {
            return new Vector2(space.X, space.Y) / GameJRPG.TILE_SIZE;
        }

        public bool CheckTileFree(Point tileIndex)
        {
            if (tileIndex.X < 0 || tileIndex.Y < 0 || tileIndex.X >= size.X || tileIndex.Y >= size.Y)
                return false;
            if (entityMap.ContainsKey(tileIndex))
                return false;
            return tilemapForeground[tileIndex.X][tileIndex.Y] < 0;
        }

        public void RemapEntity(Entity entity, Point oldMapIndex)
        {
            if (oldMapIndex == entity.currentTile) return;   // if the entity map index did not change, exit this function early

            if (entityMap.ContainsKey(oldMapIndex))
                entityMap.Remove(oldMapIndex);
            entityMap.Add(entity.currentTile, entity);
        }
    }
}
