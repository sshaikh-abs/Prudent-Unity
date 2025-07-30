
using System.Collections.Generic;
using System.Data.Common;

[System.Serializable]
public class FrameLabelFeature_DataClass : DataClass
{
    public TransverseFrameLabelManager transverseManager;
    public LongitudinalFrameLabelManager longitudinalManager;
    public DeckFrameLabelManager deckManager;
    public float sizeOffset = 5f;
    public float yOffset = 5f;
}

public class FrameLabelFeature : BaseService<FrameLabelFeature_DataClass>
{
    public override void Kill() { }
    public override void Update() { }

    public void SetTransverseLabelsActive(bool active)
    {
        data.transverseManager.SetLabelsActive(active);
    }

    public void SetLongitudinalLabelsActive(bool active)
    {
        data.longitudinalManager.SetLabelsActive(active);
    }

    public void SetDeckLabelsActive(bool active)
    {
        data.deckManager.SetLabelsActive(active);
    }

    public void SetTransverseLabelsDensity(float value)
    {
        data.transverseManager.SetLabelsDensity(value);
    }

    public void SetLongitudinalLabelsDensity(float value)
    {
        data.longitudinalManager.SetLabelsDensity(value);
    }

    public void SetDeckLabelsDensity(float value)
    {
        data.deckManager.SetLabelsDensity(value);
    }

    public void SetLabelsActive(bool active)
    {
        data.transverseManager.SetLabelsActive(active);
        data.longitudinalManager.SetLabelsActive(active);
        data.deckManager.SetLabelsActive(active);
    }

    public void SetLabelsDensity(float value)
    {
        data.transverseManager.SetLabelsDensity(value);
        data.longitudinalManager.SetLabelsDensity(value);
        data.deckManager.SetLabelsDensity(value);
    }

    public void SpawnLabels()
    {
        data.transverseManager.SpawnLabels();
        data.longitudinalManager.SpawnLabels();
        data.deckManager.SpawnLabels();
    }

    public List<FrameLabel> GetTransverseLabelsOfCompartment(Compartment compartment)
    {
        return data.transverseManager.GetFrameLabelsOfCompartment(compartment);
    }

    public List<FrameLabel> GetLongitudinalLabelsOfCompartment(Compartment compartment)
    {
        return data.longitudinalManager.GetFrameLabelsOfCompartment(compartment);
    }

    public (string, string) GetFrameFromAndTo(Compartment compartment)
    {
        return data.transverseManager.GetFrameFromAndTo(compartment);
    }
}
