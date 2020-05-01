using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class TwoColumnText : IContent
        {
            private Program program;
            public StringBuilder LeftText { get; set; }
            public StringBuilder RightText { get; set; }
            public Color BackgroundColor { get; set; }
            private readonly Dictionary<Vector2, KeyValuePair<float, List<MySprite>>> spriteCache = new Dictionary<Vector2, KeyValuePair<float, List<MySprite>>>();
            private string font = "Debug";

            public TwoColumnText(Program program)
            {
                this.program = program;
                LeftText = new StringBuilder();
                RightText = new StringBuilder();
            }

            public List<MySprite> getSprites(Display display, float offset, out float newOffset)
            {
                Vector2 displaySize = display.DisplaySize;
                if (spriteCache.ContainsKey(displaySize))
                {
                    KeyValuePair<float, List<MySprite>> cachedValue = spriteCache.GetValueOrDefault(displaySize);
                    newOffset = cachedValue.Key + offset;
                    return cachedValue.Value;
                }

                Vector2 topLeft = display.getTopLeftPositionPadded(offset);
                float displayCenter = displaySize.X / 2;
                float height = Math.Max(RightText.ToString().Split('\n').Length, LeftText.ToString().Split('\n').Length) * display.LineHeight;

                MySprite leftSprite = MySprite.CreateText(LeftText.ToString(), font, Color.White, display.Scale, TextAlignment.LEFT);
                leftSprite.Size = new Vector2(displayCenter - 1, height);
                leftSprite.Position = topLeft;

                MySprite rightSprite = MySprite.CreateText(RightText.ToString(), font, Color.White, display.Scale, TextAlignment.LEFT);
                rightSprite.Size = new Vector2(displayCenter - 1, height);
                rightSprite.Position = new Vector2(topLeft.X + displayCenter, topLeft.Y);
                List<MySprite> sprites = new List<MySprite>()
                {
                    rightSprite, leftSprite
                };
                newOffset = Math.Max(rightSprite.Size.Value.Y, leftSprite.Size.Value.Y) + offset;
                spriteCache.Add(displaySize, new KeyValuePair<float, List<MySprite>>(newOffset, sprites));
                return sprites;
            }
        }
    }
}
