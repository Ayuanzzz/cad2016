using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rdtxt
{
    public class help
    {
        [CommandMethod("PHelp")]
        public void PrintText()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor editor = doc.Editor;

            editor.WriteMessage("XColor--注记字体颜色随层\n");

            editor.WriteMessage("ReplaceText--批量替换文本\n");

            editor.WriteMessage("VST--转为竖向字体\n");

            editor.WriteMessage("CD--关闭所有层\n");
        }
    }
}
