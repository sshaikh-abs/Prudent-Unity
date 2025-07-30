using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CsvExportHelper : MonoBehaviour
{
	[ContextMenu(nameof(Write))]
	public void Write()
	{
        MetadataCsvExporter.ExportToCSV(GroupingManager.Instance.vesselObject.GetAllSubparts().Select(s => s.metadata).ToList());
	}
}
