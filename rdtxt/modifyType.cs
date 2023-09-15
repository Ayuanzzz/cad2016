using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rdtxt
{
    public  class modifyType
    {
        [CommandMethod("MT")]
        public void ModifyLinetype()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            PromptEntityOptions peo = new PromptEntityOptions("\nSelect a line: ");
            peo.SetRejectMessage("Selected object is not a line.");
            //peo.AddAllowedClass(typeof(Line), false);

            PromptEntityResult per = ed.GetEntity(peo);

            if (per.Status == PromptStatus.OK)
            {
                using (Transaction tr = doc.TransactionManager.StartTransaction())
                {
                    Polyline polyline = tr.GetObject(per.ObjectId, OpenMode.ForWrite) as Polyline;

                    if (polyline != null)
                    {
                        // 修改线型名称（将"Continuous"替换为您要使用的线型名称）
                        polyline.Linetype = "Continuous";
                    }

                    tr.Commit();
                }
            }
        }
    }
}
