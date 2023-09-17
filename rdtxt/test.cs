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
        [CommandMethod("CW")]

        public void CreateWipeout()

        {

            Document doc =

              Application.DocumentManager.MdiActiveDocument;

            Database db = doc.Database;


            Transaction tr =

              db.TransactionManager.StartTransaction();

            using (tr)

            {

                BlockTable bt =

                  (BlockTable)tr.GetObject(

                    db.BlockTableId,

                    OpenMode.ForRead,

                    false

                  );

                BlockTableRecord btr =

                  (BlockTableRecord)tr.GetObject(

                    bt[BlockTableRecord.ModelSpace],

                    OpenMode.ForWrite,

                    false

                  );


                Point2dCollection pts =

                  new Point2dCollection(5);


                pts.Add(new Point2d(0.0, 0.0));

                pts.Add(new Point2d(100.0, 0.0));

                pts.Add(new Point2d(100.0, 100.0));

                pts.Add(new Point2d(0.0, 100.0));

                pts.Add(new Point2d(0.0, 0.0));


                Wipeout wo = new Wipeout();

                wo.SetDatabaseDefaults(db);

                wo.SetFrom(pts, new Vector3d(0.0, 0.0, 0.1));


                btr.AppendEntity(wo);

                tr.AddNewlyCreatedDBObject(wo, true);

                tr.Commit();

            }

        }

        [CommandMethod("AddWipeoutToPolyline")]
        public void AddWipeoutToPolyline()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            // 提示用户选择多段线
            PromptEntityOptions peo = new PromptEntityOptions("\n选择多段线: ");
            peo.SetRejectMessage("\n请选择一个多段线。");
            peo.AddAllowedClass(typeof(Polyline), true);

            PromptEntityResult per = ed.GetEntity(peo);

            if (per.Status != PromptStatus.OK)
                return;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                // 打开多段线以进行读取和编辑
                Polyline polyline = tr.GetObject(per.ObjectId, OpenMode.ForWrite) as Polyline;

                if (polyline != null)
                {
                    // 创建wipeout
                    Wipeout wipeout = new Wipeout();
                    wipeout.SetDatabaseDefaults();
                    Point2dCollection pts = new Point2dCollection();
                    pts = GetPolylineVertices(db,ed,per);
                    if (pts == null)
                        return;

                    wipeout.SetFrom(pts, new Vector3d(0.0, 0.0, 0.1));

                    // 在模型空间中添加Wipeout
                    BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    btr.AppendEntity(wipeout);
                    tr.AddNewlyCreatedDBObject(wipeout, true);

                    tr.Commit();
                }
            }
        }

        [CommandMethod("GetPolylineVertices")]
        public Point2dCollection GetPolylineVertices(Database db,Editor ed,PromptEntityResult per)
        {
            if (per.Status != PromptStatus.OK)
                return null;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                // 打开多段线以进行读取
                Polyline polyline = tr.GetObject(per.ObjectId, OpenMode.ForRead) as Polyline;

                if (polyline != null)
                {
                    // 获取多段线的所有节点坐标并转换为Point2d
                    Point2dCollection pts = new Point2dCollection();

                    for (int i = 0; i < polyline.NumberOfVertices; i++)
                    {
                        Point3d vertex = polyline.GetPoint3dAt(i);
                        pts.Add(new Point2d(vertex.X, vertex.Y));
                    }
                    Point3d LastVertex = polyline.GetPoint3dAt(0);
                    pts.Add(new Point2d(LastVertex.X, LastVertex.Y));

                    return pts;

                    // 输出节点坐标
                    //foreach (Point2d vertex in vertices)
                    //{
                    //    ed.WriteMessage("\nVertex: X = {0}, Y = {1}", vertex.X, vertex.Y);
                    //}
                }

                return null;
            }
        }
    }
}
