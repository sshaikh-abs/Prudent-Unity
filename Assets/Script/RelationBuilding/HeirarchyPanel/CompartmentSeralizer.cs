using UnityEngine;

public class CompartmentSeralizer : SingletonMono<CompartmentSeralizer>
{
	public Vessel_JSONF vessel;
	public TextAsset textAsset;

	[ContextMenu(nameof(PrintData))]
	public void PrintData()
	{
		Debug.Log(GenerateVesselData());
	}

	[ContextMenu(nameof(LoadData))]
	public void LoadData()
	{
		string jsonData = textAsset.ToString();
		vessel = JsonUtility.FromJson<Vessel_JSONF>(jsonData);
	}

	public string GenerateVesselData()
	{
		string jsonString = JsonUtility.ToJson(vessel);
		return jsonString;
	}

	public string GetStringData()
	{
		return textAsset.ToString();
	}
}