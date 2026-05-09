using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterInf : MonoBehaviour
{
    [SerializeField]
    RawImage charAvatar;
    [SerializeField]
    Text charName;
    [SerializeField]
    Text charLevel;
    [SerializeField]
    Text charRank;

    public string CharacterName
    {
        set
        {
            charName.text = value;
        }
    }
    public string CharacterLevel
    {
        set
        {
            charLevel.text = value;
        }
    }
    public string CharacterRank
    {
        set
        {
            charRank.text = value;
        }
    }
    public void Clear()
    {
        CharacterName = string.Empty;
        CharacterLevel = string.Empty;
        CharacterRank = string.Empty;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
