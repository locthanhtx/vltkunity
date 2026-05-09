using System.Collections;
using System.Collections.Generic;
using game.network;
using Photon.ShareLibrary.Handlers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ChatButton : MonoBehaviour
{
    // Start is called before the first frame update
    private int NpcId;

    void Start()
    {

    }

    private void OnMouseUp()
    {
        CallNpc();
    }

    public void CallNpc()
    {
        if (NpcId > -1)
        {
            Dictionary<byte, object> opParameters = new()
                {
                { (byte)ParamterCode.Id, NpcId},
                };
            PhotonManager.Instance.TrySendOperation(OperationCode.NpcQuery, opParameters);
        }
    }

    public void SetNpcId(int id)
    {
        this.NpcId = id;
    }

    public void RemoveNpcID()
    {
        NpcId = -1;
    }

}
