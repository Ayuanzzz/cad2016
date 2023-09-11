using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using Excel = NetOffice.ExcelApi;

namespace rdtxt
{
    public  class replaceByExcel
    {
        [CommandMethod("ReplaceByExcel")]
        public void ReplaceByExcel()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            string fileName = this.OpenFileDialog(ed, db);

            //提示用户选择文件夹
            string rootDirectory = "";
            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialog.Description = "请选择文件夹";
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rootDirectory = folderDialog.SelectedPath;
            }

            GetDataFromExcel(fileName,rootDirectory);
            
            

        }

        //读取excel
        private void GetDataFromExcel(string fileName,string directory)
        {
            Excel.Application excelApp=new Excel.Application();
            Excel.Workbook book = excelApp.Workbooks.Open(fileName);
            Excel.Worksheet sheet = (Excel.Worksheet)book.Worksheets[1];
            int i = 2;
            while (sheet.Cells[i, 1].Value!=null)
            {
                string mapName = (string)sheet.Cells[i, 1].Value;
                string dwgPath=directory+"\\"+mapName+".dwg";
                if (File.Exists(dwgPath))
                {
                    Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(dwgPath + "\n");
                    Document doc = Application.DocumentManager.Open(dwgPath, true);
                    DocumentLock m_DocumentLock = doc.LockDocument();
                    Editor editor = doc.Editor;

                    string surveyor = "测量员:";
                    string surveyorRep = surveyor+(string)sheet.Cells[i, 2].Value;
                    ProcessDWGFile(doc,dwgPath, surveyor, surveyorRep);

                    string draftsman = "绘图员:";
                    string draftsmanRep = draftsman + (string)sheet.Cells[i, 3].Value;
                    ProcessDWGFile(doc,dwgPath, draftsman, draftsmanRep);

                    string inspector = "检查员:";
                    string inspectorRep = inspector+(string)sheet.Cells[i, 4].Value;
                    ProcessDWGFile(doc,dwgPath,inspector, inspectorRep);

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
                i++;
            }
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("修改完成");
            excelApp.Quit();
            excelApp.Dispose();
        }

        //选择excel
        private string OpenFileDialog(Editor ed,Database db)
        {
            string directoryName = Path.GetDirectoryName(db.Filename);
            string fileName=Path.GetFileName(db.Filename);
            fileName=fileName.Substring(0,fileName.IndexOf("."));
            PromptOpenFileOptions opt = new PromptOpenFileOptions("打开Excel文件");
            opt.DialogCaption = "Excel 97-2003 工作簿(*.xls)|*.xls|Excel 工作簿(*.xlsx)|*.xlsx";
            opt.FilterIndex = 1;
            opt.InitialDirectory = directoryName;
            opt.InitialFileName = fileName;
            PromptFileNameResult fileRes = ed.GetFileNameForOpen(opt);
            if(fileRes.Status==PromptStatus.OK)
            {
                fileName = fileRes.StringResult;
            }
            else
            {
                fileName = "";
            }
            return fileName;
        }

        //替换文本
        private void ProcessDWGFile(Document doc,string dwgPath, string searchText, string replaceText)
        {
            //Document doc = Application.DocumentManager.Open(dwgPath, true);
            //DocumentLock m_DocumentLock = doc.LockDocument();
            //Editor editor = doc.Editor;

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
                        text.TextString = text.TextString.Replace(searchText, replaceText);
                    }
                }

                transaction.Commit();
            }
            doc.Database.SaveAs(doc.Name, true, DwgVersion.Current, doc.Database.SecurityParameters);

            //m_DocumentLock.Dispose();
        }
    }
}
