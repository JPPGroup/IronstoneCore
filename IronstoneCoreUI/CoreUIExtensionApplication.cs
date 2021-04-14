using System;
using Autodesk.Windows;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Core.UI.Autocad;
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
        private ContextualTabManager _contextualTabManager;

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
            _container.RegisterType<Review>();
            Logger = _container.Resolve<ILogger>();
            _contextualTabManager = new ContextualTabManager(Logger);
        }

        public void Terminate()
        {
            
        }

        /// <summary>
        /// Add a contextual tab with activation delegate. No properties specific to being contextual need to be set
        /// </summary>
        /// <param name="contextualTab">Reference to contextual tab</param>
        /// <param name="filter">Boolean delegate that controls when tab is set active. Called whenever selection changes.</param>
        public void RegisterConceptTab(RibbonTab contextualTab, Func<bool> filter)
        {
            _contextualTabManager.RegisterConceptTab(contextualTab, filter);
        }

        /// <summary>
        /// Add a contextual tab with activation delegate. No properties specific to being contextual need to be set
        /// </summary>
        /// <param name="contextualTab">Reference to contextual tab</param>
        /// <param name="filter">Boolean delegate that controls when tab is set active. Called whenever selection changes.</param>
        /// /// <param name="mode">Enum flags indicating when this tab is valid to be shown</param>
        public void RegisterConceptTab(RibbonTab contextualTab, Func<bool> filter, ContextualMode mode)
        {
            _contextualTabManager.RegisterConceptTab(contextualTab, filter, mode);
        }

        /// <summary>
        /// Creates the JPP tabs and adds it to the ribbon
        /// </summary>
        /// <returns>The main design tab</returns>
        private void CreateTabs()
        {
            RibbonControl rc = ComponentManager.Ribbon;
            _designTab = new RibbonTab
            {
                Name = Resources.ExtensionApplication_IronstoneTab_Design_Name,
                Title = Resources.ExtensionApplication_IronstoneTab_Design_Name,
                Id = Ironstone.Core.Constants.IronstoneTabId
            };

            _conceptTab = new RibbonTab
            {
                Name = Resources.ExtensionApplication_IronstoneTab_Concept_Name,
                Title = Resources.ExtensionApplication_IronstoneTab_Concept_Name,
                Id = Ironstone.Core.Constants.IronstoneConceptTabId
            };
            
            rc.Tabs.Add(_conceptTab);
            rc.Tabs.Add(_designTab);
        }

        /// <summary>
        /// Add the core elements of the ui
        /// </summary>
        /// <param name="ironstoneTab">The tab to add the ui elements to</param>
        private void CreateCoreMenu(RibbonTab ironstoneTab)
        {
            RibbonPanel panel = new RibbonPanel();
            RibbonPanelSource source = new RibbonPanelSource { Title = "General" };
            RibbonRowPanel stack = new RibbonRowPanel();

            RibbonToggleButton aboutButton = UIHelper.CreateWindowToggle(Resources.ExtensionApplication_AboutWindow_Name, Resources.About, RibbonItemSize.Standard, _container.Resolve<About>(), "10992236-c8f6-4732-b5e0-2d9194f07068");
            RibbonButton feedbackButton = UIHelper.CreateButton(Resources.ExtensionApplication_UI_BtnFeedback, Resources.Feedback, RibbonItemSize.Standard, "Core_Feedback");
            RibbonToggleButton reviewButton = UIHelper.CreateWindowToggle(Resources.ExtensionApplication_ReviewWindow_Name, Resources.Review, RibbonItemSize.Large, _container.Resolve<Review>(), "18cd4414-8fc8-4978-9e97-ae3915e29e07");
            RibbonToggleButton libraryButton = UIHelper.CreateWindowToggle(Resources.ExtensionApplication_LibraryWindow_Name, Resources.Library_Small, RibbonItemSize.Standard, _container.Resolve<Libraries>(), "08ccb73d-6e6b-4ea0-8d99-61bbeb7c20af");

            RibbonRowPanel column = new RibbonRowPanel { IsTopJustified = true };
            column.Items.Add(aboutButton);
            column.Items.Add(new RibbonRowBreak());
            column.Items.Add(feedbackButton);
            column.Items.Add(new RibbonRowBreak());
            column.Items.Add(libraryButton);
            
            stack.Items.Add(column);
            stack.Items.Add(reviewButton);

            //Add the new tab section to the main tab
            source.Items.Add(stack);
            panel.Source = source;
            ironstoneTab.Panels.Add(panel);
        }
    }
}
