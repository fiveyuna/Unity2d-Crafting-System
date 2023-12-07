using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Recipe")]
public class RecipeSO : ScriptableObject
{
    public int id;
    public ItemSO outputItem;
    public List<ItemSO> ingredients;
    public RecipeType type;
    
    [TextArea]
    public string recipeDesc;
    
}

public enum RecipeType {
    None,
    Basic, // 기본
    Main1, // 메인1 - 솥 업그레이드
    Main2, // 메인2 - 포션
    Depth, // 탐험 해금 아이템
    Event // 몬스터 이벤트
}

