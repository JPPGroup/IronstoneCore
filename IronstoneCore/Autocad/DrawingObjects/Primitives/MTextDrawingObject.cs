using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Jpp.Ironstone.Core.Autocad.DrawingObjects.Primitives
{
    public class MTextDrawingObject : DrawingObject
    {
        [XmlIgnore]
        public override Point3d Location {
            get
            {
                Transaction acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
                Point3d result;
                using (MText t = acTrans.GetObject(BaseObject, OpenMode.ForRead) as MText)
                {
                    if (t == null)
                    {
                        throw new InvalidOperationException("MText base object not found");
                    }

                    return t.Location;
                }                
            }
            set
            {
                Transaction acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
                using (MText t = acTrans.GetObject(BaseObject, OpenMode.ForWrite) as MText)
                {
                    if (t == null)
                    {
                        throw new InvalidOperationException("MText base object not found");
                    }

                    t.Location = value;
                }
            }
        }
        public override double Rotation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Contents
        {
            get
            {
                Transaction acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
                Point3d result;
                using (MText t = acTrans.GetObject(BaseObject, OpenMode.ForRead) as MText)
                {
                    if (t == null)
                    {
                        throw new InvalidOperationException("MText base object not found");
                    }

                    return t.Contents;
                }
            }
            set
            {
                Transaction acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
                using (MText t = acTrans.GetObject(BaseObject, OpenMode.ForWrite) as MText)
                {
                    if (t == null)
                    {
                        throw new InvalidOperationException("MText base object not found");
                    }

                    t.Contents = value;
                }
            }
        }

        public override void Generate()
        {            
        }

        protected override void ObjectErased(object sender, ObjectErasedEventArgs e)
        {            
        }

        protected override void ObjectModified(object sender, EventArgs e)
        {            
        }

        public static MTextDrawingObject Create(Database target, Point3d location, string text)
        {           
            Transaction trans = target.TransactionManager.TopTransaction;
            BlockTableRecord record = (BlockTableRecord)trans.GetObject(target.CurrentSpaceId, OpenMode.ForWrite);

            return Create(target, record, location, text);
        }

        public static MTextDrawingObject Create(Database target, BlockTableRecord parent, Point3d location, string text)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (location == null)
                throw new ArgumentNullException(nameof(location));

            if (text == null)
                throw new ArgumentNullException(nameof(text));

            if (string.IsNullOrEmpty(text))
                throw new ArgumentOutOfRangeException(nameof(text));

            MTextDrawingObject newTextDrawingObject = new MTextDrawingObject();
            Transaction trans = target.TransactionManager.TopTransaction;

            using (MText mText = new MText())
            {
                // Add the new object to the block table record and the transaction
                mText.Location = location;
                mText.Contents = text;
                newTextDrawingObject.BaseObject = parent.AppendEntity(mText);
                trans.AddNewlyCreatedDBObject(mText, true);
            }

            return newTextDrawingObject;
        }

        private string ConvertToAutocadFormatting(string input)
        {
            //TODO: Add handling for format codes
            return input;
        }

        private string ConvertFromAutocadFormatting(string input)
        {
            return input;
        }
    }
}
