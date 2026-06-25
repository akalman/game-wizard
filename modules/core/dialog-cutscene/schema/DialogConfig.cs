using System.Collections.Generic;
using GameWizard.Engine.Schema.Logic;

namespace GameWizard.Core.DialogCutscene;

public class DialogConfig
{
    public string InitialSequence { get; set; }

    public DialogStyle Style { get; set; }

    public string Characters { get; set; }
    public string Outfits { get; set; }

    public IDictionary<string, DialogInterlude> Interludes { get; set; } = new Dictionary<string, DialogInterlude>();
    public IDictionary<string, DialogSequence> Sequences { get; set; } = new Dictionary<string, DialogSequence>();
}

public class DialogInterlude
{
    public IList<InterludeTransition> Transitions { get; set; } = new List<InterludeTransition>();
}

public class InterludeTransition
{
    public string Source { get; set; }
    public TransitionAction Action { get; set; }

    public IList<ICondition> When { get; set; } = new List<ICondition>();
}

public class DialogSequence
{
    public IList<IDialogFrame> Frames { get; set; } = new List<IDialogFrame>();
    public IList<SequenceTransition> Transitions { get; set; } = new List<SequenceTransition>();
}

public class SequenceTransition
{
    public TransitionAction Action { get; set; }

    public IList<ICondition> When { get; set; } = new List<ICondition>();
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