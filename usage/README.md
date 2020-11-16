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

## Total End-To-End Verification

```
1>------ Rebuild All started: Project: WindowsFormsApp, Configuration: Debug Any CPU ------
2>------ Rebuild All started: Project: WpfApp, Configuration: Debug Any CPU ------
3>------ Rebuild All started: Project: WpfCustomControlLibrary, Configuration: Debug Any CPU ------
4>------ Rebuild All started: Project: WpfControlLibrary, Configuration: Debug Any CPU ------
5>------ Rebuild All started: Project: WpfLibrary, Configuration: Debug Any CPU ------
5>'WpfLibrary.csproj': Bumped 'AssemblyVersion' from '1.2.64.0' to '1.2.65.0'
5>'WpfLibrary.csproj': Bumped 'FileVersion' from '1.2.64.0' to '1.2.65.0'
5>'WpfLibrary.csproj': Bumped 'InformationalVersion' from '1.2.64.0' to '1.2.65.0'
5>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] AssemblyVersion changed: 1.2.65.0
5>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] FileVersion changed: 1.2.65.0
5>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] InformationalVersion changed: 1.2.65.0
5>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] Version did not change
5>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] PackageVersion did not change
1>'WindowsFormsApp.csproj': Bumped 'AssemblyVersion' from '1.2.63.0' to '1.2.64.0'
1>'WindowsFormsApp.csproj': Bumped 'FileVersion' from '1.2.63.0' to '1.2.64.0'
1>'WindowsFormsApp.csproj': Bumped 'InformationalVersion' from '1.2.63.0' to '1.2.64.0'
1>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] AssemblyVersion changed: 1.2.64.0
1>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] FileVersion changed: 1.2.64.0
1>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] InformationalVersion changed: 1.2.64.0
1>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] Version did not change
1>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] PackageVersion did not change
3>'WpfCustomControlLibrary.csproj': Bumped 'AssemblyVersion' from '1.2.64.0' to '1.2.65.0'
3>'WpfCustomControlLibrary.csproj': Bumped 'FileVersion' from '1.2.64.0' to '1.2.65.0'
3>'WpfCustomControlLibrary.csproj': Bumped 'InformationalVersion' from '1.2.64.0' to '1.2.65.0'
3>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] AssemblyVersion changed: 1.2.65.0
3>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] FileVersion changed: 1.2.65.0
3>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] InformationalVersion changed: 1.2.65.0
3>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] Version did not change
3>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] PackageVersion did not change
2>'WpfApp.csproj': Bumped 'AssemblyVersion' from '1.2.65.0' to '1.2.66.0'
2>'WpfApp.csproj': Bumped 'FileVersion' from '1.2.65.0' to '1.2.66.0'
2>'WpfApp.csproj': Bumped 'InformationalVersion' from '1.2.65.0' to '1.2.66.0'
2>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] AssemblyVersion changed: 1.2.66.0
2>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] FileVersion changed: 1.2.66.0
2>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] InformationalVersion changed: 1.2.66.0
2>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] Version did not change
2>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] PackageVersion did not change
4>'WpfControlLibrary.csproj': Bumped 'AssemblyVersion' from '1.2.64.0' to '1.2.65.0'
4>'WpfControlLibrary.csproj': Bumped 'FileVersion' from '1.2.64.0' to '1.2.65.0'
4>'WpfControlLibrary.csproj': Bumped 'InformationalVersion' from '1.2.64.0' to '1.2.65.0'
4>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] AssemblyVersion changed: 1.2.65.0
4>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] FileVersion changed: 1.2.65.0
4>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] InformationalVersion changed: 1.2.65.0
4>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] Version did not change
4>X:\path\to\src\BumpAssemblyVersions\bin\Debug\netstandard2.0\build\BumpAssemblyVersions.targets(73,6): warning : [Configuration=Debug] PackageVersion did not change
5>WpfLibrary -> X:\path\to\usage\direct\WpfLibrary\bin\Debug\netcoreapp3.1\WpfLibrary.dll
5>Done building project "WpfLibrary.csproj".
1>WindowsFormsApp -> X:\path\to\usage\direct\WindowsFormsApp\bin\Debug\netcoreapp3.1\WindowsFormsApp.dll
1>Done building project "WindowsFormsApp.csproj".
3>WpfCustomControlLibrary -> X:\path\to\usage\direct\WpfCustomControlLibrary\bin\Debug\netcoreapp3.1\WpfCustomControlLibrary.dll
3>Done building project "WpfCustomControlLibrary.csproj".
4>WpfControlLibrary -> X:\path\to\usage\direct\WpfControlLibrary\bin\Debug\netcoreapp3.1\WpfControlLibrary.dll
4>Done building project "WpfControlLibrary.csproj".
2>WpfApp -> X:\path\to\usage\direct\WpfApp\bin\Debug\netcoreapp3.1\WpfApp.dll
2>Done building project "WpfApp.csproj".
========== Rebuild All: 5 succeeded, 0 failed, 0 skipped ==========
```