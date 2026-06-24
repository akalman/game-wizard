using System;
using System.Collections.Generic;
using GameWizard.Engine.Config;
using GameWizard.Engine.Schema.Logic;

namespace GameWizard.Core.DialogCutscene;

public class AddCharacterFrameParser : GrammarTypeMapper<AddCharacterFrame>
{
    public IDictionary<string, string> Grammar => new Dictionary<string, string>
    {
        ["[id:character] enters"] = "on [word:side]",
    };

    public AddCharacterFrame Map(IDictionary<string, string> captures) => new()
    {
        Character = captures["character"],
        Side = Enum.Parse<HorizontalDirection>(captures["side"]),
    };
}

public class RemoveCharacterFrameParser : GrammarTypeMapper<RemoveCharacterFrame>
{
    public IDictionary<string, string> Grammar => new Dictionary<string, string>
    {
        ["[id:character] leaves"] = string.Empty,
    };

    public RemoveCharacterFrame Map(IDictionary<string, string> captures) => new()
    {
        Character = captures["character"],
    };
}

public class SetOutfitFrameParser : GrammarTypeMapper<SetOutfitFrame>
{
    public IDictionary<string, string> Grammar => new Dictionary<string, string>
    {
        ["[id:character] puts on"] = "[id:outfit]",
    };

    public SetOutfitFrame Map(IDictionary<string, string> captures) => new()
    {
        Character = captures["character"],
        Outfit = captures["outfit"],
    };
}

public class SetTextFrameParser : GrammarTypeMapper<SetTextFrame>
{
    public IDictionary<string, string> Grammar => new Dictionary<string, string>
    {
        ["[id:character] says"] = "[text:text]",
    };

    public SetTextFrame Map(IDictionary<string, string> captures) => new()
    {
        Character = captures["character"],
        Text = captures["text"],
    };
}