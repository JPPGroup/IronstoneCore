using Autodesk.Windows;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Core.UI.Properties;
using Jpp.Ironstone.Core.UI.ViewModels;
using Jpp.Ironstone.Core.UI.Views;
using Unity;

namespace Jpp.Ironstone.Core.UI
{
    public class CoreUIExtensionApplication : IIronstoneExtensionApplication
    {
        public ILogger Logger { get; private set; }
        public static CoreUIExtensionApplication Current { get; private set; }

        private IUnityContainer _container;

        private RibbonTab _designTab, _conceptTab;

        public void CreateUI()
        {
            //Create the main UI
            CreateTabs();
            CreateCoreMenu(_designTab);
            CreateCoreMenu(_conceptTab);
        }

        public void Initialize()
        {
            Current = this;
            CoreExtensionApplication._current.RegisterExtension(this);
        }

        public void InjectContainer(IUnityContainer container)
        {
            _container = container;

            _container.RegisterType<About>();
            _container.RegisterType<AboutViewModel>();
            Logger = _container.Resolve<ILogger>();
        }

        public void Terminate()
        {
            
        }

        /// <summary>
        /// Creates the JPP tabs and adds it to the ribbon
        /// </summary>
        /// <returns>The main design tab</returns>
        public void CreateTabs()
        {
            RibbonControl rc = ComponentManager.Ribbon;
            _designTab = new RibbonTab
            {
                Name = Resources.ExtensionApplication_IronstoneTab_Design_Name,
                Title = Resources.ExtensionApplication_IronstoneTab_Design_Name,
                Id = Constants.IRONSTONE_TAB_ID
            };

            _conceptTab = new RibbonTab
            {
                Name = Resources.ExtensionApplication_IronstoneTab_Concept_Name,
                Title = Resources.ExtensionApplication_IronstoneTab_Concept_Name,
                Id = Constants.IRONSTONE_CONCEPT_TAB_ID
            };
            
            rc.Tabs.Add(_conceptTab);
            rc.Tabs.Add(_designTab);
        }

        /// <summary>
        /// Add the core elements of the ui
        /// </summary>
        /// <param name="ironstoneTab">The tab to add the ui elements to</param>
        public void CreateCoreMenu(RibbonTab ironstoneTab)
        {
            RibbonPanel panel = new RibbonPanel();
            RibbonPanelSource source = new RibbonPanelSource { Title = "General" };
            RibbonRowPanel stack = new RibbonRowPanel();

            RibbonToggleButton aboutButton = UIHelper.CreateWindowToggle(Resources.ExtensionApplication_AboutWindow_Name, Resources.About, RibbonItemSize.Standard, _container.Resolve<About>(), "10992236-c8f6-4732-b5e0-2d9194f07068");
            RibbonButton feedbackButton = UIHelper.CreateButton(Resources.ExtensionApplication_UI_BtnFeedback, Resources.Feedback, RibbonItemSize.Standard, "Core_Feedback");

            RibbonRowPanel column = new RibbonRowPanel { IsTopJustified = true };
            column.Items.Add(aboutButton);
            column.Items.Add(new RibbonRowBreak());
            column.Items.Add(feedbackButton);

            stack.Items.Add(column);

            //Add the new tab section to the main tab
            source.Items.Add(stack);
            panel.Source = source;
            ironstoneTab.Panels.Add(panel);
        }
    }
}
