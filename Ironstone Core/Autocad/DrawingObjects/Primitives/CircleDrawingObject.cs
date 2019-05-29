using System;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Jpp.Ironstone.Core.Autocad
{
    public abstract class CircleDrawingObject : DrawingObject
    {
        [XmlIgnore]
        public override Point3d Location
        {
            get
            {
                Transaction acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
                Point3d result;
                using (Circle c = acTrans.GetObject(BaseObject, OpenMode.ForRead) as Circle)
                {
                    if (c == null)
                    {
                        throw new NullReferenceException();
                    }

                    result = c.Center;
                }

                return result;
            }
            set
            {
                Transaction acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
                (acTrans.GetObject(BaseObject, OpenMode.ForWrite) as Circle).Center = value;
            }
        }

        [XmlIgnore]
        public override double Rotation
        {
            get { return 0; }
            set { return; }
        }
    }
}
