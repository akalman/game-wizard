using System.Collections.Generic;
using GameWizard.Engine.Schema.Logic;

namespace GameWizard.Core.DialogCutscene;

public class DialogConfig
{
    public string InitialSequence { get; set; }
    public DialogStyle Style { get; set; }

    public IDictionary<string, DialogCharacter> Characters { get; set; } = new Dictionary<string, DialogCharacter>();
    public IDictionary<string, DialogInterlude> Interludes { get; set; } = new Dictionary<string, DialogInterlude>();
    public IDictionary<string, DialogSequence> Sequences { get; set; } = new Dictionary<string, DialogSequence>();
}

public class DialogCharacter
{
    public string Sprite { get; set; }
}

public class DialogInterlude
{
    public IList<InterludeTransition> Transitions { get; set; } = new List<InterludeTransition>();
}

public class InterludeTransition
{
    public string Source { get; set; }
    public TransitionAction Action { get; set; }

    public IList<Condition> When { get; set; } = new List<Condition>();
}

public class DialogSequence
{
    public IList<IDialogFrame> Frames { get; set; } = new List<IDialogFrame>();
    public IList<SequenceTransition> Transitions { get; set; } = new List<SequenceTransition>();
}

public class SequenceTransition
{
    public TransitionAction Action { get; set; }

    public IList<Condition> When { get; set; } = new List<Condition>();
}

public class TransitionAction
{
    public TransitionActionType Type { get; set; }
    public string Destination { get; set; }
}

public enum TransitionActionType
{
    StartSequence,
    StartInterlude,
    End,
}