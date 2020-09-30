using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class GameManager : MonoBehaviourPun
{
    [Header("Players")]
    public string playerPrefabLocation1; // first team spawn points
    public string playerPrefabLocation2; // second team spawn points
    public PlayerController[] players;
    public Transform[] humanSpawnPoints;
    public Transform[] alienSpawnPoints;
    public int alivePlayers;
    public int humanPlayers;
    public int alienPlayers;

    private int playersInGame;
    private int numPlayers;

    public float postGameTime;

    public Material mat;

    // instance
    public static GameManager instance;

    void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        players = new PlayerController[PhotonNetwork.PlayerList.Length];
        alivePlayers = players.Length;

        photonView.RPC("ImInGame", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void ImInGame()
    {
        playersInGame++;

        if(PhotonNetwork.IsMasterClient && playersInGame == PhotonNetwork.PlayerList.Length)
        {
            photonView.RPC("SpawnPlayer", RpcTarget.All);
        }
    }

    [PunRPC]
    void SpawnPlayer()
    {
        GameObject playerObj;

        if (numPlayers % 2 == 1)
        {
            playerObj = PhotonNetwork.Instantiate(playerPrefabLocation1, humanSpawnPoints[
                Random.Range(0, humanSpawnPoints.Length)].position, Quaternion.identity);
            humanPlayers++;
            numPlayers++;
        }

        else
        {
            playerObj = PhotonNetwork.Instantiate(playerPrefabLocation2, alienSpawnPoints[
                Random.Range(0, alienSpawnPoints.Length)].position, Quaternion.identity);
            playerObj.GetComponent<MeshRenderer>().material = mat;
            alienPlayers++;
            numPlayers++;
        }

        // initialize player for all other players
        playerObj.GetComponent<PlayerController>().photonView.RPC("Initialize", RpcTarget.All, PhotonNetwork.LocalPlayer);
    }

    public PlayerController GetPlayer(int playerId)
    {
        foreach(PlayerController player in players)
        {
            if (player != null && player.id == playerId)
                return player;
        }

        return null;
    }

    public PlayerController GetPlayer(GameObject playerObj)
    {
        foreach(PlayerController player in players)
        {
            if (player != null && player.gameObject == playerObj)
                return player;
        }

        return null;
    }

    public void CheckWinCondition()
    {
        if (humanPlayers == 0 || alienPlayers == 0)
            photonView.RPC("WinGame", RpcTarget.All, players.First(x => !x.dead).id);
    }

    [PunRPC]
    void WinGame (int winningPlayer)
    {
        // set the UI win text
        if (alienPlayers == 0)
        {
            GameUI.instance.SetWinText("The Humans Have Prevailed!");
        }
        if (humanPlayers == 0)
        {
            GameUI.instance.SetWinText("The Aliens Have Prevailed!");
        }

        Invoke("GoBackToMenu", postGameTime);
    }

    void GoBackToMenu()
    {
        NetworkManager.instance.ChangeScene("Menu");
    }
}
