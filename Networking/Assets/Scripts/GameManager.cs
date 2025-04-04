using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public enum CommandType
    {
        Position,
        Chat,
        Connection,
        Quit,
        Shoot,
        Hit,
        Kill,
        HighScore
    }

    [SerializeField] private GameObject clientPrefab;
    [SerializeField] private GameObject localClientPrefab;

    private Dictionary<int, LocalClient> clientObjects = new Dictionary<int, LocalClient>();
    public int localId = 999;

    private string localName = "Guest";

    private List<int> pendingClients = new List<int>();
    private List<int> pendingQuitClients = new List<int>();

    public string LocalName { get => localName; set => localName = value; }

    private void Awake()
    {
        Client.end = false;
        Client.hasId = false;
    }

    private void Update()
    {
        while (pendingClients.Count > 0)
        {
            if (pendingClients[0] == localId)
            {
                GameObject client = Instantiate(clientPrefab);
                LocalClient localClient = client.GetComponent<LocalClient>();
                localClient.SetId(pendingClients[0]);
                clientObjects.Add(pendingClients[0], client.GetComponent<LocalClient>());
                Client.HandleSend($"7<c>", Client.SendType.TCP);
            }
            else
            {
                GameObject client = Instantiate(localClientPrefab);
                LocalClient localClient = client.GetComponent<LocalClient>();
                localClient.SetId(pendingClients[0]);
                clientObjects.Add(pendingClients[0], localClient);
            }

            pendingClients.RemoveAt(0);
        }

        while (pendingQuitClients.Count > 0)
        {
            if (pendingQuitClients[0] == localId)
            {
                Disconnect();
                break;
            }
            else
            {
                Destroy(clientObjects[pendingQuitClients[0]].gameObject);
                clientObjects.Remove(pendingQuitClients[0]);
            }

            pendingQuitClients.RemoveAt(0);
        }
    }

    public void HandleMessage(string message)
    {
        //Debug.Log($"Recieved: {message}");

        string[] commandPlusData = message.Split("<c>");
        CommandType commandType = (CommandType)int.Parse(commandPlusData[0]);

        switch (commandType)
        {
            case CommandType.Position:
                string[] idPlusData = commandPlusData[1].Split("<id>");
                int clientId = int.Parse(idPlusData[0]);
                if (clientId == localId || !clientObjects.ContainsKey(clientId)) break;
                string[] positionPlusInput = idPlusData[1].Split(",");
                clientObjects[clientId].SetPosition(new Vector3(float.Parse(positionPlusInput[0]), float.Parse(positionPlusInput[1]), float.Parse(positionPlusInput[2])));
                clientObjects[clientId].MovementController.SetVelocity(new Vector2(float.Parse(positionPlusInput[3]), float.Parse(positionPlusInput[4])));
                clientObjects[clientId].MovementController.SetMoveInput(new Vector2(float.Parse(positionPlusInput[5]), float.Parse(positionPlusInput[6])));
                break;
            case CommandType.Chat:
                UIManager.Instance.AddChatMessage(commandPlusData[1]);
                break;
            case CommandType.Connection:
                string[] typePlusData = commandPlusData[1].Split("<t>");
                if (typePlusData[0] == "0")
                {
                    int id = int.Parse(typePlusData[1]);
                    localId = id;
                    Client.hasId = true;
                    pendingClients.Add(id);
                }
                else
                {
                    string[] instancesPlusData = typePlusData[1].Split("<inst>");
                    foreach (string instance in instancesPlusData)
                    {
                        if (instance == string.Empty) continue;
                        string[] idPlusIdData = instance.Split("<id>");
                        int id = int.Parse(idPlusIdData[0]);
                        UIManager.Instance.AddConnectedClient(id, idPlusIdData[1]);
                        if (!clientObjects.ContainsKey(id) && !pendingClients.Contains(id))
                        {
                            pendingClients.Add(id);
                        }
                    }
                }
                break;
            case CommandType.Quit:
                int quitId = int.Parse(commandPlusData[1]);
                Debug.Log($"Client {quitId} Disconnected");
                UIManager.Instance.RemoveConnectedClient(quitId);
                if (!pendingQuitClients.Contains(quitId))
                {
                    pendingQuitClients.Add(quitId);
                }
                Client.updatePosition = true;
                break;
            case CommandType.Shoot:
                string[] idPlusBulletData = commandPlusData[1].Split("<id>");
                int shooterId = int.Parse(idPlusBulletData[0]);
                string[] positionVelocity = idPlusBulletData[1].Split(",");
                clientObjects[shooterId].GunController.FireBullet(new Vector2(float.Parse(positionVelocity[0]), float.Parse(positionVelocity[1])), new Vector2(float.Parse(positionVelocity[2]), float.Parse(positionVelocity[3])));
                break;
            //case CommandType.Hit:
            //    Debug.Log("HIT REQUEST");
            //    //string[] idPlusHealthData = commandPlusData[1].Split("<id>");
            //    //int hitId = int.Parse(idPlusHealthData[0]);
            //    //string[] damagePlusPreviousHealth = idPlusHealthData[1].Split(",");
            //    //clientObjects[hitId].Health.TakeDamage(float.Parse(damagePlusPreviousHealth[0]));
            //    break;
            //case CommandType.Kill:

            //    //string[] killerPlusKilled = commandPlusData[1].Split("<k>");
            //    //int killerId = int.Parse(killerPlusKilled[0]);
            //    //int killedId = int.Parse(killerPlusKilled[1]);
            //    //clientObjects[killerId].AddKill();
            //    //clientObjects[killedId].Die();
            //    break;
            case CommandType.HighScore:
                UIManager.Instance.SetHighscoreText(commandPlusData[1]);

                break;
            default:
                break;
        }
    }

    public void Connect(string ipAddress)
    {
        Client.end = false;
        Client.StartClient(ipAddress);
    }

    public void Disconnect()
    {
        Client.HandleSend("3<c>", Client.SendType.TCP);
        Client.StopClient();
        Quit();
    }

    public void Quit()
    {
        Client.hasId = false;
        pendingClients.Clear();
        pendingQuitClients.Clear();
        foreach (KeyValuePair<int, LocalClient> client in clientObjects)
        {
            Destroy(client.Value.gameObject);
        }
        clientObjects.Clear();
        localId = 999;
        UIManager.Instance.MainMenu();
    }
}