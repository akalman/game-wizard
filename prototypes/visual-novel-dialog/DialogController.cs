using System;
using Godot;
using System.Collections.Generic;

public partial class DialogController : Node2D
{
	[Export]
	public Node2D LeftCharactersContainer { get; set; }

	[Export]
	public Node2D RightCharactersContainer { get; set; }

	[Export]
	public Node2D DialogContainer { get; set; }

	private int CurrentFrameIndex { get; set; }
	private IList<DialogFrame> Frames { get; set; }
    private IDictionary<string, Region> Regions { get; set; }

	public override void _Ready()
	{
		Frames =
		[
			new DialogFrame()
			{
				DialogText = "test dialog box context a",
				Updates =
                [
                    "add|left-characters|sprites/char-a-right-facing.png",
                    "add|right-characters|sprites/char-b-left-facing.png",
				],
                Edges = new Dictionary<string, int>()
                {
	                { "ui_accept", 1 },
                },
			},
			new DialogFrame()
			{
				DialogText = "test dialog box context b",
				Updates =
                [
                    "add|right-characters|sprites/char-c-left-facing.png",
				],
                Edges = new Dictionary<string, int>()
                {
	                { "ui_accept", 2 },
                },
			},
			new DialogFrame()
			{
				DialogText = "test dialog box context c",
				Updates =
                [
                    "remove|right-characters|sprites/char-c-left-facing.png",
				],
                Edges = new Dictionary<string, int>()
                {
	                { "ui_accept", 3 },
                },
			},
			new DialogFrame()
			{
				DialogText = "",
				Updates =
                [
                    "end",
				],
                Edges = new Dictionary<string, int>(),
			},
		];
		Regions = new Dictionary<string, Region>()
		{
			{ "left-characters", new Region()
			{
				Node = LeftCharactersContainer, 
				Direction = Vector2.Right,
                State = new Dictionary<string, string>()
				{
					{ "sprite-count", "0" }
				},
			} },
			{ "right-characters", new Region()
			{
				Node = RightCharactersContainer, 
				Direction = Vector2.Left,
                State = new Dictionary<string, string>()
				{
					{ "sprite-count", "0" }
				},
			} },
			{ "dialog", new Region()
			{
				Node = DialogContainer, 
				Direction = Vector2.Zero,
                State = new Dictionary<string, string>(),
			} },
		};
		TransitionToFrameIdx(0);

	}

	public override void _Process(double delta)
	{
		foreach ((string action, int targetFrameIdx) in Frames[CurrentFrameIndex].Edges)
		{
			if (Input.IsActionJustPressed(action))
			{
				TransitionToFrameIdx(targetFrameIdx);
			}
		}
	}

	private void TransitionToFrameIdx(int targetFrameIdx)
	{
		var targetFrame = Frames[targetFrameIdx];
		foreach (var update in targetFrame.Updates)
		{
			var command = update.Split("|");
			string regionName;
			Region region;
			string spritePath;
			int numSpritesDisplayed;
			switch (command[0])
			{
				case "add":
					regionName = command[1];
                    if (!Regions.ContainsKey(regionName))
					{
						GD.PushError($"encountered unparseable update command {update} for dialog frame {targetFrame}: unknown region {command[1]}");
						GetTree().Quit();
					}
					region = Regions[regionName];
					numSpritesDisplayed = Convert.ToInt32(region.State["sprite-count"]);
					spritePath = command[2];

                    var sprite = new Sprite2D
                    {
                        Texture = GD.Load<Texture2D>($"res://prototypes/visual-novel-dialog/{spritePath}"),
                        Position = region.Direction.Normalized() * (360 + 270 * numSpritesDisplayed),
                        Scale = 0.36f * Vector2.One
                    };
                    region.Node.AddChild(sprite);
					region.State["sprite-count"] = $"{numSpritesDisplayed + 1}";
					break;
				case "remove":
					regionName = command[1];
                    if (!Regions.ContainsKey(regionName))
					{
						GD.PushError($"encountered unparseable update command {update} for dialog frame {targetFrame}: unknown region {command[1]}");
						GetTree().Quit();
					}
					region = Regions[regionName];
					numSpritesDisplayed = Convert.ToInt32(region.State["sprite-count"]);
					spritePath = command[2];

					foreach (var child in region.Node.GetChildren())
					{
                        if (child is Sprite2D spriteChild)
                        {
	                        if (spriteChild.Texture.ResourcePath.Contains(spritePath))
	                        {
		                        child.Dispose();
	                        }
                        }
                    }
					region.State["sprite-count"] = $"{numSpritesDisplayed - 1}";
					break;
				case "end":
                    GetTree().Quit();
                    break;
				default:
                    GD.PushError($"encountered unparseable update command {update} for dialog frame {targetFrame}: unknown update command {command[0]}");
                    GetTree().Quit();
					break;
			}
		}

		var dialogLabel = Regions["dialog"].Node.GetChild<Label>(1);
		dialogLabel.Text = targetFrame.DialogText;
		CurrentFrameIndex = targetFrameIdx;
	}

	private class DialogFrame
	{
		public IDictionary<string, int> Edges { get; set; }
		public string DialogText { get; set; }
        // TODO: replace string with typed DSL
		public IList<string> Updates { get; set; }
	}

    private class Region
	{
		public Vector2 Direction { get; set; }
        public Node2D Node { get; set; }
        public IDictionary<string, string> State { get; set; }
	}
    
}
