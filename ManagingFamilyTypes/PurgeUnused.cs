using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ManagingFamilyTypes
{
    internal class PurgeUnused
    {
        /// <summary>
        ///     ''' The guid of the 'Project contains unused families and types' PerformanceAdviserRuleId.
        ///     ''' </summary>
        const string PurgeGuid = "e8c63650-70b7-435a-9010-ec97660c1bda";
        /// <summary>
        ///     ''' Get all purgeable elements.
        ///     ''' Intended for Revit 2017+ as versions up to and including Revit 2016 throw an InternalException.
        ///     ''' </summary>
        ///     ''' <param name="doc"></param>
        ///     ''' <param name="purgeableElementIds"></param>
        ///     ''' <returns>True if successful.</returns>
        public static bool GetPurgeableElements(Document doc, ref ICollection<ElementId> purgeableElementIds)
        {
            purgeableElementIds = new List<ElementId>();

            try
            {
                // create a new list of rules.
                IList<PerformanceAdviserRuleId> ruleIds = new List<PerformanceAdviserRuleId>();
                PerformanceAdviserRuleId ruleId = null/* TODO Change to default(_) if this is not a reference type */;
                // find the intended rule.
                if (GetPerformanceAdvisorRuleId(PurgeGuid, ref ruleId))
                    // add the rule to the new list.
                    ruleIds.Add(ruleId);
                else
                    // cannot find rule.
                    return false;
                // execute our chosen rule only.
                IList<FailureMessage> failureMessages = PerformanceAdviser.GetPerformanceAdviser().ExecuteRules(doc, ruleIds);
                if (failureMessages.Count > 0)
                    // If there are any purgeable elements, we should have a failure message.
                    // the failure message should have a collection of failing elements - set to our byref collection
                    purgeableElementIds = failureMessages.ElementAt(0).GetFailingElements();
                // no errors - return true.
                return true;
            }
            catch (Autodesk.Revit.Exceptions.InternalException ex)
            {
            }
            // likely thrown an internal exception
            return false;
        }
        /// <summary>
        ///     ''' Find a PerformanceAdviserRuleId with a guid that matches a supplied guid.
        ///     ''' </summary>
        ///     ''' <param name="guidStr"></param>
        ///     ''' <param name="ruleId"></param>
        ///     ''' <returns>true if successful, along with the rule as a byref.</returns>
        private static bool GetPerformanceAdvisorRuleId(string guidStr, ref PerformanceAdviserRuleId ruleId)
        {
            ruleId = null/* TODO Change to default(_) if this is not a reference type */;
            Guid guid = new Guid(guidStr);
            foreach (PerformanceAdviserRuleId rule in PerformanceAdviser.GetPerformanceAdviser().GetAllRuleIds())
            {
                // check if the rule Id matches our rule guid
                if (rule.Guid.Equals(guid))
                {
                    // it does - set rule to our byref object
                    ruleId = rule;
                    return true;
                }
            }
            // failed to find the rule matching our guid.
            return default(Boolean);
        }
    }
}
