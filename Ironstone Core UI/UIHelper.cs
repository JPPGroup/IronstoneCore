using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using Autodesk.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media.Imaging;
using Orientation = System.Windows.Controls.Orientation;


namespace Jpp.Ironstone.Core.UI
{
    /// <summary>
    /// Class containing static helper methods for working with the Autocad UI
    /// </summary>
    public static class UIHelper
    {
        /// <summary>
        /// Gets the command global name from a given method
        /// </summary>
        /// <param name="type"></param>
        /// <param name="method"></param>
        /// <returns>Global command name</returns>
        public static string GetCommandGlobalName(Type type, string method)
        {
            var rtMethod = type.GetRuntimeMethod(method, Array.Empty<Type>());
            var attribute = rtMethod.GetCustomAttribute<CommandMethodAttribute>();

            return attribute.GlobalName;
        }

        /// <summary>
        /// Convert resource image to format useable by autocad
        /// </summary>
        /// <param name="image">Image to convert</param>
        /// <returns>BitmapImage for use in UI</returns>
        public static BitmapImage LoadImage(Bitmap image)
        {
            MemoryStream ms = new MemoryStream();
            image.Save(ms, ImageFormat.Png);
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();
            return bi;
        }

        /// <summary>
        /// Helper for creating a UI button
        /// </summary>
        /// <param name="buttonText">Text to display</param>
        /// <param name="icon">Icon to display</param>
        /// <param name="size">Small or Large</param>
        /// <param name="command">Command string to be called on button press</param>
        /// <returns></returns>
        public static RibbonButton CreateButton(string buttonText, Bitmap icon, RibbonItemSize size, string command)
        {
            RibbonButton newButton = new RibbonButton();
            newButton.ShowText = true;
            newButton.ShowImage = true;
            newButton.Text = buttonText;
            newButton.Name = buttonText;            
            newButton.Size = size;
            newButton.CommandHandler = new RibbonCommandHandler();
            newButton.CommandParameter = "._" + command + " ";

            switch (size)
            {
                case RibbonItemSize.Standard:
                    newButton.Orientation = Orientation.Horizontal;
                    newButton.Image = LoadImage(icon);
                    break;
                case RibbonItemSize.Large:
                    newButton.Orientation = Orientation.Vertical;
                    newButton.LargeImage = LoadImage(icon);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(size), size, null);
            }

            return newButton;
        }


        /// <summary>
        /// Helper for creating a toggle button an binding a panel to it
        /// </summary>
        /// <param name="buttonText">Text to display</param>
        /// <param name="icon">Icon to display</param>
        /// <param name="size">Small or Large</param>
        /// <param name="view">UI element to embed in pallete</param>
        /// <param name="windowId">GUID of window</param>
        /// <returns></returns>
        public static RibbonToggleButton CreateWindowToggle(string buttonText, Bitmap icon, RibbonItemSize size, UIElement view, string windowId)
        {
            RibbonToggleButton result = new RibbonToggleButton
            {
                ShowText = true,
                ShowImage = true,
                Text = buttonText,
                Name = buttonText,
                Size = size,
            };

            switch (size)
            {
                case RibbonItemSize.Large:
                    result.LargeImage = LoadImage(new Bitmap(icon, new System.Drawing.Size(32,32)));
                    result.Orientation = Orientation.Vertical;
                    break;
                case RibbonItemSize.Standard:
                    result.Image = LoadImage(new Bitmap(icon, new System.Drawing.Size(16, 16)));
                    result.Orientation = Orientation.Horizontal;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(size), size, null);
            }

            //TODO: Confirm this wont get accidentally garbage collected
            PaletteSet paletteSet = new PaletteSet("JPP", new Guid(windowId))
            {
                Size = new System.Drawing.Size(600, 800),
                Style = (PaletteSetStyles)((int)PaletteSetStyles.ShowAutoHideButton +
                                           (int)PaletteSetStyles.ShowCloseButton),
                DockEnabled = (DockSides)((int)DockSides.Left + (int)DockSides.Right)
            };

            ElementHost viewHost = new ElementHost();
            viewHost.AutoSize = true;
            viewHost.Dock = DockStyle.Fill;
            viewHost.Child = view;
            paletteSet.Add(buttonText, viewHost);
            paletteSet.KeepFocus = false;

            bool pendingChange = false;
            bool ignoreChange = false;

            result.CheckStateChanged += (sender, args) =>
            {
                if (!ignoreChange)
                {
                    pendingChange = true;
                    paletteSet.Visible = result.CheckState == true;
                    if (view is HostedUserControl hostedView)
                    {
                        if (paletteSet.Visible)
                        {
                            hostedView.Show();
                        }
                        else
                        {
                            hostedView.Hide();
                        }
                    }
                    pendingChange = false;
                }
            };

            paletteSet.StateChanged += (sender, args) =>
            {
                if (!pendingChange)
                {
                    ignoreChange = true;
                    result.CheckState = !paletteSet.Visible;
                    ignoreChange = false;
                }
            };

            return result;
        }

        public static RibbonToggleButton CreateWindowToggle(string buttonText, Bitmap icon, RibbonItemSize size, Dictionary<string, UIElement> views, string windowId)
        {
            RibbonToggleButton result = new RibbonToggleButton
            {
                ShowText = true,
                ShowImage = true,
                Text = buttonText,
                Name = buttonText,
                Size = size,
            };

            switch (size)
            {
                case RibbonItemSize.Large:
                    result.LargeImage = LoadImage(new Bitmap(icon, new System.Drawing.Size(32, 32)));
                    result.Orientation = Orientation.Vertical;
                    break;
                case RibbonItemSize.Standard:
                    result.Image = LoadImage(new Bitmap(icon, new System.Drawing.Size(16, 16)));
                    result.Orientation = Orientation.Horizontal;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(size), size, null);
            }

            //TODO: Confirm this wont get accidentally garbage collected
            PaletteSet paletteSet = new PaletteSet("JPP", new Guid(windowId))
            {
                Size = new System.Drawing.Size(600, 800),
                Style = (PaletteSetStyles)((int)PaletteSetStyles.ShowAutoHideButton +
                                           (int)PaletteSetStyles.ShowCloseButton),
                DockEnabled = (DockSides)((int)DockSides.Left + (int)DockSides.Right)
            };


            foreach (var view in views)
            {
                var viewHost = new ElementHost
                {
                    AutoSize = true,
                    Dock = DockStyle.Fill,
                    Child = view.Value
                };

                paletteSet.Add(view.Key, viewHost);
            }


            paletteSet.KeepFocus = false;

            bool pendingChange = false;
            bool ignoreChange = false;

            result.CheckStateChanged += (sender, args) =>
            {
                if (!ignoreChange)
                {
                    pendingChange = true;
                    paletteSet.Visible = result.CheckState == true;

                    foreach (var view in views)
                    {
                        if (!(view.Value is HostedUserControl hostedView)) continue;

                        if (paletteSet.Visible)
                        {
                            hostedView.Show();
                        }
                        else
                        {
                            hostedView.Hide();
                        }
                    }
                    pendingChange = false;
                }
            };

            paletteSet.StateChanged += (sender, args) =>
            {
                if (!pendingChange)
                {
                    ignoreChange = true;
                    result.CheckState = !paletteSet.Visible;
                    ignoreChange = false;
                }
            };

            return result;
        }

    }
}
