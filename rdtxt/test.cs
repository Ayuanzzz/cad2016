using System;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

namespace OpenAndSaveDWG
{
    public class Commands
    {
        [CommandMethod("OpenAndSaveDWG")]
        public void OpenAndSaveDWG()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor editor = doc.Editor;

            string dwgPath = "D:\\C#\\CAD2016\\data\\4562.0-437.0.dwg";

            Database db = new Database(false, true);

            try
            {
                db.ReadDwgFile(dwgPath, FileShare.ReadWrite, true, null);
                db.SaveAs(dwgPath, DwgVersion.Current);
                editor.WriteMessage($"DWG file saved: {dwgPath}");
            }
            catch (Exception ex)
            {
                editor.WriteMessage($"Error opening or saving DWG file: {ex.Message}");
            }
            finally
            {
                db.Dispose();
            }
        }
    }
}
