using System;
using Autodesk.Windows;
using Jpp.Ironstone.Core.UI.Autocad;
using Jpp.Ironstone.Core.UI.Debug;
using Jpp.Ironstone.Core.UI.Properties;
using Jpp.Ironstone.Core.UI.ViewModels;
using Jpp.Ironstone.Core.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jpp.Ironstone.Core.UI
{
    public class CoreUIExtensionApplication : IIronstoneExtensionApplication
    {
        public ILogger<CoreUIExtensionApplication> Logger { get; private set; }
        public static CoreUIExtensionApplication Current { get; private set; }

        private IServiceProvider _container;
        private RibbonTab _generalTab, _designTab, _conceptTab;
        private ContextualTabManager _contextualTabManager;

        public void CreateUI()
        {
            //Create the main UI
            CreateTabs();
            CreateCoreMenu(_generalTab);
        }

        public void Initialize()
        {
            Current = this;
            CoreExtensionApplication._current.RegisterExtension(this);
        }

        public void RegisterServices(IServiceCollection container)
        {
            container.AddSingleton<About>();
            container.AddSingleton<AboutViewModel>();
            container.AddSingleton<Review>();
            container.AddSingleton<ReviewViewModel>();
            container.AddSingleton<Libraries>();
            container.AddSingleton<LibrariesViewModel>();
            container.AddSingleton<DebugView>();
            container.AddSingleton<DebugViewModel>();
        }

        public void InjectContainer(IServiceProvider container)
        {
            _container = container;
            Logger = _container.GetRequiredService<ILogger<CoreUIExtensionApplication>>();

            _contextualTabManager = new ContextualTabManager(_container.GetRequiredService<ILogger<ContextualTabManager>>());
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

            _generalTab = new RibbonTab
            {
                Name = Resources.ExtensionApplication_IronstoneTab_General_Name,
                Title = Resources.ExtensionApplication_IronstoneTab_General_Name,
                Id = Ironstone.Core.Constants.IronstoneGeneralTabId
            };

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

            rc.Tabs.Add(_generalTab);
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

            RibbonToggleButton aboutButton = UIHelper.CreateWindowToggle(Resources.ExtensionApplication_AboutWindow_Name, Resources.About, RibbonItemSize.Standard, _container.GetRequiredService<About>(), "10992236-c8f6-4732-b5e0-2d9194f07068");
            RibbonButton feedbackButton = UIHelper.CreateButton(Resources.ExtensionApplication_UI_BtnFeedback, Resources.Feedback, RibbonItemSize.Standard, "Core_Feedback");
            RibbonToggleButton reviewButton = UIHelper.CreateWindowToggle(Resources.ExtensionApplication_ReviewWindow_Name, Resources.Review, RibbonItemSize.Large, _container.GetRequiredService<Review>(), "18cd4414-8fc8-4978-9e97-ae3915e29e07");
            RibbonToggleButton libraryButton = UIHelper.CreateWindowToggle(Resources.ExtensionApplication_LibraryWindow_Name, Resources.Library_Small, RibbonItemSize.Standard, _container.GetRequiredService<Libraries>(), "08ccb73d-6e6b-4ea0-8d99-61bbeb7c20af");
            
            RibbonRowPanel column = new RibbonRowPanel { IsTopJustified = true };
            column.Items.Add(aboutButton);
            column.Items.Add(new RibbonRowBreak());
            column.Items.Add(feedbackButton);
            column.Items.Add(new RibbonRowBreak());
            column.Items.Add(libraryButton);

            RibbonRowPanel stack = new RibbonRowPanel();
            stack.Items.Add(column);
            stack.Items.Add(reviewButton);

            //Add the new tab section to the main tab
            source.Items.Add(stack);
            panel.Source = source;
            ironstoneTab.Panels.Add(panel);

            RibbonToggleButton debugButton = UIHelper.CreateWindowToggle(Resources.ExtensionApplication_DebugStoreWindow_Name, Resources.Debug, RibbonItemSize.Large, _container.GetRequiredService<DebugView>(), "E019A23D-FE0C-4E03-BB6C-FE5CBE016841");
            RibbonPanel debugPanel = new RibbonPanel();
            RibbonPanelSource debugSource = new RibbonPanelSource { Title = Resources.ExtensionApplication_IronstoneTab_Debug_Name };
            debugSource.Items.Add(debugButton);
            debugPanel.Source = debugSource;
            ironstoneTab.Panels.Add(debugPanel);
        }
    }
}
