using UnityEngine;
using UnityEngine.UI;

public class UI_TextChat : MonoBehaviour
{
    public static UI_TextChat Instance;

    [SerializeField] private Text chatBox;

    private void Awake() => Instance = this;

    public void AddMessage(string sender, string text)
    {
        chatBox.text += $"\n<b>{sender}</b>: {text}";
    }
}