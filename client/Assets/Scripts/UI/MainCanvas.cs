using game.network;
using Photon.ShareLibrary.Constant;
using Photon.ShareLibrary.Handlers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MainCanvas : MonoBehaviour
{
    public static MainCanvas instance;

    public game.scene.world.userInterface.PanelUser PanelUser;
    public game.scene.world.userInterface.MiniMap MiniMap;

    public VariableJoystick variableJoystick;

    private UnityEngine.Vector2 joybase;

    private game.scene.World world;

    private GameObject PanelSafeArea;
    private GameObject PanelHotKeys;

    private GameObject ButtonCommonGroup;
    private GameObject ButtonCommonGuild;
    private GameObject ButtonCommonSkill;
    private GameObject ButtonCommonSettting;
    private GameObject ButtonCommonSwitch;
    private GameObject TopBar;
    private GameObject MiniChat;

    private bool isOpen = false;

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        PanelSafeArea = this.gameObject.transform.Find("PanelSafeArea").gameObject;

        PanelHotKeys = gameObject.transform.Find("PanelHotKeys").gameObject;
        ButtonCommonGroup = gameObject.transform.Find("ButtonCommonGroup").gameObject;
        ButtonCommonGuild = ButtonCommonGroup.transform.Find("ButtonCommonGuild").gameObject;
        ButtonCommonSkill = ButtonCommonGroup.transform.Find("ButtonCommonSkill").gameObject;
        ButtonCommonSettting = ButtonCommonGroup.transform.Find("ButtonCommonSettting").gameObject;
        MiniChat = gameObject.transform.Find("Chat").gameObject;

        TopBar = PanelSafeArea.transform.Find("TopBar").gameObject;

        ResolveWorld();
        joybase = new UnityEngine.Vector2(variableJoystick.getHandle().position.x, variableJoystick.getHandle().position.z);
    }

    void Update()
    {
        this.JoystickChange();
        this.SynInterval();

        //this.userInterface.ReStyle(this.currentFixedUpdateFps);
    }

    /// <summary>
    /// Sync data after syncTime
    /// </summary>
    private const float ClassicMoveSendInterval = game.network.jx.JxClassicMovement.MoveSendInterval;

    bool isMove = false;
    private UnityEngine.Vector2 moveDirection;
    private float nextClassicMoveSendTime;

    /// <summary>
    /// Call move to service.
    /// </summary>
    private void JoystickChange()
    {
        if (variableJoystick.Horizontal != 0 || variableJoystick.Vertical != 0)
        {
            UnityEngine.Vector2 direction = new UnityEngine.Vector2(variableJoystick.Horizontal, variableJoystick.Vertical);
            if (direction.sqrMagnitude > 0.01f)
            {
                moveDirection = direction.normalized;
            }
            else
            {
                moveDirection = Vector2.zero;
            }
        }
        else
        {
            /// Call stop move.
            moveDirection = Vector2.zero;
        }
    }
    private void SynInterval()
    {
        if (moveDirection != Vector2.zero)
        {
            if (!ResolveWorld() || world.GetMainPlayer() == null)
            {
                return;
            }

            bool wasMoving = isMove;
            int direction = JoystickDirectionToClassicDir(moveDirection);
            game.network.jx.JxClassicMovement.EnsureBaseSpeed(world.GetMainPlayer());
            int speed = game.network.jx.JxClassicMovement.NormalizeRunSpeed(Mathf.Max(
                PhotonManager.Instance.ClassicRunSpeed,
                game.network.jx.JxClassicMovement.GetCurrentRunSpeed(world.GetMainPlayer())));

            bool moved = world.TryStepMainPlayerClassic(direction, speed, out int actualDirection);
            if (!moved)
            {
                if (wasMoving)
                {
                    SendStopMove();
                }

                isMove = false;
                nextClassicMoveSendTime = 0f;
                PhotonManager.Instance.SetClassicLocalMovementActive(false);
                NpcAction.DoAction(world.GetMainPlayer(), NPCCMD.do_stand);
                return;
            }

            isMove = true;
            PhotonManager.Instance.SetClassicLocalMovementActive(true);
            NpcAction.DoAction(world.GetMainPlayer(), NPCCMD.do_run);

            if (Time.time < nextClassicMoveSendTime)
            {
                return;
            }

            nextClassicMoveSendTime = Time.time + ClassicMoveSendInterval;

            int distance = game.network.jx.JxClassicMovement.GetRunTargetDistance(speed);

            Vector2 playerMpsPosition = world.GetMainPlayerMpsPosition();
            Vector2 targetMpsPosition = world.ClampMoveTargetByObstacle(
                playerMpsPosition,
                actualDirection,
                distance,
                out _);
            if (Vector2.Distance(playerMpsPosition, targetMpsPosition) < 1f)
            {
                isMove = false;
                nextClassicMoveSendTime = 0f;
                world.StopMainPlayerMove();
                NpcAction.DoAction(world.GetMainPlayer(), NPCCMD.do_stand);
                SendStopMove();
                return;
            }

            int targetLeft = Mathf.RoundToInt(targetMpsPosition.x);
            int targetMapY = Mathf.RoundToInt(targetMpsPosition.y);

            PlayerMain.instance.SynCharMoveMps(targetLeft, targetMapY);
        }
        else
        {
            if (isMove)
            {
                isMove = false;
                nextClassicMoveSendTime = 0f;
                if (ResolveWorld() && world.GetMainPlayer() != null)
                {
                    world.StopMainPlayerMove();
                    NpcAction.DoAction(world.GetMainPlayer(), NPCCMD.do_stand);
                    SendStopMove();
                }
                else
                {
                    PhotonManager.Instance.SetClassicLocalMovementActive(false);
                    PhotonManager.Instance.TrySendOperation(OperationCode.StopMove, new Dictionary<byte, object>());
                }
            }
            else
            {
                //Debug.Log("KO SYNC DI CHUYEN");
            }
        }
    }

    private void SendStopMove()
    {
        if (ResolveWorld() && world.GetMainPlayer() != null)
        {
            Vector2 playerMpsPosition = world.GetMainPlayerMpsPosition();
            PhotonManager.Instance.SetClassicLocalMovementActive(false);
            PhotonManager.Instance.TrySendOperation(OperationCode.StopMove, new Dictionary<byte, object>
            {
                {(byte) ParamterCode.MapId, 0 },
                {(byte) ParamterCode.MapX, Mathf.RoundToInt(playerMpsPosition.x)},
                {(byte) ParamterCode.MapY, Mathf.RoundToInt(playerMpsPosition.y)},
            });
            return;
        }

        PhotonManager.Instance.SetClassicLocalMovementActive(false);
        PhotonManager.Instance.TrySendOperation(OperationCode.StopMove, new Dictionary<byte, object>());
    }

    private bool ResolveWorld()
    {
        if (world != null)
        {
            return true;
        }

        world = PhotonManager.Instance.world;
        if (world == null)
        {
            world = FindObjectOfType<game.scene.World>();
        }

        return world != null;
    }

    private static int JoystickDirectionToClassicDir(UnityEngine.Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0f)
        {
            angle += 360f;
        }

        if (angle >= 337.5f || angle < 22.5f)
        {
            return 48;
        }

        if (angle < 67.5f)
        {
            return 40;
        }

        if (angle < 112.5f)
        {
            return 32;
        }

        if (angle < 157.5f)
        {
            return 24;
        }

        if (angle < 202.5f)
        {
            return 16;
        }

        if (angle < 247.5f)
        {
            return 8;
        }

        if (angle < 292.5f)
        {
            return 0;
        }

        return 56;
    }

    /// <summary>
    /// Open Store
    /// </summary>
    public void OpenBag()
    {
        PopUpCanvas.instance?.OpenStorage();
    }


    /// <summary>
    /// Open Store
    /// </summary>
    public void OpenProfileDetail()
    {
        //world.GetUserInterface().panelEquipment.OpenProperties();
        //world.GetUserInterface().panelEquipment.current.SetActive(true);
    }

    /// <summary>
    /// Open setting
    /// </summary>
    public void OpenSetting()
    {
        //world.GetUserInterface().panelSettings.current.SetActive(true);
    }

    /// <summary>
    /// Skill
    /// </summary>
    public void OpenSkill()
    {
        if (PanelHotKeys != null)
        {
            isOpen = !isOpen;
            PanelHotKeys.GetComponent<SkillAction>().UpdateSkill();
            PanelHotKeys.SetActive(isOpen);
            ButtonCommonGuild.SetActive(!isOpen);
            ButtonCommonSkill.SetActive(!isOpen);
            ButtonCommonSettting.SetActive(!isOpen);
        }
    }

    public GameObject CommonSwitch() => this.ButtonCommonSwitch;
    public GameObject GetMiniChat() => this.MiniChat;
}

