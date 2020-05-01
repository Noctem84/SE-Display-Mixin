using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        public class DisplayContent
        {
            public List<IContent> Contents { get; set; }

            public DisplayContent()
            {
                Contents = new List<IContent>();
            }
        }
    }
}
