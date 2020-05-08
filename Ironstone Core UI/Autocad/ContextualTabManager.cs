using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Windows;
using Jpp.Ironstone.Core.ServiceInterfaces;
using ILogger = Jpp.Ironstone.Core.ServiceInterfaces.ILogger;

namespace Jpp.Ironstone.Core.UI.Autocad
{
    // TODO: Review tests. In particular add cases for detecting mode changes
    class ContextualTabManager
    {
        private List<ContextualTab> _contextTabs;
        private List<RibbonTab> _toActivate;
        private ILogger _logger;
        private ContextualMode _mode = ContextualMode.None;
        
        public ContextualTabManager(ILogger logger)
        {
            _contextTabs = new List<ContextualTab>();
            _toActivate = new List<RibbonTab>();

            foreach (Document document in Application.DocumentManager)
            {
                document.ImpliedSelectionChanged += DocumentOnImpliedSelectionChanged;
                document.CommandEnded += (sender, args) => UpdateMode(document);
            }

            Application.DocumentManager.DocumentCreated += delegate(object sender, DocumentCollectionEventArgs args)
            {
                args.Document.ImpliedSelectionChanged += DocumentOnImpliedSelectionChanged;
                args.Document.CommandEnded += (sender2, args2) => UpdateMode(args.Document);
            };

            Application.DocumentManager.DocumentActivated += (sender, args) => UpdateMode(args.Document);
        }

        private void UpdateMode(Document doc)
        {
            // Wrap in try/catch in case event throws an exception so we dont loose handles
            // TODO: Consider moving to attribute
            try
            {
                if (IsInModel() || IsInLayoutViewport())
                {
                    _mode = ContextualMode.ModelSpace;
                    return;
                }

                if (IsInLayoutPaper())
                {
                    _mode = ContextualMode.PaperSpace;
                    return;
                }

                if (IsInLayoutPaper())
                {
                    _mode = ContextualMode.BlockEdit;
                    return;
                }
            }
            catch (Exception e)
            {
                _logger.Entry($"Unexpected error caught in update contextual mode event: {e.Message}", Severity.Error);
            }
        }

        // TODO: Review next set of methods as "borrowed" from https://spiderinnet1.typepad.com/blog/2014/05/autocad-net-detect-current-space-model-or-paper-and-viewport.html

        public static bool IsInModel()
        {
            if (Application.DocumentManager.MdiActiveDocument.Database.TileMode)
                return true;
            else
                return false;
        }
 
        public static bool IsInLayout()
        {
            return !IsInModel();
        }
 
        public static bool IsInLayoutPaper()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;
            
            if (db.TileMode)
                return false;
            else
            {
                if (db.PaperSpaceVportId == ObjectId.Null)
                    return false;
                else if (ed.CurrentViewportObjectId == ObjectId.Null)
                    return false;
                else if (ed.CurrentViewportObjectId == db.PaperSpaceVportId)
                    return true;
                else
                    return false;
            }
        }

        public static bool IsInBlockEdit()
        {
            return (System.Convert.ToInt32(Application.GetSystemVariable("BLOCKEDITOR "))  == 1);
        }
 
        public static bool IsInLayoutViewport()
        {
            return IsInLayout() && !IsInLayoutPaper();
        }

        /// <summary>
        /// Add a contextual tab with activation delegate. No properties specific to being contextual need to be set
        /// </summary>
        /// <param name="contextualTab">Reference to contextual tab</param>
        /// <param name="filter">Boolean delegate that controls when tab is set active. Called whenever selection changes.</param>
        public void RegisterConceptTab(RibbonTab contextualTab, Func<bool> filter, ContextualMode mode = ContextualMode.All)
        {
            _contextTabs.Add(new ContextualTab()
            {
                Tab = contextualTab,
                Filter = filter,
                Mode = mode
            });
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

                foreach (ContextualTab contextTab in _contextTabs)
                {
                    contextTab.Tab.IsVisible = false;
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
                _logger.Entry($"Unexpected error caught in idle event: {exception.Message}", Severity.Error);
            }
            
        }

        private void DocumentOnImpliedSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                // Remove all active tabs
                _toActivate.Clear();

                foreach (ContextualTab contextTab in _contextTabs)
                {
                    if (contextTab.Mode.HasFlag(_mode) && contextTab.Filter())
                    {
                        _toActivate.Add(contextTab.Tab);
                    }
                }

                Application.Idle += ApplicationOnIdle;
            }
            catch (Exception exception)
            {
                _logger.Entry($"Unexpected error caught in selection changed event: {exception.Message}", Severity.Error);
            }
        }
    }

    [Flags]
    public enum ContextualMode
    {
        None =  0,
        ModelSpace = 1,
        PaperSpace = 2,
        BlockEdit = 4,
        All = ModelSpace | PaperSpace | BlockEdit
    }

    public struct ContextualTab
    {
        public RibbonTab Tab { get; set; }
        public Func<bool> Filter { get; set; }
        public ContextualMode Mode { get; set; }
    }
}
