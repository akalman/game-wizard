using System.Collections.Generic;
using GameWizard.Engine.Schema;

namespace GameWizard.Core;

public class GWDialogCutscene
{
    public string Id { get; set; }
    public string InitialBlock { get; set; }
    public string TerminalBlock { get; set; }
    public IList<GWDialogCharacter> Characters { get; set; } = new List<GWDialogCharacter>();
    public IList<GWDialogBlock> Blocks { get; set; } = new List<GWDialogBlock>();
}

public class GWDialogCharacter
{
    public string Id { get; set; }
    public string SpritePath { get; set; }
}

public class GWDialogBlock
{
    public string Id { get; set; }
    public IList<GWDialogFrameUpdate> Updates { get; set; } = new List<GWDialogFrameUpdate>();
    public IList<GWDialogEdge> Edges { get; set; } = new List<GWDialogEdge>();
}

public class GWDialogEdge
{
    public string EventId { get; set; }
    public string Destination { get; set; }
    public IList<GWCondition> Conditions { get; set; } = new List<GWCondition>();
}

public interface GWDialogFrameUpdate
{
    public string Type { get; set; }
}

public class GWSetTextUpdate : GWDialogFrameUpdate
{
    public string Type
    {
        get => "core.dialog-cutscene.set-text";
        set { }
    }

    public string Text { get; set; }
}

public interface GWCharacterUpdate : GWDialogFrameUpdate
{
    public string Character { get; set; }
}

public class GWAddLeftCharacterUpdate : GWCharacterUpdate
{
    public string Type
    {
        get => "core.dialog-cutscene.add-left-character";
        set { }
    }

    public string Character { get; set; }
}

public class GWAddRightCharacterUpdate : GWCharacterUpdate
{
    public string Type
    {
        get => "core.dialog-cutscene.add-right-character";
        set { }
    }

    public string Character { get; set; }
}

public class GWRemoveLeftCharacterUpdate : GWCharacterUpdate
{
    public string Type
    {
        get => "core.dialog-cutscene.remove-left-character";
        set { }
    }

    public string Character { get; set; }
}

public class GWRemoveRightCharacterUpdate : GWCharacterUpdate
{
    public string Type
    {
        get => "core.dialog-cutscene.remove-right-character";
        set { }
    }

    public string Character { get; set; }
}