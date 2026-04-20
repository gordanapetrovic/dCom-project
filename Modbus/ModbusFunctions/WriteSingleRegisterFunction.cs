using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus write single register functions/requests.
    /// </summary>
    public class WriteSingleRegisterFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteSingleRegisterFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public WriteSingleRegisterFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusWriteCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            ModbusWriteCommandParameters p = (ModbusWriteCommandParameters)this.CommandParameters;

            byte[] message = new byte[12];
            message[0] = (byte)(p.TransactionId >> 8);
            message[1] = (byte)p.TransactionId;
            message[2] = 0;
            message[3] = 0;
            message[4] = 0;
            message[5] = 6;
            message[6] = p.UnitId;
            message[7] = p.FunctionCode;
            message[8] = (byte)(p.OutputAddress >> 8);
            message[9] = (byte)p.OutputAddress;
            message[10] = (byte)(p.Value >> 8);
            message[11] = (byte)p.Value;

            return message;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            ModbusWriteCommandParameters p = (ModbusWriteCommandParameters)this.CommandParameters;

            if (response[7] == (byte)(p.FunctionCode + 0x80))
            {
                HandeException(response[8]);
            }

            ushort value = (ushort)((response[10] << 8) | response[11]);

            Dictionary<Tuple<PointType, ushort>, ushort> result = new Dictionary<Tuple<PointType, ushort>, ushort>();
            result.Add(new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, p.OutputAddress), value);

            return result;
        }
    }
}