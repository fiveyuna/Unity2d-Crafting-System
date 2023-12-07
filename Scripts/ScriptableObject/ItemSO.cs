using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Item")]
public class ItemSO : ScriptableObject
{
    [Header("Only GamePlay")]
    
    public string itemId; // 고유의 값. 저장된 객체 이름. 영어로 작성
    public string itemName;
    public string itemName_en;
    
    [TextArea] public string itemDesc;
    [TextArea] public string itemDesc_en;
    public ItemType itemType;

    [Header("Only UI")]
    public bool stackable = true;

    [Header("Both")]
    public Sprite image;
}

public enum ItemType {
    None,
    Drop, // 드랍템
    Basic, // 기본 조합템
    Main, // 메인1,2
    Event, // 탐험 해금 아이템, 몬스터 상호작용
}