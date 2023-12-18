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
using DotNetArX;

namespace rdtxt
{
    public class slopeSplitText
    {
        [CommandMethod("SST")]
        public void CreateText()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor editor = doc.Editor;
            Database db = doc.Database;
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
                            Color textColor = text.Color;// 获取文本的颜色
                            ObjectId textStyleId = text.TextStyleId;//获取字体的样式
                            double obliqueAngle = text.Oblique;//获取字体的倾斜角度
                            ObjectId textlayerId = text.LayerId;
                            //计算字体坐标
                            int strLen = textContent.Length;
                            Point3dCollection point_list = GeneratePointsOnLine(strLen);

                            TextStyleTable textStyleTable = (TextStyleTable)transaction.GetObject(db.TextStyleTableId, OpenMode.ForRead);

                            //拆分字符
                            int i = 0;
                            foreach (char character in textContent)
                            {
                                DBText txt = new DBText();
                                ObjectId id = txt.ObjectId;
                                //文字位置
                                Point3d sPosition = point_list[i];
                                txt.Position = sPosition; // 设置文字位置
                                txt.TextString = character.ToString(); // 设置文字内容
                                txt.Height = textHeight; // 设置文字高度
                                txt.Layer = layerName;
                                txt.Color = textColor;
                                txt.TextStyleId = textStyleId;
                                txt.Oblique = obliqueAngle;
                                txt.LayerId = textlayerId;
                                SplitText.AddTextToAutoCAD(txt);
                                i++;
                            }
                            text.Erase();
                        }
                    }
                    // 提交事务
                    transaction.Commit();
                }
            }
            AddXData.aXData();
        }

        public Point3dCollection GeneratePointsOnLine(int pointNum)
        {
            // 获取当前文档和数据库
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            // 提示用户选择起点
            PromptPointResult startPointResult = doc.Editor.GetPoint("输入起点");
            if (startPointResult.Status != PromptStatus.OK) return null;

            // 提示用户选择终点
            PromptPointResult endPointResult = doc.Editor.GetPoint("输入终点");
            if (endPointResult.Status != PromptStatus.OK) return null;

            // 创建线对象
            Line line = new Line(startPointResult.Value, endPointResult.Value);

            if (line != null)
            {
                // 计算线的起点和终点
                Point3d startPoint = line.StartPoint;
                Point3d endPoint = line.EndPoint;

                // 计算等分点
                Point3dCollection points = new Point3dCollection();
                for (int i = 0; i < pointNum; i++)
                {
                    double t = (i + 1.0) / pointNum; // 等分点的参数值
                    Point3d point = startPoint + t * (endPoint - startPoint);
                    points.Add(point);
                }

                return points;
            }
            return null;
        }

    }
}
