using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using DotNetArX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rdtxt
{
    public class textAndColor
    {
        [CommandMethod("textAndColor")]

        public void modifyAll()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            DocumentLock m_DocumentLock = doc.LockDocument();
            Editor ed = doc.Editor;
            Database db = doc.Database;

            modifyTxtStyle(doc, ed, db, "塘", "细等线体");

            ChangeColor(doc, ed, "140092", Color.FromRgb(255, 255, 255));
            ChangeColor(doc, ed, "140095", Color.FromRgb(255, 255, 255));
            ChangeColor(doc, ed, "140096", Color.FromRgb(255, 255, 255));
            ChangeColor(doc, ed, "9022221", Color.FromRgb(255, 255, 255));

            m_DocumentLock.Dispose();
        }
        public void modifyTxtStyle(Document doc, Editor ed, Database db, string txt, string newStyle)
        {
            ObjectId txtId = findTxtId(doc, db, newStyle);

            TypedValue[] values = new TypedValue[]
                {
                new TypedValue((int)DxfCode.Text,txt),
                };
            SelectionFilter filter = new SelectionFilter(values);
            PromptSelectionResult psr = ed.SelectAll(filter);
            if (psr.Status == PromptStatus.OK)
            {
                SelectionSet selectionSet = psr.Value;

                // 启动事务以进行更改
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    foreach (ObjectId entityId in selectionSet.GetObjectIds())
                    {
                        // 打开文本对象以进行修改
                        DBText text = trans.GetObject(entityId, OpenMode.ForWrite) as DBText;

                        text.TextStyleId = txtId;
                    }
                    // 提交事务
                    trans.Commit();
                }
            }
        }

        public ObjectId findTxtId(Document doc, Database db, string textStyleName)
        {
            // 获取当前数据库的TextStyle表
            TextStyleTable textStyleTable;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                textStyleTable = trans.GetObject(db.TextStyleTableId, OpenMode.ForRead) as TextStyleTable;
                if (textStyleTable == null)
                {
                    Application.ShowAlertDialog("未能打开TextStyle表。");
                    return ObjectId.Null;
                }

                // 查找字体名称为"细等线体"的TextStyleId
                ObjectId textStyleId = ObjectId.Null;

                foreach (ObjectId styleId in textStyleTable)
                {
                    TextStyleTableRecord textStyle = styleId.GetObject(OpenMode.ForRead) as TextStyleTableRecord;
                    if (textStyle != null && textStyle.Name.Equals(textStyleName, StringComparison.OrdinalIgnoreCase))
                    {
                        textStyleId = styleId;
                        break;
                    }
                }
                return textStyleId;
                trans.Commit();
            }
        }


        public void GetXData(ObjectId id, Editor ed, Document doc, string XString, Color rgbColor)
        {
            TypedValueList values = new TypedValueList();
            DBObject obj = id.GetObject(OpenMode.ForRead);
            ResultBuffer rb = obj.GetXDataForApplication("SOUTH");
            if (rb != null)
            {
                foreach (TypedValue tv in rb)
                {
                    string sTv = tv.Value.ToString();
                    if (sTv == XString)
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

        public void ChangeColor(Document doc, Editor ed, string XString, Color rgbColor)
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
    }
}
