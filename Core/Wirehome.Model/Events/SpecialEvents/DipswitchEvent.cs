﻿using System;
using Wirehome.ComponentModel.ValueTypes;
using Wirehome.Core;
using Wirehome.Core.Hardware.RemoteSockets;
using Wirehome.Core.Extensions;
using Wirehome.Model.Extensions;
using System.Collections;
using System.Collections.Generic;

namespace Wirehome.ComponentModel.Events
{
    public class DipswitchEvent : Event
    {
        public DipswitchEvent(string deviceUID, DipswitchCode code)
        {
            Type = EventType.DipswitchCode;
            Uid = Guid.NewGuid().ToString();
            this[EventProperties.SourceDeviceUid] = (StringValue)deviceUID;
            this[EventProperties.Unit] = (StringValue)code.Unit.ToString();
            this[EventProperties.System] = (StringValue)code.System.ToString();
            this[EventProperties.CommandCode] = (StringValue)code.Command.ToString();
            this[EventProperties.EventTime] = (DateTimeValue)SystemTime.Now;
        }

        public override IEnumerable<string> RoutingAttributes() => new string[] { EventProperties.Unit, EventProperties.System };
        
        public DipswitchCode DipswitchCode => DipswitchCode.ParseCode(this[EventProperties.System].ToStringValue(), this[EventProperties.Unit].ToStringValue(), this[EventProperties.CommandCode].ToStringValue());
    }
}
