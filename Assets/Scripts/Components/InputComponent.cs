/// <summary>
/// �������
/// </summary>
public class InputComponent : BaseComponent
{

    internal struct Common
    {
        public int yaw;
        public int key;

        public Common(int no)
        {
            yaw = default(int);
            key = default(int);
        }
    }

    private Common common = new Common(0);

    /// <summary>
    /// ����ķ���
    /// </summary>
    public int yaw
    {
        get { return common.yaw; }
        set { common.yaw = value; }
    }

    /// <summary>
    /// ���µİ�ť
    /// </summary>
    public int key
    {
        get { return common.key; }
        set { common.key = value; }
    }

}
