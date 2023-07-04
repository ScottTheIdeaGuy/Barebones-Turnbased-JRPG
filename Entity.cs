using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JRPG
{
    public delegate bool InteractAction(GameTime gameTime);
	public class Entity
	{
        public const float HYPOTENUSE_FACTOR = 1.0F / 1.4141235F;   // inverted square root of 2 for diagonal movementz

		public static readonly Point Left = new Point(-1, 0);
        public static readonly Point Right = new Point(1, 0);
        public static readonly Point Up = new Point(0, -1);
        public static readonly Point Down = new Point(0, 1);
		
		public static readonly Vector2 LeftVector = new Vector2(-1, 0);
        public static readonly Vector2 RightVector = new Vector2(1, 0);
        public static readonly Vector2 UpVector = new Vector2(0, -1);
        public static readonly Vector2 DownVector = new Vector2(0, 1);


		public static readonly Point OffscreenPosition = new Point(-1000, -1000);

        protected Room room;

        // graphics //
        protected Texture2D spriteTexture;
		protected Point spriteDimensions;
		protected Point outputDimensions;
		protected Color tint;

		public InteractAction OnInteract = (GameTime gameTime) => true;	// the delegate that gets called when an adjacent player object interacts with it. Expected to return "true" once the action is complete.
        public Action OnInteractStart = () => { };
        public Action OnInteractEnd = () => { };

        // positioning //
        public Point position;

		// collision //
		protected Point hitboxDimensions;

        // tile positioning //
        public float interpolationSpeed = .125F;

        public Point currentTile { get; protected set; }

        protected Point targetTile;
        protected Point nextPlannedTile;

        protected bool isMovingTiles;
        protected float tileInterpolation;      // 0 is at the original tile, 1 is at the target tile

        public Point facingDirection { get; protected set; }

        public Entity()
		{
			
		}
		virtual public void Initialize(Room room)
		{
            this.room = room;

            spriteTexture = GameJRPG.blankSprite;
			spriteDimensions = new Point(GameJRPG.TILE_SIZE, GameJRPG.TILE_SIZE);
			outputDimensions = spriteDimensions;
			tint = Color.White;
            OnInteract = (GameTime gameTime) => true;
            OnInteractStart = () => {};
            OnInteractEnd = () => {};
        }

		public void Draw(SpriteBatch spriteBatch, GameTime gameTime, Vector2 camera)
		{
			Rectangle destRect = new Rectangle(position - new Point((int)camera.X, (int)camera.Y), spriteDimensions);
			spriteBatch.Draw(spriteTexture, destRect, tint);
		}

		public void Clear()
		{
            currentTile = room.SpaceToTile(position);
            position = OffscreenPosition;
            room = null;
            nextPlannedTile = targetTile = currentTile;
		}

		virtual	public void Update(GameTime gameTiem)
		{
			
		}

        public void SetPosition(Point position)
        {
            this.position = position;
            currentTile = room.SpaceToTile(position);
            nextPlannedTile = targetTile = currentTile;
            tileInterpolation = 0;
            isMovingTiles = false;
        }

		public void InterpolateTiles(bool wasMovingTiles, Point nextTile, bool planAhead)
		{
            Point difference = targetTile - currentTile;
            float speedModifier = 1.0F;

            if (difference.X != 0 && difference.Y != 0)
                speedModifier = HYPOTENUSE_FACTOR;

            // TILE INTERPOLATION LOGIC //
            if (isMovingTiles)
            {
                if (!wasMovingTiles)
                    targetTile = nextTile;

                if (!planAhead)
                    nextPlannedTile = targetTile;

                if (tileInterpolation < 1)
                {
                    Vector2 targetPosition = room.TileToSpaceVector(targetTile);
                    Vector2 originalPosition = room.TileToSpaceVector(currentTile);

                    Vector2 positionVector = originalPosition + (targetPosition - originalPosition) * tileInterpolation;
                    position = new Point((int)Math.Round(positionVector.X), (int)Math.Round(positionVector.Y));

                    tileInterpolation += interpolationSpeed * speedModifier;
                }
                else if (tileInterpolation > 0)
                {
                    tileInterpolation = 0;
                    currentTile = targetTile;
                    if (nextPlannedTile != targetTile)
                    {
                        targetTile = nextPlannedTile;
                        isMovingTiles = true;
                    }
                    else
                    {
                        isMovingTiles = false;
                    }

                    position = room.TileToSpace(currentTile);
                }
            }

            if (isMovingTiles)
            {
                nextPlannedTile = nextTile;
            }
            else if (nextTile != currentTile)
            {
                targetTile = nextPlannedTile = nextTile;
                tileInterpolation = 0;
            }
        }
	}
}
