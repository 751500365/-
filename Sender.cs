using System.Collections;
using System.Collections.Generic;
using System;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class Sender
{
    public static int MSS = 4;
    public static double PROBABILITY = 1.1f;
    public static int WINDOW_SIZE = 3;
    public static int TIMER = 30;
    // Use this for initialization
    public static void Main(String[] args)
    {
        int lastSent = 0;
        int waitingForAck = 0;
        byte[] fileBytes = System.Text.Encoding.Default.GetBytes("ABCDEFGHIJKLMNOPQRSTUVXZ");
        Console.WriteLine("Data size: " + fileBytes.Length + " bytes");
        int lastSeq = (int)Math.Ceiling((double)fileBytes.Length / MSS) - 1;// lastSent 从 0 开始 标记，这里要减去 1
        Console.WriteLine("Number of packets to send: " + lastSeq);
        string HostName = Dns.GetHostName(); //得到主机名
        IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
        IPAddress receiverAddress = null;
        for (int i = 0; i < IpEntry.AddressList.Length; i++)
        {
            //从IP地址列表中筛选出IPv4类型的IP地址
            //AddressFamily.InterNetwork表示此IP为IPv4,
            //AddressFamily.InterNetworkV6表示此地址为IPv6类型
            if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
            {
                receiverAddress = IpEntry.AddressList[i];
                break;
            }
        }

        UdpClient toReceiver = new UdpClient(9876, AddressFamily.InterNetwork);

        List<RDTPacket> sent = new List<RDTPacket>();
        while (true)
        {

            // Sending loop
            while (lastSent - waitingForAck < WINDOW_SIZE && lastSent < lastSeq)
            {

                // Array to store part of the bytes to send
                byte[] filePacketBytes = new byte[MSS];

                // Copy segment of data bytes to array
                Array.Copy(fileBytes, lastSent * MSS, filePacketBytes, 0, MSS);

                // Create RDTPacket object
                RDTPacket rdtPacketObject = new RDTPacket(lastSent, filePacketBytes, (lastSent == lastSeq - 1) ? true : false);

                // Serialize the RDTPacket object
                byte[] sendData = BinaryFormat.Serialize(rdtPacketObject);

                Console.WriteLine("Sending packet with sequence number " + lastSent + " and size " + sendData.Length + " bytes");

                // Add packet to the sent list
                sent.Add(rdtPacketObject);

                // Send with some probability of loss

                Random rand = new Random();
                int num = rand.Next(0, 2);
                if (num > PROBABILITY)
                {
                    IPEndPoint IPPoint = new IPEndPoint(receiverAddress, 8080);
                    toReceiver.Send(sendData, sendData.Length, IPPoint);
                }
                else
                {
                    Console.WriteLine("[X] Lost packet with sequence number " + lastSent);
                }

                // Increase the last sent
                lastSent++;

            } // End of sending while

            // Byte array for the ACK sent by the receiver
            byte[] ackBytes = new byte[40];

            // Creating packet for the ACK
            IPEndPoint ack = new IPEndPoint(IPAddress.Any, 0);
            try
            {
                // If an ACK was not received in the time specified (continues on the catch clausule)
                toReceiver.Client.ReceiveTimeout = TIMER;
                // Receive the packet
                byte[] bytes = toReceiver.Receive(ref ack);


                // Unserialize the RDTAck object

                RDTAck ackObject = (RDTAck)BinaryFormat.Deserialize(bytes);

                Console.WriteLine("Received ACK for " + ackObject.getPacket());

                // If this ack is for the last packet, stop the sender (Note: gbn has a cumulative acking)
                if (ackObject.getPacket() == lastSeq)
                {
                    break;
                    //return;
                }

                waitingForAck = Math.Max(waitingForAck, ackObject.getPacket());

            }
            catch (SocketException e)
            {
                // then send all the sent but non-acked packets

                for (int i = waitingForAck; i < lastSent; i++)
                {

                    // Serialize the RDTPacket object
                    byte[] sendData = BinaryFormat.Serialize(sent[i]);

                    // Create the packet
                    IPEndPoint packet = new IPEndPoint(receiverAddress, 8080);

                    // Send with some probability
                    Random rand = new Random();
                    if (rand.Next(2, 5) > PROBABILITY)
                    {
                        toReceiver.Send(sendData, sendData.Length, packet);
                    }
                    else
                    {
                        Console.WriteLine("[X_Resending] Lost packet with sequence number " + sent[i].getSeq());
                    }

                    Console.WriteLine("REsending packet with sequence number " + sent[i].getSeq() + " and size " + sendData.Length + " bytes");
                }
            }


        }

        Console.ReadKey();
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

}
