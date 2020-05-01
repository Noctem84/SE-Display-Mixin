using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        /**

         * */
        public class DisplayService
        {
            private readonly Program program;
            private readonly Dictionary<int, HashSet<Display>> displays = new Dictionary<int, HashSet<Display>>();
            private readonly string initPhrase;
            private Dictionary<int, StringBuilder> textCache = new Dictionary<int, StringBuilder>();
            public string Format { get; set; }
            const string ADD_DISPLAY_COMMAND = "add";
            const string REMOVE_DISPLAY_COMMAND = "rm";
            public const string DISPLAY_COMMAND = "ds";

            public DisplayService(Program program, string initPhrase)
            {
                Format = "{0:0.###}";
                this.program = program;
                this.initPhrase = initPhrase;
                initScreens();
            }

            public bool isDisplayCommand(string command)
            {
                return command.StartsWith(DISPLAY_COMMAND);
            }

            private void initScreens()
            {
                List<IMyTextSurfaceProvider> surfaceProviders = new List<IMyTextSurfaceProvider>();
                program.GridTerminalSystem.GetBlocksOfType(surfaceProviders);
                foreach (IMyTextSurfaceProvider surfaceProvider in surfaceProviders)
                {
                    IMyTerminalBlock block = surfaceProvider as IMyTerminalBlock;
                    if (!block.CustomData.Contains(initPhrase)) continue;
                    int type = 0;
                    int index = 0;
                    int padding = 15;
                    string[] customDataParts = block.CustomData.Split('\n');
                    program.Echo("found display");
                    for (int i = 1; i < customDataParts.Length; i++)
                    {
                        if (customDataParts[i].Contains("id"))
                        {
                            string config = customDataParts[i].Substring(3);
                            if (config.Contains(':'))
                            {
                                string[] parts = config.Split(':');
                                type = int.Parse(parts[1]);
                                config = parts[0];
                            }

                            if (config.Contains('|'))
                            {
                                string[] parts = config.Split('|');
                                padding = int.Parse(parts[1]);
                                config = parts[0];
                            }

                            index = int.Parse(config);
                        }
                        AddDisplay(block.GetId(), type, index, padding);
                        program.Echo("Added display with type=" + type + "; index=" + index + "; padding=" + padding);
                    }
                    if (customDataParts.Length == 1) AddDisplay(block.GetId(), type, index, padding);
                }
            }

            public bool processCommand(string command)
            {
                bool status = false;
                string displayCommand = command.StartsWith(DISPLAY_COMMAND) ? command.Substring(3) : command;
                if (displayCommand.StartsWith(ADD_DISPLAY_COMMAND))
                {
                    status = AddDisplay(displayCommand.Substring(ADD_DISPLAY_COMMAND.Length + 1));
                }
                else if (displayCommand.StartsWith(REMOVE_DISPLAY_COMMAND))
                {
                    IMyTerminalBlock block = program.GridTerminalSystem.GetBlockWithName(displayCommand.Substring(REMOVE_DISPLAY_COMMAND.Length + 1));
                    if (block != null)
                    {
                        status = removeDisplay(block.GetId());
                    }
                    else
                    {
                        return false;
                    }

                }
                return status;
            }

            private bool AddDisplay(string command)
            {
                int type = 0, index = 0;



                if (command.Contains(':'))
                {
                    string[] commandParts = command.Split(':');
                    type = int.Parse(commandParts[1]);
                    command = commandParts[0];
                }

                if (command.Contains(','))
                {
                    string[] commandParts = command.Split(',');
                    index = int.Parse(commandParts[1]);
                    command = commandParts[0];
                }
                IMyTerminalBlock block = program.GridTerminalSystem.GetBlockWithName(command);
                if (block != null)
                {
                    return AddDisplay(block.GetId(), type, index, 15);
                }
                return false;
            }

            private bool AddDisplay(long displayId, int type, int index, int padding)
            {
                Display display = getDisplay(displayId, type, index, padding);

                if (display == null) return false;

                HashSet<Display> hashSet;
                if (!displays.TryGetValue(type, out hashSet))
                {
                    hashSet = new HashSet<Display>();
                    displays.Add(type, hashSet);
                }

                display.Role = type;
                display.Surface.WriteText("Display " + display.Block.CustomName + " initialized.\n\n standby...");
                hashSet.Add(display);
                //program.Echo(display.printDetails());

                return true;
            }

            /**
             * out of order
             */
            private bool removeDisplay(long displayId)
            {
                Display display = getDisplay(displayId, 0, 0, 0);
                if (display == null) return false;
                foreach (int type in displays.Keys)
                {
                    HashSet<Display> hashSet;

                    if (!displays.TryGetValue(type, out hashSet)) continue;

                    hashSet.Remove(display);
                    if (hashSet.Count == 0)
                    {
                        displays.Remove(type);
                    }
                }
                return true;
            }

            private Display getDisplay(long displayId, int type, int displayIndex, int padding)
            {
                IMyTerminalBlock block = program.GridTerminalSystem.GetBlockWithId(displayId) as IMyTerminalBlock;
                Display display = null;
                IMyTextSurface displayBlock = block as IMyTextSurface;
                if (displayBlock != null)
                {
                    display = new Display(displayBlock as IMyFunctionalBlock, displayBlock, displayIndex, padding, type);
                }
                else
                {
                    IMyTextSurfaceProvider provider = block as IMyTextSurfaceProvider;
                    if (provider != null)
                    {
                        if (provider.SurfaceCount > 0)
                        {
                            int surfaceIndex = provider.SurfaceCount > displayIndex ? displayIndex : 0;
                            display = new Display(block, provider.GetSurface(surfaceIndex), surfaceIndex, padding, type);
                        }
                    }
                }

                if (display == null) return null;
                display.Surface.ContentType = ContentType.SCRIPT;
                display.Surface.Script = "";
                display.Surface.ScriptBackgroundColor = Color.Black;
                return display;
            }

            public void printDisplayNames()
            {
                program.Echo("Displays:");
                HashSet<Display> allDisplays = new HashSet<Display>();
                foreach (HashSet<Display> displaySet in displays.Values)
                {
                    allDisplays.UnionWith(displaySet);
                }
                foreach (Display display in allDisplays)
                {
                    program.Echo(display.Block.CustomName + ":" + display.DisplayIndex + ":" + display.Role);
                }
            }

            public void resetCache(int type)
            {
                textCache.Remove(type);
            }

            public void writeToDisplays(StringBuilder text, bool append, int type)
            {
                StringBuilder outputText = new StringBuilder();
                if (append)
                {
                    textCache.TryGetValue(type, out outputText);
                    if (outputText == null || outputText.Length > 0)
                    {
                        outputText = text;
                    }
                    outputText.Append(text);
                    textCache.Remove(type);
                    textCache.Add(type, outputText);
                }
                else
                {
                    textCache.Remove(type);
                    outputText = text;
                }
                HashSet<Display> displaysOfType;
                if (displays == null || !displays.TryGetValue(type, out displaysOfType))
                {
                    program.Echo("no display found for type " + type);
                    return;
                }

                program.Echo("write to " + type);
                foreach (Display display in displaysOfType)
                {
                    Vector2 screenSize = display.Surface.SurfaceSize;

                    MySprite sprite = MySprite.CreateText(outputText.ToString(), "Debug", Color.Green, display.Scale, TextAlignment.LEFT);
                    Vector2 surfaceSize = display.Surface.SurfaceSize;
                    sprite.Size = display.DisplaySize;
                    sprite.Position = display.getTopLeftPositionPadded(0);
                    program.Echo("size: " + sprite.Size + "; position: " + sprite.Position);
                    using (var frame = display.Surface.DrawFrame())
                    {
                        frame.Add(sprite);
                    }
                }
            }

            public void draw(List<MySprite> sprites, int type, bool fullSize)
            {
                HashSet<Display> displaysOfType;
                if (displays == null || !displays.TryGetValue(type, out displaysOfType)) return;

                program.Echo("write to " + type);
                foreach (Display display in displaysOfType)
                {
                    Vector2 screenSize = display.Surface.SurfaceSize;

                    using (var frame = display.Surface.DrawFrame())
                    {
                        if (fullSize && sprites.Count == 1)
                        {
                            MySprite sprite = sprites[0];
                            Vector2 surfaceSize = display.Surface.SurfaceSize;
                            sprite.Size = surfaceSize;
                            sprite.Position = new Vector2(0, surfaceSize.Y / 2);
                            program.Echo("size: " + sprite.Size + "; position: " + sprite.Position);
                            frame.Add(sprite);
                        }
                        else
                        {
                            frame.AddRange(sprites);
                        }
                    }
                }
            }

            public void draw(IContent content, int type)
            {
                HashSet<Display> displaysOfType;
                if (displays == null || !displays.TryGetValue(type, out displaysOfType)) return;

                program.Echo("draw to type " + type);
                foreach (Display display in displaysOfType)
                {
                    float newOffset = 0;
                    List<MySprite> sprites = content.getSprites(display, 0, out newOffset);
                    using (var frame = display.Surface.DrawFrame())
                    {
                        program.Echo(
                            "new offset: " + newOffset);
                        frame.AddRange(sprites);
                    }
                }
            }
            public void draw(DisplayContent content, int type)
            {
                HashSet<Display> displaysOfType;
                if (displays == null || !displays.TryGetValue(type, out displaysOfType)) return;

                //program.Echo("draw to type " + type);
                foreach (Display display in displaysOfType)
                {
                    float offset = 0;
                    List<MySprite> sprites = new List<MySprite>();
                    foreach (var contentElement in content.Contents)
                    {
                        //program.Echo("Offset: " + offset);
                        sprites.AddRange(contentElement.getSprites(display, offset, out offset));
                    }
                    using (var frame = display.Surface.DrawFrame())
                    {
                        //program.Echo("Adding " + sprites.Count + " sprites");
                        frame.AddRange(sprites);
                    }
                }
            }

            public bool hasDeviceForType(int type)
            {
                return displays.ContainsKey(type);
            }

            public string format(double value)
            {
                return string.Format(Format, value);
            }
        }
    }
}
