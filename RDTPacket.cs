using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
[Serializable]
public class RDTPacket
{

    public int seq;

    public byte[] data;

    public bool last;

    public RDTPacket(int seq, byte[] data, bool last)
    {
        this.seq = seq;
        this.data = data;
        this.last = last;
    }

    public int getSeq()
    {
        return seq;
    }

    public void setSeq(int seq)
    {
        this.seq = seq;
    }

    public byte[] getData()
    {
        return data;
    }

    public void setData(byte[] data)
    {
        this.data = data;
    }

    public bool isLast()
    {
        return last;
    }

    public void setLast(bool last)
    {
        this.last = last;
        
    }

    public String toString()
    {
        return "UDPPacket [seq=" + seq + ", data=" + System.Text.Encoding.Unicode.GetString(data,0,data.Length)
                + ", last=" + last + "]";
    }

}
