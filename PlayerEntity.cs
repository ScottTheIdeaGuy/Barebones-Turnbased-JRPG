using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;

namespace JRPG
{
    public class PlayerEntity : Entity
    {
        public override void Initialize(Room room)
        {
            base.Initialize(room);
			tint = Color.Cyan;
            currentTile = new Point(3, 2);
            nextPlannedTile = targetTile = currentTile; //SYNC ALL TILE BASED LOGIC AT START 
            position = room.TileToSpace(currentTile);
            isMovingTiles = false;
            facingDirection = Down;
        }

        override public void Update(GameTime gameTime)
		{
            base.Update(gameTime);

            if (GameJRPG.currentState != GameState.OverworldPlay)  // if the game state does not allow playerentity movement
            {
                return; // then skip the below code this frame
            }

            /*=======================*\
            |* MOVEMENT and COLLISION |
            \*=======================*/
            Point activeTile;

            if (isMovingTiles)
            {
                activeTile = targetTile;
            }
            else
            {
                activeTile = currentTile;
            }

            bool wasMovingTiles = isMovingTiles;

            Point nextTile = activeTile;
            bool didDirectionalInput = false;

            if (InputHandler.IsHeld(Button.Left))
            {
                isMovingTiles = true;
                facingDirection = Left;
                didDirectionalInput = true;
                if (room.CheckTileFree(activeTile + Left) && room.CheckTileFree(nextTile + Left))
                    nextTile.X--;
            }
            else if (InputHandler.IsHeld(Button.Right))
            {
                isMovingTiles = true;
                facingDirection = Right;
                didDirectionalInput = true;
                if (room.CheckTileFree(activeTile + Right) && room.CheckTileFree(nextTile + Right))
                    nextTile.X++;
            }

            if (InputHandler.IsHeld(Button.Up))
            {
                isMovingTiles = true;
                facingDirection = Up;
                didDirectionalInput = true;
                if (room.CheckTileFree(activeTile + Up) && room.CheckTileFree(nextTile + Up))
                    nextTile.Y--;
            }
            else if (InputHandler.IsHeld(Button.Down))
            {
                isMovingTiles = true;
                facingDirection = Down;
                didDirectionalInput = true;
                if (room.CheckTileFree(activeTile + Down) && room.CheckTileFree(nextTile + Down))
                    nextTile.Y++;
            }

            InterpolateTiles(wasMovingTiles, nextTile, didDirectionalInput);
            GameJRPG.currentRoom.SetCameraTarget(room.TileToSpace(targetTile), targetTile - currentTile);

            /*====================================*\
            |* INTERACTION with Adjacent Entities *|
            \*====================================*/
            if (InputHandler.IsPressed(Button.Action, true))
            {
                Entity adjacentEntity = GameJRPG.GetTileEntity(currentTile + facingDirection);

                if (adjacentEntity != null)
                {
                    GameJRPG.SetInteractedEntity(adjacentEntity);
                }
            }
        }
	}
}