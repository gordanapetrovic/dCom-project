using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read discrete inputs functions/requests.
    /// </summary>
    public class ReadDiscreteInputsFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadDiscreteInputsFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public ReadDiscreteInputsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            ModbusReadCommandParameters p = (ModbusReadCommandParameters)this.CommandParameters;

            byte[] message = new byte[12];
            message[0] = (byte)(p.TransactionId >> 8);
            message[1] = (byte)p.TransactionId;
            message[2] = 0;
            message[3] = 0;
            message[4] = 0;
            message[5] = 6;
            message[6] = p.UnitId;
            message[7] = p.FunctionCode;
            message[8] = (byte)(p.StartAddress >> 8);
            message[9] = (byte)p.StartAddress;
            message[10] = (byte)(p.Quantity >> 8);
            message[11] = (byte)p.Quantity;

            return message;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            ModbusReadCommandParameters p = (ModbusReadCommandParameters)this.CommandParameters;

            if (response[7] == (byte)(p.FunctionCode + 0x80))
            {
                HandeException(response[8]);
            }

            Dictionary<Tuple<PointType, ushort>, ushort> result = new Dictionary<Tuple<PointType, ushort>, ushort>();

            for (int i = 0; i < p.Quantity; i++)
            {
                int byteIndex = 9 + (i / 8);
                int bitIndex = i % 8;
                ushort value = (ushort)(((response[byteIndex] >> bitIndex) & 0x01) == 1 ? 1 : 0);
                ushort address = (ushort)(p.StartAddress + i);

                result.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_INPUT, address), value);
            }

            return result;
        }
    }
}