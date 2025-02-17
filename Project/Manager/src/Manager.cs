using System.Net;
using System.Net.Sockets;
using System.Text;

public class Manager : IDisposable
{
    private const int AgentPort = 12345;
    private readonly UdpClient udpClient;
    private string aesKey;

    public Manager()
    {
        udpClient = new UdpClient();

        InitializeSecurity();
    }

    //Initializes the security by generating RSA keys and sending the public key to the agent
    private void InitializeSecurity()
    {
        //criptografia assimetrica RSA 
        Console.WriteLine("🔐 Generating RSA keys...");
        CryptoHelper.GenerateRSAKeys(out string publicKey, out string privateKey);
        CryptoHelper.ImportRSAPrivateKey(privateKey, true);

        Console.WriteLine("📤 Sending public key to Agent...");

        byte[] bytes = Encoding.UTF8.GetBytes(publicKey);
        udpClient.Send(bytes, bytes.Length, new IPEndPoint(IPAddress.Loopback, AgentPort));

        Console.WriteLine($"📤 Public key sent to Agent {publicKey}.\n");

        var serverEndpoint = new IPEndPoint(IPAddress.Any, 0);
        byte[] responseBytes = udpClient.Receive(ref serverEndpoint);
        string encryptedAESKey = Encoding.UTF8.GetString(responseBytes);

        //criptografia simetrica AES
        Console.WriteLine($"📩 Encrypted AES Key received {encryptedAESKey}.");
        aesKey = CryptoHelper.DecryptWithRSA(encryptedAESKey, true);
        Console.WriteLine($"🔑 AES Key received and decrypted {aesKey}.\n");
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

    //Sends PDU to agent and receives response PDU from agent, also it encrypts and decrypts the PDU
    private List<string> SendPDU(PDU pdu)
    {
        string data = pdu.Encode();
        string encryptedPDU = CryptoHelper.EncryptWithAES(data, aesKey);

        SendMessage(encryptedPDU);
        Console.WriteLine($"📤 [Manager] Sent {pdu.Type}: {HelperMethods.DisplayWithNulls(data)}\n");

        string responseData = ReceiveMessage();

        string decryptedPDU = CryptoHelper.DecryptWithAES(responseData, aesKey);

        Console.WriteLine($"📩 [Manager] [encrypted PDU string] Received Response: {HelperMethods.DisplayWithNulls(responseData)}");
        Console.WriteLine($"📩 [Manager] [decrypeted PDU string] Received Response: {HelperMethods.DisplayWithNulls(decryptedPDU)}\n");

        PDU response = PDU.Decode(decryptedPDU);
        Console.WriteLine($"📩 [Manager] Received Response PDU: {response.ToString()}\n");

        return response.ValueList;
    }

    private void SendMessage(string message)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(message);
        udpClient.Send(bytes, bytes.Length, new IPEndPoint(IPAddress.Loopback, AgentPort));
    }

    private string ReceiveMessage()
    {
        var serverEndpoint = new IPEndPoint(IPAddress.Any, 0);
        byte[] responseBytes = udpClient.Receive(ref serverEndpoint);
        string receivedMsg = Encoding.UTF8.GetString(responseBytes);

        return receivedMsg;
    }
}