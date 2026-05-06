using Godot;

namespace GameWizard.Engine.Util;

public static class GodotExtensions
{
    public static ITemplateController AsTemplate(this Node2D node, string templateId)
    {
        var controller = node as ITemplateController;
        if (controller is null)
            throw new GameWizardInternalException($"Unable to find template controller on root node on {templateId}.");

        return controller;
    }
}

