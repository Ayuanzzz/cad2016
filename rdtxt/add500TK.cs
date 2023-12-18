using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace rdtxt
{
    public class add500TK
    {
        [CommandMethod("TK500")]
        public void DrawTmp()
        {
            // 获取当前文档和数据库
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor editor = doc.Editor;

            // 起始坐标
            int minX = 0;
            int minY = 0;

            // 终点坐标
            int maxX = 0;
            int maxY = 0;

            //增量
            int sIncrement = 2000;

            // 请求选择第一个点
            PromptPointResult result1 = editor.GetPoint("请点击图幅西南角: ");

            if (result1.Status == PromptStatus.OK)
            {
                Point3d point1 = result1.Value;
                minX = calculateNum(point1.X);
                minY = calculateNum(point1.Y);

                // 请求选择第二个点
                PromptPointResult result2 = editor.GetPoint("请点击图幅东北角: ");

                if (result2.Status == PromptStatus.OK)
                {
                    Point3d point2 = result2.Value;
                    maxX = calculateNum(point2.X);
                    maxY = calculateNum(point2.Y);

                }
            }
            if (minX > maxX || minY > maxY)
            {
                editor.WriteMessage("坐标错误,请重试");
                return;
            }

            for (int x = minX; x <= maxX; x += sIncrement)
            {
                for (int y = minY; y <= maxY; y += sIncrement)
                {
                    int sX = x;
                    int sY = y;
                    string sTH = (sY / 1000).ToString() + "-" + (sX / 1000).ToString();
                    Create5000(db, sX, sY, sTH,sIncrement/2);
                }
            }
        }

        public void CreateSquare(Database db, double minX, double minY, string name, double increment)
        {
            // 开启事务
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                // 打开块表
                BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                // 打开模型空间
                BlockTableRecord btr = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // 创建正方形的四个角点
                Point2d p1 = new Point2d(minX, minY);
                Point2d p2 = new Point2d(minX + increment, minY);
                Point2d p3 = new Point2d(minX + increment, minY + increment);
                Point2d p4 = new Point2d(minX, minY + increment);

                // 添加正方形到模型空间
                Polyline square = new Polyline();
                square.AddVertexAt(0, p1, 0, 0, 0);
                square.AddVertexAt(1, p2, 0, 0, 0);
                square.AddVertexAt(2, p3, 0, 0, 0);
                square.AddVertexAt(3, p4, 0, 0, 0);
                square.Closed = true;
                btr.AppendEntity(square);
                tr.AddNewlyCreatedDBObject(square, true);

                // 创建文字的插入点
                Point3d insertPoint = new Point3d((double)minX + increment / 2, (double)minY + increment / 2, 0.0);

                // 创建文字对象
                using (DBText text = new DBText())
                {
                    text.Position = insertPoint;
                    text.TextString = name;
                    text.Height = 10;

                    // 设置文字对齐方式为居中
                    text.HorizontalMode = TextHorizontalMode.TextCenter;
                    text.VerticalMode = TextVerticalMode.TextVerticalMid;
                    text.AlignmentPoint = insertPoint;

                    // 将文字添加到模型空间
                    btr.AppendEntity(text);
                    tr.AddNewlyCreatedDBObject(text, true);
                }

                // 提交事务
                tr.Commit();
            }
        }

        private List<List<object>> Factorial(int minX, int minY, string sStr, int increment)
        {
            //Ⅰ
            int oneX = minX;
            int oneY = minY + increment;
            string oneTH = sStr + "-Ⅰ";

            //Ⅱ
            int twoX = minX + increment;
            int twoY = minY + increment;
            string twoTH = sStr + "-Ⅱ";

            //Ⅲ
            int threeX = minX;
            int threeY = minY;
            string threeTH = sStr + "-Ⅲ";

            //Ⅳ
            int fourX = minX + increment;
            int fourY = minY;
            string fourTH = sStr + "-Ⅳ";

            List<List<object>> array = new List<List<object>> {
                new List<object> { oneX, oneY, oneTH },
            new List<object> { twoX, twoY, twoTH },
            new List<object> { threeX, threeY, threeTH },
            new List<object> { fourX, fourY, fourTH }
            };

            return array;
        }

        private void Create5000(Database db, int minX, int minY, string sStr, int increment)
        {
            //2000List
            List<List<object>> arr2000 = Factorial(minX, minY, sStr, increment);

            //1000List
            List<List<List<object>>> arr1000 = new List<List<List<object>>>();
            foreach (var row in arr2000)
            {
                arr1000.Add(Factorial((int)row[0], (int)row[1], row[2].ToString(), increment / 2));
            }

            //500List
            List<List<List<List<object>>>> arr500 = new List<List<List<List<object>>>>();
            foreach (var row in arr1000)
            {
                List<List<List<object>>> tmp_arr1000 = new List<List<List<object>>>();
                foreach (var column in row)
                {
                    tmp_arr1000.Add(Factorial((int)column[0], (int)column[1], column[2].ToString(), increment / 4));
                }
                arr500.Add(tmp_arr1000);
            }

            foreach (var row in arr500)
            {
                foreach (var column in row)
                {
                    foreach (var value in column)
                    {
                        CreateSquare(db, (int)value[0], (int)value[1], value[2].ToString(), increment / 4);
                    }
                }
            }
        }

        private int calculateNum(double inputNum)
        {
            int intValue = Convert.ToInt32(inputNum);
            int divisor = 2000;

            int nearestMultiple = (intValue / divisor) * divisor;

            if (inputNum % divisor >= divisor / 2)
            {
                nearestMultiple += divisor;
            }
            return nearestMultiple;
        }
    }
}
