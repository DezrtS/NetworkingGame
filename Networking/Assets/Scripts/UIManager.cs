using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private GameObject enterIPAddressUI;
    [SerializeField] private TMP_InputField ipAddressInputField;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button connectButton;
    [SerializeField] private Button quitButton;

    [SerializeField] private GameObject sendMessageHolder;
    [SerializeField] private TMP_InputField messageInputField;
    [SerializeField] private Button sendMessageButton;

    [SerializeField] private TextMeshProUGUI chatText;
    [SerializeField] private TextMeshProUGUI connectedClientsText;

    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI highscoreText;

    private Dictionary<int, string> connectedClientsLog = new Dictionary<int, string>();
    private bool connectedClientsChanged = false;

    private List<string> chatLog = new List<string>();
    private bool chatChanged = false;
    private string highscores;
    private bool highscoresChanged = false;

    private void Awake()
    {
        connectButton.onClick.AddListener(() =>
        {
            string ipAddress = "127.0.0.1";
            if (ipAddressInputField.text != string.Empty) ipAddress = ipAddressInputField.text;
            if (nameInputField.text != string.Empty) GameManager.Instance.LocalName = nameInputField.text;
            nameText.text = GameManager.Instance.LocalName;
            GameManager.Instance.Connect(ipAddress);
            enterIPAddressUI.SetActive(false);
        });

        sendMessageButton.onClick.AddListener(() =>
        {
            string message = messageInputField.text;
            if (message == string.Empty) return;

            Client.HandleSend($"1<c>{message}", Client.SendType.TCP);
            messageInputField.text = string.Empty;
            sendMessageHolder.SetActive(false);
        });

        quitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            sendMessageHolder.SetActive(!sendMessageHolder.activeSelf);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameManager.Instance.Disconnect();
        }

        if (chatChanged)
        {
            chatChanged = false;
            chatText.text = string.Empty;
            for (int i = 0; i < chatLog.Count; i++)
            {
                chatText.text += $"{chatLog[i]}\n";
            }
        }

        if (connectedClientsChanged)
        {
            connectedClientsChanged = false;
            connectedClientsText.text = string.Empty;
            List<string> connectedClients = connectedClientsLog.Values.ToList();
            foreach (string connectedClient in connectedClients)
            {
                connectedClientsText.text += $"{connectedClient}\n";
            }
        }

        if (highscoresChanged)
        {
            highscoreText.text = highscores;
        }
    }

    public void AddChatMessage(string message)
    {
        chatLog.Add(message);
        chatChanged = true;
    }

    public void AddConnectedClient(int id, string data)
    {
        if (!connectedClientsLog.ContainsKey(id)) connectedClientsLog.Add(id, data);
        connectedClientsChanged = true;
    }

    public void SetHighscoreText(string text)
    {
        highscores = text;
        highscoresChanged = true;
    }

    public void RemoveConnectedClient(int id)
    {
        if (connectedClientsLog.ContainsKey(id)) connectedClientsLog.Remove(id);
        connectedClientsChanged = true;
    }

    public void MainMenu()
    {
        enterIPAddressUI.SetActive(true);
        chatText.text = string.Empty;
        connectedClientsText.text = string.Empty;
        chatLog.Clear();
        connectedClientsLog.Clear();
    }
}