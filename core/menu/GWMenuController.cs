using Godot;
using System;
using System.Collections.Generic;
using GameWizard.Engine;

namespace GameWizard.Core;

public partial class GWMenuController : GWTemplateController<GWMenu>
{
    [Export] public VBoxContainer OptionsContainer { get; set; }

    private IDictionary<string, GWMenuFrame> Frames { get; set; } = new Dictionary<string, GWMenuFrame>();
    private IDictionary<string, GWMenuOption> CurrentOptions { get; set; } = new Dictionary<string, GWMenuOption>();
    private string CurrentFrame { get; set; }

    public override bool ReceiveInput(string input)
    {
        switch (input)
        {
            case "select":
                break;
            case "cancel":
                break;
            default:
                throw new NotImplementedException();
        }

        return false;
    }

    protected override void ConfigUpdated()
    {
        Frames.Clear();

        foreach (var frame in Config.Frames)
        {
            Frames[frame.Id] = frame;
        }

        TransitionFrame(Config.InitialFrame);
    }

    private void TransitionFrame(string targetFrameId)
    {
        UnloadFrame();
        LoadFrame(targetFrameId);
        CurrentFrame = targetFrameId;
    }

    private void UnloadFrame()
    {
        foreach (var child in OptionsContainer.GetChildren()) child.QueueFree();
        CurrentOptions.Clear();
    }

    private void LoadFrame(string frameId)
    {
        foreach (var option in Frames[frameId].Options)
        {
            var optionContainer = new HBoxContainer();

            var optionButtonContainer = new MarginContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };

            optionContainer.AddChild(new MarginContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill });
            optionContainer.AddChild(optionButtonContainer);
            optionContainer.AddChild(new MarginContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill });

            var optionButton = new TextureButton
            {
                TextureNormal = new GradientTexture2D
                {
                    Width = 640,
                    Height = 52,
                },
            };
            optionButtonContainer.AddChild(optionButton);
            optionButton.Pressed += () => HandleButtonPressed(option.Id);

            var optionLabel = new RichTextLabel
            {
                BbcodeEnabled = true,
                FitContent = true,
                ScrollActive = false,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Text = option.Label,
                MouseFilter = Control.MouseFilterEnum.Ignore,
            };
            optionButtonContainer.AddChild(optionLabel);

            CurrentOptions[option.Id] = option;
            OptionsContainer.AddChild(optionContainer);
        }
    }

    private void HandleButtonPressed(string optionId)
    {
        var option = CurrentOptions[optionId];

        if (option.Destination is not null) TransitionFrame(option.Destination);
        else GameController.TransitionScreen("terminal-select", option.Id);
    }
}
