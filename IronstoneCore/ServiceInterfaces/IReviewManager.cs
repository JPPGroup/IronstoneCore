using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.Autocad;

namespace Jpp.Ironstone.Core.ServiceInterfaces
{
    public interface IReviewManager
    {
        IEnumerable<KeyValuePair<long, DrawingObject>> GetUnverified(Document doc);

        void Verify(Document doc, long id);

        void Refresh(Document doc);

        void FocusOn(Document doc, long id);
    }
}
