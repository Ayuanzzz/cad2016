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
    public class MoveText
    {
        [CommandMethod("moveText")]

        public void iterator()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            using (Transaction tr = doc.TransactionManager.StartTransaction())
            {
                BlockTableRecord modelSpace = tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(doc.Database), OpenMode.ForRead) as BlockTableRecord;

                if (modelSpace != null)
                {
                    foreach (ObjectId objId in modelSpace)
                    {
                        DBObject obj = tr.GetObject(objId, OpenMode.ForRead);

                        if (obj is DBText text)
                        {
                            GetXData(objId, doc, db);
                        }
                    }
                }
                tr.Commit();
            }
        }
        public void GetXData(ObjectId id, Document doc, Database db)
        {
            TypedValueList values = new TypedValueList();
            DBObject obj = id.GetObject(OpenMode.ForRead);
            ResultBuffer rb = obj.GetXDataForApplication("SOUTH");
            if (rb != null)
            {
                foreach (TypedValue tv in rb)
                {
                    string sTv = tv.Value.ToString();
                    if (sTv == "202101")
                    {
                        using (Transaction tr = doc.TransactionManager.StartTransaction())
                        {
                            Entity ent = (Entity)tr.GetObject(id, OpenMode.ForWrite);
                            move(ent, db);
                            tr.Commit();
                        }
                    }
                }
            }
        }

        public void move(Entity ent, Database db)
        {
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                if (ent is DBText text)
                {
                    // 移动文本对象的位置
                    Point3d startPt = new Point3d(0, 0, 0); 
                    Vector3d destVector = startPt.GetVectorTo(new Point3d(2.0, 0.0, 0.0));
                    text.TransformBy(Matrix3d.Displacement(destVector));
                }
                trans.Commit();
            }
        }


    }
}
