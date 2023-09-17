using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rdtxt
{
    public class test
    {
        [CommandMethod("CurveBoolean")]
        public void CurveBoolean()
        {
            ObjectIdCollection polyIds = new ObjectIdCollection();
            if (PromptSelectEnts("\n请选择:", "LWPOLYLINE", ref polyIds))
            {
                Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
                if (polyIds.Count != 2)
                {
                    ed.WriteMessage("\n必选选择两条多段线");
                    return;
                }
                Database db = HostApplicationServices.WorkingDatabase;
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    //获取交点
                    Polyline poly1 = (Polyline)trans.GetObject(polyIds[0], OpenMode.ForRead);
                    Polyline poly2 = (Polyline)trans.GetObject(polyIds[1], OpenMode.ForRead);
                    Point3dCollection point3dCollection = new Point3dCollection();
                    poly1.IntersectWith(poly2, Intersect.OnBothOperands, point3dCollection, 0, 0);
                    // 获取第一个和最后一个点
                    Point3d firstPoint = point3dCollection[0];
                    Point3d lastPoint = point3dCollection[point3dCollection.Count - 1];

                    // 创建新的Point3dCollection并添加点
                    Point3dCollection intPoints = new Point3dCollection();
                    intPoints.Add(firstPoint);
                    intPoints.Add(lastPoint);
                    if (intPoints.Count < 2)
                    {
                        ed.WriteMessage("\n交点少于2个，无法进行计算");
                    }
                    BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
                    GetCurveBetweenIntPoints(trans, btr, poly1, intPoints);
                    GetCurveBetweenIntPoints(trans, btr, poly2, intPoints);
                    trans.Commit();
                }
            }
        }

        public static bool PromptSelectEnts(string prompt, string entTypeFilter, ref ObjectIdCollection entIds)
        {
            Editor ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
            PromptSelectionOptions pso = new PromptSelectionOptions();
            pso.MessageForAdding = prompt;
            SelectionFilter sf = new SelectionFilter(new TypedValue[] { new TypedValue((int)DxfCode.Start, entTypeFilter) });
            PromptSelectionResult psr = ed.GetSelection(pso, sf);
            SelectionSet ss = psr.Value;
            if (ss != null)
            {
                entIds = new ObjectIdCollection(ss.GetObjectIds());
                return entIds.Count > 0;
            }
            else
            {
                return false;
            }
        }

        private void GetCurveBetweenIntPoints(Transaction trans, BlockTableRecord btr, Polyline poly, Point3dCollection points)
        {
            DBObjectCollection curves = poly.GetSplitCurves(points);
            Curve curve = (Curve)trans.GetObject(poly.ObjectId, OpenMode.ForWrite);
            switch (curves.Count)
            {
                case 3:
                    btr.AppendEntity((Entity)curves[0]);
                    trans.AddNewlyCreatedDBObject(curves[0], true);
                    btr.AppendEntity((Entity)curves[2]);
                    trans.AddNewlyCreatedDBObject(curves[2], true);
                    curves[1].Dispose();
                    curve.Erase();
                    break;
                case 2:
                    btr.AppendEntity((Entity)curves[0]);
                    trans.AddNewlyCreatedDBObject(curves[0], true);
                    btr.AppendEntity((Entity)curves[1]);
                    trans.AddNewlyCreatedDBObject(curves[1], true);
                    curve.Erase();
                    break;
                default:
                    break;
            }
        }
    }
}
