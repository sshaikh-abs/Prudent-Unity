using System.Collections.Generic;

[System.Serializable]
public class Vessel_JSONF : Branch_JSONFO<CompartmentType_JSONF>
{
    public Vessel_JSONF() : base() { }
    public Vessel_JSONF(string name) : base(name) { }
    public List<BadFrame_JSONF> badFrame_JSONFs;
}


[System.Serializable]
public class CompartmentType_JSONF : Branch_JSONFO<Compartment_JSONF>
{
    public CompartmentType_JSONF() : base() { }
    public CompartmentType_JSONF(string name) : base(name) { }
}

[System.Serializable]
public class Compartment_JSONF : Branch_JSONFO<Frame_JSONF>
{
    public Compartment_JSONF() : base() { }
    public Compartment_JSONF(string name) : base(name) { }
}

[System.Serializable]
public class Frame_JSONF : Branch_JSONFO<Plate_JSONF>
{
    public Frame_JSONF() : base() { }
    public Frame_JSONF(string name) : base(name) { }
}

[System.Serializable]
public class Plate_JSONF : Leaf_JSONFO { }

[System.Serializable]
public class BadFrame_JSONF : Leaf_JSONFO { }