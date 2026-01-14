using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CrystalUIController : MonoBehaviour
{
    [Header("Fire Crystal UI")]
    public Image fireCrystalIcon;
    public TMP_Text fireCrystalText;
    public Color fireCrystalColor = Color.red;

    [Header("Water Crystal UI")]
    public Image waterCrystalIcon;
    public TMP_Text waterCrystalText;
    public Color waterCrystalColor = Color.blue;

    [Header("Settings")]
    public string displayFormat = "{0}/{1}";
    public bool autoUpdateFromGameManager = true;

    void Start()
    {
        // Настраиваем цвета
        if (fireCrystalIcon != null)
            fireCrystalIcon.color = fireCrystalColor;
        if (fireCrystalText != null)
            fireCrystalText.color = fireCrystalColor;

        if (waterCrystalIcon != null)
            waterCrystalIcon.color = waterCrystalColor;
        if (waterCrystalText != null)
            waterCrystalText.color = waterCrystalColor;

        // Обновляем отображение
        if (autoUpdateFromGameManager && GameManager.Instance != null)
        {
            UpdateFromGameManager();
        }
    }

    void Update()
    {
        // Если нужно автоматическое обновление
        if (autoUpdateFromGameManager && GameManager.Instance != null)
        {
            UpdateFromGameManager();
        }
    }

    public void UpdateFromGameManager()
    {
        if (GameManager.Instance != null)
        {
            UpdateCrystalDisplay(
                GameManager.Instance.fireCrystalsCollected,
                GameManager.Instance.totalFireCrystalsInLevel,
                GameManager.Instance.waterCrystalsCollected,
                GameManager.Instance.totalWaterCrystalsInLevel
            );
        }
    }

    public void UpdateCrystalDisplay(int fireCollected, int fireTotal,
                                    int waterCollected, int waterTotal)
    {
        if (fireCrystalText != null)
        {
            fireCrystalText.text = string.Format(displayFormat, fireCollected, fireTotal);
        }

        if (waterCrystalText != null)
        {
            waterCrystalText.text = string.Format(displayFormat, waterCollected, waterTotal);
        }

        // Анимация при сборе (можно добавить позже)
        // StartCoroutine(FlashCrystalIcon(crystalType));
    }

    public void ShowCrystalSummary()
    {
        // Показываем итоги уровня
        Debug.Log($"Level Complete! Fire Crystals: {GameManager.Instance.fireCrystalsCollected}/" +
                 $"{GameManager.Instance.totalFireCrystalsInLevel}, Water Crystals: " +
                 $"{GameManager.Instance.waterCrystalsCollected}/{GameManager.Instance.totalWaterCrystalsInLevel}");
    }

    // Метод для скрытия/показа всей панели
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}