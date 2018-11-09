using System.Collections;
using System.Collections.Generic;
using System;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class Receiver {
    // Probability of ACK loss
    public static double PROBABILITY = 1.1f;
    // Use this for initialization
    public  static void Main(String[] args)
    {
      

        // 83 is the base size (in bytes) of a serialized RDTPacket object 
        byte[] receivedData = new byte[Sender.MSS + 83];

        int waitingFor = 0;
        List<RDTPacket> received = new List<RDTPacket>();
        bool end = false;
        //
        UdpClient fromSender = new UdpClient(new IPEndPoint(IPAddress.Parse("192.168.1.15"),8080));

        while (!end)
        {

            Console.WriteLine("Waiting for packet");

            // Receive packet
            IPEndPoint receivedPacket = new IPEndPoint(IPAddress.Parse("192.168.1.15"), 9876);

            byte [] bytes = fromSender.Receive(ref receivedPacket);


            // Unserialize to a RDTPacket object
            RDTPacket packet = (RDTPacket)BinaryFormat.Deserialize(bytes);

            Console.WriteLine("Packet with sequence number " + packet.getSeq() + " received (last: " + packet.isLast() + " )");

            if (packet.getSeq() == waitingFor && packet.isLast())
            {

                waitingFor++;
                received.Add(packet);

                Console.WriteLine("Last packet received");

                end = true;

            }
            else if (packet.getSeq() == waitingFor)
            {
                waitingFor++;
                received.Add(packet);
                Console.WriteLine("Packed stored in buffer");
            }
            else
            {
                Console.WriteLine("Packet discarded (not in order)");
            }

            // Create an RDTAck object
            RDTAck ackObject = new RDTAck(packet.getSeq());

            // Serialize
            byte[] ackBytes = BinaryFormat.Serialize(ackObject);


            IPEndPoint ackPacket = new IPEndPoint(IPAddress.Parse("192.168.1.15"), 9876);

            // Send with some probability of loss
            Random rand = new Random();
            if (rand.Next(2, 3) > PROBABILITY)
            {
                fromSender.Send(ackBytes, ackBytes.Length,ackPacket);
            }
            else
            {
                Console.WriteLine("[X] Lost ack with sequence number " + ackObject.getPacket());
            }

            Console.WriteLine("Sending ACK to seq " + packet.getSeq() + " with " + ackBytes.Length + " bytes");
        }

        Console.ReadKey();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

}

public class BinaryFormat
{
    public static byte[] Serialize(System.Object Urobject) //序列化 返回byte[]类型
    {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream memory = new MemoryStream();
        bf.Serialize(memory, Urobject);
        byte[] bytes = memory.GetBuffer();
        memory.Close();
        return bytes;
    }

    public static object Deserialize(byte[] bytes) //反序列化，返回object类型的
    {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream memory = new MemoryStream(bytes);
        object ss = bf.Deserialize(memory);
        memory.Close();
        return ss;
    }
}
