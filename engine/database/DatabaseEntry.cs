using System;
using System.Collections.Generic;
using System.Linq;
using GameWizard.Engine.Config;
using GameWizard.Engine.Schema.Game;
using GameWizard.Engine.Util;

namespace GameWizard.Engine.Database;

public class DatabaseEntry(GameDb definition, DbEntry entry, IConfigRepository fieldDeserializer)
{
    private static readonly IDictionary<DbFieldTypes, Type> ExpectedReturnTypes = new Dictionary<DbFieldTypes, Type>
    {
        // native types
        { DbFieldTypes.String, typeof(string) },
        { DbFieldTypes.StringList, typeof(IList<string>) },
        { DbFieldTypes.Number, typeof(decimal) },

        // state ref types
        { DbFieldTypes.Flag, typeof(string) },
        { DbFieldTypes.Attribute, typeof(string) },
        { DbFieldTypes.Bag, typeof(string) },

        // db ref types
        { DbFieldTypes.Database, typeof(string) },
    };

    private GameDb Definition { get; } = definition;
    private DbEntry Entry { get; } = entry;
    private IConfigRepository FieldDeserializer { get; } = fieldDeserializer;

    public T Get<T>(string fieldId)
    {
        var fieldType = Definition.Fields.SafeGet(fieldId);

        if (typeof(T) != ExpectedReturnTypes[fieldType])
            throw new GameWizardInternalException();

        if (fieldType == DbFieldTypes.StringList)
            return (T) Entry.CollectionFields[fieldId];

        return FieldDeserializer.Convert<T>(Entry.SimpleFields[fieldId]);
    }
}