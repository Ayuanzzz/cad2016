using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rdtxt
{
    public class splitText
    {
        [CommandMethod("VST")]
        public void ReadSelectedText()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor editor = doc.Editor;

            // 提示用户选择要删除的文本
            PromptSelectionResult result = editor.GetSelection();
            if (result.Status == PromptStatus.OK)
            {
                using (Transaction transaction = doc.TransactionManager.StartTransaction())
                {
                    SelectionSet selectionSet = result.Value;
                    foreach (SelectedObject selectedObject in selectionSet)
                    {
                        if (selectedObject.ObjectId.ObjectClass == RXClass.GetClass(typeof(DBText)))
                        {
                            DBText text = (DBText)transaction.GetObject(selectedObject.ObjectId, OpenMode.ForWrite);
                            // 读取文本属性
                            string textContent = text.TextString; // 文本内容
                            double textHeight = text.Height; // 字高
                            Point3d textPosition = text.Position; // 位置
                            string textFont = text.TextStyleName; // 字体名称
                            string layerName = text.Layer;// 获取文本的层名
                            string textStyle = text.TextStyleName;
                            Color textColor = text.Color;// 获取文本的颜色

                            //拆分字符
                            int i = 0;
                            foreach (char character in textContent)
                            {
                                DBText txt = new DBText();
                                //竖直位置
                                double vHeight = i * (textHeight + 2);
                                Point3d vPosition = new Point3d(textPosition.X, textPosition.Y - vHeight, textPosition.Z);
                                txt.Position = vPosition; // 设置文字位置
                                txt.TextString = character.ToString(); // 设置文字内容
                                txt.Height = textHeight; // 设置文字高度
                                txt.Layer = layerName;
                                txt.Color = textColor;
                                AddTextToAutoCAD(txt);
                                i++;
                            }
                            text.Erase();
                        }
                    }
                    // 提交事务
                    transaction.Commit();
                }
            }
        }

        public void AddTextToAutoCAD(DBText text)
        {
            // 获取当前文档和数据库
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            // 启动事务
            using (Transaction transaction = db.TransactionManager.StartTransaction())
            {
                // 打开模型空间（ModelSpace）
                BlockTable blockTable = transaction.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // 添加文字对象到模型空间
                modelSpace.AppendEntity(text);
                transaction.AddNewlyCreatedDBObject(text, true);

                // 提交事务
                transaction.Commit();
            }
        }
    }
}
