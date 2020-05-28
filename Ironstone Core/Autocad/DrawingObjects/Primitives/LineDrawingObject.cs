using System;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Jpp.Ironstone.Core.Autocad
{
    public class LineDrawingObject : DrawingObject
    {
        private LineDrawingObject()
        { }

        public LineDrawingObject(Document doc) : base(doc)
        { }

        protected override void ObjectModified(object sender, EventArgs e)
        {
        }

        protected override void ObjectErased(object sender, ObjectErasedEventArgs e)
        {
        }

        [XmlIgnore]
        public override Point3d Location
        {
            get
            {
                Transaction acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
                Point3d result;
                using (Line l = acTrans.GetObject(BaseObject, OpenMode.ForRead) as Line)
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

        public override void Generate()
        {
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

        public override void Erase()
        {
        }

        public Point3d StartPoint
        {
            get
            {
                Transaction acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
                Point3d result;
                using (Line l = acTrans.GetObject(BaseObject, OpenMode.ForRead) as Line)
                {
                    return l.StartPoint;
                }
            }
            set
            {
                Transaction acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
                Point3d result;
                using (Line l = acTrans.GetObject(BaseObject, OpenMode.ForWrite) as Line)
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
                using (Line l = acTrans.GetObject(BaseObject, OpenMode.ForRead) as Line)
                {
                    return l.EndPoint;
                }
            }
            set
            {
                Transaction acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
                Point3d result;
                using (Line l = acTrans.GetObject(BaseObject, OpenMode.ForWrite) as Line)
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

        public static LineDrawingObject Create(Database target, Point3d startPoint, Point3d endPoint)
        {
            LineDrawingObject newLineDrawingObject = new LineDrawingObject();

            Transaction trans = target.TransactionManager.TopTransaction;
            BlockTableRecord record = (BlockTableRecord)trans.GetObject(target.CurrentSpaceId, OpenMode.ForWrite);

            Line acLine = new Line(startPoint, endPoint);
 
            // Add the new object to the block table record and the transaction
            newLineDrawingObject.BaseObject = record.AppendEntity(acLine);
            trans.AddNewlyCreatedDBObject(acLine, true);

            return newLineDrawingObject;
        }

        public enum Side
        {
            Left,
            Right
        }
    }
}
