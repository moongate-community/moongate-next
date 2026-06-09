module.exports = {
  branches: [
    { name: "main" },
    { name: "develop", channel: "develop", prerelease: "develop" }
  ],
  tagFormat: "v${version}",
  plugins: [
    "@semantic-release/commit-analyzer",
    "@semantic-release/release-notes-generator",
    [
      "@semantic-release/changelog",
      {
        changelogFile: "CHANGELOG.md"
      }
    ],
    [
      "@semantic-release/exec",
      {
        prepareCmd: "node scripts/release/update-version.mjs ${nextRelease.version} && bash scripts/release/build-artifacts.sh ${nextRelease.version} && bash scripts/release/pack-nuget.sh ${nextRelease.version}",
        publishCmd: "bash scripts/release/publish-docker.sh ${nextRelease.version} && bash scripts/release/publish-nuget.sh ${nextRelease.version}"
      }
    ],
    [
      "@semantic-release/git",
      {
        assets: [
          "CHANGELOG.md",
          "Directory.Build.props",
          "src/Moongate.Server/Moongate.Server.csproj",
          "tools/Moongate.AssetDataConverter/Moongate.AssetDataConverter.csproj"
        ],
        message: "chore(release): ${nextRelease.version} [skip ci]\n\n${nextRelease.notes}"
      }
    ],
    [
      "@semantic-release/github",
      {
        assets: [
          { path: "artifacts/release/moongate-linux-x64-v${nextRelease.version}.tar.gz", label: "Moongate Linux x64" },
          { path: "artifacts/release/moongate-win-x64-v${nextRelease.version}.zip", label: "Moongate Windows x64" },
          { path: "artifacts/packages/*.nupkg", label: "NuGet package" },
          { path: "artifacts/packages/*.snupkg", label: "NuGet symbols package" }
        ]
      }
    ]
  ]
};
