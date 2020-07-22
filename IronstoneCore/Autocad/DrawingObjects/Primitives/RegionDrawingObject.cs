using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Jpp.Ironstone.Core.Autocad.DrawingObjects.Primitives
{
    public class RegionDrawingObject : DrawingObject
    {
        protected override void ObjectModified(object sender, EventArgs e)
        {
        }

        protected override void ObjectErased(object sender, ObjectErasedEventArgs e)
        {
        }

        public override Point3d Location { get; set; }

        protected RegionDrawingObject() : base()
        { }

        protected RegionDrawingObject(Document doc) : base(doc)
        { }

        public override void Generate()
        {
            throw new NotImplementedException();
        }

        public override double Rotation { get; set; }

        public void Hatch()
        {
            using (Hatch acHatch = new Hatch())
            {
                Transaction trans = Document.Database.TransactionManager.TopTransaction;
                BlockTableRecord modelSpace = Document.Database.GetModelSpace(true);

                modelSpace.AppendEntity(acHatch);
                trans.AddNewlyCreatedDBObject(acHatch, true);
                HatchDrawingObject hatchDrawingObject = new HatchDrawingObject(acHatch);

                this.SubObjects.Add("Hatch", hatchDrawingObject);
                

                // Set the properties of the hatch object
                // Associative must be set after the hatch object is appended to the 
                // block table record and before AppendLoop
                acHatch.SetHatchPattern(HatchPatternType.PreDefined, "SOLID");
                acHatch.Associative = true;

                /*foreach (Entity enclosed in intersectedRegions)
                {
                    ObjectIdCollection boundary = new ObjectIdCollection();
                    RingsCollection.Add(acBlkTblRec.AppendEntity(enclosed));
                    acTrans.AddNewlyCreatedDBObject(enclosed, true);
                    boundary.Add(enclosed.ObjectId);
                    acHatch.AppendLoop(HatchLoopTypes.Outermost, boundary);
                }*/

                ObjectIdCollection boundary = new ObjectIdCollection();
                boundary.Add(BaseObject);
                acHatch.AppendLoop(HatchLoopTypes.External, boundary);

                acHatch.HatchStyle = HatchStyle.Ignore;
                acHatch.EvaluateHatch(true);

                Byte alpha = (Byte)(255 * (100 - 80) / 100);
                acHatch.Transparency = new Transparency(alpha);
                
                hatchDrawingObject.DrawOnBottom();
            }
        }
    }
}
