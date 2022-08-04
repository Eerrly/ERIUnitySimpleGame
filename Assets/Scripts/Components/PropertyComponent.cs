﻿public class PropertyComponent : BaseComponent
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto, Pack = 4)]
    internal struct Common
    {
        public int camp;
        public int teamLevel;
        public float collsionSize;

        public Common(int no)
        {
            camp = default(int);
            teamLevel = default(int);
            collsionSize = default(float);
        }
    }

    private Common common = new Common();

    public ECamp camp
    {
        get { return (ECamp)common.camp; }
        set { common.camp = (int)value; }
    }

    public int teamLevel
    {
        get { return common.teamLevel; }
        set { common.teamLevel = value; }
    }

    public float collsionSize
    {
        get { return common.collsionSize; }
        set { common.collsionSize = value; }
    }

}