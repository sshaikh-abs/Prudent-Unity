using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public static class GroupingManagerExtension
{
    //public static string GetActualCompartmentName(this GroupingManager groupingManager, string compartmentName)
    //{
    //    string foundName = null;
    //    groupingManager.RunOverCompartments(c =>
    //    {
    //        string trimmedCompartmentName = compartmentName;

    //        /// this might be differnt for differnt ships as _ASM suffix might just be for 1 ship as of now, so might have to remove later.
    //        if (compartmentName.Contains("_ASM"))
    //        {
    //            trimmedCompartmentName = Regex.Replace(compartmentName, "_ASM$", "");
    //        }

    //        if (c.gameObject.name.Contains(trimmedCompartmentName))
    //        {
    //            foundName = c.gameObject.name;
    //        }
    //    });

    //    return foundName;
    //}

    //public static string GetActualFrameName(this GroupingManager groupingManager, string frameName)
    //{
    //    string foundName = null;
    //    groupingManager.RunOverFrames(f =>
    //    {
    //        if (f.gameObject.name.Contains(frameName))
    //        {
    //            foundName = f.gameObject.name;
    //        }
    //    });

    //    return foundName;
    //}

    //public static List<GameObject> GetCompartmentGameobjects(this GroupingManager groupingManager, string vesselName)
    //{
    //    return groupingManager.vesselObject.compartments.Select(c => c.Value.compartment).ToList(); ;
    //}

    //public static GameObject GetCompartmentGameobject(this GroupingManager groupingManager, string vesselName, string compartment)
    //{
    //    return GetCompartment(groupingManager, vesselName, compartment).compartment;
    //}

    //public static CompartmentObject GetCompartment(this GroupingManager groupingManager, string vesselName, string compartment)
    //{
    //    return groupingManager.vesselObject.GetCompartment(compartment);
    //}

    //public static List<GameObject> GetHullPartGameobjects(this GroupingManager groupingManager, string vesselName, string compartment)
    //{
    //    return groupingManager.vesselObject.GetCompartment(compartment).frames.Select(c => c.Value.frame).ToList();
    //}

    //public static GameObject GetHullPartGameobject(this GroupingManager groupingManager, string vesselName, string compartment, string hullPart)
    //{
    //    return GetHullPart(groupingManager, vesselName, compartment, hullPart).frame;
    //}

    //public static FrameObject GetHullPart(this GroupingManager groupingManager, string vesselName, string compartment, string hullPart)
    //{
    //    return groupingManager.vesselObject.GetCompartment(compartment).GetFrameObject(hullPart);
    //}

    //public static List<GameObject> GetPlateGameobjects(this GroupingManager groupingManager, string vesselName, string compartment, string hullPart)
    //{
    //    return groupingManager.vesselObject.GetCompartment(compartment).GetFrameObject(hullPart).plates.Select(c => c.Value.plate).ToList();
    //}

    //public static GameObject GetPlateGameobject(this GroupingManager groupingManager, string vesselName, string compartment, string hullPart, string plate)
    //{
    //    return groupingManager.vesselObject.GetCompartment(compartment).GetFrameObject(hullPart).GetPlateObject(plate).plate;
    //}
}