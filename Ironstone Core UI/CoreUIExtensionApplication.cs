using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Autodesk.Windows;
using Jpp.Ironstone.Core.Properties;
using Jpp.Ironstone.Core.UI.Properties;
using Jpp.Ironstone.Core.UI.ViewModels;
using Jpp.Ironstone.Core.UI.Views;
using Unity;

namespace Jpp.Ironstone.Core.UI
{
    public class CoreUIExtensionApplication : IIronstoneExtensionApplication
    {
        private IUnityContainer _container;

        public void CreateUI()
        {
            //Create the main UI
            RibbonTab ironstoneTab = CreateTab();
            CreateCoreMenu(ironstoneTab);
        }

        public void Initialize()
        {
            CoreExtensionApplication._current.RegisterExtension(this);
        }

        public void InjectContainer(Unity.IUnityContainer container)
        {
            _container = container;
            _container.RegisterType<About>();
            _container.RegisterType<AboutViewModel>();
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
        public void CreateCoreMenu(RibbonTab ironstoneTab)
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
            ironstoneTab.Panels.Add(panel);
        }
    }
}
