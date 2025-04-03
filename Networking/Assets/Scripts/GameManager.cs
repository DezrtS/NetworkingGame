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
    
    public string highscoreFilePath = "highscores.txt"; 

    private List<int> pendingClients = new List<int>();
    private List<int> pendingQuitClients = new List<int>();

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
        Debug.Log($"Recieved: {message}");

        string[] commandPlusData = message.Split("<c>");
        CommandType commandType = (CommandType)int.Parse(commandPlusData[0]);

        switch (commandType)
        {
            case CommandType.Position:
                string[] idPlusData = commandPlusData[1].Split("<id>");
                int clientId = int.Parse(idPlusData[0]);
                if (clientId == localId) break;
                string[] position = idPlusData[1].Split(",");
                clientObjects[clientId].SetPosition(position);
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
            case CommandType.Hit:
                //string[] hitPlusDamage = commandPlusData[1].Split("<d>");
                //int hitId = int.Parse(hitPlusDamage[0]);
                //float damage = float.Parse(hitPlusDamage[1]);
                //clientObjects[hitId].TakeDamage(damage);
                break;
            case CommandType.Kill:
                //string[] killerPlusKilled = commandPlusData[1].Split("<k>");
                //int killerId = int.Parse(killerPlusKilled[0]);
                //int killedId = int.Parse(killerPlusKilled[1]);
                //clientObjects[killerId].AddKill();
                //clientObjects[killedId].Die();
                break;
            case CommandType.HighScore:
                string[] playerData = commandPlusData[1].Split(",");
                if (playerData.Length == 2 && int.TryParse(playerData[1], out int playerScore))
                {
                    FindObjectOfType<HighScoreManager>().SaveHighScore(playerData[0], playerScore);
                }
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