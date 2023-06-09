﻿using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// MemoryStream的扩展类
/// 注意：类与方法都必须是静态，第一个参数必须传递"this System.IO.MemoryStream stream"
/// </summary>
public static class MemoryStreamEx
{
    public static void Reset(this System.IO.MemoryStream stream)
    {
        stream.Position = 0;
        stream.SetLength(0);
    }
}

/// <summary>
/// 序列化工具类
/// </summary>
public static class FormatUtil
{
    private static MemoryStream stream = new MemoryStream();

    /// <summary>
    /// 将对象序列化为二进制数据数组
    /// </summary>
    /// <param name="obj">对象</param>
    /// <returns>二进制数据数组</returns>
    public static byte[] Serialize(object obj)
    {
        byte[] data = null;
        try
        {
            stream.Reset();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, obj);
            data = stream.ToArray();
        }
        catch(System.Exception e)
        {
            Logger.Log(LogLevel.Exception, e.Message);
        }
        finally
        {
            stream.Close();
        }
        return data;
    }

    /// <summary>
    /// 将二进制数据数组反序列化为对象
    /// </summary>
    /// <param name="data">二进制数据数组</param>
    /// <returns>对象</returns>
    public static object Deserialize(byte[] data)
    {
        object obj = null;
        try
        {
            stream.Reset();
            stream.Write(data, 0, data.Length);
            stream.Position = 0;
            BinaryFormatter formatter = new BinaryFormatter();
            obj = formatter.Deserialize(stream);
        }
        catch(System.Exception e)
        {
            Logger.Log(LogLevel.Exception, e.Message);
        }
        finally
        {
            stream.Close();
        }
        return obj;
    }

    public static void Release()
    {
        if(stream != null)
        {
            stream.Close();
            stream = null;
        }
    }

}
