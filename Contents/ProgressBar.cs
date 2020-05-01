using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class ProgressBar : IContent
        {
            private Program program;
            public double Total { get; set; }
            public double Current { get; set; }
            public float BorderWeight { get; set; }
            public float Width { get; set; }
            private readonly Dictionary<Vector2, KeyValuePair<float, List<MySprite>>> spriteCache = new Dictionary<Vector2, KeyValuePair<float, List<MySprite>>>();

            public ProgressBar(Program program)
            {
                this.program = program;
                BorderWeight = 1;
            }
            /**
             * "SquareHollow" "SquareSimple"
             * 
             */
            public List<MySprite> getSprites(Display display, float offset, out float newOffset)
            {
                Vector2 displaySize = display.DisplaySize;
                if (spriteCache.ContainsKey(displaySize))
                {
                    KeyValuePair<float, List<MySprite>> cachedValue = spriteCache.GetValueOrDefault(displaySize);
                    newOffset = cachedValue.Key + offset;
                    return cachedValue.Value;
                }

                Vector2 size = new Vector2(displaySize.X, display.LineHeight);
                Vector2 topLeft = display.getTopLeftPositionPadded(offset, size);
                //program.Echo("Top Left: " + topLeft);
                //program.Echo("Line Height: " + display.LineHeight);
                MySprite backgroundSprite = MySprite.CreateSprite("SquareHollow", topLeft, size);
                backgroundSprite.Color = Color.White;
                //program.Echo("[Bar] background size: " + backgroundSprite.Size);
                //program.Echo("[Bar] background position: " + backgroundSprite.Position);
                program.Echo("[Bar] Total Value: " + Total);
                program.Echo("[Bar] Current Value: " + Current);
                program.Echo("[Bar] Usage in %: " + ((float)Current / ((float)Total / 100)));

                Vector2 barSize = new Vector2(((displaySize.X - (BorderWeight * 2)) / 100) * ((float)Current / ((float)Total / 100)), display.LineHeight - (BorderWeight * 2));
                Vector2 barPosition = display.getTopLeftPositionPadded(offset, barSize);
                barPosition.X = barPosition.X + BorderWeight;
                barPosition.Y = barPosition.Y + BorderWeight;
                MySprite barSprite = MySprite.CreateSprite("SquareSimple", barPosition, barSize);
                barSprite.Color = Color.Green;
                //program.Echo("[Bar] bar size: " + barSprite.Size);
                //program.Echo("[Bar] bar position: " + barSprite.Position);

                Vector2 fillerSize = new Vector2((displaySize.X - (BorderWeight * 2)) - barSize.X, barSize.Y);
                Vector2 fillterPosition = display.getTopLeftPositionPadded(offset, fillerSize);
                fillterPosition.X = fillterPosition.X + barSize.X;
                fillterPosition.Y = barPosition.Y;
                MySprite fillerSprite = MySprite.CreateSprite("SquareSimple", fillterPosition, fillerSize);
                fillerSprite.Color = Color.Black;
                //program.Echo("[Bar] filler size: " + fillerSprite.Size);
                //program.Echo("[Bar] filler position: " + fillerSprite.Position);

                List<MySprite> sprites = new List<MySprite>()
                {
                    backgroundSprite, barSprite, fillerSprite
                };
                newOffset = display.LineHeight + offset;
                spriteCache.Add(displaySize, new KeyValuePair<float, List<MySprite>>(newOffset, sprites));
                return sprites;
            }
        }
    }
}
