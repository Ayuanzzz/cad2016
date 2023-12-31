﻿using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using DotNetArX;

namespace rdtxt
{
    public class addwipeout
    {

        [CommandMethod("AWPL")]

        public void AWPL()
        {
            AddWipeoutToPolyline("202101");
            AddWipeoutToPolyline("180009");
            AddWipeoutToPolyline("160009");
            AddWipeoutToPolyline("170009");
            DrawOrder("WIPEOUT", null);
            DrawOrder("LWPOLYLINE", "201101");
            DrawOrder("LWPOLYLINE", "201102");
        }
        public void AddWipeoutToPolyline(string ascStr)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            TypedValueList values = new TypedValueList();

            values.Add((int)DxfCode.Start, "LWPOLYLINE");
            values.Add(DxfCode.ExtendedDataRegAppName, "SOUTH");
            values.Add(DxfCode.ExtendedDataAsciiString, ascStr);

            SelectionFilter filter = new SelectionFilter(values);
            PromptSelectionResult psr = ed.SelectAll(filter);

            if (psr.Status != PromptStatus.OK) return;
            SelectionSet ss = psr.Value;


            using (Transaction tr = db.TransactionManager.StartTransaction())
            {

                foreach (ObjectId id in ss.GetObjectIds())
                {
                    Polyline polyline = (Polyline)tr.GetObject(id, OpenMode.ForWrite);
                    if (polyline != null && polyline.Closed)
                    {
                        // 创建wipeout
                        Wipeout wipeout = new Wipeout();
                        wipeout.SetDatabaseDefaults();
                        Point2dCollection pts = new Point2dCollection();
                        pts = GetPolylineVertices(db, ed, id);
                        if (pts == null)
                            return;

                        wipeout.SetFrom(pts, new Vector3d(0.0, 0.0, 0.1));

                        // 在模型空间中添加Wipeout
                        BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                        BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                        btr.AppendEntity(wipeout);
                        tr.AddNewlyCreatedDBObject(wipeout, true);

                    }
                    polyline.Erase();
                }
                tr.Commit();
            }
        }

        public Point2dCollection GetPolylineVertices(Database db, Editor ed, ObjectId id)
        {
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                // 打开多段线以进行读取
                Polyline polyline = tr.GetObject(id, OpenMode.ForRead) as Polyline;

                
                if (polyline != null && polyline.Closed)
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

                }
                return null;
            }
        }

        public void DrawOrder(string start,string ascStr)

        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            TypedValueList values = new TypedValueList();

            if(ascStr != null)
            {
                values.Add(DxfCode.ExtendedDataAsciiString, ascStr);
            }
            values.Add((int)DxfCode.Start, start);

            SelectionFilter filter = new SelectionFilter(values);
            PromptSelectionResult psr = ed.SelectAll(filter);

            if (psr.Status != PromptStatus.OK) return;
            SelectionSet ss = psr.Value;

            foreach (ObjectId id in ss.GetObjectIds())
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {

                    Entity ent = tr.GetObject(id,OpenMode.ForRead) as Entity;
                    BlockTableRecord block = tr.GetObject(ent.BlockId,OpenMode.ForRead) as BlockTableRecord;
                    DrawOrderTable drawOrder =tr.GetObject(block.DrawOrderTableId,OpenMode.ForWrite) as DrawOrderTable;

                    ObjectIdCollection ids = new ObjectIdCollection();

                    ids.Add(id);

                    drawOrder.MoveToBottom(ids);
                    tr.Commit();
                }
            }

        }

        [CommandMethod("CheckPolylineClosure")]
        public void CheckPolylineClosure()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor editor = doc.Editor;

            // 提示用户选择多段线
            PromptEntityOptions options = new PromptEntityOptions("\n选择多段线: ");
            options.SetRejectMessage("\n请选择一个多段线。");
            options.AddAllowedClass(typeof(Polyline), true);

            PromptEntityResult result = editor.GetEntity(options);

            if (result.Status != PromptStatus.OK)
            {
                editor.WriteMessage("\n未选择多段线。");
                return;
            }

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                // 打开选定的多段线
                Polyline polyline = tr.GetObject(result.ObjectId, OpenMode.ForRead) as Polyline;

                if (polyline != null)
                {
                    // 检查多段线是否闭合
                    bool isClosed = polyline.Closed;

                    if (isClosed)
                    {
                        editor.WriteMessage("\n所选多段线是闭合的。");
                    }
                    else
                    {
                        editor.WriteMessage("\n所选多段线不是闭合的。");
                    }
                }
                else
                {
                    editor.WriteMessage("\n选择的实体不是多段线。");
                }

                tr.Commit();
            }
        }
    }
}
