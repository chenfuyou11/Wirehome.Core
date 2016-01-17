﻿using System;
using System.Xml.Linq;
using HA4IoT.Contracts.Actuators;
using HA4IoT.Contracts.Configuration;
using HA4IoT.Contracts.Hardware;
using HA4IoT.Tests.Mockups;

namespace HA4IoT.Configuration.Tests
{
    internal class TestConfigurationExtender : IConfigurationExtender
    {
        public string Namespace { get; } = "http://www.ha4iot.de/ConfigurationExtenders/Test";

        public IDevice ParseDevice(XElement element)
        {
            switch (element.Name.LocalName)
            {
                case "I2CBus": return new TestI2CBus(new DeviceId(element.GetMandatoryStringFromAttribute("id")));

                default: throw new ConfigurationInvalidException("Device not supported.", element);
            }
        }

        public IBinaryOutput ParseBinaryOutput(XElement element)
        {
            throw new NotImplementedException();
        }

        public IBinaryInput ParseBinaryInput(XElement element)
        {
            throw new NotImplementedException();
        }

        public IActuator ParseActuator(XElement element)
        {
            throw new NotImplementedException();
        }

        public void OnConfigurationParsed()
        {
        }

        public void OnInitializationFromCodeCompleted()
        {
        }
    }
}
