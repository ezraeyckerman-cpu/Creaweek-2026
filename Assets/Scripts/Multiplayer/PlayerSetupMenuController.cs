using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSetupMenuController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private GameObject _readyPanel;
    [SerializeField] private GameObject _menuPanel;
    [SerializeField] private Button _readyButton;
    [SerializeField] private TextMeshProUGUI _readyButtonText;

    private float _ignoreInputTime = .5f;
    private int _playerId;
    private bool IsInputEnabled;

    void Update()
    {
        if (Time.time > _ignoreInputTime)
        {
            IsInputEnabled = true;
        }
    }

    public void SetPlayerIndex(int index)
    {
        _playerId = index;
        _titleText.SetText($"Player {index + 1}");
        _ignoreInputTime = Time.time + _ignoreInputTime;

        //for preset colors, execute here
        StartCoroutine(SetColorDelay());
    }

    public void SetColor(/*//for allowing color selection, uncomment this: Material material*/)
    {
        //if (!IsInputEnabled) { return; }

        PlayerConfigurationManager.Instance.SetPlayerColor(_playerId);
        _readyPanel.SetActive(true);
        //_readyButton.Select();
        _menuPanel.SetActive(false);
    }

    public void ReadyPlayer()
    {
        if (!IsInputEnabled) { return; }

        PlayerConfigurationManager.Instance.ReadyPlayer(_playerId);
        _readyButton.gameObject.SetActive(true);
        _readyButtonText.text = "Ready!";
    }

    IEnumerator SetColorDelay()
    {
        yield return null;
        SetColor();
    }
}
