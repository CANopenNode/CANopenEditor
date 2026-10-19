using Google.Protobuf;
using libEDSsharp;
using LibCanOpen;
using Xunit;

namespace Tests
{
    public class UpstreamInteropRegressionTests
    {
        [Fact]
        public void DeviceOrderCodeSurvivesProtobufSerialization()
        {
            var model = new EDSsharp();
            model.di.OrderCode = "STANDARD-DEVICE";
            CanOpenDevice message = MappingEDS.MapToProtobuffer(model);
            CanOpenDevice readback = CanOpenDevice.Parser.ParseFrom(message.ToByteArray());
            Assert.Equal(model.di.OrderCode, MappingEDS.MapFromProtobuffer(readback).di.OrderCode);
        }
    }
}
