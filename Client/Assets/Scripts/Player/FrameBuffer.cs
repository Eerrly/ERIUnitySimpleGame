public class FrameBuffer
{

    /// <summary>
    /// ����
    /// </summary>
    public struct Input
    {
        /// <summary>
        /// 8 - bit
        /// | 0 0 0 0 | 0 0 0 | 0 |
        /// |   yaw   |  key  |pos|
        /// yaw  :   4 bit   :   (read & 0xF0) >> 4
        /// key  :   3 bit   :   (read & 0x0E) >> 1
        /// pos  :   1 bit   :   (read & 0x01)
        /// </summary>
        private byte raw;

        /// <summary>
        /// 8  1  2
        ///  \ | /
        /// 7����0����3
        ///  / | \
        /// 6  5  4
        /// </summary>
        public byte yaw
        {
            get { return (byte)((0xF0 & raw) >> 4); }
            set { raw = (byte)((raw & ~0xF0) | ((0xF & value) << 4)); }
        }

        /// <summary>
        /// [j] [k] [l]
        /// </summary>
        public byte key
        {
            get { return (byte)((0x0E & raw) >> 1); }
            set { raw = (byte)((raw & ~0x0E) | ((0x7 & value) << 1)); }
        }

        /// <summary>
        /// ���ID
        /// </summary>
        public byte pos
        {
            get { return (byte)(0x01 & raw); }
            set { raw = (byte)((raw & ~0x01) | value); }
        }

        public Input(byte value)
        {
            raw = value;
        }

        public Input(byte pos, byte value)
        {
            raw = (byte)(~0x01 & (value << 1) | pos);
        }

        public override string ToString()
        {
            return string.Format("pos:{0}, raw:{1}, yaw:{2}, btn:{3}", pos, raw, yaw, key);
        }

        public byte ToByte()
        {
            return raw;
        }

        public bool Compare(Input other)
        {
            return raw == other.raw;
        }

    }

    /// <summary>
    /// ֡����
    /// </summary>
    public struct Frame
    {
        public int frame;
        public int playerCount;
        public Input i0;
        public Input i1;

        public static readonly Input defInput = new Input();
        public static readonly Frame defFrame = new Frame()
        {
            frame = 0,
            playerCount = 0,
            i0 = new Input(0, 0),
            i1 = new Input(1, 0),
        };

        public int Length
        {
            get
            {
                return playerCount;
            }
        }

        public Input this[int index]
        {
            get
            {
                if (index < playerCount)
                {
                    switch (index)
                    {
                        case 0: return i0;
                        case 1: return i1;
                    }
                }
                return defInput;
            }
            set
            {
                if (index < playerCount)
                {
                    switch (index)
                    {
                        case 0: i0 = value; break;
                        case 1: i1 = value; break;
                    }
                }
            }
        }

        public void SetInputByPos(int pos, Input result)
        {
            if (i0.pos == pos)
            {
                i0 = result;
            }
            else if(i1.pos == pos)
            {
                i1 = result;
            }
            else
            {
                Logger.Log(LogLevel.Warning, $"FrameBuffer.SetInputByPos pos not found! {pos},{playerCount},{frame}");
            }
        }

        public bool GetInputByPos(int pos, ref Input result)
        {
            if (i0.pos == pos)
            {
                result = i0;
                return true;
            }
            else if(i1.pos == pos)
            {
                result = i1;
                return true;
            }
            else
            {
                Logger.Log(LogLevel.Warning, $"FrameBuffer.GetInputByPos pos not found! {pos},{playerCount},{frame}");
                return false;
            }
        }

    }




}
