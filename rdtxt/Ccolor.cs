using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rdtxt
{
    public class Ccolor
    {
        [CommandMethod("CTKW")]
        public void CTKW()
        {
            //提示用户选择文件夹
            string rootDirectory = "";
            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialog.Description = "请选择文件夹";
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rootDirectory = folderDialog.SelectedPath;
            }
            ProcessAllDWGFiles(rootDirectory);
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n修改完成");
        }

        private void ProcessAllDWGFiles(string directory)
        {
            foreach (string filePath in Directory.GetFiles(directory, "*.dwg"))
            {
                Document doc = Application.DocumentManager.Open(filePath, true);
                DocumentLock m_DocumentLock = doc.LockDocument();
                Database db = doc.Database;

                ChangeLayerColorToWhite(db, doc, "TK");

                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n" + filePath);
                doc.Database.SaveAs(doc.Name, true, DwgVersion.Current, doc.Database.SecurityParameters);
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
        }
        public void ChangeLayerColorToWhite(Database db, Document doc, string layername)
        {
            // 开始事务
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                // 打开当前数据库的图层表
                LayerTable layerTable = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);

                // 检查是否存在名为"TK"的图层
                if (layerTable.Has(layername))
                {
                    // 获取"TK"图层的ObjectId
                    ObjectId tkLayerId = layerTable[layername];

                    // 打开图层以进行写入访问
                    LayerTableRecord tkLayer = (LayerTableRecord)tr.GetObject(tkLayerId, OpenMode.ForWrite);

                    // 设置图层颜色为白色
                    tkLayer.Color = Autodesk.AutoCAD.Colors.Color.FromRgb(255, 255, 255);

                    // 提交事务以保存更改
                    tr.Commit();
                }
                
            }
        }
    }
}
