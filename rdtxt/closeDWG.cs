using Autodesk.AutoCAD.ApplicationServices;

using Autodesk.AutoCAD.Runtime;

using Autodesk.AutoCAD.Interop;


namespace CloseDocuments

{

    public class Commands

    {

        // If you change the command name, be sure to change

        // the code that checks for it, below.

        //

        [CommandMethod("CD", CommandFlags.Session)]

        static public void CloseDocuments()

        {

            DocumentCollection docs = Application.DocumentManager;

            foreach (Document Adoc in docs)

            {

                // First cancel any running command

                if (Adoc.CommandInProgress != "" &&

                    Adoc.CommandInProgress != "CD")

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

            }

        }

    }

}