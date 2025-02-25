// Licensed to Elasticsearch B.V under one or more agreements.
// Elasticsearch B.V licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information

using Elastic.Markdown.Myst.FrontMatter;
using Elastic.Markdown.Myst.Settings;
using YamlDotNet.Serialization;

namespace Elastic.Markdown.Myst;

public static class YamlSerialization
{
	public static T Deserialize<T>(string yaml)
	{
		var input = new StringReader(yaml);

		var deserializer = new StaticDeserializerBuilder(new DocsBuilderYamlStaticContext())
			.IgnoreUnmatchedProperties()
			.WithTypeConverter(new SemVersionConverter())
#pragma warning disable CS0618 // Type or member is obsolete
			.WithTypeConverter(new DeploymentConverter())
			.WithTypeConverter(new ApplicableToConverter())
#pragma warning restore CS0618 // Type or member is obsolete
			.Build();

		var frontMatter = deserializer.Deserialize<T>(input);
		return frontMatter;

	}
}

[YamlStaticContext]
[YamlSerializable(typeof(YamlSettings))]
[YamlSerializable(typeof(SettingsGrouping))]
[YamlSerializable(typeof(YamlSettings))]
[YamlSerializable(typeof(SettingsGrouping))]
[YamlSerializable(typeof(Setting))]
[YamlSerializable(typeof(AllowedValue))]
[YamlSerializable(typeof(SettingMutability))]

public partial class DocsBuilderYamlStaticContext;

