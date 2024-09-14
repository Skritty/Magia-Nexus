using System.Collections;
using System.Collections.Generic;
using TwitchLib.Unity;
using TwitchLib.Client.Models;
using UnityEngine;
using TwitchLib.Client.Events;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using UnityEngineInternal;

public class TwitchClient : MonoBehaviour
{
    public static TwitchClient Instance;
    public Client client;
    public Api api;
    private string channelName = "skritty";
    private Dictionary<string, Func<string, List<string>, CommandError>> commands = 
        new Dictionary<string, Func<string, List<string>, CommandError>>();

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(this);
            return;
        }
        else
        {
            Instance = this;
        }
        Application.runInBackground = true;

        ApiSetup();
        BotSetup();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SendChatMessage("hi");
        }
    }

    private void ApiSetup()
    {
        // API Setup
        api = new Api();
        api.Settings.AccessToken = Secrets.bot_access_token;
        api.Settings.ClientId = Secrets.client_id;
    }

    private void BotSetup()
    {
        // Bot Setup
        ConnectionCredentials credentials = new ConnectionCredentials("skrittenbot", Secrets.bot_access_token);
        client = new Client();
        client.Initialize(credentials, channelName);

        //Subscribe to events
        client.OnMessageReceived += CommandHandler;

        client.ConnectAsync();
    }

    public void SendChatMessage(string message)
    {
        Debug.Log($"Sending message: {message}");
        client.SendMessage(client.JoinedChannels[0], message);
    }

    public void SendChatReply(string user, string message)
    {
        client.SendReplyAsync(client.JoinedChannels[0], user, message);
    }

    public void AddCommand(string commandName, Func<string, List<string>, CommandError> method)
    {
        commands.Add(commandName, method);
    }

    public void RemoveCommand(string commandName)
    {
        commands.Remove(commandName);
    }

    private Task CommandHandler(object sender, OnMessageReceivedArgs args)
    {
        Debug.Log($"{args.ChatMessage.Username}: {args.ChatMessage.Message}");
        if (args.ChatMessage.Message[0] != '!') return null;
        List<string> command = Regex.Split(args.ChatMessage.Message, "[!, ]+")
            .Select(x => x.ToLower())
            .Where(x => !string.IsNullOrEmpty(x)).ToList();
        Debug.Log($"Command Detected");

        string commandName = command[0];
        command.RemoveAt(0);
        if (commands.ContainsKey(commandName))
        {
            CommandError error = commands[commandName].Invoke(args.ChatMessage.Username, command);
            if (!error.successful)
            {
                SendChatMessage($"@{args.ChatMessage.Username} "+error.errorMessage);
            }
        }

        return null;
    }

    private CommandError Test(string[] args)
    {

        if(args.Length > 1)
        {
            Debug.Log("!test failed");
            return new CommandError(false, "Too many arguments!");
        }
        else
        {
            Debug.Log("!test succeeded");
            return new CommandError(true, "All good here");
        }
    }
}

public struct CommandError
{
    public bool successful;
    public string errorMessage;

    public CommandError(bool successful, string errorMessage)
    {
        this.successful = successful;
        this.errorMessage = errorMessage;
    }
}