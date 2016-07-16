using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HA4IoT.Hardware.RemoteSwitch.Codes
{
    public class BrennenstuhlCode : IEquatable<BrennenstuhlCode>
    {
        public BrennenstuhlSystemCode System { get; }
        public BrennenstuhlUnitCode Unit { get; }
        public RemoteSocketCommand Command { get; }

        public BrennenstuhlCode(BrennenstuhlSystemCode system, BrennenstuhlUnitCode unit, RemoteSocketCommand command)
        {
            System = system;
            Unit = unit;
            Command = command;
        }

        /// <summary>
        /// Return pobject representation of code
        /// </summary>
        /// <param name="code"></param>
        /// <returns>Return null if parsing fails</returns>
        public static BrennenstuhlCode ParseCode(uint code)
        {
            var command = ParseCommand(code);
            if (!command.HasValue) return null;
            
            var unit = ParseUnit(code);
            if (unit.HasValue) return null;
            
            var system = ParseSystem(code);

            return new BrennenstuhlCode(system, unit.GetValueOrDefault(), command.GetValueOrDefault());
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

        private static BrennenstuhlUnitCode? ParseUnit(uint code)
        {
            var unitMask = "00000000 00000000 00011111 11000000";

            unitMask = unitMask.Replace(" ", "");

            var unitMaskValue = Convert.ToInt32(unitMask, 2);

            var maskedUnit = code & unitMaskValue;

            maskedUnit = maskedUnit >> 6;

            if (maskedUnit == 0x15)
            {
                return BrennenstuhlUnitCode.A;
            }
            else if (maskedUnit == 0x45)
            {
                return BrennenstuhlUnitCode.B;
            }
            else if (maskedUnit == 0x51)
            {
                return BrennenstuhlUnitCode.C;
            }
            else if (maskedUnit == 0x54)
            {
                return BrennenstuhlUnitCode.D;
            }

            return null;
        }

        private static BrennenstuhlSystemCode ParseSystem(uint code)
        {
            BrennenstuhlSystemCode parsedCode = BrennenstuhlSystemCode.AllOff;

            var systemMask = "00000000 00000000 00011000 00000000";

            systemMask = systemMask.Replace(" ", "");

            var systemMaskValue = Convert.ToInt32(systemMask, 2);

            systemMaskValue = CheckNextSystem(ref parsedCode, systemMaskValue, code, BrennenstuhlSystemCode.Switch1);
            systemMaskValue = CheckNextSystem(ref parsedCode, systemMaskValue, code, BrennenstuhlSystemCode.Switch2);
            systemMaskValue = CheckNextSystem(ref parsedCode, systemMaskValue, code, BrennenstuhlSystemCode.Switch3);
            systemMaskValue = CheckNextSystem(ref parsedCode, systemMaskValue, code, BrennenstuhlSystemCode.Switch4);
            systemMaskValue = CheckNextSystem(ref parsedCode, systemMaskValue, code, BrennenstuhlSystemCode.Switch5);


            return parsedCode;
        }

        private static int CheckNextSystem(ref BrennenstuhlSystemCode parsedCode, int mask, uint code, BrennenstuhlSystemCode systemCode)
        {
            mask = mask << 2;
            var maskedSysten = code & mask;

            if (maskedSysten == 0)
            {
                parsedCode |= systemCode;
            }

            return mask;
        }

        public bool Equals(BrennenstuhlCode other)
        {
            return AreEqual(this, other);
        }

        public override bool Equals(object obj)
        {
            var other = obj as BrennenstuhlCode;

            return AreEqual(this, other);
        }

        public override int GetHashCode()
        {
            return System.GetHashCode() ^ Unit.GetHashCode() ^ Command.GetHashCode();
        }

        public bool AreEqual(BrennenstuhlCode a, BrennenstuhlCode b)
        {
            if (a.System == b.System && a.Command == b.Command && a.Unit == b.Unit)
                return true;

            return false;
        }
    }
}
