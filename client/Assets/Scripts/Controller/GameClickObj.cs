using System.Collections;
using System.Collections.Generic;
using game.basemono;
using game.network;
using Photon.ShareLibrary.Constant;
using Photon.ShareLibrary.Handlers;
using UnityEngine;

public class GameClickObj : BaseMonoBehaviour
{
    public int id;
    public ObjKind kind;
    public game.resource.settings.objres.Controller controller;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnMouseUp()
    {
        if (kind == ObjKind.Obj_Kind_Box || kind == ObjKind.Obj_Kind_Prop || kind == ObjKind.Obj_Kind_Money || kind == ObjKind.Obj_Kind_Item)
        {
            Dictionary<byte, object> opParameters = new()
            {
                { (byte)ParamterCode.Id, id},
            };
            PhotonManager.Instance.TrySendOperation(OperationCode.PickItem, opParameters);
        }
    }
}
