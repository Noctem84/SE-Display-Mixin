using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class Display
        {
            public IMyTextSurface Surface { get; set; }

            public int Role { get; set; }
            public IMyTerminalBlock Block { get; set; }
            public int DisplayIndex { get; set; }
            public float Padding { get; set; }
            public float Scale { get; set; }
            public float LineHeight { get; set; }
            public Vector2 DisplaySize { get; set; }
            private readonly Dictionary<SpriteType, Vector2> TopLeftPositions = new Dictionary<SpriteType, Vector2>();
            private readonly Dictionary<SpriteType, Vector2> TopLeftPositionsPadded = new Dictionary<SpriteType, Vector2>();

            public Display(IMyTerminalBlock block, IMyTextSurface surface, int displayIndex, float padding, int role)
            {
                Surface = surface;
                Block = block;
                Role = role;
                DisplayIndex = displayIndex;
                Padding = padding;
                Scale = Surface.SurfaceSize.Y < 250 ? 0.5f : 1f;
                DisplaySize = new Vector2(Surface.SurfaceSize.X - padding, Surface.SurfaceSize.Y - padding);
                LineHeight = calculateLineHeight(Surface, Scale, "Debug");
                TopLeftPositions.Add(SpriteType.TEXTURE, getPositionTopLeft(Surface.SurfaceSize, Surface.TextureSize, 0, SpriteType.TEXTURE));
                TopLeftPositions.Add(SpriteType.TEXT, getPositionTopLeft(Surface.SurfaceSize, Surface.TextureSize, 0, SpriteType.TEXT));

                TopLeftPositionsPadded.Add(SpriteType.TEXTURE, getPositionTopLeft(Surface.SurfaceSize, Surface.TextureSize, Padding, SpriteType.TEXTURE));
                TopLeftPositionsPadded.Add(SpriteType.TEXT, getPositionTopLeft(Surface.SurfaceSize, Surface.TextureSize, Padding, SpriteType.TEXT));
            }

            public Vector2 getTopLeftPositionPadded(float offset)
            {
                Vector2 topLeft = TopLeftPositionsPadded.GetValueOrDefault(SpriteType.TEXT);
                return new Vector2(topLeft.X, topLeft.Y + offset);
            }
            public Vector2 getTopLeftPositionPadded(float offset, Vector2 size)
            {
                Vector2 topLeft = TopLeftPositionsPadded.GetValueOrDefault(SpriteType.TEXT);
                return new Vector2(topLeft.X + (size.X / 2), topLeft.Y + offset + (size.Y / 2));
            }

            public Vector2 getTopLeftPosition(SpriteType type, float offset)
            {
                Vector2 topLeft = TopLeftPositions.GetValueOrDefault(type);
                return new Vector2(topLeft.X, topLeft.Y + offset);
            }

            /**
             * Doesn't take corner LEDs into account
             */
            private static Vector2 getPositionTopLeft(Vector2 displaySize, Vector2 textureSize, float padding, SpriteType spriteType)
            {
                Vector2 topLeftPosition;
                Vector2 paddedDisplaySize = new Vector2(displaySize.X - padding, displaySize.Y - padding);
                Vector2.Subtract(ref textureSize, ref paddedDisplaySize, out topLeftPosition);
                if (SpriteType.TEXT.Equals(spriteType))
                {
                    return Vector2.IsZero(ref topLeftPosition) ? topLeftPosition : Vector2.Divide(topLeftPosition, 2);
                }
                Vector2 relativeScreenCenter = Vector2.Divide(paddedDisplaySize, 2);
                return Vector2.IsZero(ref topLeftPosition) ? relativeScreenCenter : Vector2.Add(relativeScreenCenter, Vector2.Divide(topLeftPosition, 2));
            }

            private static float calculateLineHeight(IMyTextSurface surface, float scale, string font)
            {
                StringBuilder sb = new StringBuilder("1");
                return surface.MeasureStringInPixels(sb, font, scale).Y - scale;
            }

            public string printDetails()
            {
                StringBuilder sb = new StringBuilder("\n");
                sb.Append("Display name: ").Append(Block.CustomName).Append(" : ").Append(DisplayIndex).Append("\n");
                sb.Append("Display size: ").Append(Surface.SurfaceSize).Append("\n");
                sb.Append("TopLeft TEXTURE: ").Append(TopLeftPositions.GetValueOrDefault(SpriteType.TEXTURE)).Append("\n");
                sb.Append("TopLeftPadded TEXTURE: ").Append(TopLeftPositionsPadded.GetValueOrDefault(SpriteType.TEXTURE)).Append("\n");
                sb.Append("TopLeft TEXT: ").Append(TopLeftPositions.GetValueOrDefault(SpriteType.TEXT)).Append("\n");
                sb.Append("TopLeftPadded TEXT: ").Append(TopLeftPositionsPadded.GetValueOrDefault(SpriteType.TEXT)).Append("\n");

                return sb.ToString();
            }
        }
    }
}
