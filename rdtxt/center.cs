using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rdtxt
{
    public class center
    {
        [CommandMethod("ToCenter")]

        public void ToCenter()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor editor = doc.Editor;
            //提示用户选择文件夹
            string roorDirectory = "";
            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialog.Description = "请选择文件夹";
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                roorDirectory = folderDialog.SelectedPath;
            }

            foreach (string filePath in Directory.GetFiles(roorDirectory, "*.dwg"))
            {
                ChangeByLayer(filePath);
                editor.WriteMessage(filePath + "\n");
                // 关闭文档
                DocumentCollection docs = Application.DocumentManager;
                foreach (Document Adoc in docs)
                {
                    if (Adoc.CommandInProgress != "")
                    {
                        AcadDocument oDoc = (AcadDocument)Adoc.GetAcadDocument();
                        oDoc.SendCommand("\x03\x03");
                    }
                    if (Adoc.IsReadOnly)
                    {
                        Adoc.CloseAndDiscard();
                    }
                }
                //CloseDocuments();
            }
            editor.WriteMessage("修改完成");

        }
        public void ChangeByLayer(string dwgPath)
        {
            Document doc = Application.DocumentManager.Open(dwgPath, true);
            DocumentLock m_DocumentLock = doc.LockDocument();
            Editor ed = doc.Editor;
            //string command1 = "change\n" + "all\n" + "\n" + "p\n" + "c\n" + "bylayer\n" + "\n\n";
            string command2 = "zoom\n"+"e\n"; // 你可以替换为你想要执行的任何AutoCAD命令
            //doc.SendStringToExecute(command1, true, false, false);
            doc.SendStringToExecute(command2, true, false, false);
            ed.Regen();


            //doc.Database.SaveAs(doc.Name, true, DwgVersion.Current, doc.Database.SecurityParameters);
            m_DocumentLock.Dispose();
        }

        public void CloseDocuments()

        {

            DocumentCollection docs = Application.DocumentManager;

            foreach (Document Adoc in docs)

            {
                DocumentLock ADocumentLock = Adoc.LockDocument();
                // First cancel any running command

                if (Adoc.CommandInProgress != "")

                {

                    AcadDocument oDoc =

                      (AcadDocument)Adoc.GetAcadDocument();

                    oDoc.SendCommand("\x03\x03");

                }


                if (Adoc.IsReadOnly)

                {

                    Adoc.CloseAndDiscard();

                }

                else

                {

                    // Activate the document, so we can check DBMOD

                    if (docs.MdiActiveDocument != Adoc)

                    {

                        docs.MdiActiveDocument = Adoc;

                    }

                    int isModified =

                      System.Convert.ToInt32(

                        Application.GetSystemVariable("DBMOD")

                      );


                    // No need to save if not modified

                    if (isModified == 0)

                    {

                        Adoc.CloseAndDiscard();

                    }

                    else

                    {

                        // This may create documents in strange places

                        Adoc.CloseAndSave(Adoc.Name);

                    }

                }
                ADocumentLock.Dispose();

            }

        }
    }
}
