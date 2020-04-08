using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Common;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Jpp.Ironstone.Core.UI.ViewModels
{
    public class ReviewViewModel : BaseNotify
    {
        private IReviewManager _manager;
        private Document _currentDocument;
        private IEnumerable<KeyValuePair<long, DrawingObject>> _currentItems;

        public int CurrentItemIndex
        {
            get { return _currentItemIndex; }
            set { SetField(ref _currentItemIndex, value, nameof(CurrentItemIndex)); }
        }
        private int _currentItemIndex;

        public int ItemCount
        {
            get { return _itemCount; }
            set { SetField(ref _itemCount, value, nameof(ItemCount)); }
        }
        private int _itemCount;

        public ICommand VerifyCommand, SkipCommand;

        public ReviewViewModel(IReviewManager review)
        {
            _manager = review;

            VerifyCommand = new Jpp.Common.DelegateCommand(() =>
            {
                _manager.Verify(_currentDocument, _currentItems.ElementAt(CurrentItemIndex).Key);
                CurrentItemIndex++;
            });

            SkipCommand = new Jpp.Common.DelegateCommand(() =>
            {
                CurrentItemIndex++;
            });
        }

        public void Refresh()
        {
            _currentDocument = Application.DocumentManager.MdiActiveDocument;
            CurrentItemIndex = 0;
            _currentItems = _manager.GetUnverified(_currentDocument);
            ItemCount = _currentItems.Count();
        }
    }
}
