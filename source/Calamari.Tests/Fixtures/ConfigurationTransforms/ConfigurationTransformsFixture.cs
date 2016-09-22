﻿using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Calamari.Integration.ConfigurationTransforms;
using Calamari.Integration.FileSystem;
using Calamari.Tests.Fixtures.Util;
using Calamari.Tests.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace Calamari.Tests.Fixtures.ConfigurationTransforms
{
    [TestFixture]
    public class ConfigurationTransformsFixture : CalamariFixture
    {
        InMemoryLog log;
        ConfigurationTransformer configurationTransformer;

        [SetUp]
        public void SetUp()
        {
            log = new InMemoryLog();
            configurationTransformer = new ConfigurationTransformer(log: log);
        }

        [Test]
        [Category(TestEnvironment.CompatibleOS.Windows)] //Problem with XML on Linux
        public void WebReleaseConfig()
        {
            var text = PerformTest(GetFixtureResouce("Samples", "Web.config"), GetFixtureResouce("Samples", "Web.Release.config"));
            var contents = XDocument.Parse(text);

            Assert.IsNull(GetDebugAttribute(contents));
            Assert.AreEqual(GetAppSettingsValue(contents).Value, "Release!");
            Assert.IsNull(GetCustomErrors(contents));
            log.Messages.Should().NotContain(m => m.Level == InMemoryLog.Level.Error, "Should not log errors");
            log.Messages.Should().NotContain(m => m.Level == InMemoryLog.Level.Warn, "Should not log warnings");
        }

        [Test]
        [Category(TestEnvironment.CompatibleOS.Windows)] //Problem with XML on Linux
#if USE_OCTOPUS_XMLT
        //vs shows ambiguous refence here but it builds and runs fine?
        [ExpectedException(typeof(Octopus.System.Xml.XmlException))]
#else
        [ExpectedException(typeof(System.Xml.XmlException))]
#endif
        public void ShouldThrowExceptionForBadConfig()
        {
            PerformTest(GetFixtureResouce("Samples", "Bad.config"), GetFixtureResouce("Samples", "Web.Release.config"));
        }

        [Test]
        [Category(TestEnvironment.CompatibleOS.Windows)] //Problem with XML on Linux
        public void ShouldSupressExceptionForBadConfig_WhenFlagIsSet()
        {
            configurationTransformer = new ConfigurationTransformer(true);
            PerformTest(GetFixtureResouce("Samples", "Bad.config"), GetFixtureResouce("Samples", "Web.Release.config"));
        }

        [Test]
        [Category(TestEnvironment.CompatibleOS.Windows)] //Problem with XML on Linux
        public void ShouldShowMessageWhenResultIsInvalidXml()
        {
            PerformTest(GetFixtureResouce("Samples", "Web.config"), GetFixtureResouce("Samples", "Web.Empty.config"));
            log.Messages.Where(m => m.Level == InMemoryLog.Level.Warn)
                .Select(m => m.MessageFormat)
                .Should()
                .Contain("The XML configuration file {0} no longer has a root element and is invalid after being transformed by {1}");
        }

        string PerformTest(string configurationFile, string transformFile)
        {
            var temp = Path.GetTempFileName();
            File.Copy(configurationFile, temp, true);

            using (new TemporaryFile(temp))
            {
                configurationTransformer.PerformTransform(temp, transformFile, temp);
                return File.ReadAllText(temp);
            }
        }

        static XAttribute GetDebugAttribute(XDocument document)
        {
            return document.Descendants("compilation").First().Attribute("debug");
        }

        static XAttribute GetAppSettingsValue(XDocument document)
        {
            return document.Descendants("appSettings").Descendants("add").First().Attribute("value");
        }

        XElement GetCustomErrors(XDocument document)
        {
            return document.Descendants("customErrors").FirstOrDefault();
        }
    }
}
