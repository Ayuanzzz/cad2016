using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.DatabaseServices.Filters;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.GraphicsInterface;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rdtxt
{
    public class modifyTextStyle
    {
        [CommandMethod("MTS")]

        public void modifyAll()
        {
            modifyTxtStyle("TK", "STANDARD", 12, "中等线体");
            modifyTxtStyle("TK", "STANDARD", 10, "长等线体");
            modifyTxtStyle("TK", "正等线体", 6, "中等线体");
        }
        public void modifyTxtStyle(string layerName, string oldStyle, int txtHeight, string newStyle)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            Database db = doc.Database;

            ObjectId txtId = findTxtId(doc, db, newStyle);

            TypedValue[] values = new TypedValue[]
                {
                new TypedValue((int)DxfCode.LayerName,layerName),
                new TypedValue((int)DxfCode.TextStyleName, oldStyle),
                new TypedValue(40, txtHeight)
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
    }
}
