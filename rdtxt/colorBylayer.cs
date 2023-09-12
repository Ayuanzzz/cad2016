using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rdtxt
{
    public class colorBylayer
    {
        [CommandMethod("CBL")]

        public void CBL()
        {
            //提示用户选择文件夹
            string rootDirectory = "";
            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialog.Description = "请选择文件夹";
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rootDirectory = folderDialog.SelectedPath;
            }

            ProcessAllDWGFiles(rootDirectory);
        }
        public void changeColor(string dwgPath)
        {
            Document doc = Application.DocumentManager.Open(dwgPath, true);
            DocumentLock m_DocumentLock = doc.LockDocument();
            Editor ed = doc.Editor;
            Database db = doc.Database;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable blockTable = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                if (blockTable != null)
                {
                    foreach (ObjectId blockId in blockTable)
                    {
                        BlockTableRecord block = tr.GetObject(blockId, OpenMode.ForRead) as BlockTableRecord;
                        if (block != null)
                        {
                            foreach (ObjectId entityId in block)
                            {
                                Entity entity = tr.GetObject(entityId, OpenMode.ForWrite) as Entity;
                                if (entity != null)
                                {
                                    // 设置实体的颜色为BYLAYER
                                    entity.Color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(
                                        Autodesk.AutoCAD.Colors.ColorMethod.ByLayer, 256);
                                }
                            }
                        }
                    }
                    tr.Commit();
                }
            }
            doc.Database.SaveAs(doc.Name, true, DwgVersion.Current, doc.Database.SecurityParameters);
            m_DocumentLock.Dispose();
        }
        public void ZoomWindow(Editor editor, Point3d pt1, Point3d pt2)
        {
            using (Line line = new Line(pt1, pt2))
            {
                Extents3d extents = new Extents3d(line.GeometricExtents.MinPoint, line.GeometricExtents.MaxPoint);
                Point2d minPt = new Point2d(extents.MinPoint.X, extents.MinPoint.Y);
                Point2d maxPt = new Point2d(extents.MaxPoint.X, extents.MaxPoint.Y);
                ////获取当前视图
                //ViewTableRecord view = editor.GetCurrentView();
                ////设置视图
                //view.CenterPoint = minPt + (maxPt - minPt) / 2;
                //view.Height = maxPt.Y - minPt.Y;
                //view.Width = maxPt.X - minPt.X;
                //editor.SetCurrentView(view);
                using(Autodesk.AutoCAD.DatabaseServices.ViewTableRecord view=new ViewTableRecord())
                {
                    //设置视图
                    view.CenterPoint = minPt + (maxPt - minPt) / 2;
                    view.Height = maxPt.Y - minPt.Y;
                    view.Width = maxPt.X - minPt.X;
                    editor.SetCurrentView(view);
                }
            }
        }

        public void ZoomExtents(Editor ed, Database db)
        {
            //更新当前模型空间范围
            db.UpdateExt(true);
            //根据当前图形进行缩放
            if (db.Extmax.X < db.Extmin.X)
            {
                Plane plane = new Plane();
                Point3d pt1 = new Point3d(plane, db.Limmax);
                Point3d pt2 = new Point3d(plane, db.Limmin);
                ZoomWindow(ed, pt1, pt2);
            }
            else
            {
                ZoomWindow(ed, db.Extmin, db.Extmax);
            }
        }

        //遍历所有dwg文件
        private void ProcessAllDWGFiles(string directory)
        {
            foreach (string filePath in Directory.GetFiles(directory, "*.dwg"))
            {
                changeColor(filePath);
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(filePath + "\n");
                // 关闭文档
                DocumentCollection docs = Application.DocumentManager;
                foreach (Document Adoc in docs)
                {
                    if (Adoc.IsReadOnly)
                    {
                        Adoc.CloseAndDiscard();
                    }
                }
            }
        }
    }
}
