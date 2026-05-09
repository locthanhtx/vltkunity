using System;
using System.Collections;
using System.Collections.Generic;
using game.network;
using Photon.ShareLibrary.Handlers;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDie : MonoBehaviour
{
    [SerializeField]
    public Text Message;

    private Action onPopupClosed;

    public void ResetTownship()
    {
        PhotonManager.Instance.TrySendOperation(OperationCode.DoDie, new Dictionary<byte, object>());
        onPopupClosed?.Invoke();
        Destroy(gameObject);
    }

    public void SetMessage(string mes, Action onCloseCallback)
    {
        Message.text = mes;
        onPopupClosed = onCloseCallback;
    }
}
