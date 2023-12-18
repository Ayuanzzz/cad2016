using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rdtxt
{
    public class xzDeleteText
    {
        [CommandMethod("deleteText")]

        public void deleteText()
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
                    try
                    {
                        DBObject dbObj = transaction.GetObject(objId, OpenMode.ForWrite);
                        if (dbObj is DBText text)
                        {
                            if (text.TextString == "十、宗教活动场所1：500数字化现状地形图" || text.TextString == "Ⅱ")
                            {
                                // 删除符合条件的文本
                                text.Erase();
                            }
                        }
                    }
                    catch { }
                    
                }
                transaction.Commit();
            }
            m_DocumentLock.Dispose();
        }
    }
}
