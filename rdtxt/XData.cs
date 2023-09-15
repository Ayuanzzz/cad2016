using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using NetOffice.ExcelApi;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace rdtxt
{
    public class XData

    {
        [CommandMethod("Xcolor")]
        public void Xcolor()
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
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n修改完成");
        }

        //遍历所有dwg文件
        private void ProcessAllDWGFiles(string directory)
        {
            foreach (string filePath in Directory.GetFiles(directory, "*.dwg"))
            {
                Document doc = Application.DocumentManager.Open(filePath, true);
                DocumentLock m_DocumentLock = doc.LockDocument();
                Editor ed = doc.Editor;

                ChangeColor(doc, ed, "14", Color.FromRgb(255, 0, 255));
                ChangeColor(doc, ed, "15", Color.FromRgb(255, 0, 255));
                ChangeColor(doc, ed, "16", Color.FromRgb(0, 0, 255));
                ChangeColor(doc, ed, "17", Color.FromRgb(255, 255, 0));
                ChangeColor(doc, ed, "18", Color.FromRgb(0, 255, 255));
                ChangeColor(doc, ed, "20", Color.FromRgb(194, 135, 0));
                ChangeColor(doc, ed, "21", Color.FromRgb(0, 255, 0));

                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n"+filePath);
                doc.Database.SaveAs(doc.Name, true, DwgVersion.Current, doc.Database.SecurityParameters);
                m_DocumentLock.Dispose();
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
        public void GetXData(ObjectId id, Editor ed, Document doc,string XString, Color rgbColor)
        {
            TypedValueList values = new TypedValueList();
            DBObject obj = id.GetObject(OpenMode.ForRead);
            ResultBuffer rb = obj.GetXDataForApplication("SOUTH");
            if (rb != null)
            {
                foreach (TypedValue tv in rb)
                {
                    string sTv = tv.Value.ToString();
                    //截取cass码前两位
                    string subTv = sTv.Substring(0, 2);
                    if (subTv == XString)
                    {
                        using (Transaction tr = doc.TransactionManager.StartTransaction())
                        {
                            Entity ent = (Entity)tr.GetObject(id, OpenMode.ForWrite);
                            ent.Color = rgbColor;
                            tr.Commit();
                        }
                    }
                }
            }
        }

        public void ChangeColor(Document doc,Editor ed,string XString, Color rgbColor)
        {
            using (Transaction tr = doc.TransactionManager.StartTransaction())
            {
                BlockTableRecord modelSpace = tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(doc.Database), OpenMode.ForRead) as BlockTableRecord;

                if (modelSpace != null)
                {
                    foreach (ObjectId objId in modelSpace)
                    {
                        DBObject obj = tr.GetObject(objId, OpenMode.ForRead);

                        if (obj is DBText text)
                        {
                            GetXData(objId, ed, doc, XString, rgbColor);
                        }
                    }
                }
                tr.Commit();
            }
        }

        //[CommandMethod("Test")]
        //public void Test()
        //{
        //    Document doc = Application.DocumentManager.MdiActiveDocument;
        //    Editor ed = doc.Editor;

        //    TypedValueList values = new TypedValueList();

        //    values.Add(DxfCode.ExtendedDataRegAppName, "SOUTH");
        //    values.Add(1000, "200009");



        //    SelectionFilter filter = new SelectionFilter(values);
        //    PromptSelectionResult psr = ed.SelectAll(filter);

        //    if (psr.Status != PromptStatus.OK) return;
        //    SelectionSet ss = psr.Value;
        //    using (Transaction tr = doc.TransactionManager.StartTransaction())
        //    {
        //        foreach (ObjectId id in ss.GetObjectIds())
        //        {
        //            Entity ent = (Entity)tr.GetObject(id, OpenMode.ForWrite);
        //            ent.ColorIndex = 1;
        //        }
        //        tr.Commit();
        //    }
        //}
    }
}