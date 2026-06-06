using Moongate.Core.Yaml;
using Moongate.UO.Data.Data.Internal;
using Moongate.UO.Data.Data.Skills;
using Moongate.UO.Data.Interfaces.Skills;
using Serilog;

namespace Moongate.UO.Data.Skills;

/// <summary>
/// Loads the UO skill table from <c>skills.yaml</c> in the data directory. A missing or malformed
/// file yields an empty store (non-fatal).
/// </summary>
public sealed class SkillDataStore : ISkillDataStore
{
    private static readonly ILogger _logger = Log.ForContext<SkillDataStore>();

    private readonly List<SkillInfo> _skills;
    private readonly Dictionary<int, SkillInfo> _byId;
    private readonly Dictionary<string, SkillInfo> _byName;

    public SkillDataStore(string dataDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataDirectory);

        _skills = [];
        _byId = new();
        _byName = new(StringComparer.OrdinalIgnoreCase);

        var path = Path.Combine(dataDirectory, "skills.yaml");

        if (!File.Exists(path))
        {
            _logger.Warning("skills.yaml not found in {Directory}; skill table is empty.", dataDirectory);

            return;
        }

        try
        {
            var model = YamlUtils.DeserializeFromFile<SkillTableModel>(path);
            _skills = model.Skill;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to parse skills.yaml; skill table is empty.");

            return;
        }

        foreach (var skill in _skills)
        {
            _byId[skill.Id] = skill;
            _byName[skill.Name] = skill;
        }

        _logger.Information("Loaded {Count} skills from {Path}", _skills.Count, path);
    }

    public IReadOnlyList<SkillInfo> Skills => _skills;

    public int Count => _skills.Count;

    public SkillInfo? GetById(int skillId)
        => _byId.GetValueOrDefault(skillId);

    public SkillInfo? GetByName(string name)
        => string.IsNullOrEmpty(name) ? null : _byName.GetValueOrDefault(name);
}
