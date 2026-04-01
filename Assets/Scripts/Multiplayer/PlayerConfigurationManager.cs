using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerConfigurationManager : MonoBehaviour
{
    [SerializeField] private int maxPlayers = 2;
    [SerializeField] private Material[] _playerMaterials;

    private List<PlayerConfiguration> _playerConfigs;
    private Dictionary<int, float> _tempScores;

    public static PlayerConfigurationManager Instance { get; private set; }


    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("Instance is null");
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
            _playerConfigs = new List<PlayerConfiguration>();
            _tempScores = new Dictionary<int, float>();
        }
    }

    public void SetPlayerColor(int index)
    {
        _playerConfigs[index].PlayerMaterial = _playerMaterials[index];
    }

    public void ReadyPlayer(int index)
    {
        _playerConfigs[index].IsReady = true;
        if (/*_playerConfigs.Count == maxPlayers &&*/ _playerConfigs.All(p => p.IsReady == true))
        {
            SceneManager.LoadScene("Main");
        }
        Debug.Log($"player {index} ready!");
    }

    public void HandlePlayerJoin(PlayerInput input)
    {
        Debug.Log($"Player joins: {input.playerIndex}");
        input.transform.SetParent(transform);
        if (!_playerConfigs.Any(p => p.PlayerId == input.playerIndex))
        {
            _playerConfigs.Add(new PlayerConfiguration(input));
        }
    }

    public List<PlayerConfiguration> GetPlayerConfigs()
    {
        return _playerConfigs;
    }

    public void AddScore(float score)
    {
        _tempScores.Add(_tempScores.Count + 1, score);
    }

    public void ClearScores()
    {
        _tempScores = new Dictionary<int, float>();
    }

    public Dictionary<int, float> GetScores()
    {
        return _tempScores;
    }
}
