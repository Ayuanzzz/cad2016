using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using DotNetArX;
using NetOffice.ExcelApi.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rdtxt
{
    public class AddXData
    {
        //[CommandMethod("AXData")]
        public static void aXData()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor editor = doc.Editor;
            Database db = doc.Database;

            TypedValue[] filter = new TypedValue[] {
                new TypedValue((int)DxfCode.LayerName, "水系"),
                new TypedValue((int)DxfCode.Start, "TEXT")
            };

            SelectionFilter selectionFilter = new SelectionFilter(filter);

            // 选择指定图层中的所有文本
            PromptSelectionResult selectionResult = editor.SelectAll(selectionFilter);

            if (selectionResult.Status == PromptStatus.OK)
            {
                SelectionSet selectionSet = selectionResult.Value;

                // 迭代选择集中的每个实体并获取其ObjectId
                foreach (ObjectId id in selectionSet.GetObjectIds())
                {
                    using(Transaction tr=db.TransactionManager.StartTransaction())
                    {
                        DBObject obj = id.GetObject(OpenMode.ForRead);
                        ResultBuffer textRb = obj.GetXDataForApplication("SOUTH");
                        if (textRb == null)
                        {
                            TypedValueList values = new TypedValueList();
                            values.Add(DxfCode.ExtendedDataAsciiString, "180009");
                            id.AddXData("SOUTH", values);
                        }
                        tr.Commit();
                    }
                }
            }

        }
    }
}
