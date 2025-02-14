using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Manager : IDisposable
{
    private const int AgentPort = 12345;
    private readonly UdpClient udpClient;

    public Manager()
    {
        udpClient = new UdpClient();
    }

    public void Dispose()
    {
        udpClient?.Close();
        udpClient?.Dispose();
    }

    public List<string> SendGetRequest(string[] iids)
    {
        PDU pdu = new PDU
        {
            Type = PDUType.GetRequest,
            TimeStamp = DateTime.UtcNow.ToString("dd:MM:yyyy:HH:mm:ss:fff"),
            MessageIdentifier = Guid.NewGuid().ToString("N").Substring(0, 16),
            IIDList = new List<string>(iids),
            ValueList = new List<string>(),
            ErrorList = new List<string>()
        };

        return SendPDU(pdu);
    }

    public List<string> SendSetRequest(Dictionary<string, string> iidValues)
    {
        PDU pdu = new PDU
        {
            Type = PDUType.SetRequest,
            TimeStamp = DateTime.UtcNow.ToString("dd:MM:yyyy:HH:mm:ss:fff"),
            MessageIdentifier = Guid.NewGuid().ToString("N").Substring(0, 16),
            IIDList = new List<string>(iidValues.Keys),
            ValueList = new List<string>(iidValues.Values),
            ErrorList = new List<string>()
        };

        return SendPDU(pdu);
    }

    private List<string> SendPDU(PDU pdu)
    {
        try
        {
            string data = pdu.Encode();
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            udpClient.Send(bytes, bytes.Length, new IPEndPoint(IPAddress.Loopback, AgentPort));

            Console.WriteLine($"📤 [Manager] Sent {pdu.Type}: {data}\n");

            var serverEndpoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] responseBytes = udpClient.Receive(ref serverEndpoint);
            string responseData = Encoding.UTF8.GetString(responseBytes);

            Console.WriteLine($"📩 [Manager] Received Response: {HelperMethods.DisplayWithNulls(responseData)}\n");
            PDU response = PDU.Decode(responseData);
            Console.WriteLine($"📩 [Manager] Received Response PDU\n: {response.ToString()}\n");

            return response.ValueList;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [Manager] Error: {ex.Message}");
            return new List<string> { "Error: Communication failure" };
        }
    }
}
