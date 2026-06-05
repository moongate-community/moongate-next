using Tomlyn.Serialization;

namespace Moongate.Tests.Core.Toml;

[TomlSerializable(typeof(TomlPerson))]
public partial class TomlTestContext : TomlSerializerContext;
