using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Windows;
using Microsoft.Extensions.Logging;

namespace Jpp.Ironstone.Core.UI.Autocad
{
    // TODO: Review tests. In particular add cases for detecting mode changes
    class ContextualTabManager
    {
        private List<ContextualTab> _contextTabs;
        private List<RibbonTab> _toActivate;
        private ILogger<ContextualTabManager> _logger;
        private ContextualMode _mode = ContextualMode.None;
        
        public ContextualTabManager(ILogger<ContextualTabManager> logger)
        {
            _contextTabs = new List<ContextualTab>();
            _toActivate = new List<RibbonTab>();
            _logger = logger;

            foreach (Document document in Application.DocumentManager)
            {
                document.ImpliedSelectionChanged += DocumentOnImpliedSelectionChanged;
                // TODO: Not sure these event handles are needed, implied changes seems to trigger on command end anyway
                document.CommandEnded += (sender, args) => UpdateMode(document);
            }

            Application.DocumentManager.DocumentCreated += delegate(object sender, DocumentCollectionEventArgs args)
            {
                args.Document.ImpliedSelectionChanged += DocumentOnImpliedSelectionChanged;
                // TODO: Not sure these event handles are needed, implied changes seems to trigger on command end anyway
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
                if (Application.DocumentManager.MdiActiveDocument == null)
                    return;

                _mode = ContextualMode.None;

                if (IsInModel() || IsInLayoutViewport())
                {
                    _mode = _mode | ContextualMode.ModelSpace;
                }

                if (IsInLayoutPaper())
                {
                    _mode = _mode | ContextualMode.PaperSpace;
                }

                if (IsInBlockEdit())
                {
                    _mode = _mode | ContextualMode.BlockEdit;
                }

                UpdateTabs();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unexpected error caught in update contextual mode event: {e.Message}");
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
            /*int variable = (int) Application.GetSystemVariable("BLOCKEDITOR");
            return (variable  == 1);*/
            return Autodesk.AutoCAD.Internal.AcAeUtilities.IsInBlockEditor();
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
                    contextTab.Tab.IsVisible = _toActivate.Contains(contextTab.Tab);

                    //TODO: Review - do this work?
                    //_toActivate.Last().IsActive = true;
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Unexpected error caught in idle event: {exception.Message}");
            }
            
        }

        private void DocumentOnImpliedSelectionChanged(object sender, EventArgs e)
        {
            UpdateTabs();
        }

        private void UpdateTabs()
        {
            try
            {
                // Remove all active tabs
                _toActivate.Clear();

                foreach (ContextualTab contextTab in _contextTabs)
                {
                    if (_mode.HasFlag(contextTab.Mode) && contextTab.Filter())
                    {
                        _toActivate.Add(contextTab.Tab);
                    }
                }

                Application.Idle += ApplicationOnIdle;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Unexpected error caught in selection changed event: {exception.Message}");
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
