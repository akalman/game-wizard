using Godot;

namespace GameWizard.Engine.Util;

public static class GodotExtensions
{
    public static ITemplateController AsTemplate(this Node2D node, string templateId)
    {
        if (node is not ITemplateController controller)
            throw new GameWizardInternalException($"Unable to find template controller on root node on {templateId}.");

        return controller;
    }
}

