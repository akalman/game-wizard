using System.Collections.Generic;
using GameWizard.Engine.Schema.Logic;

namespace GameWizard.Core.Menu;

public class MenuConfig
{
    public string InitialPage { get; set; }
    public MenuStyle Style { get; set; }

    public IDictionary<string, MenuPage> Pages { get; set; } = new Dictionary<string, MenuPage>();
}

public class MenuPage
{
    public MenuAction CancelAction { get; set; }
    public IDictionary<string, PageOption> Options { get; set; } = new Dictionary<string, PageOption>();
}

public class PageOption
{
    public MenuAction Action { get; set; }
    public string Label { get; set; }
    public IList<ICondition> When { get; set; } = new List<ICondition>();
}

public class MenuAction
{
    public MenuActionType Type { get; set; }
    public string Destination { get; set; }
}

public enum MenuActionType
{
    LoadPage,
    End,
    None,
}