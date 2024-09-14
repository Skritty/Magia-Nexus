using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance != null) Destroy(this);
        Instance = this;
        DontDestroyOnLoad(this);
    }

    public float timePerTurn;
    public int defaultActionsPerTurn;
    public Phase initialPhase;
    private Phase currentPhase;
    private Coroutine phaseTimer;

    public Entity defaultPlayer;
    public SerializedDictionary<string, List<Item>> startingClasses;
    public int baseGold;
    public SerializedDictionary<string, Viewer> viewers = new SerializedDictionary<string, Viewer>();
    public static Viewer[] Viewers => Instance.viewers.Select(x => x.Value).ToArray();

    private void OnEnable()
    {
        TwitchClient.Instance.AddCommand("join", Command_JoinGame);
    }

    private void OnDisable()
    {
        TwitchClient.Instance.RemoveCommand("join");
    }

    private CommandError Command_JoinGame(string user, List<string> args)
    {
        if (args.Count == 0 || !startingClasses.ContainsKey(args[0]))
        {
            string classes = "";
            foreach(KeyValuePair<string, List<Item>> c in startingClasses)
            {
                classes += $"{c.Key}, ";
            }
            classes = classes.Remove(classes.Length -2, 2);
            return new CommandError(false, $"Please pick a valid starting class out of: {classes}");
        }

        if (!viewers.ContainsKey(user))
        {
            Viewer viewer = new Viewer();
            viewer.viewerName = user;
            viewer.items.AddRange(startingClasses[args[0]]);
            viewer.currency = baseGold;
            viewers.Add(user, viewer);
        }
        return new CommandError(true, "");
    }

    private void Start()
    {
        SceneManager.sceneLoaded += SceneDoneLoading;
    }

    public void StartPhase(Phase phase)
    {
        EndPhase();
        currentPhase = phase;
        SceneManager.LoadScene(phase.sceneToLoad);
    }

    public void NextPhase()
    {
        StartPhase(currentPhase.nextPhase);
    }

    private void SceneDoneLoading(Scene scene, LoadSceneMode sceneMode)
    {
        currentPhase.OnEnter();
        if(phaseTimer!= null) StopCoroutine(phaseTimer);
        phaseTimer = StartCoroutine(PhaseTimer(currentPhase));
    }

    private void EndPhase()
    {
        if (currentPhase == null) return;
        currentPhase.OnExit();
        currentPhase = null;
    }

    private IEnumerator PhaseTimer(Phase phase)
    {
        for (int i = 0; i < phase.tickDuration; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        StartPhase(phase.nextPhase);
    }
}