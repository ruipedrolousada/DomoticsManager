using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class Agent
{
    private const int Port = 12345; // Porta UDP para comunicação
    private readonly UdpClient udpServer; // Servidor UDP
    private readonly MIBManager mibManager; // Gestor da MIB
    private DateTime lastBoottime;
    private string aesKey;

    public Agent(MIB mib)
    {
        udpServer = new UdpClient(Port); // Inicializa o servidor UDP
        mibManager = new MIBManager(mib); // Inicializa o Gestor da MIB com a MIB fornecida
        lastBoottime = DateTime.Now;

        InitializeSecurity();
    }

    // INitializes the security by receiving the Manager's public key and sending the AES key
    private void InitializeSecurity()
    {
        Console.WriteLine("⏳ Waiting for Manager's public key...");

        //Receber a chave publica do Manager
        var clientEndpoint = new IPEndPoint(IPAddress.Any, 0);
        byte[] receivedBytes = udpServer.Receive(ref clientEndpoint);
        string managerPublicKey = Encoding.UTF8.GetString(receivedBytes);

        Console.WriteLine($"🔐 Manager's public key received {managerPublicKey}.\n");
        
        CryptoHelper.ImportRSAPublicKey(managerPublicKey, false);

        Console.WriteLine("🔑 Generating AES key...");
        aesKey = CryptoHelper.GenerateAESKey();
        Console.WriteLine($"🔑 Generated AES key: {aesKey}\n");
        string encryptedAESKey = CryptoHelper.EncryptWithRSA(aesKey, false);

        Console.WriteLine("📤 Sending encrypted AES key to Manager...");

        byte[] responseBytes = Encoding.UTF8.GetBytes(encryptedAESKey);
        udpServer.Send(responseBytes, responseBytes.Length, clientEndpoint);


        Console.WriteLine($"📤 Sent encrypted AES key to Manager {encryptedAESKey}.\n");
    }

    public void Start()
    {
        Console.WriteLine("🚀 Agent started. Waiting for requests...");

        while (true)
        {
            // Recebe dados do gestor
            var clientEndpoint = new IPEndPoint(IPAddress.Any, 0); // Endereço do gestor
            byte[] receivedBytes = udpServer.Receive(ref clientEndpoint); // Recebe os bytes
            string receivedData = Encoding.UTF8.GetString(receivedBytes); // Converte para string

            Console.WriteLine($"📩 Received encrypted PDU: {receivedData}\n");

            string decryptedPDU = CryptoHelper.DecryptWithAES(receivedData, aesKey);

            Console.WriteLine($"📩 Received PDU: {HelperMethods.DisplayWithNulls(decryptedPDU)}\n");

            // Decodifica o PDU recebido
            PDU requestPDU = PDU.Decode(decryptedPDU);

            // Processa a solicitação e gera uma resposta
            PDU responsePDU = HandleRequest(requestPDU);

            // Codifica e envia a resposta ao gestor
            string responseDataPDU = responsePDU.Encode();
            string encryptedPDU = CryptoHelper.EncryptWithAES(responseDataPDU, aesKey);

            byte[] responseBytes = Encoding.UTF8.GetBytes(encryptedPDU);
            udpServer.Send(responseBytes, responseBytes.Length, clientEndpoint);

            Console.WriteLine($"📤 Sent Response PDU: {HelperMethods.DisplayWithNulls(responseDataPDU)}\n");
        }
    }

    private PDU HandleRequest(PDU requestPDU)
    {
        TimeSpan elapsedTime = DateTime.UtcNow - lastBoottime;
        string elapsedTimeFormatted = $"{elapsedTime.Days}:{elapsedTime.Hours}:{elapsedTime.Minutes}:{elapsedTime.Seconds}:{elapsedTime.Milliseconds}";

        // Cria uma resposta PDU
        PDU responsePDU = new PDU
        {
            Type = PDUType.Response, // Resposta ao gestor
            TimeStamp = elapsedTimeFormatted, // Timestamp atual
            MessageIdentifier = requestPDU.MessageIdentifier // Mantém o mesmo MessageIdentifier
        };

        // Processa solicitações GET
        if (requestPDU.Type == PDUType.GetRequest)
        {
            foreach (var iid in requestPDU.IIDList)
            {
                try
                {
                    // Obtém o valor da MIB usando o IID
                    string value = mibManager.GetOrSetValue(iid);

                    responsePDU.IIDList.Add(iid); // Adiciona o IID à lista de IIDs
                    
                    //para o caso de 2 index tem de retornar lista de valores recebida do MIBmnager
                    var values = value.Split(',');
                    foreach (var val in values)
                    {
                        responsePDU.ValueList.Add(val.Trim()); // Adiciona o valor à lista de valores
                    }

                    responsePDU.ErrorList.Add("0"); // Sucesso (0 = no error)
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error processing IID {iid}: {ex.Message}\n");
                    responsePDU.IIDList.Add(iid); // Adiciona o IID à lista de IIDs
                    var lsnmpErrors = new LsnmpErrors();
                    responsePDU.ErrorList.Add(((int)lsnmpErrors.errorMap[ex.GetType()]).ToString()); // Erro (7 = value not supported)
                }
            }
        }
        // Processa solicitações SET
        else if (requestPDU.Type == PDUType.SetRequest)
        {
            // Verifica se o número de IIDs e valores é o mesmo
            if (requestPDU.IIDList.Count != requestPDU.ValueList.Count)
            {
                throw new IIDsAndValuesMismatchException("Number of IIDs and values must be the same for SetRequest.");
            }

            for (int i = 0; i < requestPDU.IIDList.Count; i++)
            {
                try
                {
                    // Define o valor na MIB usando o IID e o valor fornecido
                    var value = mibManager.GetOrSetValue(requestPDU.IIDList[i], requestPDU.ValueList[i]);
                    responsePDU.IIDList.Add(requestPDU.IIDList[i]); // Adiciona o IID à lista de IIDs
                    responsePDU.ValueList.Add(value); // Adiciona o valor à lista de valores
                    responsePDU.ErrorList.Add("0"); // Sucesso (0 = no error)
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error setting IID {requestPDU.IIDList[i]}: {ex.Message}\n");
                    var lsnmpErrors = new LsnmpErrors();
                    responsePDU.IIDList.Add(requestPDU.IIDList[i]); // Adiciona o IID à lista de IIDs
                    // responsePDU.ValueList.Add(""); // Valor vazio em caso de erro
                    responsePDU.ErrorList.Add(((int)lsnmpErrors.errorMap[ex.GetType()]).ToString()); 
                }
            }
        }

        return responsePDU;
    }
}