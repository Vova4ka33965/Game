using UnityEngine;
using UnityEngine.UI;

public class DoorHintUI : MonoBehaviour
{
    [Header("Элементы UI")]
    public GameObject hintPanel;
    public Text hintText;
    public Image firePlayerIcon;
    public Image waterPlayerIcon;

    [Header("Цвета")]
    public Color insideColor = Color.green;
    public Color outsideColor = Color.white;
    public Color waitingColor = Color.yellow;

    [Header("Настройки")]
    public float updateInterval = 0.3f;

    private float timer;
    private Door fireDoor;
    private Door waterDoor;

    void Start()
    {
        FindDoors();
        if (hintPanel != null)
            hintPanel.SetActive(false);
    }

    void FindDoors()
    {
        // Используем новый метод вместо устаревшего
        Door[] allDoors = FindObjectsByType<Door>(FindObjectsSortMode.None);

        foreach (Door door in allDoors)
        {
            if (door.playerTag == "FirePlayer") fireDoor = door;
            else if (door.playerTag == "WaterPlayer") waterDoor = door;
        }
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0;
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        if (fireDoor == null || waterDoor == null)
        {
            FindDoors();
            if (fireDoor == null || waterDoor == null) return;
        }

        bool fireInside = fireDoor.IsPlayerInside;
        bool waterInside = waterDoor.IsPlayerInside;
        bool fireSinking = fireDoor.IsSinking;
        bool waterSinking = waterDoor.IsSinking;

        // Определяем, показывать ли подсказку
        bool shouldShow = (fireInside || waterInside) &&
                         !(fireDoor.IsSequenceComplete && waterDoor.IsSequenceComplete);

        if (hintPanel != null && hintPanel.activeSelf != shouldShow)
            hintPanel.SetActive(shouldShow);

        // Обновляем иконки
        UpdateIcons(fireInside, waterInside, fireSinking, waterSinking);

        // Обновляем текст
        UpdateText(fireInside, waterInside, fireSinking, waterSinking);
    }

    void UpdateIcons(bool fireInside, bool waterInside, bool fireSinking, bool waterSinking)
    {
        if (firePlayerIcon != null)
        {
            if (fireSinking) firePlayerIcon.color = Color.blue;
            else if (fireInside) firePlayerIcon.color = insideColor;
            else firePlayerIcon.color = outsideColor;
        }

        if (waterPlayerIcon != null)
        {
            if (waterSinking) waterPlayerIcon.color = Color.blue;
            else if (waterInside) waterPlayerIcon.color = insideColor;
            else waterPlayerIcon.color = outsideColor;
        }
    }

    void UpdateText(bool fireInside, bool waterInside, bool fireSinking, bool waterSinking)
    {
        if (hintText == null) return;

        if (fireDoor.IsSequenceComplete && waterDoor.IsSequenceComplete)
        {
            hintText.text = "УРОВЕНЬ ПРОЙДЕН!";
        }
        else if (fireSinking && waterSinking)
        {
            hintText.text = "ДВЕРИ ПОГРУЖАЮТСЯ...";
        }
        else if (fireInside && waterInside)
        {
            hintText.text = "ОБА ИГРОКА ВНУТРИ!";
        }
        else if (fireInside)
        {
            hintText.text = "ОГНЕННЫЙ ВНУТРИ. ЖДЕМ ВОДНОГО...";
        }
        else if (waterInside)
        {
            hintText.text = "ВОДНЫЙ ВНУТРИ. ЖДЕМ ОГНЕННОГО...";
        }
        else
        {
            hintText.text = "ПОДВЕДИТЕ ИГРОКОВ К ДВЕРЯМ";
        }
    }
}