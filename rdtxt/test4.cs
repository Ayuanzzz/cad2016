using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Interop;
using Acap = Autodesk.AutoCAD.ApplicationServices.Application;


namespace ReplaceAllTextInDWG
{
    public class Commands
    {
        [CommandMethod("ReplaceAllText")]
        public void ReplaceAllText()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor editor = doc.Editor;

            // 开始事务
            using (Transaction transaction = doc.TransactionManager.StartTransaction())
            {
                // 打开文档的模型空间
                BlockTable blockTable = transaction.GetObject(doc.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;

                // 遍历模型空间中的所有文本对象
                foreach (ObjectId objId in modelSpace)
                {
                    DBObject dbObj = transaction.GetObject(objId, OpenMode.ForWrite);
                    if (dbObj is DBText text)
                    {
                        // 查找并替换文本
                        text.TextString = text.TextString.Replace("测量员", "huhu");
                    }
                }

                // 提交事务
                transaction.Commit();
            }
            editor.WriteMessage("Text replacement complete. 'a' replaced with 'b' in all text objects. Document saved and closed.");
            //doc.Database.SaveAs(doc.Name, true, DwgVersion.Current, doc.Database.SecurityParameters);
            // 关闭文档
            string command = "CD"; // 你可以替换为你想要执行的任何AutoCAD命令
            doc.SendStringToExecute(command + "\n", true, false, false);
        }
        
     
    }
}
