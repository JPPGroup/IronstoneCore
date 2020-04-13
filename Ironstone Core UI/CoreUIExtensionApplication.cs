using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Windows;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Core.UI.Properties;
using Jpp.Ironstone.Core.UI.ViewModels;
using Jpp.Ironstone.Core.UI.Views;
using Unity;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Jpp.Ironstone.Core.UI
{
    public class CoreUIExtensionApplication : IIronstoneExtensionApplication
    {
        public ILogger Logger { get; private set; }
        public static CoreUIExtensionApplication Current { get; private set; }

        private IUnityContainer _container;

        private RibbonTab _designTab, _conceptTab;
        private List<Tuple<RibbonTab, Func<bool>>> _contextTabs;
        private List<RibbonTab> _toActivate;

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
            _contextTabs = new List<Tuple<RibbonTab, Func<bool>>>();
            _toActivate = new List<RibbonTab>();

            foreach (Document document in Application.DocumentManager)
            {
                document.ImpliedSelectionChanged += DocumentOnImpliedSelectionChanged;
            }

            Application.DocumentManager.DocumentCreated += delegate(object sender, DocumentCollectionEventArgs args)
            {
                args.Document.ImpliedSelectionChanged += DocumentOnImpliedSelectionChanged;
            };
        }

        public void InjectContainer(IUnityContainer container)
        {
            _container = container;

            _container.RegisterType<About>();
            _container.RegisterType<AboutViewModel>();
            _container.RegisterType<Review>();
            Logger = _container.Resolve<ILogger>();
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
            _contextTabs.Add(new Tuple<RibbonTab, Func<bool>>(contextualTab, filter));
            contextualTab.IsVisible = false;
            contextualTab.IsContextualTab = true;
            ComponentManager.Ribbon.Tabs.Add(contextualTab);
        }

        // Triggered when idle to display tab. Immediately unregisters event for efficiency
        private void ApplicationOnIdle(object sender, EventArgs e)
        {
            try
            {
                Application.Idle -= ApplicationOnIdle;

                foreach (Tuple<RibbonTab, Func<bool>> contextTab in _contextTabs)
                {
                    contextTab.Item1.IsVisible = false;
                }

                if (_toActivate.Any())
                {

                    foreach (RibbonTab ribbonTab in _toActivate)
                    {
                        ribbonTab.IsVisible = true;
                    }

                    _toActivate.Last().IsActive = true;
                }
            }
            catch (Exception exception)
            {
                Logger.Entry($"Unexpected error caught in idle event: {exception.Message}", Severity.Error);
            }
            
        }

        private void DocumentOnImpliedSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                // Remove all active tabs
                _toActivate.Clear();

                foreach (Tuple<RibbonTab, Func<bool>> contextTab in _contextTabs)
                {
                    if (contextTab.Item2())
                    {
                        _toActivate.Add(contextTab.Item1);
                    }
                }

                Application.Idle += ApplicationOnIdle;
            }
            catch (Exception exception)
            {
                Logger.Entry($"Unexpected error caught in selection changed event: {exception.Message}", Severity.Error);
            }
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
                Id = Ironstone.Core.Constants.IRONSTONE_TAB_ID
            };

            _conceptTab = new RibbonTab
            {
                Name = Resources.ExtensionApplication_IronstoneTab_Concept_Name,
                Title = Resources.ExtensionApplication_IronstoneTab_Concept_Name,
                Id = Ironstone.Core.Constants.IRONSTONE_CONCEPT_TAB_ID
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
