using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;

namespace IngameScript
{
    partial class Program
    {
        public interface IContent
        {
            List<MySprite> getSprites(Display display, float offset, out float newOffset);
        }
    }
}
