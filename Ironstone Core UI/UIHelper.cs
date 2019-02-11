using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media.Imaging;
using Autodesk.AutoCAD.Windows;
using Autodesk.Windows;
using Orientation = System.Windows.Controls.Orientation;


namespace Jpp.Ironstone.Core.UI
{
    /// <summary>
    /// Class containing static helper methods for working with the Autocad UI
    /// </summary>
    public static class UIHelper
    {
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
            newButton.Image = LoadImage(icon);
            newButton.Size = size;
            newButton.CommandHandler = new RibbonCommandHandler();
            newButton.CommandParameter = "._" + command + " ";

            return newButton;
        }

        /// <summary>
        /// Helper for creating a toggle button an binding a panel to it
        /// </summary>
        /// <param name="buttonText">Text to display</param>
        /// <param name="icon">Icon to display</param>
        /// <param name="size">Small or Large</param>
        /// <param name="view">UI element to embed in pallete</param>
        /// <param name="WindowId">GUID of window</param>
        /// <returns></returns>
        public static RibbonToggleButton CreateWindowToggle(string buttonText, Bitmap icon, RibbonItemSize size, Orientation orientation, UIElement view, string WindowId)
        {
            switch (size)
            {
                case RibbonItemSize.Large:
                    icon = new Bitmap(icon, new System.Drawing.Size(32,32));
                    break;

                case RibbonItemSize.Standard:
                    icon = new Bitmap(icon, new System.Drawing.Size(16, 16));
                    break;
            }

            RibbonToggleButton result = new RibbonToggleButton
            {
                ShowText = true,
                ShowImage = true,
                Text = buttonText,
                Name = buttonText,
                Size = size,
                Orientation = orientation,
                Image = LoadImage(icon)
            };

            //TODO: Confirm this wont get accidentally garbage collected
            PaletteSet paletteSet = new PaletteSet("JPP", new Guid(WindowId))
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
    }
}
