# Direct Usage Examples

The projects depend upon the `src\BumpAssemblyVersions.sln` being built in the appropriate configuration, but in truth the `Debug` configuration should be aligned.

Note that there is no project dependency here, although we do depend on the build having been done via a second solution instance having been built first.

## Project Or Package References

A proper end-to-end test might include a `PackageReference`, however, the point of these projects is to ensure that we do not get that far away from the root source for integration test purposes, as much as possible. Long term, I could be persuaded to migrate to a proper `PackageReference` in order to perform these builds and verify whether versions did indeed bump. However, in the intermediate term, there are also `Test.BumpAssemblyVersions` unit test hooks that invoke these very projects in the context of a unit test driven build process.

## Example Build

So, for instance, we build `WpfApp` in `Debug` configuration.

```
1>------ Rebuild All started: Project: WpfApp, Configuration: Debug Any CPU ------
1>'WpfApp.csproj': Bumped 'AssemblyVersion' from '1.2.64.0' to '1.2.65.0'
1>'WpfApp.csproj': Bumped 'FileVersion' from '1.2.64.0' to '1.2.65.0'
1>'WpfApp.csproj': Bumped 'InformationalVersion' from '1.2.64.0' to '1.2.65.0'
1>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] AssemblyVersion changed: 1.2.65.0
1>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] FileVersion changed: 1.2.65.0
1>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] InformationalVersion changed: 1.2.65.0
1>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] Version did not change
1>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] PackageVersion did not change
1>WpfApp -> X:\path\to\usage\direct\WpfApp\bin\Debug\netcoreapp3.1\WpfApp.dll
1>Done building project "WpfApp.csproj".
========== Rebuild All: 1 succeeded, 0 failed, 0 skipped ==========
```

## Standard SDK Build

We can also verify a `netstandard2.0` build, `NetStd.ProjXml.TestLookupNoDups`.

```
1>------ Rebuild All started: Project: NetStd.ProjXml.TestLookupNoDups, Configuration: Debug Any CPU ------
1>'NetStd.ProjXml.TestLookupNoDups.csproj': Bumped 'Version' from '2020.320.2240.0' to '2020.321.1621.0'
1>'NetStd.ProjXml.TestLookupNoDups.csproj': Bumped 'AssemblyVersion' from '2020.320.2240.0' to '2020.321.1621.0'
1>'NetStd.ProjXml.TestLookupNoDups.csproj': Bumped 'FileVersion' from '2020.320.2240.0' to '2020.321.1621.0'
1>'NetStd.ProjXml.TestLookupNoDups.csproj': Bumped 'InformationalVersion' from '2020.320.2240.0' to '2020.321.1621.0'
1>'NetStd.ProjXml.TestLookupNoDups.csproj': Bumped 'PackageVersion' from '2020.320.2240.0' to '2020.321.1621.0'
1>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] Version changed: 2020.321.1621.0
1>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] AssemblyVersion changed: 2020.321.1621.0
1>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] FileVersion changed: 2020.321.1621.0
1>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] InformationalVersion changed: 2020.321.1621.0
1>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] PackageVersion changed: 2020.321.1621.0
1>NetStd.ProjXml.TestLookupNoDups -> X:\path\to\usage\direct\NetStd.ProjXml.TestLookupNoDups\bin\Debug\netstandard2.0\NetStd.ProjXml.TestLookupNoDups.dll
1>Done building project "NetStd.ProjXml.TestLookupNoDups.csproj".
========== Rebuild All: 1 succeeded, 0 failed, 0 skipped ==========
```
