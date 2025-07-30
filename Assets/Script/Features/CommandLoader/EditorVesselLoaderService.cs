using System;
[System.Serializable]
public class EditorVesselLoaderService_DataClass : DataClass
{
    public LoaderPassThroughSO PassThrough;
}

public class EditorVesselLoaderService : BaseService<EditorVesselLoaderService_DataClass>
{
    public override void Kill() { }

    public override void Update() { }
}
