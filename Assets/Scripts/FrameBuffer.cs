public class FrameBuffer
{
    public struct Input
    {
        /// <summary>
        /// 8 - bit
        /// | 0 0 0 0 | 0 0 0 0 |
        /// |   yaw   |   key   |
        /// yaw  :   4 bit   :   (read & 0xF0) >> 4
        /// key  :   4 bit   :   (read & 0x0F)
        /// </summary>
        private byte raw;

        /// <summary>
        /// ��ҵ��ƶ�����,ֵ����Ϊ[0, 8]
        /// </summary>
        public byte yaw
        {
            get { return (byte)((0xF0 & raw) >> 4); }
            set { raw = (byte)((raw & ~0xF0) | ((0xF & value) << 4)); }
        }

        /// <summary>
        /// ��ҵ����а�������,����ʵ���ò���8����λ
        /// </summary>
        public byte key
        {
            get { return (byte)(0x0F & raw); }
            set { raw = (byte)((raw & ~0x0F) | (0xF & value)); }
        }

        public Input(byte value)
        {
            raw = value;
        }

        public override string ToString()
        {
            return string.Format("raw:{0}, yaw:{1}, btn:{2}", raw, yaw, key);
        }

    }

}
