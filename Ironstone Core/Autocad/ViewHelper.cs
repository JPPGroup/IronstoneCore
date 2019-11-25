using System.Drawing;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Jpp.Ironstone.Core.Autocad
{
    public static class ViewHelper
    {
        /// <summary>
        /// Change the current view to a model space view centered on the provided bounding box.
        /// Based on code from http://help.autodesk.com/view/ACD/2016/ENU/?guid=GUID-FAC1A5EB-2D9E-497B-8FD9-E11D2FF87B93.
        /// </summary>
        /// <param name="boundingRect">Bounding box to focus on in 2d world coordinates.</param>
        public static void FocusBoundingInModel(Rectangle boundingRect)
        {
            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                // TODO: Consider other languages?
                LayoutManager.Current.CurrentLayout = "Model";

                // Get the current view
                using (ViewTableRecord acView = acDoc.Editor.GetCurrentView())
                {
                    // TODO: Pull this value from settings.
                    const double dFactor = 1.1;

                    // Translate WCS coordinates to DCS
                    Matrix3d matWcs2Dcs = Matrix3d.PlaneToWorld(acView.ViewDirection);
                    matWcs2Dcs = Matrix3d.Displacement(acView.Target - Point3d.Origin) * matWcs2Dcs;
                    matWcs2Dcs = Matrix3d.Rotation(-acView.ViewTwist,
                                     acView.ViewDirection,
                                     acView.Target) * matWcs2Dcs;

                    Point3d min = new Point3d(boundingRect.Left, boundingRect.Bottom, 0);
                    Point3d max = new Point3d(boundingRect.Right, boundingRect.Top, 0);

                    Extents3d eExtents = new Extents3d(min, max);

                    // Calculate the ratio between the width and height of the current view
                    double dViewRatio;
                    dViewRatio = acView.Width / acView.Height;

                    // Tranform the extents of the view
                    matWcs2Dcs = matWcs2Dcs.Inverse();
                    eExtents.TransformBy(matWcs2Dcs);

                    double dWidth;
                    double dHeight;
                    Point2d pNewCentPt;

                    // Calculate the new width and height of the current view
                    dWidth = eExtents.MaxPoint.X - eExtents.MinPoint.X;
                    dHeight = eExtents.MaxPoint.Y - eExtents.MinPoint.Y;

                    // Get the center of the view
                    pNewCentPt = new Point2d(((eExtents.MaxPoint.X + eExtents.MinPoint.X) * 0.5),
                        ((eExtents.MaxPoint.Y + eExtents.MinPoint.Y) * 0.5));

                    // Check to see if the new width fits in current window
                    // TODO: Does this work?
                    if (dWidth > (dHeight * dViewRatio))
                        dHeight = dWidth / dViewRatio;

                    // Resize and scale the view to provide a 10% border around object
                    acView.Height = dHeight * dFactor;
                    acView.Width = dWidth * dFactor;

                    // Set the center of the view
                    acView.CenterPoint = pNewCentPt;

                    // Set the current view
                    acDoc.Editor.SetCurrentView(acView);
                }

                // Commit the changes
                acTrans.Commit();
            }
        }
    }
}
