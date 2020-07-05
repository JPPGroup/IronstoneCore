using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Jpp.Ironstone.Core.Autocad.DrawingObjects.Primitives
{
    /// <summary>
    /// This class is only present currently for the inheritance chain
    /// TODO: Flesh out polyline
    /// </summary>
    public abstract class CurveDrawingObject : DrawingObject
    {
        public bool IsClosed()
        {
            Transaction trans = _database.TransactionManager.TopTransaction;
            
            Curve c = (Curve)trans.GetObject(BaseObject, OpenMode.ForRead);
            return c.Closed;
        }

        public HatchDrawingObject CreateHatch(string patternName)
        {
            Transaction trans = _database.TransactionManager.TopTransaction;
            BlockTableRecord btr = _database.GetModelSpace(true);

            Hatch oHatch = new Hatch();
            Vector3d normal = Vector3d.ZAxis;

            oHatch.Normal = normal;
            oHatch.Elevation = 0.0;
            oHatch.PatternScale = 1.0;
            oHatch.SetHatchPattern(HatchPatternType.PreDefined, patternName);

            btr.AppendEntity(oHatch);
            trans.AddNewlyCreatedDBObject(oHatch, true);

            oHatch.Associative = true;
            ObjectIdCollection acObjIdColl = new ObjectIdCollection();
            acObjIdColl.Add(BaseObject);
            oHatch.AppendLoop(HatchLoopTypes.Default, acObjIdColl);
            oHatch.EvaluateHatch(true);

            return new HatchDrawingObject(oHatch);
        }
    }
}
