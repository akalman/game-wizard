using System;
using System.Collections.Generic;
using GameWizard.Engine.Config;
using GameWizard.Engine.Schema.Logic;

namespace GameWizard.Core.DialogCutscene;

public class AddCharacterFrameParser : GrammarTypeMapper<AddCharacterFrame>
{
    public string Grammar => @"[id:character] added to [word:side]";

    public AddCharacterFrame Map(IDictionary<string, string> captures) => new()
    {
        Character = captures["character"],
        Side = Enum.Parse<HorizontalDirection>(captures["side"]),
    };
}

public class RemoveCharacterFrameParser : GrammarTypeMapper<RemoveCharacterFrame>
{
    public string Grammar => @"[id:character] removed";

    public RemoveCharacterFrame Map(IDictionary<string, string> captures) => new()
    {
        Character = captures["character"],
    };
}

public class SetOutfitFrameParser : GrammarTypeMapper<SetOutfitFrame>
{
    public string Grammar => @"[id:character] puts on [id:outfit]";

    public SetOutfitFrame Map(IDictionary<string, string> captures) => new()
    {
        Character = captures["character"],
        Outfit = captures["outfit"],
    };
}

public class SetTextFrameParser : GrammarTypeMapper<SetTextFrame>
{
    public string Grammar => @"[id:character] says [ml-text:text]";

    public SetTextFrame Map(IDictionary<string, string> captures) => new()
    {
        Character = captures["character"],
        Text = captures["text"],
    };
}