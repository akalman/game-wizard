using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GameWizard.Engine.Config;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace GameWizard.Core.Menu;

public class MenuActionYamlConverter : IYamlTypeConverter
{
    private const string TermPattern = @"([.\w-]+)";

    private static readonly IDictionary<MenuActionType, string> Templates = new Dictionary<MenuActionType, string>
    {
        { MenuActionType.LoadPage, "load {0}" },
        { MenuActionType.End, "end" },
        { MenuActionType.None, "none" },
    };

    public bool Accepts(Type type) => type == typeof(MenuAction);

    public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var raw = parser.Consume<Scalar>();

        foreach (var (actionType, template) in Templates)
        {
            var match = Regex.Match(raw.Value, $"^{string.Format(template, TermPattern)}$");
            if (match.Success)
            {
                return new MenuAction
                {
                    Type = actionType,
                    Destination = match.Groups[1].Value,
                };
            }
        }

        throw new ConfigLoadingException($"Invalid menu action format: {raw.Value}");
    }

    public void WriteYaml(IEmitter emitter, object value, Type type, ObjectSerializer serializer)
    {
        throw new NotImplementedException();
    }
}