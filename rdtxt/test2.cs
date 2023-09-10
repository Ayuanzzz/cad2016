using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace rdtxt
{
    public class Commands
    {
        [CommandMethod("OpenDWG")]
        public void OpenDWG()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor editor = doc.Editor;

            string dwgPath = "D:\\C#\\CAD2016\\data\\4562.0-437.0.dwg";

            try
            {
                // 打开指定的DWG文件
                DocumentCollection acDocMgr = Application.DocumentManager;
                Document acDoc = acDocMgr.Open(dwgPath, false);

                string searchText = "测量";
                string replaceText = "吉他";
                using (Transaction transaction = acDoc.TransactionManager.StartTransaction())
                {
                    BlockTable blockTable = transaction.GetObject(acDoc.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
                    BlockTableRecord modelSpace = transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    //BlockTableRecord modelSpace=transaction.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(doc.Database),OpenMode.ForWrite) as BlockTableRecord;

                    foreach (ObjectId objId in modelSpace)
                    {
                        DBObject dbobj = transaction.GetObject(objId, OpenMode.ForWrite);
                        if (dbobj is DBText text)
                        {
                            text.TextString = text.TextString.Replace(searchText, replaceText);
                        }
                    }

                    transaction.Commit();

                    //doc.Database.SaveAs(doc.Name, true, DwgVersion.Current, doc.Database.SecurityParameters);
                    //doc.CloseAndDiscard();
                }

                // 在这里可以执行与文档相关的操作

                editor.WriteMessage($"DWG file opened: {dwgPath}");
            }
            catch (Exception ex)
            {
                editor.WriteMessage($"Error opening DWG file: {ex.Message}");
            }
        }
    }
}
