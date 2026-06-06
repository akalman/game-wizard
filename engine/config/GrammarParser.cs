using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GameWizard.Engine.Config;

public class GrammarParser<T>(IList<GrammarTypeMapper<T>> typeMappers)
{
    public IList<GrammarTypeMapper<T>> TypeMappers { get; } = typeMappers;

    public T Parse(string input)
    {
        foreach (var mapper in TypeMappers)
        {
            var (pattern, groups) = ExtractPattern(mapper.Grammar);
            var match = Regex.Match(input, pattern, RegexOptions.Singleline);
            if (match.Success)
            {
                var captures = new Dictionary<string, string>();
                foreach (var group in groups)
                    captures.Add(group, match.Groups[group].Value);
                return mapper.Map(captures);
            }
        }

        throw new GameWizardInternalException($"Did not find a matching grammar for input: {input}");
    }

    private (string pattern, List<string> groups) ExtractPattern(string grammar)
    {
        var tokens = grammar.Split();
        var pattern = string.Empty;
        var groups = new List<string>();

        foreach (var token in tokens)
        {
            if (!token.StartsWith("["))
                pattern += $" {token}";
            else
            {
                var parts = token.Split('[', ':', ']');
                pattern += TypePatterns[parts[1]](parts[2]);
                groups.Add(parts[2]);
            }
        }

        return (pattern.Trim(), groups);
    }

    private IDictionary<string, Func<string, string>> TypePatterns = new Dictionary<string, Func<string, string>>
    {
        { "id", name => @" (?<" + name + @">[.\w-]+)" },
        { "word", name => @" (?<" + name + @">\w+)" },
        { "ml-text", name => @"\n(?<" + name + @">.*)" },
    };
}

public interface GrammarTypeMapper<out T>
{
    public string Grammar { get; }
    public T Map(IDictionary<string, string> captures);
}