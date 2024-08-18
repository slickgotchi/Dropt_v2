using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressBarCanvas : MonoBehaviour
{
    public static ProgressBarCanvas Instance { get; private set; }

    public GameObject Container;
    public TextMeshProUGUI Text;
    public Slider Slider;

    [SerializeField] private float minShowTime = 0.2f;

    private Queue<ProgressMessage> messageQueue = new Queue<ProgressMessage>();
    private float timeSinceLastMessage = 0f;
    private bool isShowingMessage = false;
    private bool shouldHide = false;


    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (isShowingMessage)
        {
            timeSinceLastMessage += Time.deltaTime;

            if (timeSinceLastMessage >= minShowTime)
            {
                isShowingMessage = false;
                ShowNextMessage();
            }
        }
        else if (messageQueue.Count > 0)
        {
            ShowNextMessage();
        }
        else if (shouldHide)
        {
            // All messages are cleared, now hide the container
            Container.SetActive(false);
            shouldHide = false;
        }
    }

    public void ResetProgress()
    {
        Slider.value = 0;
        messageQueue.Clear();
        timeSinceLastMessage = 0f;
        isShowingMessage = false;
        Container.SetActive(false);
        shouldHide = false;
    }

    public void Show(string statusText, float sliderIncrement = 0.1f)
    {
        messageQueue.Enqueue(new ProgressMessage { Text = statusText, SliderIncrement = sliderIncrement });

        // Immediately show the first message if nothing is being shown
        if (!isShowingMessage)
        {
            ShowNextMessage();
        }
    }

    private void ShowNextMessage()
    {
        if (messageQueue.Count > 0)
        {
            var nextMessage = messageQueue.Dequeue();
            Container.SetActive(true);
            Text.text = nextMessage.Text;
            Slider.value += nextMessage.SliderIncrement;

            timeSinceLastMessage = 0f;
            isShowingMessage = true;
        }
        else if (shouldHide)
        {
            // If there are no more messages and Hide() was called, hide the container
            Container.SetActive(false);
            shouldHide = false;
        }
    }

    public void Hide()
    {
        if (messageQueue.Count == 0 && !isShowingMessage)
        {
            Container.SetActive(false);
        }
        else
        {
            shouldHide = true; // Indicate that the container should be hidden after all messages are processed
        }
    }

    private class ProgressMessage
    {
        public string Text { get; set; }
        public float SliderIncrement { get; set; }
    }
}
