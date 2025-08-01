using UnityEngine;
using TMPro;

public class UI_Boss_HP_Bar : UI_StatBar
{
    [SerializeField] AIBossCharacterManager bossCharacter;

    public void EnableBossHPBar(AIBossCharacterManager boss)
    {
        bossCharacter = boss;
        bossCharacter.aICharacterNetworkManager.currentHealth.OnValueChanged += OnBossHPChanged;
        SetMaxStat(bossCharacter.characterNetworkManager.maxHealth.Value);
        SetStat(bossCharacter.characterNetworkManager.currentHealth.Value);
        GetComponentInChildren<TextMeshProUGUI>().text = bossCharacter.characterName;
    }

    private void OnDestroy()
    {
        bossCharacter.aICharacterNetworkManager.currentHealth.OnValueChanged -= OnBossHPChanged;
    }

    private void OnBossHPChanged(int oldValue, int newValue)
    {
        SetStat(newValue);
        if(newValue <= 0)
        {
            RemoveHPBar(2.5f);
        }
    }

    public void RemoveHPBar(float time)
    {
        Destroy(gameObject, time);
    }
}
