#region Namespaces
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Collections.Generic;

#endregion

namespace ManagingFamilyTypes
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            Document doc = uidoc.Document;
            var familyManager = doc.FamilyManager;      

            // initiate transaction to delete symbols

            using (Transaction tx = new Transaction(doc))
            {
                tx.Start("Delete Types");

                while (familyManager.Types.Size > 2)
                    familyManager.DeleteCurrentType();
                

                tx.Commit();
            }

            ICollection<ElementId> purgeableElements = null;
            if (PurgeUnused.GetPurgeableElements(doc, ref purgeableElements) && purgeableElements.Count > 0)
            {
                // separate transaction for purge; can be used with the previous. Its good for undoing purge, separately.
                using (Transaction transaction = new Transaction(doc, "Purge Unused"))
                {
                    transaction.Start();
                    doc.Delete(purgeableElements);
                    transaction.Commit();
                    return Result.Succeeded;
                }
            }
            else
                return Result.Failed;
        }
    }
}
