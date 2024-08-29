using Godot;
using Godot.Collections;
using System;
using System.Linq;

public partial class LoadingScreen : CanvasLayer
{
    [Export] public AnimationPlayer _animationPlayer;
    [Export] public ProgressBar _progressBar;
    [Export] public Timer _dotTimer;
    [Export] public Label _loadingLabel;
    [Export] public VBoxContainer _vBoxContainer;
    [Export] public Card Card;

    private string cardDirectory = "res://Scenes/CardData/";
    private string[] allCardData;

    private int _dotCount = 0;
    private string _baseText = "Loading";

    public override void _Ready()
    {
        AudioManager.GetInstance().PlayMenuSwitch();
        allCardData = DirAccess.GetFilesAt(cardDirectory);
        _dotTimer.Timeout += () => OnDotTimerTimeout();
        LoadRandomCard();
    }

    private void LoadRandomCard()
    {
        Random random = new Random();
        int randomIndex = random.Next(allCardData.Length);
        string randomCardName = allCardData[randomIndex];
        CardData carddata = (CardData)ResourceLoader.Load(cardDirectory + randomCardName);
        Card.SetCardData(carddata);
        Card.Disabled = true;
    }

    private void OnDotTimerTimeout()
    {
        _dotCount = (_dotCount + 1) % 4; // Cycle through 0, 1, 2, 3
        switch (_dotCount)
        {
            case 0:
                _loadingLabel.Text = _baseText;
                break;
            case 1:
                _loadingLabel.Text = _baseText + ".";
                break;
            case 2:
                _loadingLabel.Text = _baseText + "..";
                break;
            case 3:
                _loadingLabel.Text = _baseText + "...";
                break;
        }
    }

    public void UpdateProgressBar(float newValue)
    {
        _progressBar.Value = newValue * 100;
    }

    public void StartOutroAnimation()
    {
        _animationPlayer.Play("end_load");
    }
}