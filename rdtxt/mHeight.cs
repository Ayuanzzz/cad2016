using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace rdtxt
{
    public class mHeight
    {
        //修改文本高度
        [CommandMethod("mHeight")]
        public void modify()
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
                        if (text.Height == 5)
                        {
                            text.Height = 4;
                        }
                    }
                }
                transaction.Commit();
            }
            doc.Database.SaveAs(doc.Name, true, DwgVersion.Current, doc.Database.SecurityParameters);
        }
    }
}
