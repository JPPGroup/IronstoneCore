using System;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Jpp.Ironstone.Core.Autocad
{
    public abstract class CurveDrawingObject : DrawingObject
    {
        [XmlIgnore]
        public override Point3d Location
        {
            get
            {
                Transaction acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
                Point3d result;
                using (Curve l = acTrans.GetObject(BaseObject, OpenMode.ForRead) as Curve)
                {
                    if (l == null)
                    {
                        throw new NullReferenceException();
                    }

                    Vector3d lineVector = l.EndPoint.GetAsVector() - l.StartPoint.GetAsVector();

                    result = l.StartPoint + lineVector * 0.5;
                }
                return result;
            }
            set
            {
                Transaction acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
                //(acTrans.GetObject(BaseObject, OpenMode.ForWrite) as Circle).Center = value;
                throw new NotImplementedException();
            }
        }

        [XmlIgnore]
        public override double Rotation
        {
            get
            {
                return 0;
            }
            set
            {
                return;
            }
        }

        public Point3d StartPoint
        {
            get
            {
                Transaction acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
                Point3d result;
                using (Curve l = acTrans.GetObject(BaseObject, OpenMode.ForRead) as Curve)
                {
                    return l.StartPoint;
                }
            }
            set
            {
                Transaction acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
                Point3d result;
                using (Curve l = acTrans.GetObject(BaseObject, OpenMode.ForWrite) as Curve)
                {
                    l.StartPoint = value;
                }
            }
        }

        public Point3d EndPoint
        {
            get
            {
                Transaction acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
                Point3d result;
                using (Curve l = acTrans.GetObject(BaseObject, OpenMode.ForRead) as Curve)
                {
                    return l.EndPoint;
                }
            }
            set
            {
                Transaction acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
                Point3d result;
                using (Curve l = acTrans.GetObject(BaseObject, OpenMode.ForWrite) as Curve)
                {
                    l.EndPoint = value;
                }
            }
        }
    }
}
