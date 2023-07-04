using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace JRPG
{
    public enum TextAlign { Left = 0, Right = 1, Center = 2};
    public class TextObject
	{
        public static readonly Vector2 OffscreenPosition = new Vector2(-1000, -1000);
        public Vector2 position;
		public string text;
        public TextAlign alignment;
        public Color color;
        public bool bounce = false;
        public float lineBounceOffset = 0;
        public float textDelay = 0;
        public bool skippable;
        public Color dropshadowColor = Color.Transparent;

        private int charIndex = 0;
        public float delayTime = 0;


        public bool Complete => charIndex >= text.Length - 1;

        public bool visible = true;


        public void Initialize(string text, Point position, TextAlign alignment, Color color)
		{
            this.text = text;
            this.position = new Vector2(position.X, position.Y);
            this.alignment = alignment;
            this.color = color;
            bounce = false;
            lineBounceOffset = 0;
            textDelay = 0;
            charIndex = 0;
            delayTime = 0;
            skippable = false;
            dropshadowColor = Color.Transparent;
            visible = true;
        }

        public void ResetTextOnly(string newText, float textDelay)
        {
            text = newText;
            charIndex = 0;
            delayTime = 0;
            this.textDelay = textDelay;
            visible = true;
        }

        public void Clear()
        {
            position = OffscreenPosition;
            text = "";
            alignment = TextAlign.Center;
            color = Color.Transparent;
        }

        public void Update(GameTime gameTime)
        {
            if (text == null || text.Length == 0) return;

            if (skippable && textDelay > 0)
            {
                if (InputHandler.IsPressed(Button.Action, true))
                    charIndex = text.Length - 1;
            }
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (!visible) return;

            if (text == null || text.Length == 0) return;

            if (textDelay == 0)
                charIndex = text.Length - 1;
            else
            {
                delayTime += gameTime.ElapsedGameTime.Milliseconds / 1000F;
                if (delayTime >= textDelay)
                {
                    delayTime = 0;
                    charIndex++;
                    if (charIndex >= text.Length)
                    {
                        textDelay = 0;
                        charIndex = text.Length - 1;
                    }
                }
            }

            string substring = text.Substring(0, charIndex + 1);
            

            string[] lines = substring.Split('\n');
            float lineHeight = GameJRPG.mainFont.LineSpacing;

            Vector2 offset = Vector2.Zero;
            
            for (int i = 0; i < lines.Length; i++)
            {
                if (bounce)
                {
                    offset.Y = (float)Math.Sin(gameTime.TotalGameTime.TotalMilliseconds / 200.0 + i * lineBounceOffset * 10) * 2.0F;
                }

                Vector2 textOrigin = GameJRPG.mainFont.MeasureString(lines[i]) / 2;

                switch (alignment)
                {
                    case TextAlign.Left:
                        textOrigin.X = 0;
                        break;
                    case TextAlign.Right:
                        textOrigin.X = GameJRPG.mainFont.MeasureString(lines[i]).X;
                        break;
                }

                textOrigin.Y -= i * lineHeight;
                // draw shadow first //
                spriteBatch.DrawString(GameJRPG.mainFont, lines[i], position + offset + new Vector2(-1, 1), dropshadowColor, 0, textOrigin, 1.0f, SpriteEffects.None, 0.5f);
                spriteBatch.DrawString(GameJRPG.mainFont, lines[i], position + offset + new Vector2(1, 1), dropshadowColor, 0, textOrigin, 1.0f, SpriteEffects.None, 0.5f);
                spriteBatch.DrawString(GameJRPG.mainFont, lines[i], position + offset + new Vector2(0, 1), dropshadowColor, 0, textOrigin, 1.0f, SpriteEffects.None, 0.5f);

                // draw top text last //
                spriteBatch.DrawString(GameJRPG.mainFont, lines[i], position + offset, color, 0, textOrigin, 1.0f, SpriteEffects.None, 0.5f);
            }
        }
    }
}