using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public interface ICompartmentRootSolver
{
    GameObject SolveForRootGameobject(MetadataComponent metadata);
}

public interface IHullpartNameSolver
{
    string SolverForHullpartName(MetadataComponent metadata);
}

public interface ISubpartTypeSolver
{
    SubpartType SolveForSubparType(MetadataComponent metadata);
}

public class TestFpsoSubpartTypeSolver : ISubpartTypeSolver, IDisposable
{
    public void Dispose() { }

    public SubpartType SolveForSubparType(MetadataComponent metadata)
    {
        string type = "";
        if (metadata.ContainsKey("TYPE"))
        {
            type = metadata.GetValue("TYPE").ToLower();
        }

        string subType = "";
        if (metadata.ContainsKey("SUB_TYPE"))
        {
            subType = metadata.GetValue("SUB_TYPE").ToLower();
        }
        else if (metadata.ContainsKey("IDX_LONGDESC"))
        {
            subType = metadata.GetValue("IDX_LONGDESC").ToLower();
        }

        if (type == "plate")
        {
            if(subType == "bracket")
            {
                return SubpartType.Bracket;
            }
            else if(subType == "flange")
            {
                return SubpartType.Stiffener;
            }
            else
            {
                return SubpartType.Plate;
            }
        }
        else if(type == "bracket")
        {
            if(subType.Contains("stiffener"))
            {
                return SubpartType.Stiffener;
            }
            else
            {
                return SubpartType.Bracket;
            }
        }
        else if(type == "stiffener")
        {
            return SubpartType.Stiffener;
        }
        else
        {
            return SubpartType.Plate;
        }
    }
}

public class TestFpsoHullpartSolver : IHullpartNameSolver, IDisposable
{
    public void Dispose() { }

    public string SolverForHullpartName(MetadataComponent metadata)
    {
        var parentMetadata = metadata.transform.parent.GetComponent<MetadataComponent>();
        while (parentMetadata != null && !parentMetadata.GetValue("CLASS").Equals("GenericSystem"))
        {
            parentMetadata = parentMetadata.transform.parent.GetComponent<MetadataComponent>();
        }

        if(parentMetadata.GetValue("CLASS").Equals("GenericSystem"))
        {
            if(Regex.IsMatch(parentMetadata.GetValue("NAME"), @"Brackets$") || parentMetadata.GetValue("NAME").Equals("Brackets"))
            {
                parentMetadata = parentMetadata.transform.parent.GetComponent<MetadataComponent>();
            }
        }

        return parentMetadata.GetValue("NAME");
    }
}

public class TestFpsoCompartmentRootSolver : ICompartmentRootSolver, IDisposable
{
    public void Dispose() { }

    public GameObject SolveForRootGameobject(MetadataComponent metadata)
    {
        if(metadata.GetComponent<MeshRenderer>() != null)
        {
            return metadata.gameObject;
        }
        else
        {
            return metadata.transform.parent.parent.gameObject;
        }
    }
}

public class PetrobrasHullpartSolver : IHullpartNameSolver, IDisposable
{
    public void Dispose() { }

    public string SolverForHullpartName(MetadataComponent metadata)
    {
        //string position = metadata.GetValue("POSITION");

        //if (position.Equals("OuterShell") || position.Equals("UpperDeck") || position.Equals("LongiBHD"))
        //{
        //    return position;
        //}
        //else
        //{
            return metadata.GetValue("STRUCTURE");
        //}
    }
}

public class PetrobrasRootSolver : ICompartmentRootSolver, IDisposable
{
    public void Dispose() { }

    public GameObject SolveForRootGameobject(MetadataComponent metadata)
    {
        return metadata.gameObject;
    }
}

public static class OldToNewFrameNameAdaptor
{
    public static Dictionary<string, string> oldToNewFrameName => new Dictionary<string, string>()
    {
        {"Deck", "Main Deck"},
        {"MainDeck", "Main Deck"},
        {"maindeck", "Main Deck"},
        {"deck", "Main Deck"},
        {"Hull", "ShipHull"},
        {"hull", "ShipHull"},
        {"ship hull", "ShipHull"},
        {"shiphull", "ShipHull"},
    };

    public static string GetNewFrameName(string oldFrameName)
    {
        if (oldToNewFrameName.ContainsKey(oldFrameName))
        {
            return oldToNewFrameName[oldFrameName];
        }
        else
        {
            return oldFrameName;
        }
    }
}