using Wirehome.Contracts.Hardware.RemoteSockets.Protocols;
using Wirehome.Hardware.RemoteSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wirehome.Extensions.Core
{
    public class DipswitchCode : IEquatable<DipswitchCode>
    {
        public DipswitchSystemCode System { get; }
        public DipswitchUnitCode Unit { get; }
        public RemoteSocketCommand Command { get; }

        public DipswitchCode(DipswitchSystemCode system, DipswitchUnitCode unit, RemoteSocketCommand command)
        {
            System = system;
            Unit = unit;
            Command = command;
        }

    
        public static DipswitchCode ParseCode(uint code)
        {
            var command = ParseCommand(code);
            if (!command.HasValue) return null;

            var unit = ParseUnit(code);
            if (!unit.HasValue) return null;

            var system = ParseSystem(code);

            return new DipswitchCode(system, unit.GetValueOrDefault(), command.GetValueOrDefault());
        }

        private static RemoteSocketCommand? ParseCommand(uint code)
        {
            var commandMask = "00000000 00000000 00000000 00111111";

            commandMask = commandMask.Replace(" ", "");

            var commandMaskValue = Convert.ToInt32(commandMask, 2);

            var maskedCommand = code & commandMaskValue;

            if (maskedCommand == 0x11)
            {
                return RemoteSocketCommand.TurnOn;
            }
            else if (maskedCommand == 0x14)
            {
                return RemoteSocketCommand.TurnOff;
            }

            return null;
        }

        private static DipswitchUnitCode? ParseUnit(uint code)
        {
            var unitMask = "00000000 00000000 00011111 11000000";

            unitMask = unitMask.Replace(" ", "");

            var unitMaskValue = Convert.ToInt32(unitMask, 2);

            var maskedUnit = code & unitMaskValue;

            maskedUnit = maskedUnit >> 6;

            if (maskedUnit == 0x15)
            {
                return DipswitchUnitCode.A;
            }
            else if (maskedUnit == 0x45)
            {
                return DipswitchUnitCode.B;
            }
            else if (maskedUnit == 0x51)
            {
                return DipswitchUnitCode.C;
            }
            else if (maskedUnit == 0x54)
            {
                return DipswitchUnitCode.D;
            }

            return null;
        }

        private static DipswitchSystemCode ParseSystem(uint code)
        {
            DipswitchSystemCode parsedCode = DipswitchSystemCode.AllOff;

            var systemMask = "00000000 00000000 00011000 00000000";

            systemMask = systemMask.Replace(" ", "");

            var systemMaskValue = Convert.ToInt32(systemMask, 2);

            systemMaskValue = CheckNextSystem(ref parsedCode, systemMaskValue, code, DipswitchSystemCode.Switch1);
            systemMaskValue = CheckNextSystem(ref parsedCode, systemMaskValue, code, DipswitchSystemCode.Switch2);
            systemMaskValue = CheckNextSystem(ref parsedCode, systemMaskValue, code, DipswitchSystemCode.Switch3);
            systemMaskValue = CheckNextSystem(ref parsedCode, systemMaskValue, code, DipswitchSystemCode.Switch4);
            systemMaskValue = CheckNextSystem(ref parsedCode, systemMaskValue, code, DipswitchSystemCode.Switch5);


            return parsedCode;
        }

        private static int CheckNextSystem(ref DipswitchSystemCode parsedCode, int mask, uint code, DipswitchSystemCode systemCode)
        {
            mask = mask << 2;
            var maskedSysten = code & mask;

            if (maskedSysten == 0)
            {
                parsedCode |= systemCode;
            }

            return mask;
        }

        public bool Equals(DipswitchCode other)
        {
            return AreEqual(this, other);
        }

        public override bool Equals(object obj)
        {
            var other = obj as DipswitchCode;

            return AreEqual(this, other);
        }

        public override int GetHashCode()
        {
            return System.GetHashCode() ^ Unit.GetHashCode() ^ Command.GetHashCode();
        }

        public bool AreEqual(DipswitchCode a, DipswitchCode b)
        {
            if (a.System == b.System && a.Command == b.Command && a.Unit == b.Unit)
                return true;

            return false;
        }
    }
}