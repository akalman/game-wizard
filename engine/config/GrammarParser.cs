using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;

namespace GameWizard.Engine.Config;

public class GrammarParser<T>(IList<GrammarTypeMapper<T>> typeMappers)
{
    public IList<GrammarTypeMapper<T>> TypeMappers { get; } = typeMappers;

    public T Parse(IDictionary<string, string> input)
    {
        var captures = new Dictionary<string, string>();
        foreach (var mapper in TypeMappers)
        {
            if (TryParseForMapper(input, mapper, captures))
                return mapper.Map(captures);
        }

        foreach (var (x,y) in input)
            GD.PushWarning($"{x} {y}");
        throw new GameWizardInternalException($"Did not find a matching grammar for input: {input}");
    }

    private bool TryParseForMapper(IDictionary<string, string> input, GrammarTypeMapper<T> mapper, IDictionary<string, string> result)
    {
        var captures = new Dictionary<string, string>();
        foreach (var (keyGrammar, valueGrammar) in mapper.Grammar)
        {
            if (!TryParseLineForMapper(input, keyGrammar, valueGrammar, captures))
                return false;
        }

        result.Clear();
        foreach (var (key, value) in captures)
            result[key] = value;
        return true;
    }

    private bool TryParseLineForMapper(
        IDictionary<string, string> input,
        string keyGrammar,
        string valueGrammar,
        IDictionary<string, string> captures)
    {
        var (keyPattern, keyGroups) = ExtractPattern(keyGrammar);
        foreach (var (key, value) in input)
        {
            var keyMatch = Regex.Match(key, keyPattern, RegexOptions.Singleline);
            if (keyMatch.Success)
            {
                var (valuePattern, valueGroups) = ExtractPattern(valueGrammar);
                var valueMatch = Regex.Match(value, valuePattern, RegexOptions.Singleline);
                if (!valueMatch.Success)
                    return false;
                foreach (var group in keyGroups)
                    captures.Add(group, keyMatch.Groups[group].Value);
                foreach (var group in valueGroups)
                    captures.Add(group, valueMatch.Groups[group].Value);
                return true;
            }
        }

        return false;
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
        // { "ml-text", name => @"\n(?<" + name + @">.*)" },
        { "text", name => @" (?<" + name + @">.*)" },
        { "id-list", name => @" \[(?<" + name + @">(?:[.\w-]+)(?:,[.\w-]+)*)\]" },
        { "num", name => @" (?<" + name + @">[+\d]+)" },
    };
}

public interface GrammarTypeMapper<out T>
{
    public IDictionary<string, string> Grammar { get; }
    public T Map(IDictionary<string, string> captures);
}