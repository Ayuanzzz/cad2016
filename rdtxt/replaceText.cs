using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadApp = Autodesk.AutoCAD.Windows;
using Wnd = System.Windows.Forms;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace rdtxt
{
    public class replaceText
    {
        //[CommandMethod("OpenTxt")]
        //public void OpenTxt()
        //{
        //    Wnd.OpenFileDialog openDlg=new Wnd.OpenFileDialog();
        //    openDlg.Title = "打开数据文件";
        //    openDlg.Filter = "文本文件(*.txt)|*.txt";
        //    Wnd.DialogResult openRes = openDlg.ShowDialog();
        //    if(openRes==Wnd.DialogResult.OK) 
        //    {
        //        string[] contents=File.ReadAllLines(openDlg.FileName);
        //    }
        //}
        [CommandMethod("ReplaceText")]
        public void ReplaceText()
        {
            //提示用户选择文件夹
            string roorDirectory = "";
            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialog.Description = "请选择文件夹";
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                roorDirectory = folderDialog.SelectedPath;
            }

            string searchText = GetStringFromUser("请输入原始名称");
            if (string.IsNullOrEmpty(searchText))
                return;
            string replaceText = GetStringFromUser("请输入修改名称");
            if (string.IsNullOrEmpty(replaceText))
                return;

            ProcessAllDWGFiles(roorDirectory, searchText, replaceText);

            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("修改完成");
        }
        //获取用户输入
        private string GetStringFromUser(string prompt)
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor editor = doc.Editor;

            PromptResult result = editor.GetString(prompt);
            if (result.Status == PromptStatus.OK)
            {
                return result.StringResult;
            }

            return null;
        }
        //遍历所有dwg文件
        private void ProcessAllDWGFiles(string directory, string searchText, string replaceText)
        {
            foreach (string filePath in Directory.GetFiles(directory, "*.dwg"))
            {
                ProcessDWGFile(filePath, searchText, replaceText);
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(filePath+"\n");
                // 关闭文档
                //Document doc = Application.DocumentManager.MdiActiveDocument;
                //string command = "CD";
                //doc.SendStringToExecute(command + "\n", true, false, false);
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
        //替换文本
        private void ProcessDWGFile(string dwgPath, string searchText, string replaceText)
        {
            Document doc = Application.DocumentManager.Open(dwgPath, true);
            DocumentLock m_DocumentLock = doc.LockDocument();
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
                        text.TextString = text.TextString.Replace(searchText, replaceText);
                    }
                }

                transaction.Commit();
            }
            doc.Database.SaveAs(doc.Name, true, DwgVersion.Current, doc.Database.SecurityParameters);

            m_DocumentLock.Dispose();
        }
    }
}
