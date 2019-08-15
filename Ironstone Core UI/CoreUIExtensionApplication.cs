using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using Autodesk.Windows;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Core.UI.Properties;
using Jpp.Ironstone.Core.UI.ViewModels;
using Jpp.Ironstone.Core.UI.Views;
using Unity;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: ExtensionApplication(typeof(Jpp.Ironstone.Core.UI.CoreUIExtensionApplication))]
[assembly: CommandClass(typeof(Jpp.Ironstone.Core.UI.CoreUIExtensionApplication))]

namespace Jpp.Ironstone.Core.UI
{
    public class CoreUIExtensionApplication : IIronstoneExtensionApplication
    {
        public ILogger Logger { get; private set; }
        public static CoreUIExtensionApplication Current { get; private set; }

        private IUnityContainer _container;
        private RibbonTab _ironstoneTab;
        private RibbonPanel _debugPanel;

        private bool _debugMenuEnabled = false;

        public void CreateUI()
        {
            Current = this;

            //Create the main UI
            _ironstoneTab = CreateTab();
            CreateCoreMenu();
            CreateDebugMenu();
#if DEBUG
            ToggleDebug();
#endif
        }

        public void Initialize()
        {
            Current = this;
            CoreExtensionApplication._current.RegisterExtension(this);
        }

        public void InjectContainer(IUnityContainer container)
        {
            _container = container;

	        Logger = _container.Resolve<ILogger>();

            _container.RegisterType<About>();
            _container.RegisterType<AboutViewModel>();
            _container.RegisterType<ObjectInspectorView>();
            _container.RegisterType<ObjectInspectorViewModel>();
        }

        public void Terminate()
        {
            
        }

        /// <summary>
        /// Creates the JPP tab and adds it to the ribbon
        /// </summary>
        /// <returns>The created tab</returns>
        public RibbonTab CreateTab()
        {
            RibbonControl rc = ComponentManager.Ribbon;
            RibbonTab ironstoneTab = new RibbonTab
            {
                Name = Constants.IRONSTONE_TAB_TITLE,
                Title = Constants.IRONSTONE_TAB_TITLE,
                Id = Constants.IRONSTONE_TAB_ID
            };

            rc.Tabs.Add(ironstoneTab);
            return ironstoneTab;
        }

        /// <summary>
        /// Add the core elements of the ui
        /// </summary>
        /// <param name="ironstoneTab">The tab to add the ui elements to</param>
        public void CreateCoreMenu()
        {
            RibbonPanel panel = new RibbonPanel();
            RibbonPanelSource source = new RibbonPanelSource { Title = "General" };
            RibbonRowPanel stack = new RibbonRowPanel();

            //stack.Items.Add(_settingsButton);
            RibbonToggleButton aboutButton = UIHelper.CreateWindowToggle(Resources.ExtensionApplication_AboutWindow_Name, Resources.About, RibbonItemSize.Standard, _container.Resolve<About>(), "10992236-c8f6-4732-b5e0-2d9194f07068");

            stack.Items.Add(new RibbonRowBreak());
            stack.Items.Add(aboutButton);

            //Add the new tab section to the main tab
            source.Items.Add(stack);
            panel.Source = source;
            _ironstoneTab.Panels.Add(panel);
        }

        public void CreateDebugMenu()
        {

            //Add the debug menu
            _debugPanel = new RibbonPanel();
            RibbonPanelSource debugSource = new RibbonPanelSource { Title = "Debug" };
            RibbonRowPanel debugStack = new RibbonRowPanel();

            //stack.Items.Add(_settingsButton);
            RibbonButton inspectorButton =
                UIHelper.CreateButton("Inspect Object", Resources.Target_Small, RibbonItemSize.Standard, "InspectObject");

            RibbonButton databaseButton =
                UIHelper.CreateButton("Explore Database", Resources.Database_Small, RibbonItemSize.Standard, "DatabaseExplorer");

            debugStack.Items.Add(new RibbonRowBreak());
            debugStack.Items.Add(inspectorButton);
            debugStack.Items.Add(new RibbonRowBreak());
            debugStack.Items.Add(databaseButton);

            //Add the new tab section to the main tab
            debugSource.Items.Add(debugStack);
            _debugPanel.Source = debugSource;
            
        }

        [CommandMethod("ToggleDebug")]
        public static void ToggleDebug()
        {
            Current._debugMenuEnabled = !Current._debugMenuEnabled;
            if (Current._debugMenuEnabled)
            {
                Current._ironstoneTab.Panels.Add(Current._debugPanel);
            }
            else
            {
                Current._ironstoneTab.Panels.Remove(Current._debugPanel);
            }
        }

        [CommandMethod("InspectObject")]
        public static void InspectObject()
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // Request for objects to be selected in the drawing area
                PromptEntityOptions pee = new PromptEntityOptions("Select object to inspect: ");
                PromptEntityResult per = acDoc.Editor.GetEntity(pee);

                DBObject e = acTrans.GetObject(per.ObjectId, OpenMode.ForRead);

                ObjectInspectorViewModel oivm = new ObjectInspectorViewModel(e);
                ObjectInspectorView oiv = new ObjectInspectorView(oivm);

                PaletteSet paletteSet = new PaletteSet("Inspector", Guid.NewGuid())
                {
                    Size = new System.Drawing.Size(600, 800),
                    Style = (PaletteSetStyles)((int)PaletteSetStyles.ShowAutoHideButton +
                                               (int)PaletteSetStyles.ShowCloseButton),
                    DockEnabled = (DockSides)((int)DockSides.Left + (int)DockSides.Right)
                };

                ElementHost viewHost = new ElementHost();
                viewHost.AutoSize = true;
                viewHost.Dock = DockStyle.Fill;
                viewHost.Child = oiv;
                paletteSet.Add("", viewHost);
                paletteSet.KeepFocus = false;

                paletteSet.Visible = true;

            }
        }

        [CommandMethod("DatabaseExplorer")]
        public static void DatabaseExplorer()
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            // Start a transaction
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                DatabaseExplorerViewModel devm = new DatabaseExplorerViewModel(acDoc);
                DatabaseExplorerView dev = new DatabaseExplorerView(devm);

                PaletteSet paletteSet = new PaletteSet("Database Explorer", Guid.NewGuid())
                {
                    Size = new System.Drawing.Size(600, 800),
                    Style = (PaletteSetStyles)((int)PaletteSetStyles.ShowAutoHideButton +
                                               (int)PaletteSetStyles.ShowCloseButton),
                    DockEnabled = (DockSides)((int)DockSides.Left + (int)DockSides.Right)
                };

                ElementHost viewHost = new ElementHost();
                viewHost.AutoSize = true;
                viewHost.Dock = DockStyle.Fill;
                viewHost.Child = dev;
                paletteSet.Add("", viewHost);
                paletteSet.KeepFocus = false;

                paletteSet.Visible = true;
            }
        }
    }
}
