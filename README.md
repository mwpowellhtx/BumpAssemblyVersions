<!-- before I am ready to publish this one, let's review the packaging, make sure we can copy it to local package sources, and verify that we can add the reference, etc, and it all works as expected, fine tune the details, etc. -->

# BumpAssemblyVersions

BumpAssemblyVersions is an *MSBuild 15* task that bumps the versions of a *Visual Studio 2017* project. Testing done on *CSharp* projects only, i.e. ``.csproj`` or ``AssemblyInfo.cs`` files, at this time.

## Objectives

I wanted to automate a seamless version bumping mechanism for my projects as I continued to work on them, yet with a variety of choices in terms of how I decided to bump the versions. Along these lines, I evaluated a couple of projects as potential fits for this purposes, both of which informed my approaching the problem at hand, but neither of which adequately solved the issue in its entirety, in my professional opinion, at least not without additional pain points.

I took at look at [MSBump](http://github.com/BalassaMarton/MSBump), which in and of itself, basically did what I wanted it to do. However, it lacked in terms of the versions I can influence, and in terms of the breadth of bumping strategies.

I also took at look at [Precision Infinity's Automatic Versions 1](http://marketplace.visualstudio.com/items?itemName=PrecisionInfinity.AutomaticVersions), which did offer the breadth of versioning strategies, but which was too invasive, in my opinion, in terms of hooking into my projects, let alone the painful installation procedure.

Take aways from this, in and of itself, are, gone are the days of installed extensions, I think. If you are not delivering at least a [VSIX](http://docs.microsoft.com/en-us/visualstudio/extensibility/shipping-visual-studio-extensions), let alone a simple [development only NuGet package](http://docs.microsoft.com/en-us/nuget/quickstart/install-and-use-a-package-in-visual-studio), chances are your product will be left in the stone ages.

Given these two points, I drew from the best of both of these and delivered ``BumpAssemblyVersions``, which has the flexibility of a development only NuGet available assembly version bumping package, as well as a breadth of version bumping strategies.

## Usage

1. Add the ``BumpAssemblyVersions`` NuGet package reference to your project.
1. Edit your ``.csproj`` file with the appropriate assembly version instructions.

Caution: You may need to indicate ``PrivateAssets="All"`` to the ``PackageReference`` declaration in order to avoid carrying the ``BumpAssemblyVersions`` dependency with your project into unit testing and deployment phases.

Tested using ``NuGet v4.6+``.

All versions are bumped just prior to the ``MSBuild`` ``Build`` step.

## Instructional Options

There are several inputs that are considered during the version bumping process. We will cover the items first that should be in your project, followed by the specific instructions, followed by the options.

### Version Bumping Form Factors

Generally speaking, two varieties of versions are considered during the version bumping task.

1. ``AssemblyInfo`` based versions in the form of ``[assembly: AssemblyVersion("1.2.3.4")]``.
1. Project file XML based versions in the form of ``<Version>1.2.3.4</Version>`` found in your ``.csproj`` files.

### Default Version Bumping Candidates

A couple of default version bumping candidates are included out of the box:

1. The custom task includes your ``.csproj`` file by default at first.
1. The default set of included files also includes the ``AssemblyInfo.cs`` file when such a one exists.

### Specifying Additional Files to Bump

Specifying additional files to bump is the simplest of use cases to consider. Remember, ``AssemblyInfo`` is not the only place where additional [*Assembly Attributes*](http://docs.microsoft.com/en-us/dotnet/framework/app-domains/set-assembly-attributes) may be specified. The way to specify these in additional files is via the ``FilesToBump`` item, for example:

```Xml
<ItemGroup>
  <FilesToBump Include="Properties\AdditionalAssemblyInfo.cs" />
</ItemGroup>
```

Note, these do not have to live in ``Properties`` nor do they have to be called ``AdditionalAssemblyInfo.cs``. See also caveats <sup>\[[1](#msbuild-caveats)\]</sup> concerning mixing with different ``MSBuild`` conventions.

### Bump Version Specifications

After the input files have been determined, then you must configure which versions to bump, and how you would like to bump them.

There are several known versions which may be bumped depending upon whether you are talking about the ``Project XML`` variety or the ``Assembly Attribute`` sort. These are enumerated as follows:

|Version Kind|Description|
|---|---|
|Version|Usage with the ``.csproj`` file format, i.e. ``<Version>...</Version>``.|
|FileVersion|The ``.csproj`` analog to the [AssemblyFileVersionAttribute](http://docs.microsoft.com/en-us/dotnet/api/system.reflection.assemblyfileversionattribute).|
|InformationalVersion|The ``.csproj`` analog to the [AssemblyInformationalVersionAttribute](http://docs.microsoft.com/en-us/dotnet/api/system.reflection.assemblyinformationalversionattribute).|
|PackageVersion|Usage with the [``.csproj``](http://docs.microsoft.com/en-us/dotnet/core/tools/csproj#packageversion) additions.|
|AssemblyVersion|This one serves a dual purpose for both the ``.csproj`` file format, i.e. ``<AssemblyVersion>...</AssemblyVersion>``, as well as for [AssemblyVersionAttribute](http://docs.microsoft.com/en-us/dotnet/api/system.reflection.assemblyversionattribute).|
|AssemblyFileVersion|This one is for use with [AssemblyFileVersionAttribute](http://docs.microsoft.com/en-us/dotnet/api/system.reflection.assemblyfileversionattribute).|
|AssemblyInformationalVersion|This one is for use with [AssemblyInformationalVersionAttribute](http://docs.microsoft.com/en-us/dotnet/api/system.reflection.assemblyinformationalversionattribute).|

And which may be specified as follows, for example:

```Xml
<ItemGroup>
  <BumpVersionSpec Include="FileVersion" ... />
</ItemGroup>
```

See [caveats](#msbuild-caveats) concerning mixing with different ``MSBuild`` conventions. Also refer to Microsoft documentation <sup>\[[12](#dotnet-docs-issues-6476) [13](#microsoft-dotnet-generateassemblyinfo-targets)\]</sup> concerning the project file format, assembly attributes, etc. For now, this is the best guidance I can offer anyone doing the math and figuring on connecting which versions with which other versions.

### Bump Version Element Specifications

After you have determined the versions you would like to bump, the next thing is to decide on how you would like to bump those versions. To do this, there are four, sometimes five, version elements which you may configure, *Major*, *Minor*, *Patch*, *Build*, and, sometimes, *Release*.

You must choose not only the [*Version Provider*](#version-providers), which we will postpone in discussion, but also the [*Version Provider Attributes*](#version-provider-attributes), which require a little more in depth presentation up front.

Version elements in order of significants, most significant first:

|Element|Specification|
|---|---|
|Major|MajorProviderTemplate|
|Minor|MinorProviderTemplate|
|Patch|PatchProviderTemplate|
|Build|BuildProviderTemplate|
|Release|ReleaseProviderTemplate|

#### Version Provider Attributes

Unless otherwise indicated, attributes must all be prefixed with the desired [Version Element](#bump-version-element-specifications), i.e. ``{Major|Minor|Patch|Build|Release}ProviderTemplate``. The ``ProviderTemplate`` itself is [always required](#version-providers), which prefixed element name must always be included.

|Spec Attribute|Values|Description|
|---|---|---|
|CreateNew <sup>\[[6](#specification-only-attribute)\]</sup>|``true \| false``|Indicates whether the version may be created afresh when it is discovered not to exist prior to build.|
|DefaultVersion <sup>\[[6](#specification-only-attribute)\]</sup>|``MAJOR.MINOR[.PATCH[.BUILD]][-RELEASE]``|Generally given in the specified form. ``MAJOR`` and ``MINOR`` elements are both required no matter what. ``PATCH`` and ``BUILD`` are both optional, but if ``BUILD`` is present, so must ``PATCH``. ``RELEASE`` is entirely optional regardless.|
|IncludeWildcard <sup>\[[4](#bump-version-spec-attribs), [5](#version-provider-attrib-prefixes)\]</sup>|``true \| false``|Indicates whether any of the elements may be the wildcard (\*). This is assumed to be the last unspecified version element; for example, if *Major* and *Minor* elements are specified, then *Patch* is assumed to be the wildcard.|
|MayReset <sup>\[[4](#bump-version-spec-attribs), [5](#version-provider-attrib-prefixes)\]</sup>|``true \| false``|Indicates whether the [Version Element](#bump-version-element-specifications) may reset. Reset occurs when any of the more significant elements have changed in any way.|
|UseUtc <sup>\[[4](#bump-version-spec-attribs), [5](#version-provider-attrib-prefixes)\]</sup>|``true \| false``|Indicates whether to use [Coordinated Universal Time](http://en.wikipedia.org/wiki/Coordinated_Universal_Time), namely [DateTime.UtcNow](http://docs.microsoft.com/en-us/dotnet/api/system.datetime.utcnow), or the [converted universal time](http://docs.microsoft.com/en-us/dotnet/api/system.datetime.touniversaltime) of the task at the time of the build.|
|Label <sup>\[[7](#release-increment-version-provider-specific)\]</sup> |``[a-zA-Z]+``|Any textual label decorating the release label version provider.|
|ValueWidth <sup>\[[7](#release-increment-version-provider-specific)\]</sup> |``[0-9]+``|Any integer numeric value indicating desired width of the release label incremental value.|
|ShouldDiscard <sup>\[[7](#release-increment-version-provider-specific)\]</sup> |``true \| false``|Indicates whether the release label, whatever it happened to be, should be discarded.|

#### Version Providers

The prefixed ``ProviderTemplate`` attribute is always required. The attribute value itself may be any unique prefix selecting one of the following *Version Providers*. For instance, ``DayOfYearVersionProvider``, then you may shorten that to ``DayOfYear`` or even ``DayOf``, and the task will select the correct provider. You may not, however, shorten that to ``Day`` as that would lead to a confusion between ``DayOfYearVersionProvider`` and ``DayVersionProvider``, in this particular instance. Rinse and repeat for the several [*Version Elements*](#bump-version-element-specifications).

|Version Provider|Description|
|---|---|
|DayOfYearVersionProvider <sup>\[[8](#date-time-based-version-provider), [11](#version-provider-right-justified-zero-padded)\]</sup>|Yields the Day of Year in the form ``DD``.|
|DayVersionProvider <sup>\[[8](#date-time-based-version-provider), [11](#version-provider-right-justified-zero-padded)\]</sup>|Yields the Day of the Month in the form ``DD``.|
|DeltaDays1900VersionProvider <sup>\[[8](#date-time-based-version-provider)\]</sup>|Yields the number of Days that occurred since ``1900``.|
|DeltaDays1970VersionProvider <sup>\[[8](#date-time-based-version-provider)\]</sup>|Yields the number of Days that occurred since ``1970``.|
|DeltaDays1980VersionProvider <sup>\[[8](#date-time-based-version-provider)\]</sup>|Yields the number of Days that occurred since ``1980``.|
|DeltaDays1990VersionProvider <sup>\[[8](#date-time-based-version-provider)\]</sup>|Yields the number of Days that occurred since ``1990``.|
|DeltaDays2000VersionProvider <sup>\[[8](#date-time-based-version-provider)\]</sup>|Yields the number of Days that occurred since ``2000``.|
|DeltaDays2010VersionProvider <sup>\[[8](#date-time-based-version-provider)\]</sup>|Yields the number of Days that occurred since ``2010``.|
|HourMinuteMultipartVersionProvider <sup>\[[8](#date-time-based-version-provider), [11](#version-provider-right-justified-zero-padded)\]</sup>|Yields the Hour and Minute multipart element in the form ``hhMM``.|
|IncrementVersionProvider <sup>\[[9](#version-provider-may-reset)\]</sup>||
|MonthDayOfMonthMultipartVersionProvider <sup>\[[8](#date-time-based-version-provider), [11](#version-provider-right-justified-zero-padded)\]</sup>|Yields the Month and Day of the Month multipart element in the form ``MMDD``.|
|NoOpVersionProvider <sup>\[[10](#version-provider-for-internal-use-only)\]</sup>|The *No Op* provider is used when no provider was specified. It assumes a pass through for whatever the [version element](#bump-version-element-specifications) might have been.|
|PreReleaseIncrementVersionProvider <sup>\[[9](#version-provider-may-reset)\]</sup>||
|SecondsSinceMidnightVersionProvider <sup>\[[8](#date-time-based-version-provider), [11](#version-provider-right-justified-zero-padded)\]</sup>|Yields the Seconds Since Midnight element.|
|ShortYearDayOfYearMultipart <sup>\[[8](#date-time-based-version-provider), [11](#version-provider-right-justified-zero-padded)\]</sup>|Yields the Short Year and Day of Year multipart element in the form ``YYDDD``.|
|ShortYearVersionProvider <sup>\[[8](#date-time-based-version-provider), [11](#version-provider-right-justified-zero-padded)\]</sup>|Yields the Short Year element in the form ``YY``.|
|UnknownVersionProvider <sup>\[[10](#version-provider-for-internal-use-only)\]</sup>|The *Unknown* provider is used when an unknown specification was given by the consumer. There is no fall back position other than to assume failure at that point.|
|YearDayOfYearMultipart <sup>\[[8](#date-time-based-version-provider), [11](#version-provider-right-justified-zero-padded)\]</sup>|Yields the Year and Day of Year multipart element in the form ``YYYYDDD``.|
|YearVersionProvider <sup>\[[8](#date-time-based-version-provider), [11](#version-provider-right-justified-zero-padded)\]</sup>|Yields the Year element in the form ``YYYY``.|

## Examples

This is a lot of verbiage to digest, but I promise you it is not that difficult once you grasp the approach. All the same, let us illustrate with a couple of examples. These are not meant to be exhaustive by any means, but only suggestive as to how to approach usage in your own projects.

### Refer to the Targets

The first thing you must do is specify the bump targets:

```Xml
<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  ...
  <Import Project="\path\to\package\build\BumpAssemblyVersions.targets" />
  ...
</Project>
```

For argument's sake, let's call ``\path\to\package\build`` something like ``$(ProjectDir)..\packages\BumpAssemblyVersions.1.0.1\build`` or ``path\to\your\global\packages\BumpAssemblyVersions\1.0.1\build``, depending on how you add your package references, your [version of NuGet](http://www.nuget.org/downloads), and so on. Basically, this is the relative path from your consumer project to the package. You may specify an absolute path if the package is landing elsewhere. You may use built-in predefined macros such as ``$(ProjectDir)``.

Refer to the targets similarly with the new project file format:

```Xml
<Project Sdk="Microsoft.NET.Sdk">
  ...
  <Import Project="\path\to\package\build\BumpAssemblyVersions.targets" />
  ...
</Project>
```

If you are using the new style ``<PackageReference />`` then it is likely the *targets* assets are already included and would not need to be imported. For example:

```Xml
<ItemGroup>
  <PackageReference Include="BumpAssemblyVersions" Version="1.2.0">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    <!--                    ^^^^^ -->
  </PackageReference>
</ItemGroup>
```

### Files to Bump

If you are building a legacy project format involving ``AssemblyInfo`` assembly attributes, these may not always exist in the ``AssemblyInfo.cs`` file, which is furnished by default. In this case, you may specify additional ``FilesToBump``, such as:

```Xml
<ItemGroup>
  <FilesToBump Condition="Exists('Properties\TestAssemblyInfo.cs')" Include="Properties\TestAssemblyInfo.cs" />
</ItemGroup>
```

In the above example, you may have a file called ``Properties\TestAssemblyInfo.cs`` in which additional assembly attributes have been specified. Assembly attributes may exist in any CSharp source file, so you are free to specify any number of additional ``FilesToBump`` beyond the default, conventional ``Properties\AssemblyInfo.cs``.

### Bump Version Specs

After that, it's all about furnishing your specification items. Here are just a few illustrative examples how you may approach the problem. We may note that the specifications may be furnished consistently, regardless whether we are talking about the new or old project file format. Let's have a look at the first example in a bit of depth.

```Xml
<ItemGroup>
  <BumpVersionSpec Include="AssemblyVersion" UseUtc="false" IncludeWildcard="true"
                   PatchProviderTemplate="Increment" PatchProviderTemplateMayReset="true" />
</ItemGroup>
```

In this example, ``AssemblyVersion`` is the indicated pattern, which serves a dual purposes for both legacy and the new project file versions in either the ``AssemblyVersionAttribute`` or the ``<AssemblyVersion />`` project property. The specification will use local time since ``UseUtc`` is ``false``, and it may ``IncludeWildcard``. The ``PatchProviderTemplate`` is configured for ``Increment``, which will select the ``IncrementVersionProvider``, which also ``MayReset`` for this element only on account of the element specific notation. All other elements default to the ``NoOpVersionProvider`` passthrough.

We will not discuss the following examples in a great deal of further depth, only to present them and discuss a couple of highlights.

```Xml
<ItemGroup>
  <BumpVersionSpec Include="AssemblyFileVersion" UseUtc="false"
                   PatchProviderTemplate="Increment" PatchProviderTemplateMayReset="true" />
  <BumpVersionSpec Include="AssemblyInformationalVersion" UseUtc="false"
                   PatchProviderTemplate="Increment" PatchProviderTemplateMayReset="true" />
</ItemGroup>
```

Both of these examples are specified for the assembly attributes [``AssemblyFileVersionAttribute``](http://docs.microsoft.com/en-us/dotnet/api/system.reflection.assemblyfileversionattribute) and [``AssemblyInformationalVersionAttribute``](http://docs.microsoft.com/en-us/dotnet/api/system.reflection.assemblyinformationalversionattribute), respectively. Otherwise, the specification is consistent with the first.

```Xml
<ItemGroup>
  <BumpVersionSpec Include="FileVersion" UseUtc="false"
                   PatchProviderTemplate="Increment" PatchProviderTemplateMayReset="true" />
  <BumpVersionSpec Include="InformationalVersion" UseUtc="false"
                   PatchProviderTemplate="Increment" PatchProviderTemplateMayReset="true" />
</ItemGroup>
```

Both of these examples are specified for the new project file format ``<FileVersion />`` and ``<InformationalVersion />``, respectively. Otherwise, the specification is consistent with the first.

Again, this is by no means an exhaustive set of examples, but only furnished for illustration purposes.

### End to End Examples

Let's examine a couple of end to end, live fire examples, so to speak. These are actual projects that I've committed to the repository and which make references to the actual package which I published on my local system.

#### BumpAssemblyVersions.Usage.Net472

First, let's take a look at the ``BumpAssemblyVersions.Usage.Net472`` project. I was pleasantly surprised to discover a build preparation task which is injected upon subscription.

```Xml
<Target Name="EnsureNuGetPackageBuildImports" BeforeTargets="PrepareForBuild">
  <PropertyGroup>
    <ErrorText>This project references NuGet package(s) that are missing on this computer. Use NuGet Package Restore to download them.  For more information, see http://go.microsoft.com/fwlink/?LinkID=322105. The missing file is {0}.</ErrorText>
  </PropertyGroup>
  <Error Condition="!Exists('..\packages\BumpAssemblyVersions.1.0.1\build\BumpAssemblyVersions.props')" Text="$([System.String]::Format('$(ErrorText)', '..\packages\BumpAssemblyVersions.1.0.1\build\BumpAssemblyVersions.props'))" />
  <Error Condition="!Exists('..\packages\BumpAssemblyVersions.1.0.1\build\BumpAssemblyVersions.targets')" Text="$([System.String]::Format('$(ErrorText)', '..\packages\BumpAssemblyVersions.1.0.1\build\BumpAssemblyVersions.targets'))" />
</Target>
```

Then we automatically import the targets when they are determined to exist:

```Xml
<Import Project="..\packages\BumpAssemblyVersions.1.0.1\build\BumpAssemblyVersions.targets"
        Condition="Exists('..\packages\BumpAssemblyVersions.1.0.1\build\BumpAssemblyVersions.targets')" />
```

Then we make the specifications. In this case, we will also make the specification condition in the same way the import is conditional. These are all decisions that you should make for yourselves, but it does seem like good practice to me:

```Xml
<ItemGroup Condition="Exists('..\packages\BumpAssemblyVersions.1.0.1\build\BumpAssemblyVersions.targets')">
  <BumpVersionSpec Include="AssemblyVersion" IncludeWildcard="true"
                   PatchProviderTemplate="Increment" PatchProviderTemplateMayReset="true" />
  <BumpVersionSpec Include="AssemblyFileVersion" UseUtc="true" PatchProviderTemplate="MonthDayOfMonth"
                   BuildProviderTemplate="Increment" BuildProviderTemplateMayReset="true" />
  <BumpVersionSpec Include="AssemblyInformationalVersion" UseUtc="true" PatchProviderTemplate="MonthDayOfMonth"
                   BuildProviderTemplate="Increment" BuildProviderTemplateMayReset="true" />
</ItemGroup>
```

We see a lot of the same elements here that we have explained in previous examples, so I will not dwell on those. The ``AssemblyVersion`` specification is fairly self-explanatory. We see ``AssemblyFileVersion`` and ``AssemblyInformationalVersion`` incorporate the ``MonthDayOfMonthMultipartVersionProvider`` via the ``MonthDayOfMonth`` shorthand. Instead of resetting the ``Patch`` specification, we see that pattern declared for the ``BuildProviderTemplate``. These have all been tested and work very well.

#### BumpAssemblyVersions.Usage.NetStandard

Next, let's take a look at the ``BumpAssemblyVersions.Usage.NetStandard`` project. First, we import the development only package reference.

```Xml
<Import Project="$(ProjectDir)..\packages\BumpAssemblyVersions.1.0.1\build\BumpAssemblyVersions.targets"
        Condition="Exists('$(ProjectDir)..\packages\BumpAssemblyVersions.1.0.1\build\BumpAssemblyVersions.targets')" />
```

Next, we make the appropriate specifications:

```Xml
<ItemGroup Condition="Exists('$(ProjectDir)..\packages\BumpAssemblyVersions.1.0.1\build\BumpAssemblyVersions.targets')">
  <BumpVersionSpec Include="Version" DefaultVersion="0.0.0.0" CreateNew="true" UseUtc="true" MayReset="true"
                   MajorProviderTemplate="YearVersion" MinorProviderTemplate="DayOfYear"
                   PatchProviderTemplate="HourMinute" BuildProviderTemplate="Increment" />
  <BumpVersionSpec Include="AssemblyVersion" DefaultVersion="0.0.0.0" CreateNew="true" UseUtc="true" MayReset="true"
                   MajorProviderTemplate="YearVersion" MinorProviderTemplate="DayOfYear"
                   PatchProviderTemplate="HourMinute" BuildProviderTemplate="Increment" />
  <BumpVersionSpec Include="FileVersion" DefaultVersion="0.0.0.0" CreateNew="true" UseUtc="true" MayReset="true"
                   MajorProviderTemplate="YearVersion" MinorProviderTemplate="DayOfYear"
                   PatchProviderTemplate="HourMinute" BuildProviderTemplate="Increment" />
  <BumpVersionSpec Include="InformationalVersion" DefaultVersion="0.0.0.0" CreateNew="true" UseUtc="true" MayReset="true"
                   MajorProviderTemplate="YearVersion" MinorProviderTemplate="DayOfYear"
                   PatchProviderTemplate="HourMinute" BuildProviderTemplate="Increment" />
  <BumpVersionSpec Include="PackageVersion" DefaultVersion="0.0.0.0" CreateNew="true" UseUtc="true" MayReset="true"
                   MajorProviderTemplate="YearVersion" MinorProviderTemplate="DayOfYear"
                   PatchProviderTemplate="HourMinute" BuildProviderTemplate="Increment" />
</ItemGroup>
```

Couple of points to make here. The initial project contained no version elements whatsoever. This tested that ``CreateNew`` does in fact work; which it did.

Next, ``MayReset`` now works as expected. This was a gap I had been meaning to correct, but had not quite gotten around to. It was simple enough to correct, and does work very well. Other than that, each of the templates and their specifications are working as expected.

An example of the resulting elements is as follows. Completely new ``PropertyGroup`` elements were injected for each version, which is perfectly fine for the time being. You may decide to clean that up in your projects before, during, or after your usage, but this is completely left to your discretion to do so.

```Xml
<PropertyGroup>
  <PackageVersion>2018.249.1429.0</PackageVersion>
</PropertyGroup>
<PropertyGroup>
  <InformationalVersion>2018.249.1429.0</InformationalVersion>
</PropertyGroup>
<PropertyGroup>
  <FileVersion>2018.249.1429.0</FileVersion>
</PropertyGroup>
<PropertyGroup>
 <AssemblyVersion>2018.249.1429.0</AssemblyVersion>
</PropertyGroup>
<PropertyGroup>
  <Version>2018.249.1429.0</Version>
</PropertyGroup>
```

## Footnotes

\[<a name="msbuild-caveats">1</a>\] With any of these inputs, usual ``MSBuild`` ``Condition`` rules apply, so you may mix and match the specified files to bump depending on ``Configuration`` or other criteria at your discretion.

\[<a name="bump-version-spec-wildcards">2</a>\] Not all [Bump Version Specifications](#bump-version-specifications) support wildcards; it is left to consumer discretion when to leverage wildcards.

\[<a name="bump-version-spec-release-labels">3</a>\] Not all [Bump Version Specifications](#bump-version-specifications) allow support for release labels; it is left to consumer discretion when to release labels.

\[<a name="bump-version-spec-attribs">4</a>\] Attributes provided without *Version Provider Prefixes* <sup>\[[5](#version-provider-attrib-prefixes)\]</sup> apply for all [Elements](#bump-version-element-specifications) given by the [Specification](#bump-version-specifications).

\[<a name="version-provider-attrib-prefixes">5</a>\] Some Bump Version Attributes may be specified for individual Version Providers as follows: ``[Major|Minor|Patch|Template|Release]ProviderTemplate]AttributeName``. As indicated here, this prefix is optional; when not specified, the attribute applies for the whole [Specification](#bump-version-specifications).

\[<a name="specification-only-attribute">6</a>\] These attributes are all [Specification](#bump-version-specifications) only attributes. That is, they apply for the Specification as a whole and are not part of the individual [Elements](#bump-version-element-specifications) specifications.

\[<a name="release-increment-version-provider-specific">7</a>\] These attributes apply only for the ``PreReleaseIncrementVersionProvider``.

\[<a name="date-time-based-version-provider">8</a>\] All *Version Providers* support a ``Timestamp``, but some depend on it as essential to their operation. In addition, you may specify whether to [``UseUtc``](#version-provider-attributes) either for the individual [Elements](#bump-version-element-specifications) or for the [Specification](#bump-version-specifications) as a whole.

\[<a name="version-provider-may-reset">9</a>\] Some *Version Providers* support whether it ``May Reset``, either for the individual [Elements](#bump-version-element-specifications) or for the [Specification](#bump-version-specifications) as a whole. If any of the *more significant* providers ``Changed`` in any way, then the affected provider will reset.

\[<a name="version-provider-for-internal-use-only">10</a>\] Some *Version Providers* are ``For Internal Use Only``.

\[<a name="version-provider-right-justified-zero-padded">11</a>\] All version providers yield right justified zero padded textual results using an intuitive, appropriate number of digits.

\[<a name="dotnet-docs-issues-6476">12</a>\] [Document AssemblyInfo properties](http://github.com/dotnet/docs/issues/6476)

\[<a name="microsoft-dotnet-generateassemblyinfo-targets">13</a>\] [http://github.com/dotnet/sdk/blob/master/src/Tasks/Microsoft.NET.Build.Tasks/targets/Microsoft.NET.GenerateAssemblyInfo.targets](http://github.com/dotnet/sdk/blob/master/src/Tasks/Microsoft.NET.Build.Tasks/targets/Microsoft.NET.GenerateAssemblyInfo.targets)
