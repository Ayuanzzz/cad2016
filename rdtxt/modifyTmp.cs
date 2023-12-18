using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using DotNetArX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rdtxt
{
    public class modifyTmp
    {
        [CommandMethod("modifyTmp")]
        
        public void modifyHeightAndColor()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            DocumentLock m_DocumentLock = doc.LockDocument();

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
                        if (text.Layer == "名称")
                        {
                            text.Height = 0.3;
                            text.Color = Color.FromRgb(0, 255, 0); // 绿色
                        }
                    }
                }
                transaction.Commit();
            }
            doc.Database.SaveAs(doc.Name, true, DwgVersion.Current, doc.Database.SecurityParameters);
        }
    }
}
