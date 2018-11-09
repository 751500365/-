using System.Collections;
using System.Collections.Generic;
using System;
[Serializable]
public class RDTAck
{

    private int packet;

    public RDTAck(int packet)
    {
        this.packet = packet;
    }

    public int getPacket()
    {
        return packet;
    }

    public void setPacket(int packet)
    {
        this.packet = packet;
    }
}
