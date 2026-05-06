using System.Collections.Generic;
using GameWizard.Engine.Schema.Game;

namespace GameWizard.Engine.Schema.Modules;

public class Module
{
    public string Id { get; set; }

    public IDictionary<string, Template> Templates { get; set; } = new Dictionary<string, Template>();
}

public class Template
{
    public string Scene { get; set; }

    public IList<string> Inputs { get; set; } = new List<string>();
    public IDictionary<string, TemplateOutput> Outputs { get; set; } = new Dictionary<string, TemplateOutput>();
}

public class TemplateOutput
{
    public EdgeType Default { get; set; }

    public IList<EdgeType> Allowed { get; set; } = new List<EdgeType>();
}