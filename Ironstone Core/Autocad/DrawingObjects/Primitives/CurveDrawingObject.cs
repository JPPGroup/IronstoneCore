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
                Transaction acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
                Point3d result;
                using (Curve l = acTrans.GetObject(BaseObject, OpenMode.ForWrite) as Curve)
                {
                    switch (l)
                    {
                        case Line line:
                            return line.Angle;
                            
                        case Polyline polyline:
                            //TODO: Move to generic extension
                            Point3d start3d = polyline.GetPoint3dAt(0);
                            Point3d end3d = polyline.GetPoint3dAt(1);

                            Point3d start = new Point3d(start3d.X, start3d.Y, 0);
                            Point3d end = new Point3d(end3d.X, end3d.Y, 0);
                            Vector3d lineVector = start.GetVectorTo(end);
                            return lineVector.GetAngleTo(Vector3d.XAxis, Vector3d.ZAxis);

                            
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
            set
            {
                throw new NotImplementedException();
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

        protected Curve CreateLeftOffset(double offsetDistance)
        {
            return CreateOffset(offsetDistance, Side.Left);
        }

        protected Curve CreateRightOffset(double offsetDistance)
        {
            return CreateOffset(offsetDistance, Side.Right);
        }

        protected Curve CreateOffset(double offsetDistance, Side side)
        {
            Transaction acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;

            Curve baseEntity = acTrans.GetObject(this.BaseObject, OpenMode.ForRead) as Curve;

            Curve positiveOffset = baseEntity.GetOffsetCurves(offsetDistance)[0] as Curve;
            Curve negativeOffset = baseEntity.GetOffsetCurves(-offsetDistance)[0] as Curve;

            Point2d start = new Point2d(baseEntity.StartPoint.X, baseEntity.StartPoint.Y);
            var offSetVector = start.GetVectorTo(new Point2d(positiveOffset.StartPoint.X, positiveOffset.StartPoint.Y));
            double offsetAngle;
            switch (side)
            {
                case Side.Left:
                    offsetAngle = Rotation + Math.PI / 2;
                    while (offsetAngle >= Math.PI * 2)
                    {
                        offsetAngle -= 2 * Math.PI;
                    }
                    break;

                case Side.Right:
                    offsetAngle = Rotation - Math.PI / 2;
                    while (offsetAngle < 0)
                    {
                        offsetAngle += 2 * Math.PI;
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(side), "Invalid enum");

            }
            if (Math.Abs(offsetAngle - offSetVector.Angle) < Tolerance.Global.GetAngle())
            {
                return positiveOffset;
            }
            else
            {
                return negativeOffset;
            }
        }
    }

    public enum Side
    {
        Left,
        Right
    }
}
